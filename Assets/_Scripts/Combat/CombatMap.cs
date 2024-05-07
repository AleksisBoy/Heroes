using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

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
    public Vector2Int MapSize => mapSize;

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
    private static ReadOnlyCollection<Vector2Int> DirectionsAdjacent { get; } = new ReadOnlyCollection<Vector2Int>(new[]
{
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(1, 0),
        new Vector2Int(1, 1), new Vector2Int(0, 1), new Vector2Int(-1, 1), new Vector2Int(-1, 0)
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
                tile.name = x / tileScale + ":" + y / tileScale;
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
            tile.ClearStates();
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
    public void RemoveUnitPrepare(UnitContainer unit, bool attacker)
    {
        CombatTile tile = GetUnitTile(unit);
        if(tile == null)
        {
            Debug.LogWarning("Did not find tile for unit " + unit.DebugName);
            return;
        }

        if (attacker)
        {
            attackerPrepareUnits.Remove(tile.Unit);
        }
        else
        {
            defenderPrepareUnits.Remove(tile.Unit);
        }
        if (onRemoveUnit != null) onRemoveUnit(tile.Unit);
        Destroy(tile.Unit.gameObject);
        tile.ClearTile();
        //tile.UpdateState(CombatTile.State.Active);
        tile.AddState(CombatTile.State.Active);
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
    public void RemoveUnit(CombatUnit unit)
    {
        CombatTile tile = GetUnitTile(unit.Container);
        if (tile == null)
        {
            Debug.LogWarning("Did not find tile for unit " + unit.Container.DebugName);
            return;
        }

        if (attackerUnits.Contains(unit)) attackerUnits.Remove(unit);
        else if(defenderUnits.Contains(unit)) defenderUnits.Remove(unit);
        else
        {
            Debug.LogError("NO UNIT TO BE REMOVE NO DATA");
            return;
        }

        if (onRemoveUnit != null) onRemoveUnit(tile.Unit);
        Destroy(tile.Unit.gameObject);
        tile.ClearTile();
        //tile.UpdateState(CombatTile.State.Active);
        tile.AddState(CombatTile.State.Active);
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

    public List<CombatUnit> AttackerUnits => attackerUnits;
    public List<CombatUnit> DefenderUnits => defenderUnits;
    public List<CombatTile> GetTilesAroundTile(CombatTile tileBegin, int radius)
    {
        List<CombatTile> tilesChecked = new List<CombatTile>();
        tilesChecked.Add(tileBegin);

        Queue<CombatTile> qTiles = new Queue<CombatTile>();

        tileBegin.hCost = 0;
        qTiles.Enqueue(tileBegin);
        List<CombatTile> tilesActive = new List<CombatTile>();
        while (qTiles.Count > 0)
        {
            CombatTile tile = qTiles.Dequeue();
            for (int i = 0; i < DirectionsPathfind.Count; i++)
            {
                CombatTile adjTile = GetByCoors(tile.Coordinates + DirectionsPathfind[i]);
                float nextOrder = tile.hCost;
                if (i > 3)
                {
                    nextOrder += 1.4f;
                }
                else
                {
                    nextOrder += 1f;
                }
                if (!adjTile || !adjTile.IsFree() || tilesChecked.Contains(adjTile) || nextOrder > radius) continue;

                adjTile.hCost = nextOrder;
                qTiles.Enqueue(adjTile);
                tilesChecked.Add(adjTile);
            }

            tilesActive.Add(tile);
        }

        foreach (CombatTile tile in tilesActive)
        {
            tile.hCost = 0;
        }
        //tilesActive.Remove(tileBegin);
        return tilesActive;
    }
    public List<CombatTile> GetEnemyUnitsTilesOnBorderForPlayer(Player player, List<CombatTile> activeTiles)
    {
        List<CombatTile> enemyTiles = new List<CombatTile>();
        List<CombatTile> checkedTiles = new List<CombatTile>();
        foreach (CombatTile tile in activeTiles)
        {
            for(int i = 0; i < DirectionsPathfind.Count; i++)
            {
                CombatTile checkingTile = GetByCoors(tile.Coordinates.x + DirectionsPathfind[i].x, tile.Coordinates.y + DirectionsPathfind[i].y);
                if (!checkingTile) continue;

                if (activeTiles.Contains(checkingTile))
                {
                    checkedTiles.Add(checkingTile);
                    continue;
                }
                if (checkedTiles.Contains(checkingTile))
                {
                    continue;
                }
                if (checkingTile.Unit && checkingTile.Unit.IsOpponent(player))
                {
                    checkedTiles.Add(checkingTile);
                    enemyTiles.Add(checkingTile);
                }
            }
        }
        return enemyTiles;
    }
    public List<CombatTile> GetEnemyUnitTiles(Player ally)
    {
        List<CombatTile> enemyUnitTiles = new List<CombatTile>();
        foreach(CombatTile tile in tiles)
        {
            if (tile.Unit && tile.Unit.Container.Player != ally) enemyUnitTiles.Add(tile);
        }
        return enemyUnitTiles;
    }
    public CombatTile GetAdjacentTileInDirectionWithin(CombatTile centerTile, List<CombatTile> tileRange, Vector2 direction, CombatTile thisUnitTile)
    {
        if (thisUnitTile) tileRange.Add(thisUnitTile);
        float angle = (Mathf.Atan2(direction.y, direction.x) + Mathf.PI) / (2 * Mathf.PI); // value of 0 to 1
        CombatTile adjacentTile = null;
        float currentValue = 0.0625f;
        int i = 0;
        while (adjacentTile == null)
        {
            bool left = false;
            float topValue = currentValue + 0.125f;
            if (topValue > 1f) topValue -= 1f;
            if(currentValue >= 0.9375f || currentValue < 0.0625f)
            {
                i = 7;
                left = true;
            }
            if ((angle >= currentValue && angle < topValue) || left)
            {
                if (left) left = false;
                CombatTile tile = GetByCoors(centerTile.Coordinates.x + DirectionsAdjacent[i].x, centerTile.Coordinates.y + DirectionsAdjacent[i].y);
                if (tile && tileRange.Contains(tile) && !tile.Unit)
                {
                    adjacentTile = tile;
                    break;
                }
                else if (tile && tileRange.Contains(tile) && tile.Unit && thisUnitTile && tile.Unit == thisUnitTile.Unit)
                {
                    adjacentTile = tile;
                    break;
                }
                else
                {
                    // lowest distance to closest and active tile
                    float diff1 = Mathf.Abs(angle - currentValue);
                    float diff2 = Mathf.Abs(angle - topValue);
                    if (diff1 < diff2)
                    {
                        angle -= 0.125f;
                        if (angle < 0f) angle += 1f;
                        currentValue -= 0.125f;
                        if (currentValue < 0f) currentValue += 1f;
                        if (--i < 0) i = 7;
                    }
                    else
                    {
                        angle += 0.125f;
                        if(angle > 1f) angle -= 1f;
                        currentValue += 0.125f;
                        if (currentValue > 1f) currentValue -= 1f;

                        if (++i > 7) i = 0;
                    }
                    //i = 0;
                    //currentValue = 0.0625f;
                    continue;
                }
            }
            currentValue += 0.125f;
            if (currentValue > 1f) currentValue -= 1f;
            if (++i > 7)
            {
                Debug.LogError("NO ADJACENT TILES");
                break;
            }
        }
        
        return adjacentTile;
    }
    public Stack<CombatTile> GetPathToTile(CombatTile startingTile, CombatTile endingTile, List<CombatTile> tileRange)
    {
        if(startingTile == endingTile) return new Stack<CombatTile>();

        List<CombatTile> openTiles = new List<CombatTile>();
        List<CombatTile> closedTiles = new List<CombatTile>();
        openTiles.Add(startingTile);

        while (openTiles.Count > 0)
        {
            CombatTile current = openTiles.OrderBy(x => x.fCost).FirstOrDefault();
            if (!current) continue;
            openTiles.Remove(current);
            closedTiles.Add(current);

            if (current == endingTile) break;

            for (int i = 0; i < DirectionsPathfind.Count; i++)
            {
                CombatTile neighbour = GetByCoors(current.Coordinates + DirectionsPathfind[i]);
                if (!neighbour || !neighbour.IsFree() || closedTiles.Contains(neighbour)) continue;

                float gCost = Vector2.Distance(neighbour.Coordinates, current.Coordinates) + current.gCost;
                float hCost = Vector2.Distance(neighbour.Coordinates, endingTile.Coordinates);
                if (!openTiles.Contains(neighbour) || gCost < neighbour.gCost)
                {
                    neighbour.gCost = gCost;
                    neighbour.hCost = hCost;
                    neighbour.fCost = gCost + hCost;
                    neighbour.parentTile = current;
                    if (!openTiles.Contains(neighbour)) openTiles.Add(neighbour);
                }
            }
        }
        Stack<CombatTile> path = new Stack<CombatTile>();
        path.Push(endingTile);
        while (true)
        {
            CombatTile pathTile = path.Peek().parentTile;
            if(pathTile == startingTile) break;
            path.Push(pathTile);
        }
        foreach(CombatTile tile in tiles)
        {
            tile.ResetPF();
        }
        return path;
    }
    public CombatTile GetClosestTileToTile(CombatTile startingTile, CombatTile endingTile, List<CombatTile> tileRange)
    {
        if (startingTile == endingTile) return endingTile;
        if (tileRange.Contains(endingTile)) return endingTile;

        List<CombatTile> openTiles = new List<CombatTile>();
        List<CombatTile> closedTiles = new List<CombatTile>();
        openTiles.Add(startingTile);

        while (openTiles.Count > 0)
        {
            CombatTile current = openTiles.OrderBy(x => x.fCost).FirstOrDefault();
            if (!current) continue; //incorrect
            openTiles.Remove(current);
            closedTiles.Add(current);

            if (current == endingTile) break;

            for (int i = 0; i < DirectionsPathfind.Count; i++)
            {
                CombatTile neighbour = GetByCoors(current.Coordinates + DirectionsPathfind[i]);
                if (neighbour != endingTile) if (!neighbour || !neighbour.IsFree() || closedTiles.Contains(neighbour)) continue;

                float gCost = Vector2.Distance(neighbour.Coordinates, current.Coordinates) + current.gCost;
                float hCost = Vector2.Distance(neighbour.Coordinates, endingTile.Coordinates);
                if (!openTiles.Contains(neighbour) || gCost < neighbour.gCost)
                {
                    neighbour.gCost = gCost;
                    neighbour.hCost = hCost;
                    neighbour.fCost = gCost + hCost;
                    neighbour.parentTile = current;
                    Debug.Log(neighbour.Coordinates);
                    //neighbour.UpdateState(CombatTile.State.Highlight);
                    neighbour.AddState(CombatTile.State.Highlight);
                    if (!openTiles.Contains(neighbour)) openTiles.Add(neighbour);
                }
            }
        }
        CombatTile closestTile = null;
        Stack<CombatTile> path = new Stack<CombatTile>();
        path.Push(endingTile);
        while (true)
        {
            CombatTile pathTile = path.Peek().parentTile;
            if (tileRange.Contains(pathTile))
            {
                closestTile = pathTile;
                break;
            }
            if (pathTile == startingTile) break;
            path.Push(pathTile);
        }
        
        foreach (CombatTile tile in tiles)
        {
            tile.ResetPF();
        }
        return closestTile;
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
    public CombatTile GetTileInColomn(int colomn, int index)
    {
        return GetTilesByColomns(new int[] { colomn })[index];
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
    public List<CombatTile> GetTiles()
    {
        return new List<CombatTile>(tiles);
    }
    // Setters
    public void AddActionOnUnitRemove(Action<CombatUnit> action)
    {
        onRemoveUnit += action;
    }
    public List<CombatTile> ActivateColomns(int[] colomns)
    {
        List<CombatTile> tilesActive = GetTilesByColomns(colomns);
        foreach (CombatTile tile in tilesActive)
        {
            //tile.UpdateState(CombatTile.State.Active);
            tile.AddState(CombatTile.State.Active);
        }
        return tilesActive;
    }
}
public struct CombatPath
{
    public List<CombatTile> tiles;
}
