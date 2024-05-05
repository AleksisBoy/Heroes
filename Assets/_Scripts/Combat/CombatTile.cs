using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


public class CombatTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr = null;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color intersectingColor = Color.white;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color previewColor = Color.white;

    // Pathfinding
    public float hCost = 0f;
    public float gCost = 0f;
    public float fCost = 0f;
    public CombatTile parentTile = null;

    public float tempOrder = 0;
    private Vector2Int coordinates;
    public Vector2Int Coordinates => coordinates;
    //private bool occupied = false;
    private CombatUnit unit = null;
    public CombatUnit Unit => unit;


    public static List<CombatTile> Array = new List<CombatTile>();
    private void Awake()
    {
        Array.Add(this);
    }
    public void Set(int x, int y)
    {
        sr.color = inactiveColor;
        coordinates = new Vector2Int(x, y);
        unit = null;
    }
    public void Selected()
    {
        sr.color = selectedColor;
    }
    public void Actived()
    {
        sr.color = activeColor;
    }
    public void Inactived()
    {
        sr.color = inactiveColor;
    }

    public void ShowUnitPlaceHolder(CombatUnit unitPlacehold)
    {
        unitPlacehold.transform.position = transform.position; 
        unitPlacehold.transform.rotation = unitPlacehold.Attacker ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
        unitPlacehold.gameObject.SetActive(true);
    }
    public void UpdateUnitTransform()
    {
        if(!unit) return;

        unit.transform.position = transform.position;
        UpdateUnitRotation();
        unit.gameObject.SetActive(true);
        unit.OnUnitUpdateUI();
    }
    public void UpdateUnitRotation()
    {
        if (!unit) return;

        unit.transform.rotation = unit.Attacker ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.Euler(0f, -90f, 0f);
    }
    public void SetUnit(CombatUnit unit)
    {
        this.unit = unit; 
    }
    public void ClearTile()
    {
        unit = null;
    }
    public bool IsFree()
    {
        return unit == null;
    }
    public void ResetPF()
    {
        gCost = 0f;
        hCost = 0f;
        fCost = 0f;
        parentTile = null;
    }
    private void OnDestroy()
    {
        if(Array.Contains(this)) Array.Remove(this);
    }
}
