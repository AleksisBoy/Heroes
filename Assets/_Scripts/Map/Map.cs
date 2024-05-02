using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] public int chunkSize = 16;
    [SerializeField] public int pointScale = 2;
    [SerializeField] private float gridRowOffset = 1f;

    private Terrain terrain;
    private List<Point> points;
    private Point[][] pointsChunked;

    public int chunkOneDimension;

    private Vector3[] neighbourChecks = new Vector3[8];
    public List<Point> Points { get { return points; } }
    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        neighbourChecks = new Vector3[8]
        {
            Vector3.right * pointScale, new Vector3(1, 0, -1) * pointScale, -Vector3.forward * pointScale,
            new Vector3(-1, 0, -1) * pointScale, -Vector3.right * pointScale, new Vector3(-1, 0, 1) * pointScale,
            Vector3.forward * pointScale, new Vector3(1, 0, 1) * pointScale
        };
    }
    private void Start()
    {
        GeneratePoints();
    }
    private void GeneratePoints()
    {
        points = new List<Point>();
        float offsetZ = 0f;
        int terrainSizeX = int.Parse(terrain.terrainData.size.x.ToString());

        chunkOneDimension = terrainSizeX / chunkSize;
        pointsChunked = new Point[chunkOneDimension * chunkOneDimension][];
        for (int i = 0; i < pointsChunked.Length; i++)
        {
            pointsChunked[i] = new Point[chunkSize * chunkSize];
        }
        int startZ = 0;
        int countZ = chunkSize;

        for (int chunk = 0; chunk < pointsChunked.Length; chunk++)
        {
            // check for end of one dimension
            if (chunk % chunkOneDimension == 0 && chunk != 0)
            {
                startZ += chunkSize;
                countZ = startZ + chunkSize;
            }

            // clamp chunk from 0 to chunkOneDimension
            int chunkClamped = chunk;
            while (chunkClamped >= chunkOneDimension)
            {
                chunkClamped -= chunkOneDimension;
            }

            int index = -1;
            GameObject chunkParent = new GameObject();
            chunkParent.name = "Chunk " + chunk;
            for (int x = chunkClamped * chunkSize; x < (chunkClamped * chunkSize) + chunkSize; x += pointScale)
            {
                for (int z = startZ; z < countZ; z += pointScale)
                {
                    index++;
                    float height = terrain.SampleHeight(new Vector3(x, 0, z + offsetZ)) + terrain.transform.position.y;
                    if (height > 0.5f) continue;
                    if (height < -0.1f) continue;

                    Vector3 pointPosition = new Vector3(x, height, z + offsetZ);

                    //collision check
                    Collider[] colls = Physics.OverlapSphere(pointPosition, pointScale / 1.5f);
                    bool blocked = false;
                    foreach (Collider coll in colls)
                    {
                        //entrance generator
                        Building building = coll.GetComponent<Building>();
                        if (!building) continue;
                        //if (building.CurrentLocation != null) continue;

                        if (building.EntrancePreSetup && Vector3.Distance(pointPosition, building.EntrancePreSetup.position) < pointScale / 2f)
                        {
                            Point entrancePoint = CreatePoint(chunk, index, pointPosition);
                            entrancePoint.AssignOnInteract(building.Interact);
                            building.Setup(entrancePoint);
                        }
                        //else if (!building.EntrancePreSetup)
                        //{
                        //    Point entrancePoint = CreatePoint(chunk, index, pointPosition);
                        //    entrancePoint.AssignOnInteract(building.Interact);
                        //    building.Setup(entrancePoint);
                        //}
                        blocked = true;
                        break;
                    }
                    if (blocked) continue;

                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.SetParent(chunkParent.transform);
                    go.transform.position = pointPosition;
                    go.transform.localScale *= 0.6f;
                    go.SetActive(false);

                    CreatePoint(chunk, index, pointPosition);
                }
                offsetZ = offsetZ == 0f ? gridRowOffset : 0f;
            }
        }
    }

    private Point CreatePoint(int chunk, int index, Vector3 pointPosition)
    {
        Point point = new Point(pointPosition, 0, false);
        points.Add(point);
        pointsChunked[chunk][index] = point;
        return point;
    }

    public Path CalculatePath(Point pointA, Point pointB)
    {
        Path path = new Path(pointA, pointB, this);
        return path;
    }
    public static (Point, PointPathData) GetLowestFCostPoint(Dictionary<Point, PointPathData> list)
    {
        Point lowestFPoint = list.ElementAt(0).Key;
        PointPathData data = list.ElementAt(0).Value;
        for(int i = 0; i < list.Count; i++)
        {
            var element = list.ElementAt(i);
            if (element.Value.gCost + element.Value.hCost < data.gCost + data.hCost)
            {
                lowestFPoint = element.Key;
                data = element.Value;
            }
        }
        return (lowestFPoint, data);
    }
    public static float CalculateDistanceCost(Point pointA, Point pointB)
    {
        float xDistance = Mathf.Abs(pointA.Position.x - pointB.Position.x);
        float zDistance = Mathf.Abs(pointA.Position.z - pointB.Position.z);
        float remaining = Mathf.Abs(xDistance - zDistance);
        return 14 * Mathf.Min(xDistance, zDistance) + 10 * remaining;
    }
    public List<Point> GetNeighbourPoints(Point point)
    {
        List<Point> neighbours = new List<Point>();

        foreach(Vector3 neighbourVector in neighbourChecks)
        {
            Point neighbourPoint = points.Find(x => x.Position == (point.Position + neighbourVector));
            if (neighbourPoint != null) neighbours.Add(neighbourPoint);
        }

        return neighbours;
    }
    public Point GetClosestPointInChunk(Vector3 worldPos, int chunk)
    {
        Point closestPoint = null;
        float closestDistance = Mathf.Infinity;
        foreach(Point point in pointsChunked[chunk])
        {
            if (point == null) continue;

            if (closestPoint == null)
            {
                closestPoint = point;
                continue;
            }
            float distance = Vector3.Distance(point.Position, worldPos);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
    public int GetChunkSizeScaled()
    {
        return chunkSize / pointScale;
    }
}
