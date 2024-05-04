using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatPreparation : MonoBehaviour
{
    [SerializeField] private CanvasUnitUtility canvasUnitUtility = null;
    [SerializeField] private UnitBar unitBar = null;
    [SerializeField] private float rotationSpeed = 20f;

    protected bool playerReady = false;

    private bool isRotating = false;
    protected CombatManager manager; 
    protected CombatMap map = null;
    public virtual void StartPreparation(CombatManager manager, HeroMount hero, CombatMap map, bool attacker)
    {
        this.manager = manager;
        this.map = map;
        playerReady = false;
        canvasUnitUtility.SetPlayer(hero.Player);

        gameObject.SetActive(true);

        unitBar.Setup(hero, map.ActivateColomns(map.GetPreparationColomns(attacker)), map, attacker);
    }
    private IEnumerator RotateCamera(float angle)
    {
        isRotating = true;
        Transform camera = Camera.main.transform;
        float moved = 0f;
        float sign = Mathf.Sign(angle);
        while (moved < Mathf.Abs(angle))
        {
            float temp = Time.deltaTime * sign * rotationSpeed;
            moved += temp;
            camera.RotateAround(map.MiddlePosition, Vector3.up, temp);
            yield return null;
        }
        isRotating = false;
    }
    public void Button_PlayerReady()
    {
        if (playerReady || isRotating) return;

        playerReady = true;
        manager.PlayerReady();
    }
}
