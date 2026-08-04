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

    // Update is called once per frame
    void Update()
    {
        
    }

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
            return new Vector3Int(size.z, size.y, size.x);
        return size;

    }

    Vector3Int WorldToCell(Vector3 world) => new Vector3Int(
       Mathf.RoundToInt(world.x / gridSize),
       Mathf.RoundToInt(world.y / gridSize),
       Mathf.RoundToInt(world.z / gridSize)
       );

    Vector3 FootprintOffset(Vector3Int size)
    {
        return new Vector3(
            (size.x - 1) * gridSize * 0.5f,
            0f,
            (size.z - 1) * gridSize * 0.5f
        );
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

            Vector3Int size = GetRotatedSize(GetFootprintSize(instance), instance.transform.rotation);
            Vector3Int origin = WorldToCell(instance.transform.position - FootprintOffset(size));
        }

        Instantiate(playerPrefab, playerSpawnPoint.transform.position, playerSpawnPoint.transform.rotation);
        UIcam.SetActive(false);
        canvas.SetActive(false);
    }
}
