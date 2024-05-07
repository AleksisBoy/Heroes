using UnityEngine;

public class InternalSettings : MonoBehaviour
{
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
}
