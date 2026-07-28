using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public void loadScene(string SceneName)
    {
        SceneManager.LoadScene(SceneName);
    }

}
