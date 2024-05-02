using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityCanvas : MonoBehaviour
{
    [SerializeField] private IconUI iconPrefab = null;
    [SerializeField] private IconContainerUI cityHeroFrame = null;
    [SerializeField] private IconContainerUI gateHeroFrame = null;
    [SerializeField] private IconContainerUI[] gateUnitFrame = new IconContainerUI[7];
    [SerializeField] private IconContainerUI[] cityUnitFrame = new IconContainerUI[7];

    private City city = null;
    public void Enable(City city)
    {
        ClearFrames();

        gameObject.SetActive(true);

        if (city.HeroAtGate != null)
        {
            IconUI heroIcon = Instantiate(iconPrefab, gateHeroFrame.transform);
            heroIcon.AddOnEndDrag(IconMoveCheck);
            heroIcon.Set(typeof(HeroMount).ToString(), city.HeroAtGate.Hero.name, -1, city.HeroAtGate.Hero.Sprite, 1, gateHeroFrame, true);

            for (int i = 0; i < 7; i++)
            {
                var unit = city.HeroAtGate.Units[i];
                if (unit == null) continue;

                IconUI unitIcon = Instantiate(iconPrefab, gateUnitFrame[i].transform);
                unitIcon.AddOnEndDrag(IconMoveCheck);
                unitIcon.Set(typeof(Unit).ToString(), unit.Data.name, i, unit.Data.Sprite, unit.Count, gateUnitFrame[i], true);
            }
        }
        this.city = city;
    }

    private void ClearFrames()
    {
        foreach (Transform child in cityHeroFrame.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in gateHeroFrame.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (IconContainerUI itemContainer in cityUnitFrame)
        {
            foreach (Transform child in itemContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
        foreach (IconContainerUI itemContainer in gateUnitFrame)
        {
            foreach (Transform child in itemContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void IconMoveCheck(IconContainerUI iconMovedContainer)
    {
        foreach (IconContainerUI iconContainer in transform.GetComponentsInChildren<IconContainerUI>())
        {
            if (iconContainer.Icon == iconMovedContainer.Icon) continue; // same one check

            if(RectTransformUtility.RectangleContainsScreenPoint(iconContainer.RT, Input.mousePosition))
            {
                SwapIcons(iconMovedContainer, iconContainer);
                return;
            }
        }
    }
    private void SwapIcons(IconContainerUI iconContainer1, IconContainerUI iconContainer2)
    {
        if(iconContainer1.Icon.Data.Type == "HeroMount" 
            && iconContainer2 == cityHeroFrame || iconContainer2 == gateHeroFrame)
        {
            HeroMount hero = iconContainer1 == cityHeroFrame ? city.HeroInCity : city.HeroAtGate;
            
            // move hero to city frame
            return;
        } 
        else if(iconContainer1.Icon.Data.Type == "HeroMount" 
            && iconContainer2 != cityHeroFrame && iconContainer2 != gateHeroFrame)
        {
            return; // cannot move hero to unit frame
        } 
        else if(iconContainer1.Icon.Data == iconContainer2.Icon?.Data)
        {
            iconContainer2.Icon.Merge(iconContainer1.TakeOut());
            return;
        }
        

        IconUI icon1 = iconContainer1.TakeOut();
        iconContainer1.Assign(iconContainer2.TakeOut());
        iconContainer2.Assign(icon1);
    }
}
