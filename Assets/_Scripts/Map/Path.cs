using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Path
{
    private Point startPoint;
    private Point endPoint;
    private Dictionary<Point, PointPathData> usedPoints = null;
    private Dictionary<Point, PointPathData> queue = null;

    public List<GameObject> pathMarkers = null;

    private readonly Map map;
    private Dictionary<Point, PointPathData> pathPoints = null;
    private float movementCost = -1;
    public Dictionary<Point, PointPathData> PathPoints { get { return pathPoints; } }    
    public float MovementCost { get { return movementCost; } }
    public Point StartPoint { get { return startPoint; } }
    public Point EndPoint { get { return endPoint; } }

    public Path(Point startPoint, Point endPoint, Map map)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        this.map = map;
        usedPoints = new Dictionary<Point, PointPathData>();
        queue = new Dictionary<Point, PointPathData>();
        pathPoints = new Dictionary<Point, PointPathData>();

        BuildPathToEndPoint();
    }
    private void BuildPathToEndPoint()
    {
        // setting up start point
        PointPathData startPointData = new PointPathData()
        {
            gCost = 0f,
            hCost = Map.CalculateDistanceCost(startPoint, endPoint),
            previousPoint = null
        };
        queue.Add(startPoint, startPointData);

        while (queue.Count > 0)
        {
            (Point, PointPathData) pointData = Map.GetLowestFCostPoint(queue);
            Point currentPoint = pointData.Item1;
            PointPathData currentPointData = pointData.Item2;
            if (currentPoint == endPoint)
            {
                //finish
                pathPoints.Add(currentPoint, currentPointData);
                movementCost = currentPointData.gCost;
                while (currentPointData.previousPoint != null)
                {
                    currentPoint = currentPointData.previousPoint;
                    currentPointData = usedPoints[currentPoint];

                    pathPoints.Add(currentPoint, currentPointData);
                }
                return;
            }

            usedPoints.Add(currentPoint, currentPointData);
            queue.Remove(currentPoint);

            foreach (Point neighbourPoint in map.GetNeighbourPoints(currentPoint))
            {
                if (usedPoints.ContainsKey(neighbourPoint)) continue;
                if (neighbourPoint.occupied) continue;
                float tentativeGCost = currentPointData.gCost + Map.CalculateDistanceCost(currentPoint, neighbourPoint);

                PointPathData neighbourData = new PointPathData()
                {
                    previousPoint = currentPoint,
                    gCost = tentativeGCost,
                    hCost = Map.CalculateDistanceCost(neighbourPoint, endPoint)
                };
                if (!queue.ContainsKey(neighbourPoint))
                {
                    queue.Add(neighbourPoint, neighbourData);
                }
            }
        }
    }
    public void EnableMarkers(bool state)
    {
        foreach(GameObject go in pathMarkers)
        {
            go.SetActive(state);
        }
    }
    public void PlacePathMarkers(float movementAmount, GameObject pointPrefab)
    {
        if(pathMarkers != null)
        {
            foreach(GameObject go in pathMarkers)
            {
                Object.Destroy(go);
            }
        }
        pathMarkers = new List<GameObject>();
        Point previousPoint = null;
        var reversedPathPoints = PathPoints.Keys.Reverse();
        foreach (Point pathPoint in reversedPathPoints)
        {
            GameObject go = GameObject.Instantiate(pointPrefab);
            if (previousPoint != null) movementAmount -= Map.CalculateDistanceCost(previousPoint, pathPoint);
            if (movementAmount < 0) go.GetComponent<SpriteRenderer>().color = Color.white;
            pathMarkers.Add(go);
            go.transform.position = pathPoint.Position + new Vector3(0f, 0.01f, 0f);
            previousPoint = pathPoint;
        }
    }
    public void RemoveFirstPathPoint()
    {
        pathPoints.Remove(pathPoints.ElementAt(0).Key);
    }
    /// <summary>
    /// Returns null
    /// </summary>
    public Path DestroySelf() 
    {
        foreach(GameObject go in pathMarkers)
        {
            Object.Destroy(go);
        }
        return null;
    }
}
public struct PointPathData
{
    public Point previousPoint;
    public float gCost;
    public float hCost;
}
