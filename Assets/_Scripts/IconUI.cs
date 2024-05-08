using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class IconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image = null;
    [SerializeField] private TMP_Text countText = null;

    public IconContainerUI container;
    private bool draggable = false;

    private IconData data;
    private Action<IconContainerUI> onEndDrag = null;
    private Action onDrag = null;
    private Action onBeginDrag = null;
    public IconData Data => data;
    public static IconUI Current { get; private set; }
    public RectTransform RT { get { return (RectTransform)transform; } }
    public void Set(string type, string name, int order, Sprite iconSprite, int count, IconContainerUI container, bool draggable)
    {
        data = new IconData(type, name, order);
        image.sprite = iconSprite;
        UpdateCount(count);
        this.container = container;
        container.Assign(this);
        this.draggable = draggable;
    }
    public void Set(CombatUnit combatUnit, IconContainerUI container, bool draggable)
    {
        data = new IconData(typeof(Unit).ToString(), combatUnit.Container.Data.name, 0);
        image.sprite = combatUnit.Container.Data.Sprite;
        UpdateCount(combatUnit.Container.Count);
        this.container = container;
        container.Assign(this);
        this.draggable = draggable;
    }
    private void UpdateCount(int count)
    {
        data.Count = count;
        if (count == 1)
        {
            countText.text = string.Empty;
        }
        else
        {
            countText.text = count.ToString();
        }
    }
    public void AddOnBeginDrag(Action onBeginAction)
    {
        onBeginDrag += onBeginAction;
    }
    public void AddOnDrag(Action onDragAction)
    {
        onDrag += onDragAction;
    }
    public void AddOnEndDrag(Action<IconContainerUI> onEndDragAction)
    {
        onEndDrag += onEndDragAction;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        Current = this;
        transform.SetParent(transform.GetComponentInParent<Canvas>().transform, true);
        transform.SetAsLastSibling();
        if (onBeginDrag != null) onBeginDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)RT.parent, eventData.position, eventData.pressEventCamera, out Vector2 mousePos);
        RT.anchoredPosition = mousePos;
        if (onDrag != null) onDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        RT.anchoredPosition = Vector2.zero;
        transform.SetParent(container.transform, false);
        RT.offsetMin = new Vector2(0, 0);
        RT.offsetMax = new Vector2(0, 0);
        transform.localScale = Vector3.one;
        EnableImage(true);
        if (onEndDrag != null) onEndDrag(container);
        Current = null;
        
    }
    public void Merge(IconUI iconToMerge)
    {
        UpdateCount(data.Count + iconToMerge.data.Count);
        Destroy(iconToMerge.gameObject);
    }
    public void EnableImage(bool state)
    {
        image.enabled = state;
        countText.enabled = state;
    }
}
public struct IconData
{
    public string Type;
    public string Name;
    public int Count;
    public int Order;
   
    public IconData(string type, string name, int count, int order)
    {
        this.Type = type;
        this.Name = name;
        this.Count = count;
        this.Order = order;
    }
    public IconData(string type, string name, int order)
    {
        this.Type = type;
        this.Name = name;
        this.Count = 0;
        this.Order = order;
    }
    public static bool operator ==(IconData x, IconData y)
    {
        if(x.Type != y.Type) return false;
        if(x.Name != y.Name) return false;
        //if(x.Count != y.Count) return false; // think of maybe not including count
        return true;
    }
    public static bool operator !=(IconData x, IconData y)
    {
        return !(x == y);
    }

    // GPT
    // Override Equals method to define equality logic
    public override bool Equals(object obj)
    {
        if (!(obj is IconData))
            return false;

        IconData other = (IconData)obj;
        return Type == other.Type && Name == other.Name && Count == other.Count;
    }

    // Override GetHashCode method to provide a hash code implementation
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Type?.GetHashCode() ?? 0;
            hash = hash * 23 + Name?.GetHashCode() ?? 0;
            hash = hash * 23 + Count.GetHashCode();
            return hash;
        }
    }
}
