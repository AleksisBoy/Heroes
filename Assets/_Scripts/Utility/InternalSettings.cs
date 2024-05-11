using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InternalSettings : MonoBehaviour
{
    [Header("Cursors")]
    [SerializeField] private List<CursorHeroes> cursors = new List<CursorHeroes>();
    [Header("Combat Tiles")]
    [Header("Colors Single State")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color previewColor = Color.white;
    [SerializeField] private Color enemyColor = Color.white;
    [SerializeField] private Color unitTurnColor = Color.white;
    [Header("Colors Dual State")]
    [SerializeField] private Color activePreviewColor = Color.white;
    [SerializeField] private Color activeSelectedColor = Color.white;
    [Header("Colors Trio State")]
    [SerializeField] private Color activeSelectedPreviewColor = Color.white;

    public static CursorHeroes CurrentCursor { get; private set; } = default;
    public static InternalSettings Get { get; private set; } = null;
    private void Awake()
    {
        if (Get == null) Get = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        SetCursorHeroes(cursors[0]);
    }
    public Color SelectedColor => selectedColor;
    public Color InactiveColor => inactiveColor;
    public Color ActiveColor => activeColor;
    public Color PreviewColor => previewColor;
    public Color EnemyColor => enemyColor;
    public Color UnitTurnColor => unitTurnColor;
    public Color ActivePreviewColor => activePreviewColor;
    public Color ActiveSelectedColor => activeSelectedColor;
    public Color ActiveSelectedPreviewColor => activeSelectedPreviewColor;

    [Serializable]
    public struct CursorHeroes
    {
        public CursorState state;
        public Texture2D texture;
        public Vector2 hotspot;
    }

    public static void SwitchCursorTo(CursorState state)
    {
        if(CurrentCursor.state == state) return;

        SetCursorHeroes(Get.cursors.FirstOrDefault(x => x.state == state));
    }
    private static void SetCursorHeroes(CursorHeroes newCursor)
    {
        CurrentCursor = newCursor;
        Cursor.SetCursor(CurrentCursor.texture, CurrentCursor.hotspot, CursorMode.Auto);
    }
    public enum CursorState
    {
        Select,
        Blocked,
        Busy,
        Pick,
        AttackingLeft,
        AttackingBottomLeft,
        AttackingBottom,
        AttackingBottomRight,
        AttackingRight,
        AttackingTopRight,
        AttackingTop,
        AttackingTopLeft,
    }
}
