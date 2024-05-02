using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [SerializeField] private Player player = null;
    [SerializeField] private MapNavigator mapNavigator = null;
    [SerializeField] private HeroIconUI heroIconPrefab = null;
    [SerializeField] private GridLayoutGroup heroesGrid = null;
    [SerializeField] private ResourceUI[] resourceUIs = null;

    private void Start()
    {
        UpdateUI();
        player.resourcesUpdated += UpdateResourceBarUI;
    }
    public void UpdateUI()
    {
        foreach(Transform child in heroesGrid.transform)
        {
            Destroy(child.gameObject);
        }
        RectTransform rtHeroesGrid = (RectTransform)heroesGrid.transform;
        rtHeroesGrid.sizeDelta = new Vector2((heroesGrid.cellSize.x + heroesGrid.spacing.x) * player.Heroes.Count, heroesGrid.cellSize.x + heroesGrid.spacing.x);
        foreach(HeroMount hero in player.Heroes)
        {
            HeroIconUI icon = Instantiate(heroIconPrefab, heroesGrid.transform);
            icon.Set(hero, false, HeroSelected, heroesGrid.cellSize);
        }
        UpdateResourceBarUI();
    }
    private void UpdateResourceBarUI()
    {
        var playerResources = player.Resources;
        foreach(ResourceUI ui in resourceUIs)
        {
            ui.UpdateCount(playerResources[ui.Resource]);
        }
    }
    public void HeroSelected(HeroMount hero)
    {
        mapNavigator.SelectHero(hero);
    }
}
