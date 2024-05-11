using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatMainState : MonoBehaviour
{
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;
    [SerializeField] private CombatATB_UI combatATB = null;

    [SerializeField] private Button buttonDefense = null;
    [SerializeField] private Button buttonSurrender = null;
    [SerializeField] private Button buttonSpellbook = null;
    [SerializeField] private Button buttonSkipTurn = null;
    [SerializeField] private Button buttonAutoBattle = null;
    [SerializeField] private Button buttonSettings = null;

    protected CombatPlayerTurnInput playerInput = null;
    protected CombatMap map = null;
    private Vector2 direction;
    protected List<CombatTile> activeTiles = null;
    protected CombatTile actingUnitTile = null;
    public CombatTile ActingUnitTile => actingUnitTile;
    protected bool isActive = false;
    protected virtual void Update()
    {
        if (isActive)
        {
            var selection = canvasUnitUtility.HighlightSelection(activeTiles, actingUnitTile, out bool selectedTileIsAdjacent);
            if (!selection.Item1) return;

            if (selection.Item1.Unit && selection.Item1 != actingUnitTile) canvasUnitUtility.PreviewMovementTiles(selection.Item1, new List<CombatTile>());
            else canvasUnitUtility.ResetLastPreviewTiles();

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (activeTiles.Contains(selection.Item1))
                {
                    if (selectedTileIsAdjacent) playerInput = CombatPlayerTurnInput.Attack(selection.Item1, selection.Item2);
                    else playerInput = CombatPlayerTurnInput.Move(selection.Item1);
                }
            }
        }

        // can check unit popup either if isActive or no
        if (Input.GetMouseButtonDown(1))
        {
            canvasUnitUtility.SetUnitPopup();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            canvasUnitUtility.DisableUnitPopup();
        }
    }
    public virtual void StartCombat(CombatManager manager, Player player)
    {
        map = manager.Map;
        combatATB?.SetupBar(manager);
        gameObject.SetActive(true);
        List<CombatUnit> allUnits = map.GetAllUnits();
        canvasUnitUtility.SetAllUnits(allUnits);
        foreach(CombatUnit unit in allUnits)
        {
            canvasUnitUtility.UpdateUnitCount(unit, map);
        }
    }
    public void SetActive(List<CombatTile> activeTiles, bool state, CombatTile actingUnitTile)
    {
        this.activeTiles = activeTiles;
        this.actingUnitTile = actingUnitTile;
        isActive = state;
        playerInput = null;
        if (!state)
        {
            canvasUnitUtility?.ResetSelection();
            InternalSettings.SwitchCursorTo(InternalSettings.CursorState.Busy);

            buttonDefense.interactable = false;
            buttonSurrender.interactable = false;
            buttonSpellbook.interactable = false;
            buttonSkipTurn.interactable = false;
        }
        else
        {
            buttonDefense.interactable = true;
            buttonSurrender.interactable = true;
            buttonSpellbook.interactable = true;
            buttonSkipTurn.interactable = true;
        }
    }
    public void DeactivateTiles()
    {
        map.DeactivateTiles();
    }
    public Vector2 GetSelectedTileDirection()
    {
        return direction;
    }
    public CombatPlayerTurnInput GetPlayerInput()
    {
        return playerInput;
    }
    // Button calls
    public void Button_Defense()
    {
        playerInput = CombatPlayerTurnInput.Defend();
    }
    public void Button_Surrender()
    {

    }
    public void Button_Spellbook()
    {

    }
    public void Button_SkipTurn()
    {
        playerInput = CombatPlayerTurnInput.Wait();
    }
    public void Button_AutoBattle()
    {

    }
    public void Button_Settings()
    {

    }
}
