using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    private GameObject ghostObject;
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
    public bool placingMode = false;
    public bool onPlane = false;
    public bool deleteMode = false;
    public bool moveMode = false;
    [SerializeField] private GameObject hoveredObject;
    private Color OGHoveredColor;
    [SerializeField] private GameObject buildingsCanvas;
    public InputActionReference objRotAction;
    void OnEnable() => objRotAction.action.Enable();
    void OnDisable() => objRotAction.action.Disable();
    public BuildingRegistry buildingRegistry;


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

    Vector3 CellToWorld(Vector3Int cell) => new Vector3(
        cell.x * gridSize,
        cell.y * gridSize,
        cell.z * gridSize
        );


    Vector3Int GetFootprintSize(GameObject obj)
    {
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
        if (filters.Length == 0) return Vector3Int.one;

        Bounds local = filters[0].sharedMesh.bounds;

        for (int i = 1; i < filters.Length; i++)
            local.Encapsulate(filters[i].sharedMesh.bounds);

        Vector3 scaled = Vector3.Scale(local.size, obj.transform.lossyScale);

        return new Vector3Int(
            Mathf.Max(1, Mathf.CeilToInt(scaled.x / gridSize - 0.01f)),
            Mathf.Max(1, Mathf.CeilToInt(scaled.y / gridSize - 0.01f)),
            Mathf.Max(1, Mathf.CeilToInt(scaled.z / gridSize - 0.01f))
            );

    }

    Vector3Int GetRotatedSize(Vector3Int size, Quaternion rot)
    {
        int steps = Mathf.RoundToInt(rot.eulerAngles.y / 90f) & 3;
        if (steps % 2 == 1)
            return new Vector3Int (size.z, size.y, size.x);
        return size;

    }

    IEnumerable<Vector3Int> GetCells(Vector3Int origin, Vector3Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.z; z++)
                    yield return new Vector3Int(origin.x + x, origin.y + y, origin.z + z);
    }

    bool AreCellsFree(Vector3Int origin, Vector3Int size)
    {
        foreach (Vector3Int c in GetCells(origin, size))
            if (occupiedPositions.Contains(c))
                return false;
        return true;
    }

    Vector3 FootprintOffset(Vector3Int size)
    {
        return new Vector3(
            (size.x -1) * gridSize * 0.5f,
            0f,
            (size.z - 1) * gridSize * 0.5f
        );
    }


    void Update()
    {   if (placingMode == true)
        {
            if (!ghostObject)
            {
                CreateGhostObject();
            }
            UpdateGhostPosition();
            if (Mouse.current.leftButton.wasPressedThisFrame && onPlane)
                PlaceObject();
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                setPlaceMode();
                StopGhostPosition();
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
                highlightHover(Color.white);
            }
            if (Mouse.current.leftButton.wasPressedThisFrame && hoveredObject)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit) && (hit.transform.CompareTag("Ground") || !moveMode))
                {
                    moveMode = !moveMode;
                    hoveredObject.GetComponent<Collider>().enabled = !hoveredObject.GetComponent<Collider>().enabled;

                    Vector3Int size = GetRotatedSize(GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
                    Vector3Int origin = WorldToCell(hoveredObject.transform.position - FootprintOffset(size));

                    foreach (Vector3Int c in GetCells(origin, size))
                    {
                        if (moveMode) occupiedPositions.Remove(c);
                        else occupiedPositions.Add(c);
                    }
                }
                
            }

            if (moveMode && hoveredObject)
            {
                UpdateHoveredObjPos();
            }
        }
    }

    

    void CreateGhostObject()
    {
        ghostObject = Instantiate(objectToPlace);
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
        ghostObject.GetComponent<MeshRenderer>().enabled = false;
    }

    void UpdateGhostPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ghostObject.GetComponent<MeshRenderer>().enabled = true;
            onPlane = true;


            Vector3Int size = GetRotatedSize(GetFootprintSize(ghostObject), ghostObject.transform.rotation);
            Vector3Int origin = WorldToCell(hit.point);

            ghostObject.transform.position = CellToWorld(origin) + FootprintOffset(size);

            
            if (AreCellsFree(origin, size))
                SetColor(new Color(1f, 1f, 1f, 0.5f));
            else
                SetColor(Color.red);

            if (!hit.transform.CompareTag("Ground"))
                SetColor(Color.red);

            if (objRotAction.action.WasPressedThisFrame())
            {
                ghostObject.transform.rotation *= Quaternion.Euler(0, 90, 0);
            }
        }
        else
        {
            onPlane = false;
            ghostObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void UpdateHoveredObjPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hoveredObject.GetComponent<MeshRenderer>().enabled = true;
            onPlane = true;


            Vector3Int size = GetRotatedSize(GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
            Vector3Int origin = WorldToCell(hit.point);

            hoveredObject.transform.position = CellToWorld(origin) + FootprintOffset(size);


            if (AreCellsFree(origin, size))
                SetColor(new Color(1f, 1f, 1f, 0.5f));
            else
                SetColor(Color.red);

            if (!hit.transform.CompareTag("Ground"))
                SetColor(Color.red);


            if (objRotAction.action.WasPressedThisFrame())
            {
                hoveredObject.transform.rotation *= Quaternion.Euler(0, 90, 0);
            }
        }
        else
        {
            onPlane = false;
            hoveredObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void StopGhostPosition()
    {
        Destroy(ghostObject);
        ghostObject = null;
    }

    public void highlightHover(Color color)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Building"))
        {
            if (hoveredObject != hit.collider.gameObject)
            {
                if (hoveredObject)
                {
                    SetColor(OGHoveredColor);
                }

                hoveredObject = hit.collider.gameObject;

                OGHoveredColor = hoveredObject.GetComponentInChildren<Renderer>().material.color;
                SetColor(color);
            }
        }
        else
        {
            if (hoveredObject)
            {
                SetColor(OGHoveredColor);
                hoveredObject = null;
            }
        }

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
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Ground"))
        {
            Vector3Int size = GetRotatedSize(
            GetFootprintSize(ghostObject), ghostObject.transform.rotation);
            Vector3Int origin = WorldToCell(ghostObject.transform.position - FootprintOffset(size));

            if (AreCellsFree(origin, size))
            {
                Instantiate(objectToPlace, CellToWorld(origin) + FootprintOffset(size), ghostObject.transform.rotation);
                foreach (Vector3Int c in GetCells(origin, size))
                    occupiedPositions.Add(c);
                BuildManager.instance.buildCount++;
            }
        }
}

    void RemoveObject()
    {
        Vector3Int size = GetRotatedSize(
            GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
        Vector3Int origin = WorldToCell(hoveredObject.transform.position - FootprintOffset(size));

        Destroy(hoveredObject);

        foreach (Vector3Int c in GetCells(origin, size))
            occupiedPositions.Remove(c);
        BuildManager.instance.buildCount--;
    }

    public void RemoveAll()
    {
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject i in placedBuildings)
        {
            Vector3Int size = GetRotatedSize(
            GetFootprintSize(i), i.transform.rotation);
            Vector3Int origin = WorldToCell(i.transform.position - FootprintOffset(size));

            Destroy(i);

            foreach (Vector3Int c in GetCells(origin, size))
                occupiedPositions.Remove(c);
            BuildManager.instance.buildCount--;
        }
    }

    public void Save()
    {
        BuildingSaveData saveData = new BuildingSaveData();
        GameObject[] placedBuildings = GameObject.FindGameObjectsWithTag("Building");

        foreach (GameObject building in placedBuildings)
        {
            BuildingIdentity identifier = building.GetComponent<BuildingIdentity>();

            saveData.buildings.Add(new BuildingData(
                identifier.buildId,
                building.transform.position,
                building.transform.rotation
                ));
        }

        string json = JsonUtility.ToJson(saveData, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "buildings.json");
        System.IO.File.WriteAllText(path, json);
    }

    public void Load()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "buildings.json");

        string json = System.IO.File.ReadAllText(path);

        BuildingSaveData saveData = JsonUtility.FromJson<BuildingSaveData>(json);

        RemoveAll();

        foreach (BuildingData data in saveData.buildings)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            Vector3Int size = GetRotatedSize(GetFootprintSize(instance), instance.transform.rotation);
            Vector3Int origin = WorldToCell(instance.transform.position - FootprintOffset(size));

            foreach (Vector3Int c in GetCells(origin, size))
                occupiedPositions.Add(c);

            BuildManager.instance.buildCount++;
        }
    }
}
