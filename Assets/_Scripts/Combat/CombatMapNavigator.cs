using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMapNavigator : MonoBehaviour
{
    [SerializeField] private CombatMap map = null;

    private void Update()
    {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);

        //map.GetTileInPosition(hitInfo.point).Selected();
    }
}
