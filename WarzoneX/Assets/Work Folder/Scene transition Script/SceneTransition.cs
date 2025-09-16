using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Public method to load a scene by name
    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Example: Method to reload the current scene
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
