using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMainState : MonoBehaviour
{
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;

    protected CombatMap map = null;
    protected CombatTile selectedTile = null;
    protected List<CombatTile> activeTiles = null;
    protected bool isActive = false;
    protected virtual void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hitInfo.transform == null) return;

            CombatTile tile = map.GetTileInPosition(hitInfo.point);
            if(activeTiles.Contains(tile)) selectedTile = tile;
        }
    }
    public virtual void StartCombat(CombatMap map)
    {
        this.map = map;
        selectedTile = null;
        gameObject.SetActive(true);
        foreach(CombatUnit unit in map.GetAllUnits())
        {
            canvasUnitUtility.UpdateUnitCount(unit, map);
        }
    }
    public void SetActive(List<CombatTile> activeTiles, bool state)
    {
        this.activeTiles = activeTiles;
        isActive = state;
        if (!state) selectedTile = null;
    }
    public void DeactivateTiles()
    {
        map.DeactivateTiles();
    }
    public CombatTile GetSelectedTile()
    {
        return selectedTile;
    }

}
