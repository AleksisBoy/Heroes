using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : Building
{
    [SerializeField] private Player playerStart = null;
    [SerializeField] private CityCanvas cityCanvas = null;

    private bool builtThisDay = false;

    private HeroMount heroInCity = null;
    private HeroMount heroAtGate = null;

    public HeroMount HeroInCity { get { return heroInCity; } }
    public HeroMount HeroAtGate { get { return heroAtGate; } }
    private void Start()
    {
        if (playerStart != null)
        {
            playerControlled = playerStart;
            GetComponent<MeshRenderer>().material.color = playerControlled.PlayerColor;
        }
    }
    public override void Interact(HeroMount hero)
    {
        base.Interact(hero);
        heroAtGate = hero;
        OpenCityMenu();
    }
    public void OpenCityMenu()
    {
        cityCanvas.Enable(this);
    }
    public override IEnumerator DayStarted(int day)
    {
        yield return base.DayStarted(day);

        builtThisDay = false;
    }
}
