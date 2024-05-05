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
    [SerializeField] private Color enemyColor = Color.white;
    [SerializeField] private Color unitTurnColor = Color.white;

    // Pathfinding
    [HideInInspector] public float hCost = 0f;
    [HideInInspector] public float gCost = 0f;
    [HideInInspector] public float fCost = 0f;
    [HideInInspector] public CombatTile parentTile = null;

    private State state;
    public State S => state;
    private Vector2Int coordinates;
    public Vector2Int Coordinates => coordinates;
    //private bool occupied = false;
    private CombatUnit unit = null;
    public CombatUnit Unit => unit;
    public void Set(int x, int y, State state)
    {
        UpdateState(state);
        coordinates = new Vector2Int(x, y);
        unit = null;
    }
    public void UpdateState(State state)
    {
        this.state = state;
        switch(state)
        {
            case State.None:
                sr.color = inactiveColor;
                break;
            case State.Active: 
                sr.color = activeColor;
                break;
            case State.Highlight:
                sr.color = unitTurnColor;
                break;
            case State.Endangered:
                sr.color = enemyColor;
                break;
            case State.Selected:
                sr.color = selectedColor;
                break;
            default:
                sr.color = inactiveColor;
                break;
        }
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
    public enum State
    {
        None, 
        Active,
        Selected,
        Endangered, 
        Highlight
    }
}
