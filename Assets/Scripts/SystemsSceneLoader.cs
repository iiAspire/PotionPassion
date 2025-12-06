using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemsSceneLoader : MonoBehaviour
{
    void Start()
    {
        // Load game entry scene ADDITIVELY
        SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
    }
}