using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PhysicsConstants;

public class Subsystem : MonoBehaviour
{
    public double G = 0.001d;
    public int NRiterations = 4; //number of iterations of Newton-Raphson algorithm

    public Transform BodyTransform; // If the system is one body

    [HideInInspector]
    public LineRenderer lineRenderer;

    public Subsystem BodyA;
    public Subsystem BodyB;

    public OrbitalParameters RelativeOrbParams;

    [HideInInspector]
    public Vector3d AscendingNodeA; 
    [HideInInspector]
    public Vector3d AscendingNodeB;
    [HideInInspector]
    public Vector3d NormalToOrbitalPlane;

    public Subsystem ParentSystem;
    public Subsystem orbitReference = null;
    SystemRoot root;

    public double systemMass;

    public double standardGravitationalParameter;

    [HideInInspector]
    public Vector3d relativePositionA; // Relative to orbital center
    [HideInInspector]
    public Vector3d relativePositionB;

    [HideInInspector]
    public Vector3d globalPosition; // COM Position or object position for one body system, set by the system root to avoid nasty recursion crashes

    public Vector3d[] SolvePositions(double time) //Keplerian parametrization of elliptical orbit
    {
        double mu = (BodyA.systemMass + BodyB.systemMass) * G; // standard gravitational parameter
        double n = Math.Sqrt(mu/(Math.Pow(RelativeOrbParams.SemiMajorAxis,3))); // mean motion
        double M = n * (time - RelativeOrbParams.phase) % (2 * Math.PI); // mean anomaly
        double E = M; //eccentric anomaly
        double Eprev;

        for (int i = 0; i < NRiterations; i++)
        {
            Eprev = E; 
            double fE = E - RelativeOrbParams.e * Math.Sin(E) - M;
            double fPrimeE = 1 - RelativeOrbParams.e * Math.Cos(E);
            E = E - fE/fPrimeE;
        }

        double theta = 2 * Math.Atan(Math.Sqrt((1 + RelativeOrbParams.e)/(1 - RelativeOrbParams.e)) * Math.Tan(E/2));
        double radius = (RelativeOrbParams.SemiMajorAxis * (1 - RelativeOrbParams.e * RelativeOrbParams.e)) / (1 + RelativeOrbParams.e * Math.Cos(theta));
        double reducedMass = (BodyA.systemMass * BodyB.systemMass) / (BodyA.systemMass + BodyB.systemMass);
        
        Vector3d posA = -1.0d * (reducedMass / BodyA.systemMass) * radius * new Vector3d(Math.Cos(theta), 0, Math.Sin(theta));
        Vector3d posB = (reducedMass / BodyB.systemMass) * radius * new Vector3d(Math.Cos(theta), 0, Math.Sin(theta));

        // Rotational transformations

        posA = LongitudinalRotation * posA;
        posA = InclinationRotation * posA;
        posA = ArgumentRotation * posA;

        posB = LongitudinalRotation * posB;
        posB = InclinationRotation * posB;
        posB = ArgumentRotation * posB;

        Vector3d[] returnArray = new Vector3d[2];
        returnArray[0] = posA;
        returnArray[1] = posB;
        return returnArray;
    }

    [HideInInspector]
    public bool IsOneBody{
        get {return (BodyA==null && BodyB==null);}
        set {;}
    }

    public double period{
        get {return Math.PI * 2 * Math.Sqrt((Math.Pow(RelativeOrbParams.SemiMajorAxis,3)) / ((BodyA.systemMass + BodyB.systemMass) * G));}
        set {;}
    }

    void Awake()
    {
        root = FindObjectOfType<SystemRoot>();
        LongitudinalRotation = Matrix3d.AxisAngleRot(new Vector3d(0,1,0), RelativeOrbParams.LongitudeOfAscendingNode);

        Vector3d InclinationRotAxis = LongitudinalRotation * new Vector3d(0,0,-1);
        InclinationRotation = Matrix3d.AxisAngleRot(InclinationRotAxis, RelativeOrbParams.Inclination);

        Vector3d ArgumentRotAxis = InclinationRotation * new Vector3d(0,1,0);
        NormalToOrbitalPlane = ArgumentRotAxis;
        ArgumentRotation = Matrix3d.AxisAngleRot(ArgumentRotAxis, RelativeOrbParams.ArgumentOfPeriapsis);
    }

    public void RenderChildOrbits(Subsystem referenceSystem = null, float Scale = 1)
    {
        int points = (int)(period * RelativeOrbParams.pointsPerSecond);

        BodyA.lineRenderer.positionCount = points + 1;
        BodyB.lineRenderer.positionCount = points + 1;

        List<Vector3> BodyAList = new List<Vector3>();
        List<Vector3> BodyBList = new List<Vector3>();

        for (int i = 0; i < points; i++)
        {
            root.SetGlobalPositions(root.SolarSystem, i * (1/RelativeOrbParams.pointsPerSecond));

            Vector3d posA = BodyA.globalPosition;
            Vector3d posB = BodyB.globalPosition;

            if (referenceSystem != null)
            {
                Vector3d refPos = referenceSystem.globalPosition;
                posA = posA - refPos;
                posB = posB - refPos;
            }
           

            if (IsValid(posA.ToVector3()))
            {
                BodyAList.Add(Scale * (posA.ToVector3() - root.SystemCenter.position));
            }
            else
            {
                Debug.Log("fail");
            }
            if (IsValid(posB.ToVector3()))
            {
                BodyBList.Add(Scale * (posB.ToVector3() - root.SystemCenter.position));
            }
            else
            {
                Debug.Log("fail");
            }
            
        }

        BodyAList.Add(BodyAList[0]);
        BodyBList.Add(BodyBList[0]);

        BodyA.lineRenderer.SetPositions(BodyAList.ToArray());
        BodyB.lineRenderer.SetPositions(BodyBList.ToArray());
        
    }

    public void SetMass()
    {
        if (!IsOneBody)
        {
            systemMass = BodyA.systemMass + BodyB.systemMass;
        }

        if (ParentSystem != null)
        {
            ParentSystem.SetMass();
        }
    }

    void OnValidate()
    {
        root = FindObjectOfType<SystemRoot>();

        RelativeOrbParams.e = Math.Clamp(RelativeOrbParams.e, 0, 0.99d);

        LongitudinalRotation = Matrix3d.AxisAngleRot(new Vector3d(0,1,0), -1 * RelativeOrbParams.LongitudeOfAscendingNode);

        Vector3d InclinationRotAxis = LongitudinalRotation * new Vector3d(0,0,-1);
        InclinationRotation = Matrix3d.AxisAngleRot(InclinationRotAxis, -1 * RelativeOrbParams.Inclination);

        Vector3d ArgumentRotAxis = InclinationRotation * new Vector3d(0,1,0);
        NormalToOrbitalPlane = ArgumentRotAxis;
        ArgumentRotation = Matrix3d.AxisAngleRot(ArgumentRotAxis, -1 * RelativeOrbParams.ArgumentOfPeriapsis);

        SetMass();

        RenderOrbits();

        if (ParentSystem == null || !IsOneBody)
        {
            GetComponent<LineRenderer>().enabled = false;
        }
        else
        {
            GetComponent<LineRenderer>().enabled = true;
        }

        root.SetInitialTransforms();
    }

    public void RenderOrbits()
    {
        if (!IsOneBody)
        {
            BodyA.ParentSystem = this;
            BodyB.ParentSystem = this;
            RenderChildOrbits(orbitReference, root.SystemScale);       
        }
        else
        {
            if (!GetComponent<LineRenderer>())
            {
                gameObject.AddComponent<LineRenderer>();
                lineRenderer = GetComponent<LineRenderer>();
            }

            ParentSystem.RenderChildOrbits(ParentSystem.orbitReference, ParentSystem.root.SystemScale);
        }
    }

    public static bool IsValid(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }

    [System.Serializable]
    public struct OrbitalParameters // Reference direction is in the position x direction
    {
        public double SemiMajorAxis; // Semi-major axis

        public double e; // Eccentricity

    
        public double LongitudeOfAscendingNode;

        public double Inclination;

        public double ArgumentOfPeriapsis;


        public double phase; //Time at which object is at periapsis;

        public double pointsPerSecond;
    }

    // Orbital rotation matrices

    Matrix3d LongitudinalRotation = Matrix3d.Identity();
    Matrix3d InclinationRotation = Matrix3d.Identity();
    Matrix3d ArgumentRotation = Matrix3d.Identity();
}
