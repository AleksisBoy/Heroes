using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatEnding : MonoBehaviour
{
    [SerializeField] private CombatResultsUI combatResultsUI = null;

    public virtual void StartEnding(bool winner, List<UnitContainer> myCasualties, List<UnitContainer> opponentCasualties)
    {
        gameObject.SetActive(true);
        combatResultsUI.Setup(winner ? "Enemy was eliminated.\n\nGained x exp.p." : "Battle was lost.\n\nGained x exp.p.", winner, myCasualties, opponentCasualties);
    }
}
