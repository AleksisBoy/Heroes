using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public List<Vector3> points = new List<Vector3>();

    public Vector3 GetPositionAt(float time)
    {
        time = Mathf.Clamp01(time);
        List<Vector3> qs = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            qs.Add(Interpolate(points[i], points[i + 1], time));
        }
        while (qs.Count > 2)
        {
            List<Vector3> dup = new List<Vector3>(qs);
            qs.Clear();
            for (int j = 0; j < dup.Count - 1; j++)
            {
                qs.Add(Interpolate(dup[j], dup[j + 1], time));
            }
        }
        return Interpolate(qs[0], qs[1], time);
    }
    public Vector3 GetPositionReversedAt(float time)
    {
        time = Mathf.Clamp01(time);
        List<Vector3> reversedPoints = new List<Vector3>(points);
        reversedPoints.Reverse();
        List<Vector3> qs = new List<Vector3>();
        for (int i = 0; i < reversedPoints.Count - 1; i++)
        {
            qs.Add(Interpolate(reversedPoints[i], reversedPoints[i + 1], time));
        }
        while (qs.Count > 2)
        {
            List<Vector3> dup = new List<Vector3>(qs);
            qs.Clear();
            for (int j = 0; j < dup.Count - 1; j++)
            {
                qs.Add(Interpolate(dup[j], dup[j + 1], time));
            }
        }
        return Interpolate(qs[0], qs[1], time);
    }
    public Vector3 GetRandomPositionOnCurve()
    {
        return GetPositionAt(Random.value);
    }
    // Static
    public static Vector3 Interpolate(Vector3 point1, Vector3 point2, float time)
    {
        Vector3 q = default;
        q.x = point1.x + (point2.x - point1.x) * time;
        q.y = point1.y + (point2.y - point1.y) * time;
        q.z = point1.z + (point2.z - point1.z) * time;
        return q;
    }
}
