using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitCountUI : MonoBehaviour
{
    [SerializeField] private Image background = null;
    [SerializeField] private TMP_Text countText = null;

    private float tileScale;
    private CombatUnit unit = null;
    public CombatUnit Unit => unit;
    public void SetCount(int count)
    {
        countText.text = count.ToString(); 
    }
    public void SetUI(CombatUnit unit, float tileScale, Color color)
    {
        this.unit = unit;
        this.tileScale = tileScale;
        background.color = color;
        UpdateCount();
        UpdatePosition();
    }
    public void UpdateCount()
    {
        SetCount(unit.Container.Count);
    }
    public void UpdatePosition()
    {
        Vector3 offset;
        if (unit.Attacker)
        {
            offset = new Vector3(-tileScale / 4f, 0f, -tileScale / 4f);
        }
        else
        {
            offset = new Vector3(tileScale / 4f, 0f, -tileScale / 4f);
        }
        PlaceToWorld(unit.transform.position + offset);
    }
    private void PlaceToWorld(Vector3 worldPos)
    {
        RectTransform rt = (RectTransform)transform;

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
        rt.anchoredPosition = Camera.main.WorldToScreenPoint(worldPos);
    }
}
