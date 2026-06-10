using UnityEngine;

public class BuildManager : MonoBehaviour
{

    [SerializeField] private GameObject StartBuildingButton;

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
    }

    public void SelectBuilding()
    {
        
    }
}
