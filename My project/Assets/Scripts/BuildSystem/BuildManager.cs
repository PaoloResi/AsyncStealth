using UnityEngine;

public class BuildManager : MonoBehaviour
{

    [SerializeField] private GameObject StartBuildingButton;

    public GridSystem gridSystem;

    [SerializeField] private GameObject buildingsCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartBuildMode()
    {
        buildingsCanvas.SetActive(true);
        StartBuildingButton.SetActive(false);
        gridSystem.placingMode = true;
    }

    public void ExitBuildMode()
    {
        buildingsCanvas.SetActive(false);
        StartBuildingButton.SetActive(true);
        gridSystem.placingMode = false;
    }

    public void SelectBuilding()
    {
        
    }
}
