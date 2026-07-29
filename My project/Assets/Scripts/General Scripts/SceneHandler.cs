using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public void loadScene(string SceneName)
    {
        string json = JsonUtility.ToJson(GameManager.instance.savesList, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");
        System.IO.File.WriteAllText(path, json);
        SceneManager.LoadScene(SceneName);
    }

    public void ExitGame()
    {
        Debug.Log("gameexited");
        string json = JsonUtility.ToJson(GameManager.instance.savesList, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "Savedbuilds.json");
        System.IO.File.WriteAllText(path, json);
        Application.Quit();

    }

}
