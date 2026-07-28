using UnityEngine;

public class BuildManager : MonoBehaviour
{

    [SerializeField] private GameObject StartBuildingButton;
    private int maxBuilds = 100;
    public int buildCount = 0;

    public GridSystem gridSystem;

    public static BuildManager instance;

    [SerializeField] private GameObject buildingsCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartBuildMode()
    {
        buildingsCanvas.SetActive(true);
        StartBuildingButton.SetActive(false);
        //gridSystem.placingMode = true;
    }

    public void ExitBuildMode()
    {
        buildingsCanvas.SetActive(false);
        StartBuildingButton.SetActive(true);
    }

    public void DeleteBuildMode()
    {
        gridSystem.deleteMode = !gridSystem.deleteMode;
        buildingsCanvas.SetActive(false);
    }

}
