using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatATB_UI : MonoBehaviour
{
    [SerializeField] private RectTransform grid = null;
    [SerializeField] private IconContainerUI currentUnitIcon = null;
    [SerializeField] private IconUI iconPrefab = null;
    [SerializeField] private IconContainerUI iconContainerPrefab = null;

    private Dictionary<CombatUnit, IconContainerUI> icons = new Dictionary<CombatUnit, IconContainerUI>();
    private CombatManager manager = null;
    public void SetupBar(CombatManager manager)
    {
        this.manager = manager;
        this.manager.OnProgressATB += UpdateUI;
        this.manager.OnUnitDestroy += RemoveUnitUI;
        icons.Clear();
        foreach(Transform child in grid)
        {
            Destroy(child.gameObject);
        }
        List<CombatUnit> units = manager.GetCombatUnits().OrderByDescending(x => x.ATB).ToList();
        foreach (CombatUnit unit in units)
        {
            IconContainerUI container = Instantiate(iconContainerPrefab, grid);
            RectTransform rtContainer = (RectTransform)container.transform;
            rtContainer.anchorMin = new Vector2(0f, 0.5f);
            rtContainer.anchorMax = new Vector2(0f, 0.5f);
            rtContainer.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (icons.Count * iconContainerPrefab.RT.sizeDelta.x), 0f);

            IconUI icon = Instantiate(iconPrefab, container.transform);
            icon.Set(unit.GetType().ToString(), unit.Container.Data.name, 1, unit.Container.Data.Sprite, unit.Container.Count, container, false);

            icons.Add(unit, container);
        }
    }
    private void UpdateUI()
    {
        List<CombatUnit> units = manager.GetCombatUnits();

        foreach(Transform child in currentUnitIcon.transform)
        {
            Destroy(child.gameObject);
        }
        IconUI current = Instantiate(iconPrefab, currentUnitIcon.transform);
        current.Set(units[0], currentUnitIcon, false);
        foreach(CombatUnit unit in icons.Keys)
        {
            icons[unit].Icon.Set(unit, icons[unit], false);
            RectTransform rtIcon = (RectTransform)icons[unit].transform;
            rtIcon.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (units.IndexOf(unit) * iconContainerPrefab.RT.sizeDelta.x), 0f);
        }
    }
    public void RemoveUnitUI(CombatUnit unit)
    {
        Destroy(icons[unit].gameObject);
        icons.Remove(unit);
    }
    private void OnDestroy()
    {
        manager.OnProgressATB -= UpdateUI;
        manager.OnUnitDestroy -= RemoveUnitUI;
    }
}
