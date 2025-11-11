/* Grid Projection Planetary Terrain Generator - By Bishop Ibrahim */
/* Based on techniques presented in Florian Michelic's 'Real-Time Rendering of Procedurally Generated Planets' paper */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Inspector variables
    public int resolution;
    public int exponent;
    public float radius;
    int framesBetweenRefresh = 0;
    public Transform referenceTransform;
    public ComputeShader surfaceComputeShader;

    // Mesh-gen variables
    Mesh surfaceMesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Vector3[] vertices;
    int[] triangles;

    // Noise
    Noise simplexNoise;
    public float amplitude = 1;
    public float frequency = 1;

    int frameCounter = 0;

    void OnValidate()
    {
        GuarenteeComponents(); 
        GenerateMesh();
    }

    void Update()
    {
        if (frameCounter >= framesBetweenRefresh)
        {
            GenerateMesh();
            frameCounter = 0;
        }
        frameCounter += 1;
    }

    void GuarenteeComponents()
    {
        if (meshFilter == null)
        {
            if (GetComponent<MeshFilter>())
            {
                meshFilter = GetComponent<MeshFilter>();
            }
            else
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
        }
        if (meshRenderer == null)
        {
            if (GetComponent<MeshRenderer>())
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }
            else
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }
        if (simplexNoise == null)
        {
            simplexNoise = new Noise();
        }
    }

    public void GenerateMesh()
    {
        if (surfaceMesh == null)
        {
            surfaceMesh = new Mesh();
        }

        vertices = new Vector3[resolution * resolution];
        triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        
        Vector3 w = referenceTransform.position.normalized;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, w);
        Matrix4x4 cameraRotMat = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        float gridOffset = GetGridDisplacement();

        // Compute shader setup
        int count = resolution*resolution;

        ComputeBuffer outputBuffer = new ComputeBuffer(count, sizeof(float) * 3);
        int kernel = surfaceComputeShader.FindKernel("CSMain");

        surfaceComputeShader.SetBuffer(kernel, "outputVertices", outputBuffer);
        surfaceComputeShader.SetFloat("radius", radius);
        surfaceComputeShader.SetInt("resolution", resolution);
        surfaceComputeShader.SetMatrix("rotMat", cameraRotMat);
        surfaceComputeShader.SetFloat("grid_displacement", gridOffset);

        // Planar triangulation and vertex position setting
        int triangleIndex = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + resolution * y;
                
                // Add triangles
                if (x < resolution - 1 && y < resolution - 1)
                {
                    // Upper triangle
                    triangles[triangleIndex] = i;
                    triangles[triangleIndex + 1] = i + 1;
                    triangles[triangleIndex + 2] = i + 1 + resolution;
                    // Lower triangle
                    triangles[triangleIndex + 3] = i;
                    triangles[triangleIndex + 4] = i + 1 + resolution;
                    triangles[triangleIndex + 5] = i + resolution;

                    triangleIndex += 6;
                }

                /*
                Vector2 Percent = new Vector2((float)x/(resolution-1), (float)y/(resolution-1));
                Vector3 newVertex = 2 * Percent.x * Vector3.right + 2 * Percent.y * Vector3.up + new Vector3(-1, -1, 0);
                newVertex += Vector3.forward * (1-Mathf.Pow(newVertex.x, exponent))*(1-Mathf.Pow(newVertex.y, exponent)) + Vector3.forward * GetGridDisplacement();
                Vector3 pointOnUnitSphere = GetPointOnUnitSphereFacingCamera(newVertex, cameraRotMat);
                float elevation = amplitude * simplexNoise.Evaluate(pointOnUnitSphere * frequency).Item1; 
                vertices[i] = pointOnUnitSphere * radius * (1+elevation);*/
            }
        }

        // Compute shader execution
        int threadGroupSize = Mathf.CeilToInt(count / 256f);
        surfaceComputeShader.Dispatch(kernel, threadGroupSize, 1, 1);

        outputBuffer.GetData(vertices);
        outputBuffer.Release();

        surfaceMesh.Clear();
        surfaceMesh.vertices = vertices;
        surfaceMesh.triangles = triangles;
        surfaceMesh.RecalculateNormals();

        meshFilter.sharedMesh = surfaceMesh;
    }

    public Vector3 GetPointOnUnitSphereFacingCamera(Vector3 point, Matrix4x4 mat)
    {
        return mat.MultiplyPoint(point).normalized;
    }

    public float GetGridDisplacement()
    {
        float R = (radius * (1+amplitude));
        Vector3 cameraPos = referenceTransform.localPosition;
        float d = cameraPos.magnitude;
        float r2 = radius*radius;
        float h = Mathf.Sqrt(d*d - r2);
        float s = Mathf.Sqrt(R*R - r2);
        
        return ( R*R + d*d - (h+s) * (h+s) ) / (2*radius * (h+s));
    }
}
