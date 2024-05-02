using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroIconUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image heroImage = null;
    [SerializeField] private Image backgroundSelectionImage = null;

    public static HeroIconUI Selected { get; private set; }
    public Action<HeroMount> uiClicked;
    private HeroMount mount = null;
    public void Set(HeroMount mount, bool isSelected, Action<HeroMount> onClick, Vector2 heroImageDimensions)
    {
        this.mount = mount;
        heroImage.sprite = mount.Hero.Sprite;
        uiClicked += onClick;
        HighlightSelection(isSelected);
        heroImage.rectTransform.sizeDelta = heroImageDimensions;
    }
    public void HighlightSelection(bool isSelected)
    {
        if(Selected && Selected != this) Selected.HighlightSelection(false);
        Selected = this;
        backgroundSelectionImage.gameObject.SetActive(isSelected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HighlightSelection(true);
        if(uiClicked != null) uiClicked(mount);
    }
    private void OnDestroy()
    {
        if (Selected == this) Selected = null;
    }
}
