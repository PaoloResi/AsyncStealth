using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public SavesList savesList;
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
}
