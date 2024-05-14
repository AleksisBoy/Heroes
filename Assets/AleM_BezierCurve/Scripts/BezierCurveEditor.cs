using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    private int quality = 30;

    private int lastSelectedPoint = -1;
    public override void OnInspectorGUI()
    {
        bool repaintScene = false;
        BezierCurve curve = (BezierCurve)target;

        int newQuality = EditorGUILayout.IntField("Quality", quality);
        if (newQuality != quality)
        {
            if (newQuality < 0) newQuality = 0;
            else if (newQuality > 1000) newQuality = 1000;
            quality = newQuality;
            repaintScene = true;
        }

        if(lastSelectedPoint > -1)
        {
            curve.points[lastSelectedPoint] = EditorGUILayout.Vector3Field("Point position", curve.points[lastSelectedPoint]);
            repaintScene = true;
        }
        else
        {
            EditorGUILayout.LabelField("Start moving point to modify it");
        }
        if(GUILayout.Button("Add Point"))
        {
            AddPoint(curve);
            repaintScene = true;
        }
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Remove Point"))
        {
            RemovePoint(curve);
            repaintScene = true;
        }
        if(GUILayout.Button("Clear Points"))
        {
            ClearPoints(curve);
            repaintScene = true;
        }
        GUILayout.EndHorizontal();
        if (repaintScene) SceneView.RepaintAll();
    }
    private void BuildCurve(BezierCurve curve)
    {
        if(curve.points.Count < 3) return;

        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        float timeStep = 1f / quality;
        Handles.color = Color.white;
        for (int i = 0; i < quality + 1; i++)
        {
            float time = timeStep * i;
            Vector3 b = curve.GetPositionAt(time);

            Handles.color = Color.red;
            Handles.DrawSolidDisc(b + curve.transform.position, sceneCamera.transform.forward, 0.1f);
        }
    }
    private void AddPoint(BezierCurve curve)
    {
        if (curve.points.Count >= 5) return;

        Undo.RecordObject(curve, "Add point");
        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        Physics.Raycast(sceneCamera.transform.position, sceneCamera.transform.forward, out RaycastHit hit, 20f);
        if(hit.transform != null)
        {
            curve.points.Add(hit.point);
        }
        else
        {
            curve.points.Add(sceneCamera.transform.position + sceneCamera.transform.forward * 5f);
        }
    }
    private void RemovePoint(BezierCurve curve)
    {
        if(curve.points.Count == 0) return;

        Undo.RecordObject(curve, "Remove point");
        int lastPointIndex = curve.points.Count - 1;
        if (lastSelectedPoint == lastPointIndex) lastSelectedPoint = -1;
        curve.points.RemoveAt(lastPointIndex);
    }
    private void ClearPoints(BezierCurve curve)
    {
        Undo.RecordObject(curve, "Clear Points");
        curve.points.Clear();
        lastSelectedPoint = -1;
    }
    private void OnSceneGUI()
    {
        BezierCurve curve = (BezierCurve)target;
        Camera sceneCamera = SceneView.lastActiveSceneView.camera;

        List<Vector3> points = curve.points; 
        if(points.Count > 0)
        {
            Tools.current = Tool.None;
        }
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pointPosition = points[i] + curve.transform.position;
            Vector3 nextPointPosition = points[i + 1 >= points.Count ? 0 : i + 1] + curve.transform.position;
            Handles.color = Color.black;
            Handles.DrawAAPolyLine(10f, pointPosition, nextPointPosition);
            Handles.color = i == 0 || i == points.Count - 1 ? Color.red : Color.gray;
            Handles.DrawSolidDisc(pointPosition, sceneCamera.transform.forward, 0.4f);

            pointPosition = Handles.PositionHandle(pointPosition, Quaternion.identity) - curve.transform.position;
            if(pointPosition != points[i])
            {
                Undo.RecordObject(curve, "Moving point " + i);
                points[i] = pointPosition;
                lastSelectedPoint = i;
            }
        }
        curve.points = points;
        Handles.DrawAAPolyLine(0.1f, curve.points.ToArray());
        BuildCurve(curve);
    }
}
