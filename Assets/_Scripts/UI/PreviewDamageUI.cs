using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PreviewDamageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText = null;
    [SerializeField] private TMP_Text retaliateText = null;
    [SerializeField] private float bottomOffset = -100f;
    [SerializeField] private float topOffset = 100f;

    private RectTransform rt;
    private void Awake()
    {
        rt = (RectTransform)transform;
    }
    public void Set(Vector2Int damage, bool retaliate)
    {
        // add colors too
        gameObject.SetActive(true);
        damageText.text = string.Format("Damage: {0}-{1}", damage.x, damage.y);
        retaliateText.text = string.Format("Retaliate: {0}", retaliate ? "Yes" : "No");
    }
    private void Update()
    {
        float offset = 0;
        if (InternalSettings.CurrentCursor.state.ToString().Contains("Top"))
        {
            offset = topOffset;
        }
        else if (InternalSettings.CurrentCursor.state.ToString().Contains("Bottom"))
        {
            offset = bottomOffset;
        }
        else
        {
            offset = bottomOffset / 2f;
        }
        rt.anchoredPosition = (Vector2)Input.mousePosition + new Vector2(0f, offset);
    }
}
