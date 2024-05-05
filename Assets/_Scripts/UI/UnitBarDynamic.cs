using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBarDynamic : UnitBar
{
    public override void Setup(HeroMount mount)
    {
        base.Setup(mount);
        foreach(IconUI iconUnit in unitIcons)
        {
            iconUnit.AddOnBeginDrag(UnitIconBeginDrag);
            iconUnit.AddOnDrag(UnitIconDrag);
            iconUnit.AddOnEndDrag(UnitIconEndDrag);
        }
    }

    protected virtual void UnitIconBeginDrag()
    {

    }
    protected virtual void UnitIconDrag()
    {

    }
    protected virtual void UnitIconEndDrag(IconContainerUI container)
    {

    }
}
