using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Recenterizer : MonoBehaviour
{
    public GameObject root;

    public void Recenter(List<Body> others)
    {
        Transform  rootTransform = root.GetComponent<Transform>();
        Vector3 rootPosition = rootTransform.position;

        for (int i = 0; i < others.Count; i++)
        {
            if (others[i] != root.GetComponent<Body>())
            {
                Vector3d pos1 = others[i].globalPosition;
                if (root.GetComponent<Body>() == null)
                {
                    Debug.Log("null root");
                }
                Vector3d pos2 = root.GetComponent<Body>().globalPosition;
                Vector3 newPos = (pos1 - pos2).ToVector3();
                others[i].GetComponent<Transform>().position = newPos;
            }
        }

        rootTransform.position = new Vector3(0, 0, 0);
    }
}
