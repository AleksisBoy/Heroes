using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconContainerUI : MonoBehaviour
{
    private IconUI icon = null;

    public IconUI Icon { get { return icon; } }

    public RectTransform RT { get { return (RectTransform)transform; } }

    public IconUI TakeOut()
    {
        IconUI icon = this.icon;
        this.icon = null;
        return icon;
    }
    public void Assign(IconUI icon)
    {
        if (icon == null) return;

        this.icon = icon;
        this.icon.transform.SetParent(transform, false);
        this.icon.container = this;
    }
}
