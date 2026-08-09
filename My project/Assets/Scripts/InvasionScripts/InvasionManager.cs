using System.Collections.Generic;
using UnityEngine;

public class InvasionManager : MonoBehaviour
{

    public BuildingRegistry buildingRegistry;
    public float gridSize = 1f;
    public static InvasionManager instance;
    public GameObject playerPrefab;
    public GameObject UIcam;
    public GameObject playerSpawnPoint;
    public GameObject canvas;


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
    }   
    public void Load(int saveNum)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");

        string json = System.IO.File.ReadAllText(path);

        BuildingDataList saveData = GameManager.instance.savesList.Saves[saveNum];

        foreach (BuildingData data in saveData.buildings)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);

            GameObject instance = Instantiate(prefab, data.position, data.rotation);

            BuildManager.instance.buildCount++;
        }
    }
}
