using System.Collections;
using UnityEngine;

public class MapNavigator : ManageableBehaviour
{
    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private Map map = null;
    [SerializeField] private Terrain terrain = null;
    [SerializeField] private Transform targetView = null;
    [SerializeField] private float zoomSensitivity = 10f;
    [SerializeField] private float rotationSensitivity = 1.5f;
    [SerializeField] private float moveSensitivity = 1f;
    [SerializeField] private float borderMoveSpeed = 25f;
    [SerializeField] private Vector2 fovRange = Vector2.zero;
    [SerializeField] private Vector2 rotationRange = Vector2.zero;

    private HeroMount selectedHeroMount = null;
    [SerializeField] private Transform targetPoint = null;
    [SerializeField] private GameObject pointObject = null;
    private Point lastClickedPoint = null;
    private City lastCityClicked = null;
    private Path path = null;

    private bool moving = false;
    private bool rotating = false;
    private Vector3 lastTargetViewPos = Vector3.zero;
    private Vector3 desiredTargetViewPos = Vector3.zero;

    public HeroMount SelectedHeroMount { get { return selectedHeroMount; }}
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        mainCamera.transform.LookAt(targetView);
        lastTargetViewPos = targetView.position;
        desiredTargetViewPos = targetView.position;
    }
    private void LateUpdate()
    {
        HandleClick();
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        CameraMoveUpdate(mouseX, mouseY);
        RotationUpdate(mouseX, mouseY);
        ZoomUpdate();
        BorderMoveUpdate();
        TargetViewPositionUpdate();
    }
    private void CameraMoveUpdate(float mouseX, float mouseY)
    {
        if (Input.GetMouseButtonDown(2))
        {
            moving = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetMouseButton(2))
        {
            //chatgpt blessing that fixed movement
            // Calculate the movement in local space
            Vector3 localMovement = new Vector3(-mouseX, 0, -mouseY) * moveSensitivity;

            // Transform the movement vector to world space
            Vector3 worldMovement = targetView.TransformDirection(localMovement);

            // Apply the movement to the position
            desiredTargetViewPos += worldMovement;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            moving = false;
            Cursor.visible = true;
            if(!rotating) Cursor.lockState = CursorLockMode.Confined;
        }
    }
    private void RotationUpdate(float mouseX, float mouseY)
    {
        if (Input.GetMouseButtonDown(1))
        {
            rotating = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 euler = targetView.eulerAngles;
            float predictedX = euler.x + (mouseY * rotationSensitivity);
            if (predictedX < rotationRange.x || predictedX > rotationRange.y)
            {
                predictedX = euler.x;
            }
            targetView.rotation = Quaternion.Euler(predictedX, euler.y + (mouseX * rotationSensitivity), 0f);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            rotating = false;
            Cursor.visible = true;
            if(!moving) Cursor.lockState = CursorLockMode.Confined;
        }
    }
    private void ZoomUpdate()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        float predictedFOV = mainCamera.fieldOfView;
        predictedFOV += scroll > 0 ? -zoomSensitivity : zoomSensitivity;

        if (predictedFOV >= fovRange.x && predictedFOV <= fovRange.y)
        {
            mainCamera.fieldOfView = predictedFOV;
        }
    }
    private void BorderMoveUpdate()
    {
        if (Input.mousePosition.x + 1 >= Screen.width)
        {
            desiredTargetViewPos += targetView.TransformDirection(Vector3.right * borderMoveSpeed * Time.deltaTime);
        }
        else if (Input.mousePosition.x - 1 <= 0)
        {
            desiredTargetViewPos += targetView.TransformDirection(-Vector3.right * borderMoveSpeed * Time.deltaTime);
        }
        if (Input.mousePosition.y + 1 >= Screen.height)
        {
            desiredTargetViewPos += targetView.TransformDirection(Vector3.forward * borderMoveSpeed * Time.deltaTime);
        }
        else if (Input.mousePosition.y - 1 <= 0)
        {
            desiredTargetViewPos += targetView.TransformDirection(-Vector3.forward * borderMoveSpeed * Time.deltaTime);
        }
    }
    private void TargetViewPositionUpdate()
    {
        if (desiredTargetViewPos == lastTargetViewPos) return;

        TargetViewOutOfBoundsCheck();
        targetView.position = new Vector3(desiredTargetViewPos.x, terrain.SampleHeight(desiredTargetViewPos) + terrain.transform.position.y, desiredTargetViewPos.z);
        lastTargetViewPos = targetView.position;
    }

    private void TargetViewOutOfBoundsCheck()
    {
        if (desiredTargetViewPos.x < terrain.terrainData.bounds.min.x)
        {
            desiredTargetViewPos.x = terrain.terrainData.bounds.min.x;
        }
        else if (desiredTargetViewPos.x > terrain.terrainData.bounds.max.x)
        {
            desiredTargetViewPos.x = terrain.terrainData.bounds.max.x;
        }
        if (desiredTargetViewPos.z < terrain.terrainData.bounds.min.z)
        {
            desiredTargetViewPos.z = terrain.terrainData.bounds.min.z;
        }
        else if (desiredTargetViewPos.z > terrain.terrainData.bounds.max.z)
        {
            desiredTargetViewPos.z = terrain.terrainData.bounds.max.z;
        }
    }

    private void HandleClick()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
        if (HeroesInputModule.Module.GetFocused()) return;
        if (!selectedHeroMount) return;

        if (selectedHeroMount.IsMoving)
        {
            selectedHeroMount.StopMoving();
            return;
        }

        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        if (hit.transform == null) return;

        Point selectedPoint;

        //City city = hit.transform.GetComponent<City>();
        //if (city)
        //{
        //    if(city == lastCityClicked)
        //    {
        //        city.en
        //    }
        //}
        //HeroMount heroMount = hit.transform.GetComponentInParent<HeroMount>();
        //if (heroMount)
        //{
        //    Debug.Log(heroMount.name);
        //    selectedPoint = heroMount.CurrentLocation;
        //    return;
        //}

        MapObject mapObject = hit.transform.GetComponent<MapObject>();
        if (mapObject)
        {
            selectedPoint = mapObject.CurrentLocation;
        }
        else
        {
            // find closest point to mouse
            Vector3 mousePos = new Vector3(hit.point.x, terrain.SampleHeight(hit.point), hit.point.z);
            selectedPoint = map.GetClosestPointInChunk(mousePos, GetChunk(mousePos));
        }



        // Move hero on path

        if (lastClickedPoint == selectedPoint)
        {
            if (path != null)
            {
                selectedHeroMount.MoveTo(path);
                path = null;
                selectedPoint = null;
            }
        }
        // create new path if different point selected
        else
        {
            if (path != null) path = path.DestroySelf();
            path = map.CalculatePath(selectedHeroMount.CurrentLocation, selectedPoint);

            targetPoint.position = path.EndPoint.Position + new Vector3(0, 0.01f, 0f);
            path.PlacePathMarkers(selectedHeroMount.Movement, pointObject);
        }
        lastClickedPoint = selectedPoint;
    }

    private int GetChunk(Vector3 mousePos)
    {
        int countX = 0;
        float num0 = mousePos.x;
        while (num0 > map.chunkSize - 0.5f * map.pointScale)
        {
            countX++;
            num0 -= map.chunkSize;
        }

        int countZ = 0;
        float num1 = mousePos.z;
        while (num1 > map.chunkSize - 0.5f * map.pointScale)
        {
            countZ++;
            num1 -= map.chunkSize;
        }

        int chunk = countX + countZ * map.chunkOneDimension;
        return chunk;
    }
    public void SelectHero(HeroMount hero)
    {
        selectedHeroMount?.EnableSelectionVisual(false);
        selectedHeroMount = hero;
        selectedHeroMount.EnableSelectionVisual(true);
        if(path != null) path = path.DestroySelf();
        desiredTargetViewPos = hero.CurrentLocation.Position;
    }
    public override IEnumerator DayStarted(int day)
    {
        yield return null;
        //selectedHeroMount.UpdatePath(pointObject);
    }
}
