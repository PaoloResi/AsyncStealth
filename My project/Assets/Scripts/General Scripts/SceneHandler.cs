using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneHandler : MonoBehaviour
{
    public void loadScene(string SceneName)
    {
        if (BuildManager.instance != null)
            BuildManager.instance.ExitBuildMode();
        string json = JsonUtility.ToJson(GameManager.instance.savesList, true);
        string path = System.IO.Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Savedbuilds.json");

        string json2 = JsonUtility.ToJson(GameManager.instance.uploadList, true);
        string path2 = System.IO.Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Uploaded.json");


        System.IO.File.WriteAllText(path, json);
        System.IO.File.WriteAllText(path2, json2);

        SceneManager.LoadScene(SceneName);
    }

    public void ExitGame()
    {
        Debug.Log("gameexited");
        string json = JsonUtility.ToJson(GameManager.instance.savesList, true);
        string path = System.IO.Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Savedbuilds.json");

        string json2 = JsonUtility.ToJson(GameManager.instance.uploadList, true);
        string path2 = System.IO.Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Uploaded.json");

        System.IO.File.WriteAllText(path, json);
        System.IO.File.WriteAllText(path2, json2);
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }

}
