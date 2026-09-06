using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem; 

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    private GameObject ghostObject;
    private PatrolIdentity prevPatrolPoint = null;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    public bool placingMode = false;
    public bool onPlane = false;
    public bool deleteMode = false;
    public bool moveMode = false;
    [SerializeField] private GameObject hoveredObject;
    [SerializeField] private GameObject buildingsCanvas;
    public InputActionReference objRotAction;
    void OnEnable() => objRotAction.action.Enable();
    void OnDisable() => objRotAction.action.Disable();
    public BuildingRegistry buildingRegistry;
    private Dictionary<Renderer, Color> OGColors = new Dictionary<Renderer, Color>();
    private Dictionary<string, PatrolIdentity> PatrolPointDic = new Dictionary<string, PatrolIdentity>();
    private string routeValue = "A";
    private int pointValue = 0;

    [SerializeField] private TextMeshProUGUI messageText;
    private Coroutine activeMessage;

    [SerializeField] private SceneHandler sceneHandler;
    [SerializeField] private LayerMask groundLayer;


    public void Start()
    {
        if (GameManager.instance.returning == true)
        {
            loadFromTemp();
            GameManager.instance.returning = false;
        }

        routeValue = CheckRouteValue();
        print(routeValue);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetObjectToPlace(GameObject objToPlace)
    {
        objectToPlace = objToPlace;
    }

    public void setPlaceMode()
    {
        placingMode = !placingMode;
        buildingsCanvas.SetActive(!buildingsCanvas.activeSelf);
    }

    Vector3Int WorldToCell(Vector3 world) => new Vector3Int(
        Mathf.RoundToInt(world.x / gridSize),
        Mathf.RoundToInt(world.y / gridSize),
        Mathf.RoundToInt(world.z / gridSize)
        );

    Vector3 CellToWorld(Vector3 cell) => new Vector3(
        cell.x * gridSize,
        cell.y * gridSize,
        cell.z * gridSize
        );


    List<BuildingPiece> GetObjectSize(GameObject objToPlace)
    {
        BuildingIdentity objInfo = objToPlace.GetComponent<BuildingIdentity>();

        List<BuildingPiece> locInfo = objInfo.locInfo;
       
        return locInfo;
    }

    int GetObjectRotation(GameObject objToPlace)
    {
        BuildingIdentity objInfo = objToPlace.GetComponent<BuildingIdentity>();
        int rotation = objInfo.rotation;
        return rotation;
    }

    IEnumerable<Vector3> GetCells(Vector3 origin, List<BuildingPiece> objInfoList, int rotation)
    {
        List<Vector3> Testcells = new List<Vector3>();
        Vector3 localOrigin = new Vector3(0f, 0f, 0f);


        foreach (BuildingPiece objInfo in objInfoList)
        {
            for (int x = 0; x < objInfo.size.x; x++)
            {
                for (int z = 0; z < objInfo.size.z; z++)
                {
                    Testcells.Add(new Vector3(localOrigin.x + objInfo.offset.x + x, localOrigin.y, localOrigin.z + objInfo.offset.z + z));
                } 
            }
        }
        List<Vector3> cells = new List<Vector3>();

        foreach (Vector3 cell in Testcells)
        {
            if (rotation == 0)
            {
                Vector3 worldCell = origin + cell;
                cells.Add(worldCell);
            }
            else if (rotation == 1)
            {
                Vector3 rotCell = new Vector3(cell.z, cell.y, -cell.x);
                Vector3 worldCell = origin + rotCell;
                cells.Add(worldCell);
            }
            else if (rotation == 2)
            {
                Vector3 rotCell = new Vector3(-cell.x, cell.y, -cell.z);
                Vector3 worldCell = origin + rotCell;
                cells.Add(worldCell);
            }
            else if (rotation == 3)
            {
                Vector3 rotCell = new Vector3(-cell.z, cell.y, cell.x);
                Vector3 worldCell = origin + rotCell;
                cells.Add(worldCell);
            }
        }

       return cells;
    }

    bool AreCellsFree(Vector3 origin, List<BuildingPiece> objInfoList, int rotation)
    {
        foreach (Vector3 c in GetCells(origin, objInfoList, rotation))
            if (occupiedPositions.Contains(c))
                return false;
        return true;
    }

    


    void Update()
    {   if (placingMode == true)
        {
            if (!ghostObject)
            {
                CreateGhostObject();
            }
            if (Mouse.current.leftButton.wasPressedThisFrame && onPlane)
                PlaceObject();
            else if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                setPlaceMode();
                StopGhostPosition();
            }
            else
            { 
                UpdateGhostPosition(); 
            }

        }
        else if (deleteMode)
        {
            if (ghostObject)
            {
                Destroy(ghostObject);
                ghostObject = null;
            }
            highlightHover(Color.red);
            if (Mouse.current.leftButton.wasPressedThisFrame && hoveredObject)
            {
                RemoveObject();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                deleteMode = false;
                buildingsCanvas.SetActive(!buildingsCanvas.activeSelf);
            }
        }
        else
        {   if (!moveMode)
            {
                highlightHover(new Color(1f, 0.9f, 0.4f));
            }
            if (Mouse.current.leftButton.wasPressedThisFrame && hoveredObject)
            {
                buildingsCanvas.SetActive(true);
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer) && (hit.transform.CompareTag("Ground") || !moveMode))
                {
                    moveMode = !moveMode;
                    Collider[] allColliders = hoveredObject.GetComponentsInChildren<Collider>();

                    foreach (Collider collider in allColliders)
                    {
                        collider.enabled = !collider.enabled;
                    }

                    List<BuildingPiece> size = GetObjectSize(hoveredObject);
                    int rotation = GetObjectRotation(hoveredObject);

                    foreach (Vector3 c in GetCells(hoveredObject.transform.position, size,rotation))
                    {
                        if (moveMode) occupiedPositions.Remove(c);
                        else occupiedPositions.Add(c);
                    }
                }
                
            }

            if (moveMode && hoveredObject)
            {
                buildingsCanvas.SetActive(false);
                UpdateHoveredObjPos();
            }
        }
    }

    

    void CreateGhostObject()
    {
        ghostObject = Instantiate(objectToPlace);

        var ghostPatrol = ghostObject.GetComponent<PatrolIdentity>();
        if (ghostPatrol != null)
        {
            ghostPatrol.previousPoint = null;
            ghostPatrol.nextPoint = null;
        }

        foreach (Collider child in ghostObject.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }
        if (ghostObject.GetComponent<Collider>() != null)
            ghostObject.GetComponent<Collider>().enabled = false;

        Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();

        foreach(Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = 0.5f;
            mat.color = color;

            mat.SetFloat("_Surface", 1);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        foreach (MeshRenderer child in ghostObject.GetComponentsInChildren<MeshRenderer>())
        {
            child.enabled = true;
        }
        if (ghostObject.GetComponent<MeshRenderer>() != null)
            ghostObject.GetComponent<MeshRenderer>().enabled = true;

        
    }

    void UpdateGhostPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (MeshRenderer child in ghostObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = true;
            }
            if (ghostObject.GetComponent<MeshRenderer>() != null)
                ghostObject.GetComponent<MeshRenderer>().enabled = true;
            onPlane = true;

            ghostObject.transform.position = WorldToCell(hit.point);

            

            if (objRotAction.action.WasPressedThisFrame())
            {
                ghostObject.transform.rotation *= Quaternion.Euler(0, 90, 0);
                List<BuildingPiece> pieces = ghostObject.GetComponent<BuildingIdentity>().locInfo;
                int rotation = ghostObject.GetComponent<BuildingIdentity>().rotation;
                rotation = (rotation + 1) % 4;
                ghostObject.GetComponent<BuildingIdentity>().rotation = rotation;

            }

            if (AreCellsFree(WorldToCell(hit.point), GetObjectSize(ghostObject), GetObjectRotation(ghostObject)))
                SetColor(new Color(1f, 1f, 1f, 0.5f));
            else
                SetColor(Color.red);

            if (!hit.transform.CompareTag("Ground"))
                SetColor(Color.red);
        }
        else
        {
            onPlane = false;
            foreach (MeshRenderer child in ghostObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = false;
            }
            if (ghostObject.GetComponent<MeshRenderer>() != null)
                ghostObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void UpdateHoveredObjPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            foreach (MeshRenderer child in hoveredObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = true;
            }
            if (hoveredObject.GetComponent<MeshRenderer>() != null)
                hoveredObject.GetComponent<MeshRenderer>().enabled = true;
            onPlane = true;

            hoveredObject.transform.position = WorldToCell(hit.point);


            if (AreCellsFree(WorldToCell(hit.point), GetObjectSize(hoveredObject), GetObjectRotation(hoveredObject)))
                SetColor(Color.white);
            else 
                SetColor(Color.red);

            if (!hit.transform.CompareTag("Ground"))
                SetColor(Color.red);


            if (objRotAction.action.WasPressedThisFrame())
            {
                hoveredObject.transform.rotation *= Quaternion.Euler(0, 90, 0);
                List<BuildingPiece> pieces = hoveredObject.GetComponent<BuildingIdentity>().locInfo;
                int rotation = hoveredObject.GetComponent<BuildingIdentity>().rotation;
                rotation = (rotation + 1) % 4;
                for (int i = 0; i < pieces.Count; i++)
                {
                    BuildingPiece buildingpiece = pieces[i];
                    float tempX = buildingpiece.size.x;
                    float tempZ = buildingpiece.size.z;

                    buildingpiece.size.x = tempZ;
                    buildingpiece.size.z = -tempX;



                }
            }
        }
        else
        {
            onPlane = false;
            foreach (MeshRenderer child in hoveredObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = false;
            }
            if (hoveredObject.GetComponent<MeshRenderer>() != null)
                hoveredObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void StopGhostPosition()
    {
        Destroy(ghostObject);
        ghostObject = null;
        prevPatrolPoint = null;
        routeValue = ((char)(routeValue[0] + 1)).ToString();
        pointValue = 0;
    }

    public void highlightHover(Color color)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.root.CompareTag("Building"))
        {
            GameObject root = hit.transform.root.gameObject;
           
            if (hoveredObject != root)
            {
                if (hoveredObject) restoreColors();

                hoveredObject = root;
                cacheColors();        
                SetColor(color);
            }
            
        }
        else if (hoveredObject)
        {
            restoreColors();
            hoveredObject = null;
        }

    }

    void cacheColors()
    {
        OGColors.Clear();
        foreach (Renderer r in hoveredObject.GetComponentsInChildren<Renderer>())
            OGColors[r] = r.material.color;
    }

    void restoreColors()
    {
        foreach (var KeyValue in OGColors)
        {
            if (KeyValue.Key != null)
                KeyValue.Key.material.color = KeyValue.Value;
        }
        OGColors.Clear();
    }

    void SetColor(Color color)
    {
        Renderer[] renderers;
        if (placingMode)
        {
            renderers = ghostObject.GetComponentsInChildren<Renderer>();
        }
        else
        {
            renderers = hoveredObject.GetComponentsInChildren<Renderer>();
        }

        foreach (Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.color = color;
        }
    }

    void PlaceObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        GameObject placed = null;
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Ground"))
        {
            List<BuildingPiece> objInfoList = GetObjectSize(ghostObject);
            int rotation = GetObjectRotation(ghostObject);
            Vector3 origin = WorldToCell(ghostObject.transform.position);

            if (AreCellsFree(origin, objInfoList, rotation))
            {
                placed = Instantiate(objectToPlace, CellToWorld(origin), ghostObject.transform.rotation);
                BuildingIdentity ghostID = ghostObject.GetComponent<BuildingIdentity>();
                BuildingIdentity placedID = placed.GetComponent<BuildingIdentity>();
                placedID.rotation = ghostID.rotation;
                placedID.locInfo = ghostID.locInfo;
                placedID.rotation = ghostID.rotation;

                foreach (Vector3 c in GetCells(origin, objInfoList, rotation))
                {
                    occupiedPositions.Add(c);
                }
                BuildManager.instance.buildCount++;
            }

            if (placed.transform.GetComponent<PatrolIdentity>() != null)
            {
                PatrolPointDic.Add(routeValue + pointValue, placed.transform.GetComponent<PatrolIdentity>());
                placed.transform.GetComponent<PatrolIdentity>().RouteID = routeValue;
                placed.transform.GetComponent<PatrolIdentity>().PointID = pointValue.ToString();
                //print(placed.transform.GetComponent<PatrolIdentity>().RouteID + placed.transform.GetComponent<PatrolIdentity>().PointID);
                pointValue += 1;
                if (prevPatrolPoint != null)
                {
                    prevPatrolPoint.nextPoint = placed.transform.GetComponent<PatrolIdentity>().RouteID 
                        + placed.transform.GetComponent<PatrolIdentity>().PointID;
                    placed.transform.GetComponent<PatrolIdentity>().previousPoint = prevPatrolPoint.RouteID + prevPatrolPoint.PointID;
                }

                prevPatrolPoint = placed.transform.GetComponent<PatrolIdentity>();
            }
        }
  
}

    void RemoveObject()
    {
        List<BuildingPiece> size = GetObjectSize(hoveredObject);
        int rotation = GetObjectRotation(hoveredObject);

        foreach (Vector3 c in GetCells(WorldToCell(hoveredObject.transform.position), size, rotation))
            occupiedPositions.Remove(c);
        if (hoveredObject.GetComponent<PatrolIdentity>() != null)
        {
            PatrolIdentity previousPatrolIdentity = PatrolPointDic.TryGetValue(hoveredObject.GetComponent<PatrolIdentity>().previousPoint, out PatrolIdentity prev) ? prev : null;
            PatrolIdentity nextPatrolIdentity = PatrolPointDic.TryGetValue(hoveredObject.GetComponent<PatrolIdentity>().nextPoint, out PatrolIdentity next) ? next : null;

            //print(previousPatrolIdentity.RouteID + previousPatrolIdentity.PointID);
            //print(nextPatrolIdentity.RouteID + nextPatrolIdentity.PointID);

            if (previousPatrolIdentity != null)
            {
                previousPatrolIdentity.nextPoint = nextPatrolIdentity.RouteID + nextPatrolIdentity.PointID;
            }

            if (nextPatrolIdentity != null)
            {
                nextPatrolIdentity.previousPoint = previousPatrolIdentity.RouteID + previousPatrolIdentity.PointID;
            }
        }

        Destroy(hoveredObject);
        BuildManager.instance.buildCount--;
    }

    public void RemoveAll()
    {
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject i in placedBuildings)
        {
            List<BuildingPiece> size = GetObjectSize(i);
            int rotation = GetObjectRotation(i);


            foreach (Vector3 c in GetCells(WorldToCell(i.transform.position), size, rotation))
                occupiedPositions.Remove(c);
            Destroy(i);
            BuildManager.instance.buildCount--;
        }
    }

    public void Save(int saveNum)
    {
        //if (placingMode)
        //{
        //    setPlaceMode();
        //    StopGhostPosition();
        //}
        BuildingDataList saveData = new BuildingDataList();
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject building in placedBuildings)
        {
            if (building.GetComponent<PatrolIdentity>() != null)
            {
                PatrolIdentity identifier = building.GetComponent<PatrolIdentity>();
                saveData.patrols.Add(new PatrolData(
                    identifier.buildId,
                    building.transform.position,
                    building.transform.rotation,
                    identifier.RouteID,
                    identifier.PointID,
                    identifier.previousPoint != null ? identifier.previousPoint : null,
                    identifier.nextPoint != null ? identifier.nextPoint : null
                    ));
            }
            else
            {
                BuildingIdentity identifier = building.GetComponent<BuildingIdentity>();

                print(building.name);

                if (identifier == null) print("null identifier");

                saveData.buildings.Add(new BuildingData(
                    identifier.buildId,
                    building.transform.position,
                    building.transform.rotation
                    ));
            }

        }
        
        GameManager.instance.savesList.Saves.Insert(saveNum, saveData);
    }

    public void Load(int saveNum)
    {
        //if (placingMode)
        //{
        //    setPlaceMode();
        //    StopGhostPosition();
        //}

        BuildingDataList saveData = GameManager.instance.savesList.Saves[saveNum];

        RemoveAll();

        foreach (BuildingData data in saveData.buildings)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            List<BuildingPiece> size = GetObjectSize(instance);
            int rotation = GetObjectRotation(instance);

            foreach (Vector3 c in GetCells(WorldToCell(instance.transform.position), size, rotation))
                occupiedPositions.Add(c);

            BuildManager.instance.buildCount++;
        }

        foreach (PatrolData data in saveData.patrols)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            List<BuildingPiece> size = GetObjectSize(instance);
            int rotation = GetObjectRotation(instance);

            foreach (Vector3 c in GetCells(WorldToCell(instance.transform.position), size, rotation))
                occupiedPositions.Add(c);

            PatrolIdentity instanceIdentity = instance.GetComponent<PatrolIdentity>();
            instanceIdentity.RouteID = data.RouteID;
            instanceIdentity.PointID = data.PointID;
            instanceIdentity.previousPoint = data.previousPointID;
            instanceIdentity.nextPoint = data.nextPointID;

            BuildManager.instance.buildCount++;
        }
        routeValue = CheckRouteValue();
    }


    public void saveToTemp()
    {
        BuildingDataList saveData = new BuildingDataList();
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");
        BuildingIdentity endPoint = null;

        foreach (GameObject building in placedBuildings)
        {
            if (building.GetComponent<PatrolIdentity>() != null)
            {
                PatrolIdentity identifier = building.GetComponent<PatrolIdentity>();
                saveData.patrols.Add(new PatrolData(
                    identifier.buildId,
                    building.transform.position,
                    building.transform.rotation,
                    identifier.RouteID,
                    identifier.PointID,
                    identifier.previousPoint != null ? identifier.previousPoint : null,
                    identifier.nextPoint != null ? identifier.nextPoint : null
                    ));
            }
            else
            {
                BuildingIdentity identifier = building.GetComponent<BuildingIdentity>();

                if (identifier.buildId == "EndPoint")
                {
                    endPoint = identifier;
                }

                if (identifier == null) print("null identifier");

                saveData.buildings.Add(new BuildingData(
                    identifier.buildId,
                    building.transform.position,
                    building.transform.rotation
                    ));
            }

        }
        //print(endPoint.name);
        if (endPoint == null)
        {
            if (activeMessage != null)
            {
                StopCoroutine(activeMessage);
            }
            activeMessage = StartCoroutine(MessageRoutine(5f));
            return;
        }
        else
        {
            GameManager.instance.tempSave = saveData;
            sceneHandler.loadScene("PlayerPTScene");
        }

    }

    public void loadFromTemp()
    {
        BuildingDataList saveData = GameManager.instance.tempSave;

        RemoveAll();

        foreach (BuildingData data in saveData.buildings)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            List<BuildingPiece> size = GetObjectSize(instance);
            int rotation = GetObjectRotation(instance);

            foreach (Vector3 c in GetCells(WorldToCell(instance.transform.position), size, rotation))
                occupiedPositions.Add(c);

            BuildManager.instance.buildCount++;
        }

        foreach (PatrolData data in saveData.patrols)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            List<BuildingPiece> size = GetObjectSize(instance);
            int rotation = GetObjectRotation(instance);

            foreach (Vector3 c in GetCells(WorldToCell(instance.transform.position), size, rotation))
                occupiedPositions.Add(c);

            PatrolIdentity instanceIdentity = instance.GetComponent<PatrolIdentity>();
            instanceIdentity.RouteID = data.RouteID;
            instanceIdentity.PointID = data.PointID;
            instanceIdentity.previousPoint = data.previousPointID;
            instanceIdentity.nextPoint = data.nextPointID;

            BuildManager.instance.buildCount++;
        }

        GameManager.instance.tempSave = new BuildingDataList();
    }

    private IEnumerator MessageRoutine(float duration)
    {
        messageText.text = "No active End Point in Scene";
        yield return new WaitForSeconds(duration);
        messageText.text = "";
        activeMessage = null;
    }

    private string CheckRouteValue()
    {
        string routeValueCheck = "A";
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");

        foreach(GameObject placedBuilding in placedBuildings)
        {
            if (placedBuilding.GetComponent<PatrolIdentity>() != null)
            {
                if (string.Compare(routeValueCheck, placedBuilding.GetComponent<PatrolIdentity>().RouteID) < 0) 
                {
                    routeValueCheck = ((char)(placedBuilding.GetComponent<PatrolIdentity>().RouteID[0] + 1)).ToString();
                }
            }
        }

        return routeValueCheck;
    }
}
