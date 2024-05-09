using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class CombatATB_UI : MonoBehaviour
{
    [SerializeField] private RectTransform grid = null;
    [SerializeField] private IconContainerUI currentUnitIcon = null;
    [SerializeField] private IconUI iconPrefab = null;
    [SerializeField] private IconContainerUI iconContainerPrefab = null;
    [SerializeField] private int iconCount = 10;

    //private Dictionary<CombatUnit, IconContainerUI> icons = new Dictionary<CombatUnit, IconContainerUI>();
    private CombatManager manager = null;
    public void SetupBar(CombatManager manager)
    {
        if(!this.manager) manager.OnProgressATB += UpdateUI;
        this.manager = manager;
        //this.manager.OnUnitDestroy += RemoveUnitUI;
        //icons.Clear();
        foreach(Transform child in grid)
        {
            Destroy(child.gameObject);
        }
        List<CombatUnit> units = manager.GetCombatUnits().OrderByDescending(x => x.ATB).ToList();

        if (currentUnitIcon.Icon) Destroy(currentUnitIcon.TakeOut().gameObject);

        Color backColor = manager.GetPlayer(units[0]).PlayerColor;
        backColor.a = 0.5f;
        currentUnitIcon.Background.color = backColor;

        IconUI current = Instantiate(iconPrefab, currentUnitIcon.transform);
        current.Set(units[0], currentUnitIcon, false);

        int unitIndex = 0;
        int iconAmount = 0;
        while(iconAmount < iconCount)
        {
            CombatUnit unit = units[unitIndex];
            IconContainerUI container = Instantiate(iconContainerPrefab, grid);
            backColor = manager.GetPlayer(unit).PlayerColor;
            backColor.a = 0.5f;
            container.Background.color = backColor;

            RectTransform rtContainer = (RectTransform)container.transform;
            rtContainer.anchorMin = new Vector2(0f, 0.5f);
            rtContainer.anchorMax = new Vector2(0f, 0.5f);
            rtContainer.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (iconAmount * iconContainerPrefab.RT.sizeDelta.x), 0f);

            IconUI icon = Instantiate(iconPrefab, container.transform);
            //icon.Set(unit.GetType().ToString(), unit.Container.Data.name, 1, unit.Container.Data.Sprite, unit.Container.Count, container, false);
            icon.Set(unit, container, false);
            //icons.Add(unit, container);
            if(++unitIndex >= units.Count)
            {
                unitIndex = 0;
            }
            iconAmount++;
        }
        //foreach (CombatUnit unit in units)
        //{
        //    IconContainerUI container = Instantiate(iconContainerPrefab, grid);
        //    RectTransform rtContainer = (RectTransform)container.transform;
        //    rtContainer.anchorMin = new Vector2(0f, 0.5f);
        //    rtContainer.anchorMax = new Vector2(0f, 0.5f);
        //    rtContainer.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (icons.Count * iconContainerPrefab.RT.sizeDelta.x), 0f);

        //    IconUI icon = Instantiate(iconPrefab, container.transform);
        //    icon.Set(unit.GetType().ToString(), unit.Container.Data.name, 1, unit.Container.Data.Sprite, unit.Container.Count, container, false);

        //    icons.Add(unit, container);
        //}
    }
    private void UpdateUI()
    {
        SetupBar(manager);
        //List<CombatUnit> units = manager.GetCombatUnits();

        //foreach(Transform child in currentUnitIcon.transform)
        //{
        //    Destroy(child.gameObject);
        //}
        //IconUI current = Instantiate(iconPrefab, currentUnitIcon.transform);
        //current.Set(units[0], currentUnitIcon, false);
        //foreach(CombatUnit unit in icons.Keys)
        //{
        //    icons[unit].Icon.Set(unit, icons[unit], false);
        //    RectTransform rtIcon = (RectTransform)icons[unit].transform;
        //    rtIcon.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (units.IndexOf(unit) * iconContainerPrefab.RT.sizeDelta.x), 0f);
        //}
    }
    public void RemoveUnitUI(CombatUnit unit)
    {
        //Destroy(icons[unit].gameObject);
        //icons.Remove(unit);
    }
    private void OnDestroy()
    {
        manager.OnProgressATB -= UpdateUI;
        manager.OnUnitDestroy -= RemoveUnitUI;
    }
}
