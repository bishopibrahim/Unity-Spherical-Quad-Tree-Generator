using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemRoot : MonoBehaviour
{
    public Transform SystemCenter; // Client sided

    public Subsystem SolarSystem;

    [Range(0.01f,1f)]
    public float SystemScale;

    public double TimeScale;
    public double TimeStep;

    void OnValidate()
    {
        Time.fixedDeltaTime = (float)TimeStep;
        SetInitialTransforms();
    }

    public void SetInitialTransforms()
    {
        /*SetGlobalPositions(SolarSystem, 0);
        List<Subsystem> bodies = GetBodies(SolarSystem);
        
        foreach (Subsystem body in bodies)
        {
            body.BodyTransform.position = body.globalPosition.ToVector3();
        }*/

        DrawObjects(new Vector3d(SystemCenter.position), SolarSystem, 0);
    }

    public void SetGlobalPositions(Subsystem rootSystem, double time)
    {
        if (rootSystem.ParentSystem == null) // rootest root of them all
        {
            rootSystem.globalPosition = new Vector3d(0,0,0);
        }

        if (!rootSystem.IsOneBody)
        {
            Vector3d[] newPos = rootSystem.SolvePositions(time);
            rootSystem.BodyA.globalPosition = rootSystem.globalPosition + newPos[0];
            rootSystem.BodyB.globalPosition = rootSystem.globalPosition + newPos[1];

            SetGlobalPositions(rootSystem.BodyA, time);
            SetGlobalPositions(rootSystem.BodyB, time);
        }        
    }

    public List<Subsystem> GetBodies(Subsystem rootSystem)
    {
        List<Subsystem> bodyList = new List<Subsystem>();

        if (rootSystem.IsOneBody)
        {
            bodyList.Add(rootSystem);
        }
        else
        {
            bodyList.AddRange(GetBodies(rootSystem.BodyA));
            bodyList.AddRange(GetBodies(rootSystem.BodyB));
        }
        return bodyList;
    }

    public void DrawObjects(Vector3d referencePosition, Subsystem rootSystem, double time)
    {
        SetGlobalPositions(SolarSystem, time);
        List<Subsystem> bodyList = GetBodies(SolarSystem);

        foreach (Subsystem body in bodyList)
        {
            body.BodyTransform.position = SystemScale * (body.globalPosition - referencePosition).ToVector3();
            body.RenderOrbits();
        }
    }

    void FixedUpdate()
    {
        DrawObjects(new Vector3d(SystemCenter.position), SolarSystem, (float)(Time.time*TimeScale));
    }
}
