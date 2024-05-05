using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBar : MonoBehaviour
{
    [Header("Unit Bar")]
    [SerializeField] private IconUI iconPrefab = null;
    [SerializeField] private IconContainerUI heroIconContainer = null;
    [SerializeField] private IconContainerUI[] unitIconsContainer = null;

    protected IconUI iconHero = null;
    protected List<IconUI> unitIcons = new List<IconUI>();
    protected HeroMount mount;
    public virtual void Setup(HeroMount mount)
    {
        this.mount = mount;

        iconHero = Instantiate(iconPrefab);
        iconHero.Set(typeof(HeroMount).ToString(), mount.Hero.name, -1, mount.Hero.Sprite, 1, this.heroIconContainer, false);

        for(int i = 0; i < mount.Units.Length; i++)
        {
            if (mount.Units[i] == null) continue;

            IconUI iconUnit = Instantiate(iconPrefab);
            iconUnit.Set(typeof(Unit).ToString(), mount.Units[i].Data.name, i, mount.Units[i].Data.Sprite, mount.Units[i].Count, unitIconsContainer[i], true);
            unitIcons.Add(iconUnit);
        }
    }
    public void Setup(List<UnitContainer> units)
    {
        heroIconContainer.gameObject.SetActive(false);
        int i = 0;
        foreach(UnitContainer unitContainer in units)
        {
            IconUI iconUnit = Instantiate(iconPrefab);
            iconUnit.Set(typeof(Unit).ToString(), unitContainer.Data.name, i, unitContainer.Data.Sprite, unitContainer.Count, unitIconsContainer[i], false);
            unitIcons.Add(iconUnit);
            i++;
        }
    }
}
