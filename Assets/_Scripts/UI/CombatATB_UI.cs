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
    [SerializeField] private int iconCount = 10;

    private CombatManager manager = null;
    public void SetupBar(CombatManager manager)
    {
        if (!this.manager) manager.OnProgressATB += UpdateUI;
        this.manager = manager;

        UpdateUI();
    }
    private void CreateCurrentUnitIcon(Color playerColor, List<CombatUnit> units)
    {
        if (currentUnitIcon.Icon) Destroy(currentUnitIcon.TakeOut().gameObject);

        Color backColor = playerColor;
        backColor.a = 0.5f;
        currentUnitIcon.Background.color = backColor;

        IconUI current = Instantiate(iconPrefab, currentUnitIcon.transform);
        current.Set(units[0], currentUnitIcon, false);
    }
    private static Dictionary<float, CombatUnit> GetSortedDictionaryOfUnits(List<CombatUnit> units, int maxIcons)
    {
        int globalCycle = 0;
        Dictionary<float, CombatUnit> unitDic = new Dictionary<float, CombatUnit>();
        while (unitDic.Count < maxIcons)
        {
            foreach (CombatUnit unit1 in units)
            {
                int localCycle = 0;
                float remainToATB = CombatManager.GetRemainToATB(unit1, localCycle + globalCycle);
                while (remainToATB < 1 + globalCycle)
                {
                    bool added = unitDic.TryAdd(remainToATB, unit1);
                    if (!added) Debug.LogError("didnt add " + unit1.Container.DebugName + " atb remain " + remainToATB);
                    localCycle++;
                    remainToATB = CombatManager.GetRemainToATB(unit1, localCycle + globalCycle);
                }
            }

            globalCycle++;
        }
        unitDic = unitDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        return unitDic;
    }

    private void CreateIconsForUnitList(List<CombatUnit> list, int maxIcons)
    {
        for(int i = 0; i < maxIcons; i++)
        {
            IconContainerUI container = Instantiate(iconContainerPrefab, grid);
            Color backColor = manager.GetPlayer(list[i]).PlayerColor;
            backColor.a = 0.5f;
            container.Background.color = backColor;

            RectTransform rtContainer = (RectTransform)container.transform;
            rtContainer.anchorMin = new Vector2(0f, 0.5f);
            rtContainer.anchorMax = new Vector2(0f, 0.5f);
            rtContainer.anchoredPosition = new Vector2((iconContainerPrefab.RT.sizeDelta.x / 2f) + (i * iconContainerPrefab.RT.sizeDelta.x), 0f);

            IconUI icon = Instantiate(iconPrefab, container.transform);
            icon.Set(list[i], container, false);
        }
    }
    private void UpdateUI()
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }
        List<CombatUnit> units = manager.GetCombatUnits().OrderByDescending(x => x.ATB).ToList();

        int maxIcons = Mathf.Max(units.Count, iconCount);
        Dictionary<float, CombatUnit> unitDic = GetSortedDictionaryOfUnits(units, maxIcons);

        CreateCurrentUnitIcon(manager.GetPlayer(units[0]).PlayerColor, units);
        CreateIconsForUnitList(unitDic.Values.ToList(), maxIcons);
    }
    private void OnDestroy()
    {
        if(manager) manager.OnProgressATB -= UpdateUI;
    }
}
