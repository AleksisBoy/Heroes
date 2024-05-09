using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatUnitIconUI : IconUI, IPointerEnterHandler, IPointerExitHandler
{
    public static CombatUnitIconUI HoverOver { get; private set; }

    private CombatUnit unit = null;
    public CombatUnit Unit => unit;

    public override void Set(CombatUnit combatUnit, IconContainerUI container, bool draggable)
    {
        base.Set(combatUnit, container, draggable);
        unit = combatUnit;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverOver = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(HoverOver == this)HoverOver = null;
    }

}
