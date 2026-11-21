using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSetup : MonoBehaviour
{
    public Scene beginningScene;
    void Start()
    {
        SceneManager.SetActiveScene(beginningScene);
    }
}