using UnityEngine;

public class SceneLoaderButton : MonoBehaviour
{
    public void LoadMainRoom()
    {
        SceneLoadManager.Instance.LoadRoom("MainScene");
    }

    public void LoadPlanterRoom()
    {
        SceneLoadManager.Instance.LoadRoom("PlanterScene");
    }

    public void LoadBruteRoom()
    {
        SceneLoadManager.Instance.LoadRoom("BruteScene");
    }
}