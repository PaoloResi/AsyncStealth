using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaytestManager : MonoBehaviour
{

    public bool testing;
    public bool uploading;
    public bool completed;

    public BuildingRegistry buildingRegistry;
    public float gridSize = 1f;
    public static PlaytestManager instance;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject UIcam;
    public GameObject playerSpawnPoint;
    public PatrolFinder patrolFinder;

    public SceneHandler sceneHandler;
    public Dictionary<string, PatrolIdentity> patrolPointsDic = new Dictionary<string, PatrolIdentity>();

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
        testing = GameManager.instance.testing;
        uploading = GameManager.instance.uploading;
        LoadTemp();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            finishLevel();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadTemp()
    {

        BuildingDataList saveData = GameManager.instance.tempSave;

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
        GameObject player = Instantiate(playerPrefab, playerSpawnPoint.transform.position, playerSpawnPoint.transform.rotation);
        player.name = "PlayerCapsule";
        player.layer = 3;
        UIcam.SetActive(false);
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

    public void finishLevel()
    {
        GameManager.instance.returning = true;
        if (uploading && completed)
        {
            GameManager.instance.uploadList.Saves.Add(GameManager.instance.tempSave);
        }
        sceneHandler.loadScene("BuildingScene");
    }
}
