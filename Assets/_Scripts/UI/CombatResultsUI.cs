using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatResultsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resultsLogsText = null;
    [SerializeField] private RectTransform resultsContentScrollView = null;

    [SerializeField] private TMP_Text resultText = null;
    [SerializeField] private string winnerString = "Victory!";
    [SerializeField] private string loserString = "Defeat...";

    [SerializeField] private Image resultImage = null;
    [SerializeField] private Sprite winnerSprite = null;
    [SerializeField] private Sprite loserSprite = null;

    [SerializeField] private UnitBar casualtiesBar = null;
    [SerializeField] private UnitBar killedUnitsBar = null;

    public void Setup(string resultsLog, bool winner, List<UnitContainer> myCasulties, List<UnitContainer> opponentCasualties)
    {
        resultsLogsText.text = resultsLog;
        int lineCount = resultsLogsText.text.Split('\n').Length + 1;

        float preferredHeight = resultsLogsText.fontSize * lineCount;
        resultsContentScrollView.sizeDelta = new Vector2(resultsContentScrollView.sizeDelta.x, preferredHeight);
        resultsLogsText.rectTransform.sizeDelta = new Vector2(resultsLogsText.rectTransform.sizeDelta.x, preferredHeight);
        resultsLogsText.rectTransform.anchoredPosition = new Vector2(0f, -preferredHeight / 2f);

        resultText.text = winner ? winnerString : loserString;
        resultImage.sprite = winner ? winnerSprite : loserSprite;

        casualtiesBar.Setup(myCasulties);
        killedUnitsBar.Setup(opponentCasualties);
    }
    public void Button_Continue()
    {

    }
}
