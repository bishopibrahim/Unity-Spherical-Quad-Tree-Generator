using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PhysicsConstants;

[System.Serializable, ExecuteAlways]
public class Body : MonoBehaviour
{
    public Vector3d initialPosition = new Vector3d(0,0,0);
    public Vector3d initialVelocity = new Vector3d(0,0,0);

    [HideInInspector]
    public Vector3d lastGlobalPosition = new Vector3d(0,0,0);
    [HideInInspector]
    public Vector3d globalPosition = new Vector3d(0,0,0);
    
    [HideInInspector]
    public Vector3d localPosition = new Vector3d(0,0,0);

    [HideInInspector]
    public Vector3d currentVelocity = new Vector3d(0,0,0);

    [HideInInspector]
    public Vector3d currentAcceleration = new Vector3d(0,0,0);

    // Used for projecting out orbits
    [HideInInspector]
    public Vector3d virtualPosition = new Vector3d(0,0,0);
    [HideInInspector]
    public Vector3d lastVirtualPosition = new Vector3d(0,0,0);
    [HideInInspector]
    public Vector3d virtualVelocity = new Vector3d(0,0,0);

    public double mass;
    public double radius;
    
    [Header("Trajectory display:")]
    private LineRenderer lineRenderer;
    public Color orbitColor = Color.white;
    public Body orbitReference = null;

    void Awake()
    {
        Vector3 pos = GetComponent<Transform>().position;
        initialPosition = new Vector3d((double)pos.x, (double)pos.y, (double)pos.z);

        SystemManager systemManager = FindObjectOfType<SystemManager>();

        if (systemManager != null && !systemManager.SystemObjects.Contains(this))
        {
            Debug.Log("Adding body.");
            systemManager.RegisterBody(this);
        }

        GetLineRenderer();
    }

    void GetLineRenderer()
    {
        if (lineRenderer == null)
        {
            if (GetComponent<LineRenderer>())
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            else
            {
                gameObject.AddComponent<LineRenderer>();
                lineRenderer = GetComponent<LineRenderer>();
            }

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
    }

    void OnValidate()
    {
        SetCircularVelocity();
        SystemManager systemManager = FindObjectOfType<SystemManager>();
        systemManager.UpdateBodyLGP();
        systemManager.trajectoryViewer.OnValidate();
    }

    void Start()
    {
        currentVelocity = initialVelocity.Copy();
        globalPosition = initialPosition.Copy();
    }

    [HideInInspector]
    public List<Vector3> trajectory = new List<Vector3>();

    public void DisplayTrajectory(int SamplePoints, int TimeSteps)
    {
        GetLineRenderer();

        int indexDelta = 1;

        if (SamplePoints >= TimeSteps/2 || SamplePoints <= 1)
        {
            SamplePoints = TimeSteps;
        }
        else
        {
            indexDelta = TimeSteps / SamplePoints;
        }

        lineRenderer.positionCount = SamplePoints;

        Vector3[] trajectoryArray = new Vector3[SamplePoints];

        for (int i = 0; i < SamplePoints; i++)
        {
            if (orbitReference == null)
            {
                trajectoryArray[i] = trajectory[i*indexDelta];
            }
            else
            {
                trajectoryArray[i] = (trajectory[i*indexDelta] - orbitReference.trajectory[i*indexDelta]) + orbitReference.initialPosition.ToVector3();
            }
        }

        lineRenderer.SetPositions(trajectoryArray);
        
    }

    public GameObject orbitParent;
    public bool circularize = false;

    void SetCircularVelocity()
    {
        if (orbitParent != null && circularize)
        {
            Vector3d displacement = initialPosition - orbitParent.GetComponent<Body>().initialPosition;
            double GM = Constants.G * orbitParent.GetComponent<Body>().mass;
            float orbitalVelocity = (float)Math.Sqrt(GM / displacement.magnitude);
            Vector3 velDir = Vector3.Cross(orbitParent.GetComponent<Transform>().up, displacement.ToVector3().normalized);
            initialVelocity = new Vector3d(velDir * orbitalVelocity);
        }
    }
}
