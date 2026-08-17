using System.Collections.Generic;
using UnityEngine;

public class InvasionManager : MonoBehaviour
{

    public BuildingRegistry buildingRegistry;
    public float gridSize = 1f;
    public static InvasionManager instance;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject UIcam;
    public GameObject playerSpawnPoint;
    public GameObject canvas;
    public PatrolFinder patrolFinder;
    public Dictionary<string, PatrolIdentity> patrolPointsDic = new Dictionary<string, PatrolIdentity>();


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

    private void Awake()
    {
        patrolFinder = FindFirstObjectByType<PatrolFinder>();
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
             
        }

        foreach (PatrolData data in saveData.patrols)
        {
            GameObject prefab = buildingRegistry.GetPrefab(data.ID);
            Vector3 enemySpawn = new Vector3(data.position.x, data.position.y + 1f, data.position.z);
            GameObject instance = Instantiate(prefab, enemySpawn, data.rotation);
            PatrolData x = (PatrolData)data;
            PatrolIdentity identity = instance.GetComponent<PatrolIdentity>();
            identity.previousPoint = x.previousPointID;
            identity.nextPoint = x.nextPointID;
            identity.RouteID = x.RouteID;
            identity.PointID = x.PointID;
            patrolPointsDic.Add(instance.GetComponent<PatrolIdentity>().RouteID + instance.GetComponent<PatrolIdentity>().PointID,
                instance.GetComponent<PatrolIdentity>());
            instance.GetComponentInChildren<MeshRenderer>().enabled = false;
            instance.GetComponentInChildren<SphereCollider>().enabled = false;
        }

        SpawnEnemies();
        Instantiate(playerPrefab, playerSpawnPoint.transform.position, playerSpawnPoint.transform.rotation);
        UIcam.SetActive(false);
        canvas.SetActive(false);
    }

    public void SpawnEnemies()
    {
        List<PatrolIdentity> startPoints = patrolFinder.FindStartPoints();

        foreach (PatrolIdentity startPoint in startPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, startPoint.transform.position, Quaternion.identity);
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            enemyController.SetPatrol(startPoint, patrolPointsDic);
        }
    }
}
