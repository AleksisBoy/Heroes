using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(CustomTerrain))]
public class CustomTerrainEditor : Editor
{
    private static Mesh meshTest = null;

    private static List<TerrainBrush> brushes = new List<TerrainBrush>();
    private TerrainBrush selectedBrush = null;

    private static Mesh brushMesh = null;
    private static Material brushMaterial = null;
    private static TerrainEditorState state;

    private float brushSize = 1f;
    private float brushStrength = 1f;

    private bool rightClicked = false;
    private void OnValidate()
    {
        state = (TerrainEditorState)EditorPrefs.GetInt("LastTerrainEditorState", 0);
        UpdateBrushes();
    }
    private void OnSceneGUI()
    {
        if (state != TerrainEditorState.Paint) return;
        if (selectedBrush == null) return;

        CustomTerrain terrain = (CustomTerrain)target;
        Event currentEvent = Event.current;

        if(currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
        {
            rightClicked = true;
        }
        if(currentEvent.type == EventType.MouseUp && currentEvent.button == 1)
        {
            rightClicked = false;
        }
        if (rightClicked) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Terrain")))
        {
            Selection.activeGameObject = terrain.gameObject;
            Vector3 mouseWorldPosition = hit.point;


            // on terrain uv should have brush texture where if r value is 0 should not show it,
            // but increasing it will show texture

            Vector2 uvPos = new Vector2(mouseWorldPosition.x / terrain.width, mouseWorldPosition.z / terrain.length);
            //terrain.ChangeTextureOnePointTest(uvPos);

            //terrain.ChangeTexture(uvPos, selectedBrush.texture, brushSize);
            if (brushMesh == null)
            {
                brushMesh = MeshUtility.BuildRectangleMesh(brushSize, brushSize, 2);
                MeshUtility.SaveMesh(brushMesh, "brushmesh");
            }
            else MeshUtility.UpdateRectangleMeshSameResolution(brushMesh, brushSize, brushSize);

            Vector3 worldPositionOffset = mouseWorldPosition + new Vector3(-brushSize / 2f, 0.1f, -brushSize / 2f);
            RenderBrush(worldPositionOffset);
            PreviewRaiseTerrainBrush(worldPositionOffset, terrain);

            // On click raise or lower terrain under brush
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                terrain.RaiseTerrain(mouseWorldPosition, brushSize / 2f);
            }
        }
    }
    private void RenderBrush(Vector3 worldPosition)
    {
        // Brush material creation and assigning properties
        brushMaterial = new Material(Shader.Find("Standard"));
        brushMaterial.enableInstancing = true;
        MaterialExtensions.ToFadeMode(brushMaterial);
        brushMaterial.color = new Color(0.4f, 0.41f, 0.4f, 0.6f);
        brushMaterial.mainTexture = selectedBrush.texture;

        // Rendering brush material on a brush mesh
        RenderParams rp = new RenderParams(brushMaterial);
        Matrix4x4[] instData = new Matrix4x4[1];
        instData[0] = Matrix4x4.Translate(worldPosition);
        Graphics.RenderMeshInstanced(rp, brushMesh, 0, instData);
    }
    private void PreviewRaiseTerrainBrush(Vector3 worldPosition, CustomTerrain terrain)
    {
        // Brush material creation and assigning properties
        Material previewMaterial = new Material(Shader.Find("Standard"));
        previewMaterial.enableInstancing = true;
        MaterialExtensions.ToFadeMode(previewMaterial);
        previewMaterial.color = new Color(0.92f, 1f, 1f, 0.7f);

        Texture2D previewTexture = new Texture2D(selectedBrush.texture.width, selectedBrush.texture.height);

        //for(int y = 0; y < selectedBrush.texture.height; y++)
        //{
        //    for(int x = 0; x < selectedBrush.texture.width; x++)
        //    {
        //        Color pixel = selectedBrush.texture.GetPixel(x, y);
        //        previewTexture.SetPixel(x, y, new Color(1f, 1f, 1f, pixel.a));
        //    }
        //}
        previewMaterial.mainTexture = selectedBrush.texture;

        // Creating preview mesh
        Mesh previewMesh = new Mesh();

        int resolution = 60;
        float width = brushSize;
        float length = brushSize;

        if (resolution > 200) previewMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

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
                Vector3 meshPos = new Vector3(relativeX * width, 0f, relativeY * length);
                float meshHeight = terrain.GetHeight(meshPos) + brushStrength * GetAlphaOnBrush(meshPos);
                if (x == 0 || y == 0 || x == resolution - 1 || y == resolution - 1) meshHeight = 0f;
                verts[vertIndex] = new Vector3(meshPos.x, meshHeight, meshPos.z);
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
        previewMesh.vertices = verts;
        //previewMesh.normals = normals;
        previewMesh.uv = uv;
        previewMesh.triangles = tris;
        previewMesh.normals = MeshUtility.CalculateSmoothNormals(verts, tris);

        previewMesh.Optimize();

        //Vector3[] normals = new Vector3[previewMesh.vertices.Length];

        //Vector3[] verts = new Vector3[previewMesh.vertices.Length];
        //for(int i = 0; i < previewMesh.vertices.Length; i++)
        //{
        //    verts[i] = new Vector3(previewMesh.vertices[i].x, terrain.GetHeight(previewMesh.vertices[i]) + brushStrength * GetAlphaOnBrush(previewMesh.vertices[i]), previewMesh.vertices[i].z);
        //}

        //previewMesh.vertices = verts;
        //previewMesh.normals = brushMesh.normals;
        //previewMesh.uv = brushMesh.uv;
        //previewMesh.triangles = brushMesh.triangles;

        // Rendering raised terrain mesh under brush
        RenderParams rp = new RenderParams(previewMaterial);
        Matrix4x4[] instData = new Matrix4x4[1];
        instData[0] = Matrix4x4.Translate(worldPosition);
        Graphics.RenderMeshInstanced(rp, previewMesh, 0, instData);
    }

    private float GetAlphaOnBrush(Vector3 meshPos)
    {
        float xUV = meshPos.x / brushSize;
        float yUV = meshPos.z / brushSize;
        Texture2D tex = (Texture2D)brushMaterial.mainTexture;
        Color pixel = tex.GetPixel(Mathf.RoundToInt(xUV * tex.width), Mathf.RoundToInt(yUV * tex.height));
        //Debug.Log(xUV + "/" + yUV + " = " + pixel.a);
        return pixel.a;
    }
    public override void OnInspectorGUI()
    {
        CustomTerrain terrain = (CustomTerrain)target;

        ShowStateButtons();

        switch (state)
        {
            case TerrainEditorState.General:
                ShowGeneralTab(terrain);
                break;
            case TerrainEditorState.Paint:
                ShowPaintTab(terrain);
                break;
        }

    }
   
    private void ShowStateButtons()
    {
        GUILayout.BeginHorizontal();
        GUIStyle styleButton = new GUIStyle(GUI.skin.button);

        // Buttons
        if (GUILayout.Button("General", state == TerrainEditorState.General ? GetButtonSelectionStyle() : styleButton) && state != TerrainEditorState.General)
        {
            state = TerrainEditorState.General;
            EditorPrefs.SetInt("LastTerrainEditorState", (int)state);
        }

        if (GUILayout.Button("Paint", state == TerrainEditorState.Paint ? GetButtonSelectionStyle() : styleButton) && state != TerrainEditorState.Paint)
        {
            state = TerrainEditorState.Paint;
            EditorPrefs.SetInt("LastTerrainEditorState", (int)state);
        }
        GUILayout.EndHorizontal();
    }

    private void ShowGeneralTab(CustomTerrain terrain)
    {
        // General Settings
        GUILayout.Label("General Settings", EditorStyles.boldLabel);
        terrain.terrainSaveName = EditorGUILayout.TextField("Mesh Save Name", terrain.terrainSaveName);
        terrain.width = EditorGUILayout.FloatField("Terrain Width", terrain.width);
        terrain.length = EditorGUILayout.FloatField("Terrain Length", terrain.length);
        terrain.resolution = EditorGUILayout.IntField("Terrain Resolution", terrain.resolution);
        terrain.textureResolution = EditorGUILayout.IntField("Texture Resolution", terrain.textureResolution);

        string buttonName1 = terrain.HasMesh() ? "Update Terrain" : "Create Terrain";

        if (GUILayout.Button(buttonName1))
        {
            terrain.BuildMesh();
        }
        if (!terrain.HasMesh()) return;

        string buttonSaveName = "Save Mesh";
        GUIStyle style = new GUIStyle(GUI.skin.button);
        if (GUILayout.Button(buttonSaveName, style))
        {
            terrain.SaveMesh();
        }

        string buttonDestroyName = "Destroy Terrain";
        if (GUILayout.Button(buttonDestroyName))
        {
            terrain.DestroyMesh();
        }
    }
    private void ShowPaintTab(CustomTerrain terrain)
    {
        GUILayout.Label("Paint Terrain", EditorStyles.boldLabel);

        if (GUILayout.Button("Update Brushes"))
        {
            UpdateBrushes();
        }
        if (GUILayout.Button("Create Brush"))
        {
            CreateBrush();
        }
        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 10f);
        brushStrength = EditorGUILayout.Slider("Brush Strength", brushStrength, 0.1f, 10f);

        GUIStyle style = new GUIStyle(GUI.skin.box);
        EditorGUILayout.BeginHorizontal(style);

        foreach(TerrainBrush brush in brushes)
        {
            GUIStyle styleButton = new GUIStyle(GUI.skin.button);
            if (brush == selectedBrush) styleButton.normal.background = Texture2D.linearGrayTexture;
            GUILayoutOption[] options = new GUILayoutOption[]
            {
                GUILayout.Width(50f),
                GUILayout.Height(50f)
            };
            //if(EditorGUILayout.Toggle(label, brush == selectedBrush, styleToggle, options))
            if(GUILayout.Button(brush.texture, styleButton, options))
            {
                selectedBrush = brush;
                brushMesh = null;
                Debug.Log("selected " + brush.name);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    private void UpdateBrushes()
    {
        brushes.Clear();
        foreach(string guid in AssetDatabase.FindAssets("Brush", new[] { "Assets/SavedFiles/Terrain/Brushes" }))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TerrainBrush brush = AssetDatabase.LoadAssetAtPath<TerrainBrush>(assetPath);
            if(brush != null) brushes.Add(brush);
        }

    }
    private void CreateBrush()
    {
        string filePath = EditorUtility.OpenFilePanel("Select File", "Assets/Sprites", "png, jpeg");
        if (!string.IsNullOrEmpty(filePath))
        {
            Debug.Log("Selected file path: " + filePath);
            filePath = filePath.Substring(filePath.IndexOf("/Assets") + 1);
            Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            if (asset != null)
            {
                Debug.Log("Loaded asset: " + asset.name);

                TerrainBrush brush = TerrainBrush.CreateInstance<TerrainBrush>();
                brush.texture = asset;
                brushes.Add(brush);

                int brushCount = AssetDatabase.FindAssets("Brush", new[] { "Assets/SavedFiles/Terrain/Brushes" }).Length;
                AssetDatabase.CreateAsset(brush, "Assets/SavedFiles/Terrain/Brushes/Brush" + brushCount + ".asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.LogError("Failed to load asset at path: " + filePath);
            }
        }
    }
    //private Vector3[] GetHeightsOnBrush()
    //{
    //    Vector3[] heights = new Vector3[sele];

    //    return heights;
    //}
    private static GUIStyle GetButtonSelectionStyle()
    {
        GUIStyle styleSelection = new GUIStyle(GUI.skin.button);
        styleSelection.fontStyle = FontStyle.Bold;
        styleSelection.normal.background = Texture2D.linearGrayTexture;
        styleSelection.normal.textColor = Color.white;
        return styleSelection;
    }

    private enum TerrainEditorState
    {
        General, 
        Paint
    }
}
