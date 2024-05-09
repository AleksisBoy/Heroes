using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconContainerUI : MonoBehaviour
{
    [SerializeField] private Image background = null;

    private IconUI icon = null;

    public IconUI Icon { get { return icon; } }

    public RectTransform RT { get { return (RectTransform)transform; } }
    public Image Background => background;

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
