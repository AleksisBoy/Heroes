using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CustomTerrain : MonoBehaviour
{
    public Texture2D mainTexture;
    public int textureResolution = 2048;
    public string terrainSaveName = "TerrainMesh";
    public float width = 10f;
    public float length = 10f;
    public int resolution = 10;

    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;
    private void OnValidate()
    {
        Debug.Log("Validate TErrain");
        if (resolution <= 1) resolution = 2;

        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>();

        if(!mf) mf = gameObject.AddComponent<MeshFilter>();
        if(!mr) mr = gameObject.AddComponent<MeshRenderer>();
        if(!mc) mc = gameObject.AddComponent<MeshCollider>();
        gameObject.name = "Terrain";
        transform.position = Vector3.zero;
    }
    public void BuildMesh()
    {
        Mesh meshTerrain = MeshUtility.BuildRectangleMesh(width, length, resolution);
        mainTexture = new Texture2D(textureResolution, textureResolution);
        mr.sharedMaterial.mainTexture = mainTexture;
        meshTerrain.name = "Terrain";
        mf.sharedMesh = meshTerrain;
    }
    public void ChangeTextureOnePointTest(Vector2 uvPos)
    {
        Texture2D texture = (Texture2D)mr.sharedMaterial.mainTexture;
        Vector2Int pixelPos = ConvertUVToPixel(uvPos);
        texture.SetPixel(pixelPos.x, pixelPos.y, Color.black);
        texture.Apply();
    }
    public void ChangeTexture(Vector2 uvPos, Texture2D newTexture, float sizeWorld)
    {
        Texture2D texture = (Texture2D)mr.sharedMaterial.mainTexture;

        Vector2Int pixelPos = ConvertUVToPixel(uvPos);

        float sizeUV = sizeWorld / width;
        float sizePixel = sizeWorld / textureResolution;

        Vector2Int pixelPos00 = ConvertUVToPixel(uvPos + new Vector2(-sizeUV / 2f, -sizeUV / 2f));
        Vector2Int pixelPos11 = ConvertUVToPixel(uvPos + new Vector2(sizeUV / 2f, sizeUV / 2f));

        Texture2D rescaledTexture = new Texture2D(pixelPos11.x - pixelPos00.x, pixelPos11.y - pixelPos00.y);

        rescaledTexture.SetPixels(ResizePixels(newTexture.GetPixels(), newTexture.width, newTexture.height, rescaledTexture.width, rescaledTexture.height));
        rescaledTexture.Apply();

        for (int y = pixelPos00.y, ix = 0; y <= pixelPos11.y; y++)
        {
            for(int x = pixelPos00.x, iy = 0; x <= pixelPos11.x; x++)
            {
                Color pixelColor = rescaledTexture.GetPixel(ix, iy);
                if (pixelColor == new Color(0f, 0f, 0f, 0f))
                {
                    iy++;
                    continue;
                }
                texture.SetPixel(x, y, pixelColor);
                iy++;
            }
            ix++;
        }

        texture.Apply();
    }
    public void RaiseTerrain(Vector3 worldPos, float radius)
    {
        Vector3 pos00 = new Vector3(worldPos.x - radius, 0, worldPos.z - radius);
        Vector3 pos11 = new Vector3(worldPos.x + radius, 0, worldPos.z + radius);

        var vertPos = GetVertPositions(pos00, pos11);
        Vector3[] vertsRaised = vertPos.Item1;
        int[] vertIndexes = vertPos.Item2;

        Transform tr = new GameObject().transform;
        foreach(Vector3 v in vertsRaised)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(tr, true);
            go.transform.position = new Vector3(v.x, 0f, v.z);
            go.transform.localScale *= 0.1f;
        }
        Vector3[] verts = mf.sharedMesh.vertices;
        //List<Vector3> vv = verts.ToList();
        int vertIndex = 0;
        foreach(int i in vertIndexes)
        {
            Debug.Log("vert(" + verts[i] + ") is new vert(" + vertsRaised[vertIndex] + ")");
            verts[i] = vertsRaised[vertIndex++];
        }
        mf.sharedMesh.vertices = verts;

        //for (float z = pos00.z; z < pos11.z; z += length / (resolution - 1))
        //{
        //    for (float x = pos00.x; x < pos11.x; x += width / (resolution - 1))
        //    {
        //        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //        go.transform.position = new Vector3(x, 0f, z);
        //        go.transform.localScale *= 0.1f;
        //    }
        //}
    }
    public (Vector3[], int[]) GetVertPositions(Vector3 worldPos00, Vector3 worldPos11)
    {
        List<Vector3> verts = new List<Vector3>();
        List<int> vertIndexes = new List<int>();
        int index = 0;
        for(float z = 0f; z <= length; z += length / (resolution - 1))
        {
            for(float x = 0f; x <= width; x += width / (resolution - 1))
            {
                if(x >= worldPos00.x && x <= worldPos11.x
                    && z >= worldPos00.z && z <= worldPos11.z)
                {
                    verts.Add(new Vector3(x, 1, z));
                    vertIndexes.Add(index);
                }
                index++;
            }
        }
        return (verts.ToArray(), vertIndexes.ToArray());
    }
    public float GetHeight(Vector3 worldPosition)
    {
        return 0f;
    }
    private Color[] ResizePixels(Color[] pixels, int origWidth, int origHeight, int targetWidth, int targetHeight)
    {
        Color[] resizedPixels = new Color[targetWidth * targetHeight];

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                float xFrac = (float)x / (float)(targetWidth - 1) * (float)(origWidth - 1);
                float yFrac = (float)y / (float)(targetHeight - 1) * (float)(origHeight - 1);

                int x0 = Mathf.FloorToInt(xFrac);
                int y0 = Mathf.FloorToInt(yFrac);
                int x1 = Mathf.CeilToInt(xFrac);
                int y1 = Mathf.CeilToInt(yFrac);

                Color c00 = pixels[y0 * origWidth + x0];
                Color c10 = pixels[y0 * origWidth + x1];
                Color c01 = pixels[y1 * origWidth + x0];
                Color c11 = pixels[y1 * origWidth + x1];

                float xFracRemap = xFrac - x0;
                float yFracRemap = yFrac - y0;

                Color color = Color.Lerp(Color.Lerp(c00, c10, xFracRemap), Color.Lerp(c01, c11, xFracRemap), yFracRemap);

                resizedPixels[y * targetWidth + x] = color;
            }
        }

        return resizedPixels;
    }
    private Vector2Int ConvertUVToPixel(Vector2 uvPos)
    {
        Vector2Int pixelPos = new Vector2Int(Mathf.RoundToInt(uvPos.x * textureResolution), Mathf.RoundToInt(uvPos.y * textureResolution));
        return pixelPos;
    }
    private Vector2Int ConvertPixelToUV(Vector2Int pixelPos)
    {
        Vector2Int uvPos = new Vector2Int(pixelPos.x / textureResolution, pixelPos.y / textureResolution);
        return uvPos;
    }
    private static void ShowDebugTris(int[] tris)
    {
        Debug.Log("tris length: " + tris.Length);
        string debugLog = "";
        for (int i = 0; i < tris.Length; i += 3)
        {
            debugLog += "(";
            debugLog += tris[i];
            debugLog += "|";
            debugLog += tris[i + 1];
            debugLog += "|";
            debugLog += tris[i + 2];
            debugLog += ")";
            debugLog += " ";
        }
        Debug.Log(debugLog);
    }

    public void DestroyMesh()
    {
        if (!HasMesh()) return;

        mf.sharedMesh = null;
    }
    public void SaveMesh()
    {
        MeshUtility.SaveMesh(mf.sharedMesh, terrainSaveName);
    }
    public bool HasMesh()
    {
        return mf.sharedMesh != null;
    }
}
