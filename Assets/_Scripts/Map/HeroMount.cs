using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroMount : MapObject
{
    [SerializeField] private Player playerDebug = null;

    [SerializeField] private Hero hero = null;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private GameObject selectionCircle = null;
    public bool ai = false;

    private Player player;
    private UnitContainer[] units = new UnitContainer[7];
    private float movement = 0;
    private bool isMoving = false;
    private Point nextPoint = null;
    private Coroutine movingCoroutine = null;
    private Path currentPath = null;
    public bool IsMoving { get { return isMoving; } }   
    public float Movement { get { return movement; } }
    public Player Player { get { return player; } }
    public Hero Hero { get { return hero; } }
    public UnitContainer[] Units { get { return units; } }
    private void Start()
    {
        EnableSelectionVisual(false);
    }
    public override void Setup(Point startPoint)
    {
        hero = Hero.CreateInstanceOf(hero);
        currentLocation = startPoint;
        transform.position = startPoint.Position;
        SpawnStarterUnit();
    }
    public void Setup(Point startPoint, Player player)
    {
        this.player = player;
        Setup(startPoint);
    }
    public void SpawnStarterUnit() // debug
    {
        UnitContainer unit = new UnitContainer(hero.starterUnit, hero.starterUnitCount, playerDebug);
        units[0] = unit;
        UnitContainer unit1 = new UnitContainer(hero.starterUnit, hero.starterUnitCount + 5, playerDebug);
        units[1] = unit1;
        UnitContainer unit2 = new UnitContainer(hero.starterUnit, hero.starterUnitCount + 10, playerDebug);
        units[2] = unit2;
        UnitContainer unit3 = new UnitContainer(hero.starterUnit, hero.starterUnitCount + 15, playerDebug);
        units[3] = unit3;
    }
    private void HeroDailySetup()
    {
        movement = hero.MovementPerDay;
    }
    public void MoveTo(Path path)
    {
        currentPath = path;
        movingCoroutine = StartCoroutine(MoveOnCurrentPathAnimated());
    }
    public IEnumerator MoveOnCurrentPathAnimated()
    {
        if(currentPath == null) yield break;

        currentLocation.occupied = false;
        isMoving = true;
        int index = currentPath.PathPoints.Count - 1;
        while(currentLocation != currentPath.EndPoint)
        {
            GameObject marker = currentPath.pathMarkers[0];
            currentPath.pathMarkers.Remove(marker);
            Destroy(marker);

            index--;
            var pathPoint = currentPath.PathPoints.ElementAt(index);
            nextPoint = pathPoint.Key;

            float predictedMovement = movement - Map.CalculateDistanceCost(currentLocation, nextPoint);
            if (predictedMovement < 0)
            {
                break;
            }
            movement = predictedMovement;

            float elapsedTime = 0f;
            Vector3 startPos = transform.position;
            float totalTime = Vector3.Distance(startPos, nextPoint.Position) / moveSpeed;
            while(elapsedTime < totalTime)
            {
                float t = elapsedTime / totalTime;
                transform.position = Vector3.Lerp(startPos, nextPoint.Position, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            currentLocation = nextPoint;
        }
        transform.position = currentLocation.Position;
        currentLocation.occupied = true;
        currentPath.DestroySelf();
        isMoving = false;
        movingCoroutine = null;

        InteractWithCurrentPoint();
    }
    public void StopMoving()
    {
        if (movingCoroutine == null) return;

        currentLocation = nextPoint;
        currentLocation.occupied = true;
        transform.position = currentLocation.Position;
        currentPath.DestroySelf();
        nextPoint = null;
        StopCoroutine(movingCoroutine);
        isMoving = false;
        InteractWithCurrentPoint();
    }
    private void InteractWithCurrentPoint()
    {
        currentLocation.OnInteract(this);
    }

    public override IEnumerator DayStarted(int day)
    {
        Debug.Log("Day " + day + " started for " + name);
        HeroDailySetup();
        yield break;
    }
    public void UpdatePath(GameObject pointPrefab)
    {
        if (currentPath == null) return;

        currentPath.PlacePathMarkers(movement, pointPrefab);
    }
    public void EnableSelectionVisual(bool state)
    {
        selectionCircle.SetActive(state);
    }
    public UnitContainer GetUnitContainerFromIconData(IconData data)
    {
        if (data.Type != "Unit") return null;

        for(int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;

            if (units[i].Data.name != data.Name) continue;
            if (units[i].Count != data.Count) continue;
            if (i != data.Order) continue;
            return units[i];
        }
        return null;
    }
}
