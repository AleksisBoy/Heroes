using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;


public class CombatTile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr = null;

    // Pathfinding
    [HideInInspector] public float hCost = 0f;
    [HideInInspector] public float gCost = 0f;
    [HideInInspector] public float fCost = 0f;
    [HideInInspector] public CombatTile parentTile = null;

    private List<State> states = new List<State>();

    private Vector2Int coordinates;
    public Vector2Int Coordinates => coordinates;
    //private bool occupied = false;
    private CombatUnit unit = null;
    public CombatUnit Unit => unit;
    public void Set(int x, int y)
    {
        coordinates = new Vector2Int(x, y);
        unit = null;
        UpdateColor();
    }
    public void AddState(State state)
    {
        if (states.Contains(state)) return;

        states.Add(state);
        UpdateColor();
    }
    public void RemoveState(State state)
    {
        if (!states.Contains(state)) return;

        states.Remove(state);
        UpdateColor();
    }
    public void ClearStates()
    {
        states.Clear();
        UpdateColor();
    }
    public void UpdateColor()
    {
        if (states.Count == 0)
        {
            sr.color = InternalSettings.Get.InactiveColor;
        }
        else if (states.Count == 1)
        {
            switch (states[0])
            {
                case State.None:
                    sr.color = InternalSettings.Get.InactiveColor;
                    break;
                case State.Active:
                    sr.color = InternalSettings.Get.ActiveColor;
                    break;
                case State.Highlight:
                    sr.color = InternalSettings.Get.UnitTurnColor;
                    break;
                case State.Endangered:
                    sr.color = InternalSettings.Get.EnemyColor;
                    break;
                case State.Preview:
                    sr.color = InternalSettings.Get.PreviewColor;
                    break;
                case State.Selected:
                    sr.color = InternalSettings.Get.SelectedColor;
                    break;
                default:
                    sr.color = InternalSettings.Get.InactiveColor;
                    break;
            }
        }
        else if (states.Count == 2)
        {
            if (states.Contains(State.Active) && states.Contains(State.Preview))
            {
                sr.color = InternalSettings.Get.ActivePreviewColor;
            }
            else if(states.Contains(State.Active) && states.Contains(State.Selected))
            {
                sr.color = InternalSettings.Get.ActiveSelectedColor;
            }
            else
            {
                sr.color = Color.black;
            }
        }
        else
        {
            if (states.Contains(State.Active) && states.Contains(State.Preview) && states.Contains(State.Selected))
            {
                sr.color = InternalSettings.Get.ActiveSelectedPreviewColor;
            }
            else
            {
                sr.color = Color.black;
            }
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
        Highlight,
        Preview
    }
}
