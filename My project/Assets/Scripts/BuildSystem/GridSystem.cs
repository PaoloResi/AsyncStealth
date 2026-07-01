using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridSystem : MonoBehaviour
{
    public GameObject objectToPlace;
    public float gridSize = 1f;
    private GameObject ghostObject;
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    public bool placingMode = false;
    public bool onPlane = false;
    public bool deleteMode = false;
    private GameObject hoveredObject;
    private Color OGHoveredColor;
    [SerializeField] private GameObject buildingsCanvas;
    public InputActionReference objRotAction;
    void OnEnable() => objRotAction.action.Enable();
    void OnDisable() => objRotAction.action.Disable();





    void Start()
    {

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

        if (deleteMode)
        {
            if (ghostObject)
            {
                Destroy(ghostObject);
                ghostObject = null;
            }
            highlightHover();
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
            Vector3 point = hit.point;

            Vector3 snappedPosition = new Vector3(
                Mathf.Round(point.x/gridSize) * gridSize,
                Mathf.Round(point.y/gridSize) * gridSize,
                Mathf.Round(point.z/gridSize) * gridSize
            );

            

            ghostObject.transform.position = snappedPosition;

            if (occupiedPositions.Contains(snappedPosition))
                SetColor(Color.red);
            else
                SetColor(new  Color(1f,1f,1f,0.5f));

            if (objRotAction.action.WasPressedThisFrame())
            {
                print("test");
                ghostObject.transform.rotation *= Quaternion.Euler(0, 90, 0);
            }
        }
        else
        {
            onPlane = false;
            ghostObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void StopGhostPosition()
    {
        Destroy(ghostObject);
        ghostObject = null;
    }

    public void highlightHover()
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
                SetColor(Color.red);
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
        Vector3 placementPosition = ghostObject.transform.position;

        if (!occupiedPositions.Contains(placementPosition))
        {
            Instantiate(objectToPlace, placementPosition, Quaternion.identity);

            occupiedPositions.Add(placementPosition);
        }
    }

    void RemoveObject()
    {
        Vector3 objectPos = hoveredObject.transform.position;
        Destroy(hoveredObject);

        occupiedPositions.Remove(objectPos);
    }
}
