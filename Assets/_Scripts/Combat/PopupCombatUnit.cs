using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupCombatUnit : MonoBehaviour
{
    [SerializeField] private float horizontalPixelOffset = 200f;
    [SerializeField] private float verticalBorderPixelOffset = 50f;
    [Header("Attributes")]
    [SerializeField] private TMP_Text unitNameText = null;
    [SerializeField] private TMP_Text attackValueText = null;
    [SerializeField] private TMP_Text defenseValueText = null;
    [SerializeField] private TMP_Text damageValueText = null;
    [SerializeField] private TMP_Text hpValueText = null;
    [SerializeField] private TMP_Text speedValueText = null;
    [SerializeField] private TMP_Text initiativeValueText = null;

    private CombatUnit unit = null;
    public void Set(CombatUnit unit)
    {
        this.unit = unit;
        UpdatePosition();
        unitNameText.text = unit.Container.Data.name;
        SetAttributesUI(unit);
        gameObject.SetActive(true);
    }
    private void SetAttributesUI(CombatUnit unit) // then would need to change to either unitcontainer or combatunit to find updated info
    {
        // would need to compare to default stats of units to color if its different good or bad way
        attackValueText.text = unit.Container.Data.Attack.ToString();
        defenseValueText.text = unit.Container.Data.Defense.ToString();
        damageValueText.text = string.Format("{0} - {1}", unit.Container.Data.DamageRange.x, unit.Container.Data.DamageRange.y);
        hpValueText.text = string.Format("{0}/{1}", unit.HP, unit.Container.Data.HP);
        speedValueText.text = unit.Container.Data.Speed.ToString();
        initiativeValueText.text = unit.Container.Data.Initiative.ToString();
    }
    public void UpdatePosition()
    {
        PlaceToWorld(unit.transform.position);
    }
    private void PlaceToWorld(Vector3 worldPos)
    {
        RectTransform rt = (RectTransform)transform;

        // make horizontal offset depending on where is mouse left of right side of screen
        Vector3 offsetPixel;
        if (Input.mousePosition.x / Screen.width > 0.5f)
        {
            offsetPixel = new Vector3(-horizontalPixelOffset, 0f, 0f);
        }
        else
        {
            offsetPixel = new Vector3(horizontalPixelOffset, 0f, 0f);
        }

        Vector3 targetPosition = Camera.main.WorldToScreenPoint(worldPos) + offsetPixel;

        // do vertical offset if ui is over screen on top
        float diff = (targetPosition.y + (rt.sizeDelta.y / 2f)) - Screen.height;
        if (diff > -verticalBorderPixelOffset) targetPosition.y += -diff - verticalBorderPixelOffset;

        // do vertical offset if ui is over screen on bottom
        diff = targetPosition.y - (rt.sizeDelta.y / 2f);
        if (diff < verticalBorderPixelOffset) targetPosition.y += Mathf.Abs(diff) + verticalBorderPixelOffset;

        rt.anchoredPosition = targetPosition;
    }
}
