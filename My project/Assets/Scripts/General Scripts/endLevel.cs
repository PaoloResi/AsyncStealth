using UnityEngine;
using UnityEngine.SceneManagement;

public class endLevel : MonoBehaviour
{
    private string sceneName;
    
    public void Awake()
    {
        
    }
    public void HandleTrigger(Collider other)
    {
        if (other.name == "PlayerCapsule")
        {
            if (SceneManager.GetActiveScene().name == "PlayerScene")
            {
                InvasionManager.instance.finishLevel();
            }
            else if (SceneManager.GetActiveScene().name == "PlayerPTScene")
            {
                PlaytestManager.instance.completed = true;
                PlaytestManager.instance.finishLevel();
            }
        }
    }
}
