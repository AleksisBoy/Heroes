using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatUnitIconUI : IconUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static CombatUnitIconUI HoverOver { get; private set; }

    private Action<CombatUnit> onPointerClick = null;

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
        if(HoverOver == this) HoverOver = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onPointerClick != null) onPointerClick(unit);
    }
    public void AddOnPointerClick(Action<CombatUnit> onPointerClickAction)
    {
        onPointerClick += onPointerClickAction;
    }
    public void RemovePointerClick(Action<CombatUnit> onPointerClickAction)
    {
        onPointerClick -= onPointerClickAction; 
    }
}
