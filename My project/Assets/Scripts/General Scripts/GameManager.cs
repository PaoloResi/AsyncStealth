using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public SavesList savesList;
    public SavesList uploadList;
    public BuildingDataList tempSave;
    public bool testing;
    public bool uploading;

    public bool returning;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
    
    void Start()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");

        if (File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            savesList = JsonUtility.FromJson<SavesList>(json);
        }
        else
        {
            string json = JsonUtility.ToJson(savesList, true);
            System.IO.File.WriteAllText(path, json);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void loadScene (string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

    //private void OnApplicationQuit()
    //{
    //    //added this to ensure things get saved but may cause issues
    //    Debug.Log("gameexited");
    //    string json = JsonUtility.ToJson(GameManager.instance.savesList, true);
    //    string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");
    //    System.IO.File.WriteAllText(path, json);
    //    Application.Quit();
    //}
}
