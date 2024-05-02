using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainWindow : EditorWindow
{
    private static TerrainWindow window;
    private static CustomTerrain terrain;
    private static float width = 0f;
    private static float length = 0f;
    private static float resolution = 0f;
    [MenuItem("Tools/Custom Terrain")]
    private static void Init()
    {
        window = (TerrainWindow)EditorWindow.GetWindow(typeof(TerrainWindow));
        window.titleContent.text = "Custom Terrain";
    }
    void OnSelectionChange()
    {
        Repaint();
    }
    void OnGUI()
    {
        width = EditorGUILayout.FloatField("Terrain Width", width);
        length = EditorGUILayout.FloatField("Terrain Length", length);
        resolution = EditorGUILayout.FloatField("Terrain Length", resolution);
    }
}
