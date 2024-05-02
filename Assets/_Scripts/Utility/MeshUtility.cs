using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MeshUtility
{
    public static Mesh BuildRectangleMesh(float width, float length, int resolution)//, ComputeShader meshShader)
    {
        /*
        Mesh mesh = new Mesh();

        if(resolution > 200) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Vertices & Normals & UVs
        Vector3[] verts = new Vector3[resolution * resolution];
        Vector3[] normals = new Vector3[resolution * resolution];
        Vector2[] uv = new Vector2[resolution * resolution];

        int vertIndex = 0;
        for (float y = 0; y < resolution; y++)
        {
            for (float x = 0; x < resolution; x++)
            {
                float relativeX = x / (resolution - 1);
                float relativeY = y / (resolution - 1);
                verts[vertIndex] = new Vector3(relativeX * width, 0f, relativeY * length);
                normals[vertIndex] = Vector3.up;
                uv[vertIndex] = new Vector2(relativeX, relativeY);
                vertIndex++;
            }
        }

        // Triangles
        int triangleCount = (resolution - 1) * 2 * (resolution - 1);
        int[] tris = new int[triangleCount * 3];
        int skipRow = 0;
        for (int i = 0, triIndex = 0; i < tris.Length; i += 6)
        {
            if (skipRow + 1 == resolution)
            {
                skipRow = 0;
                triIndex++;
            }
            tris[i] = triIndex;
            tris[i + 1] = triIndex + resolution + 1;
            tris[i + 2] = triIndex + 1;

            tris[i + 3] = triIndex;
            tris[i + 4] = triIndex + resolution;
            tris[i + 5] = triIndex + resolution + 1;
            triIndex++;
            skipRow++;
        }
        mesh.vertices = verts;
        //mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = tris;

        //mesh.Optimize();
        mesh.RecalculateNormals();
        */


        // shader

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[resolution * resolution];

        var bufferVertices = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        bufferVertices.SetData(vertices);
        //meshShader.SetBuffer(0, "vertices", bufferVertices);

        int triangleCount = (resolution - 1) * 2 * (resolution - 1);
        int[] triangles = new int[triangleCount * 3];

        var bufferTriangles = new ComputeBuffer(triangleCount * 3, sizeof(int));
        bufferTriangles.SetData(triangles);
        //meshShader.SetBuffer(0, "triangles", bufferTriangles);

        //meshShader.SetFloat("width", width);
        //meshShader.SetFloat("length", length);
        //meshShader.SetInt("resolution", resolution);

        //meshShader.Dispatch(0, Mathf.CeilToInt(vertices.Length / 64f), 1, 1);
        bufferVertices.GetData(vertices);
        bufferVertices.Release();

        bufferTriangles.GetData(triangles);
        bufferTriangles.Release();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
    
    public static void UpdateRectangleMesh(Mesh mesh, float width, float length, int resolution)
    {
        
    }
    public static void UpdateRectangleMeshSameResolution(Mesh mesh, float width, float length)
    {
        if (mesh == null)
        {
            Debug.LogError("No Mesh to update");
            return;
        }
        int resolution = (int)Mathf.Sqrt(mesh.vertices.Length);
        // Vertices & Normals & UVs
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = new Vector3[mesh.vertices.Length];
        Vector2[] uv = new Vector2[mesh.vertices.Length];

        int vertIndex = 0;
        for (float y = 0; y < resolution; y++)
        {
            for (float x = 0; x < resolution; x++)
            {
                float relativeX = x / (resolution - 1);
                float relativeY = y / (resolution - 1);
                verts[vertIndex] = new Vector3(relativeX * width, 0f, relativeY * length);
                normals[vertIndex] = Vector3.up;
                uv[vertIndex] = new Vector2(relativeX, relativeY);
                vertIndex++;
            }
        }
        // would like to remove tris from updating mesh
        // Triangles
        int triangleCount = (resolution - 1) * 2 * (resolution - 1);
        int[] tris = new int[triangleCount * 3];
        int skipRow = 0;
        for (int i = 0, triIndex = 0; i < tris.Length; i += 6)
        {
            if (skipRow + 1 == resolution)
            {
                skipRow = 0;
                triIndex++;
            }
            tris[i] = triIndex;
            tris[i + 1] = triIndex + resolution + 1;
            tris[i + 2] = triIndex + 1;

            tris[i + 3] = triIndex;
            tris[i + 4] = triIndex + resolution;
            tris[i + 5] = triIndex + resolution + 1;
            triIndex++;
            skipRow++;
        }

        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = tris;

        //mesh.Optimize();
    }
    public static void SaveMesh(Mesh mesh, string name)
    {
        if (!mesh) return;

        AssetDatabase.CreateAsset(mesh, "Assets/SavedFiles/Terrain/" + name + ".mesh");
        AssetDatabase.SaveAssets();
    }
    public static Vector3[] CalculateFlatNormals(Vector3[] vertices, int[] triangles)
    {
        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            normals[i1] = normal;
            normals[i2] = normal;
            normals[i3] = normal;
        }

        return normals;
    }
    public static Vector3[] CalculateSmoothNormals(Vector3[] vertices, int[] triangles)
    {
        Vector3[] normals = new Vector3[vertices.Length];
        int[] vertexCount = new int[vertices.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i1 = triangles[i];
            int i2 = triangles[i + 1];
            int i3 = triangles[i + 2];

            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];

            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;

            normals[i1] += normal;
            normals[i2] += normal;
            normals[i3] += normal;

            vertexCount[i1]++;
            vertexCount[i2]++;
            vertexCount[i3]++;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] /= vertexCount[i];
        }

        return normals;
    }
}
