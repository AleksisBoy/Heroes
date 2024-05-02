using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class CombatMap : MonoBehaviour
{
    [SerializeField] private Terrain terrain = null;
    [SerializeField] private CombatTile tilePrefab = null;
    [SerializeField] private Vector2Int mapSize = Vector2Int.zero;
    [SerializeField] private float tileScale = 3;

    private Action<CombatUnit> onRemoveUnit;
    private Vector3 middlePos;
    public Vector3 MiddlePosition => middlePos;
    public float TileScale => tileScale;

    private List<CombatUnit> attackerPrepareUnits = new List<CombatUnit>();
    private List<CombatUnit> defenderPrepareUnits = new List<CombatUnit>();

    private List<CombatUnit> attackerUnits = new List<CombatUnit>();
    private List<CombatUnit> defenderUnits = new List<CombatUnit>();

    private List<CombatTile> tiles = new List<CombatTile>();

    private static ReadOnlyCollection<Vector2Int> DirectionsPathfind { get; } = new ReadOnlyCollection<Vector2Int>(new[]
{
        new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1),
        new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1)
    });
    private void Start()
    {
        middlePos = new Vector3(terrain.terrainData.size.x / 2, 0, terrain.terrainData.size.z / 2);

        CreateTiles();

        Vector3 attackerPosition = new Vector3(middlePos.x - mapSize.x - tileScale * 2, 0f, middlePos.z + mapSize.y + tileScale * 2);
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = attackerPosition;
    }

    private void CreateTiles()
    {
        
        for (float x = tileScale; x < mapSize.x * tileScale + tileScale; x += tileScale)
        {
            for (float y = tileScale; y < mapSize.y * tileScale + tileScale; y += tileScale)
            {
                CombatTile tile = Instantiate(tilePrefab);
                tile.transform.position = new Vector3(middlePos.x - (mapSize.x / 2) * tileScale + x, 0.01f,
                    middlePos.z - (mapSize.y / 2) * tileScale + y);
                tile.transform.localScale *= tileScale;
                tile.name = x / tileScale + ":" + y + tileScale;
                tile.Set(Mathf.RoundToInt(x / tileScale), Mathf.RoundToInt(y / tileScale));
                tile.transform.SetParent(transform);
                tiles.Add(tile);
            }
        }
    }
    public void DeactivateTiles()
    {
        foreach(CombatTile tile in tiles)
        {
            tile.Inactived();
        }
    }

    // Preparation calls
    public void AddAttackerUnitOnMapPrepare(CombatUnit unit, CombatTile tile)
    {
        attackerPrepareUnits.Add(unit);
        tile.SetUnit(unit);
    }
    public void AddDefenderUnitOnMapPrepare(CombatUnit unit, CombatTile tile)
    {
        defenderPrepareUnits.Add(unit);
        tile.SetUnit(unit);
    }
    public void RemoveUnitPrepare(UnitContainer unit)
    {
        CombatTile tile = GetUnitTile(unit);
        if(tile == null)
        {
            Debug.LogWarning("Did not find tile for unit " + unit.DebugName);
            return;
        }
        attackerPrepareUnits.Remove(tile.Unit);
        if (onRemoveUnit != null) onRemoveUnit(tile.Unit);
        Destroy(tile.Unit.gameObject);
        tile.ClearTile();
        tile.Actived();
    }
    public bool IsUnitOnMapPrepare(CombatUnit unit, bool attacker)
    {
        if (attacker)
        {
            foreach (CombatUnit attackerUnit in attackerPrepareUnits)
            {
                if (attackerUnit.Container == unit.Container)
                {
                    return true;
                }
            }
        }
        else
        {
            foreach (CombatUnit defenderUnit in defenderPrepareUnits)
            {
                if (defenderUnit.Container == unit.Container)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Combat Main State Calls

    public void AssignAndShowPreparedUnits()
    {
        attackerUnits = attackerPrepareUnits;
        defenderUnits = defenderPrepareUnits;

        attackerPrepareUnits = null;
        defenderPrepareUnits = null;

        foreach(CombatUnit unit in GetAllUnits())
        {
            GetUnitTile(unit.Container).UpdateUnitTransform();
        }
    }

    // Getters
    public CombatTile GetUnitTile(UnitContainer unit)
    {
        if (unit == null) return null;

        foreach(CombatTile tile in tiles)
        {
            if (tile.Unit == null) continue;

            if (tile.Unit.Container == unit)
            {
                return tile;
            }
        }
        return null;
    }
    public CombatTile GetTileInPosition(Vector3 position)
    {
        CombatTile closestTile = null;
        float closestDistance = Mathf.Infinity;
        foreach(CombatTile cTile in tiles)
        {
            float distance = Vector3.Distance(cTile.transform.position, position);
            if(distance < closestDistance)
            {
                closestTile = cTile;
                closestDistance = distance;
            }
        }
        if (closestDistance > tileScale / 1.5f) return null;

        return closestTile;
    }

    public List<CombatUnit> GetAllUnits()
    {
        List<CombatUnit> list = new List<CombatUnit>(attackerUnits);
        list.AddRange(defenderUnits);
        return list;
    }
    public List<CombatTile> GetTilesAroundTile(CombatTile tileBegin, int radius)
    {
        List<CombatTile> tilesChecked = new List<CombatTile>();
        tilesChecked.Add(tileBegin);

        Queue<CombatTile> qTiles = new Queue<CombatTile>();

        tileBegin.tempOrder = 0;
        qTiles.Enqueue(tileBegin);
        List<CombatTile> tilesActive = new List<CombatTile>();
        while (qTiles.Count > 0)
        {
            CombatTile tile = qTiles.Dequeue();
            for (int i = 0; i < DirectionsPathfind.Count; i++)
            {
                CombatTile adjTile = GetByCoors(tile.Coordinates + DirectionsPathfind[i]);
                float nextOrder = tile.tempOrder;
                if (i > 3)
                {
                    nextOrder += 1.4f;
                }
                else
                {
                    nextOrder += 1f;
                }
                if (!adjTile || !adjTile.IsFree() || tilesChecked.Contains(adjTile) || nextOrder > radius) continue;

                adjTile.tempOrder = nextOrder;
                qTiles.Enqueue(adjTile);
                tilesChecked.Add(adjTile);
            }

            tilesActive.Add(tile);
        }

        foreach (CombatTile tile in tilesActive)
        {
            tile.tempOrder = 0;
        }
        tilesActive.Remove(tileBegin);
        return tilesActive;
    }

    // Setters
    public void AddActionOnUnitRemove(Action<CombatUnit> action)
    {
        onRemoveUnit += action;
    }
    public List<CombatTile> ActivateColomns(int[] colomns)
    {
        List<CombatTile> tilesActive = GetTilesByColomns(colomns);
        foreach(CombatTile tile in tilesActive)
        {
            tile.Actived();
        }
        return tilesActive;
    }
    public List<CombatTile> GetTilesByColomns(int[] colomns)
    {
        List<CombatTile> tilesActive = new List<CombatTile>();
        foreach (int colomn in colomns)
        {
            for (int i = colomn * mapSize.y; i < colomn * mapSize.y + mapSize.y; i++)
            {
                tilesActive.Add(tiles[i]);
            }
        }
        return tilesActive;
    }
    public CombatTile GetRandomFreeTile(List<CombatTile> tileRange)
    {
        CombatTile tile = null;
        int debugCount = 0;
        while(tile == null)
        {
            int randomIndex = UnityEngine.Random.Range(0, tileRange.Count);
            if (tileRange[randomIndex].IsFree()) tile = tileRange[randomIndex];
            debugCount++;
            if(debugCount > 50)
            {
                Debug.LogError("infinite loop");
                break;
            }
        }
        return tile;
    }
    public int[] GetPreparationColomns(bool attacker)
    {
        if (attacker)
        {
            return new int[] { 0, 1 };
        }
        else
        {
            return new int[] { mapSize.x - 2, mapSize.x - 1};
        }
    }
    public CombatTile GetByCoors(int x, int y)
    {
        return tiles.FirstOrDefault(obj => obj.Coordinates.x == x && obj.Coordinates.y == y);
    }
    public CombatTile GetByCoors(Vector2Int coordinates)
    {
        return tiles.FirstOrDefault(obj => obj.Coordinates == coordinates);
    }
}
public struct CombatPath
{
    public List<CombatTile> tiles;
}
