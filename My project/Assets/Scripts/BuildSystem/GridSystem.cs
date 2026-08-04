using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.UI.Image;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1.5f;
    private GameObject ghostObject;
    private HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();
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


    List<Vector3Int> GetFootprintSize(GameObject obj)
    {
        MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
        HashSet<Vector3Int> cells = new HashSet<Vector3Int>();
        foreach (MeshFilter filter in filters)
        {
            Bounds localBounds = filter.sharedMesh.bounds;

            Vector3 localCenter = obj.transform.InverseTransformPoint(filter.transform.TransformPoint(localBounds.center));
            Vector3 worldSize = Vector3.Scale(localBounds.size, filter.transform.lossyScale);

            
            int sizeX = Mathf.Max(1, Mathf.CeilToInt(worldSize.x / gridSize));
            int sizeY = Mathf.Max(1, Mathf.CeilToInt(worldSize.y / gridSize));
            int sizeZ = Mathf.Max(1, Mathf.CeilToInt(worldSize.z / gridSize));

            Vector3Int baseCell = new Vector3Int
                (
                Mathf.RoundToInt(localCenter.x / gridSize), 
                Mathf.RoundToInt(localCenter.y / gridSize), 
                Mathf.RoundToInt(localCenter.z / gridSize)
                );

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeY; y++)
                    for (int z = 0; z < sizeZ; z++)
                        cells.Add(baseCell + new Vector3Int(x, y, z));
        }

        return new List<Vector3Int>(cells);
    }

    List<Vector3Int > GetRotatedSize(List<Vector3Int> localCells, Quaternion rot)
    {
        int steps = (Mathf.RoundToInt(rot.eulerAngles.y/90f) % 4 + 4) % 4;
        List<Vector3Int> result = new List<Vector3Int>(localCells.Count);

        foreach (Vector3Int cell in localCells)
        {
            Vector3Int rotatedCell = cell;
            for (int i = 0; i < steps; i++)
            {
                rotatedCell = new Vector3Int(rotatedCell.z, rotatedCell.y, -rotatedCell.x);
            }
            result.Add(rotatedCell);
        }
        return result;
    }

    IEnumerable<Vector3Int> GetCells(Vector3Int origin, List<Vector3Int> size)
    {
       foreach (Vector3Int cell in size)
        {
            yield return origin + cell;
        }
    }

    bool AreCellsFree(Vector3Int origin, List<Vector3Int> size)
    {
        foreach (Vector3Int c in GetCells(origin, size))
            if (occupiedPositions.Contains(c))
                return false;
        return true;
    }

    Vector3 FootprintOffset(List<Vector3Int> relativeCells)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;

        foreach (Vector3Int cell in relativeCells)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);

            minZ = Mathf.Min(minZ, cell.z);
            maxZ = Mathf.Max(maxZ, cell.z);
        }

        return new Vector3((minX + maxX) * gridSize * 0.5f, 0f, (minZ + maxZ) * gridSize * 0.5f);
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

                    List<Vector3Int> size = GetRotatedSize(GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
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

        foreach (Collider child in ghostObject.GetComponentsInChildren<Collider>())
        {
            child.enabled = false;
        }
        if (ghostObject.GetComponent<Collider>() != null)
            ghostObject.GetComponent<Collider>().enabled = false;
        //ghostObject.GetComponent<Collider>().enabled = false;

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


            List<Vector3Int> size = GetRotatedSize(GetFootprintSize(ghostObject), ghostObject.transform.rotation);
            Vector3 targetPos = hit.point;
            Vector3Int origin = WorldToCell(targetPos - FootprintOffset(size));

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

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (MeshRenderer child in hoveredObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.enabled = true;
            }
            if (hoveredObject.GetComponent<MeshRenderer>() != null)
                hoveredObject.GetComponent<MeshRenderer>().enabled = true;
            onPlane = true;


            List<Vector3Int> size = GetRotatedSize(GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
            Vector3 targetPos = hit.point;
            Vector3Int origin = WorldToCell(targetPos - FootprintOffset(size));

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
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Ground"))
        {
            List<Vector3Int> size = GetRotatedSize(GetFootprintSize(ghostObject), ghostObject.transform.rotation);
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
        List<Vector3Int> size = GetRotatedSize(GetFootprintSize(hoveredObject), hoveredObject.transform.rotation);
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
            List<Vector3Int> size = GetRotatedSize(GetFootprintSize(i), i.transform.rotation);
            Vector3Int origin = WorldToCell(i.transform.position - FootprintOffset(size));

            Destroy(i);

            foreach (Vector3Int c in GetCells(origin, size))
                occupiedPositions.Remove(c);
            BuildManager.instance.buildCount--;
        }
    }

    public void Save(int saveNum)
    {
        
        BuildingDataList saveData = new BuildingDataList();
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

        GameManager.instance.savesList.Saves.Insert(saveNum, saveData);
    }

    public void Load(int saveNum)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");

        string json = System.IO.File.ReadAllText(path);

        BuildingDataList saveData = GameManager.instance.savesList.Saves[saveNum];

        RemoveAll();

        foreach (BuildingData data in saveData.buildings)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            List<Vector3Int> size = GetRotatedSize(GetFootprintSize(instance), instance.transform.rotation);
            Vector3Int origin = WorldToCell(instance.transform.position - FootprintOffset(size));

            foreach (Vector3Int c in GetCells(origin, size))
                occupiedPositions.Add(c);

            BuildManager.instance.buildCount++;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (float x = -5f; x <= 5f; x += gridSize)
            Gizmos.DrawLine(new Vector3(x, 0, -5f), new Vector3(x, 0, 5f));
        for (float z = -5f; z <= 5f; z += gridSize)
            Gizmos.DrawLine(new Vector3(-5f, 0, z), new Vector3(5f, 0, z));
    }

}
