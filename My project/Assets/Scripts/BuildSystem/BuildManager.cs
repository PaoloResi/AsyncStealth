using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{

    [SerializeField] private GameObject StartBuildingButton;
    //private int maxBuilds = 100;
    public int buildCount = 0;

    public GridSystem gridSystem;

    public static BuildManager instance;

    public GameObject savePanel;

    public GameObject loadPanel;

    [SerializeField] private GameObject buildingsCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        Cursor.visible = true;
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

    public void SavePanel()
    {
        if (!loadPanel.activeSelf)
            savePanel.SetActive(!savePanel.activeSelf);
    }

    public void LoadPanel()
    {
        if (!savePanel.activeSelf)
            loadPanel.SetActive(!loadPanel.activeSelf);
    }

    public void testBuild()
    {
        GameManager.instance.testing = true;
        gridSystem.saveToTemp();
    }

    public void uploadBuild()
    {
        GameManager.instance.uploading = true;
        gridSystem.saveToTemp();
    }

}
