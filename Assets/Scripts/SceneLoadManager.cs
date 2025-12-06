using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance;
    public bool IsTransitioning { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadRoom(string sceneName)
    {
        StartCoroutine(LoadRoomRoutine(sceneName));
    }

    public IEnumerator LoadRoomRoutine(string sceneName)
    {
        if (IsTransitioning)
            yield break;

        IsTransitioning = true;

        // 🔹 Ask all workbench stations to store any in-progress manual tools
        foreach (WorkbenchStation station in FindObjectsOfType<WorkbenchStation>())
        {
            station.PersistIfPaused();
        }

        InventoryDebug.Dump(
            "BEFORE SaveAllCards",
            CardPersistenceManager.Instance.playerInventoryParent,
            CardPersistenceManager.Instance.ingredientsInventoryParent,
            CardPersistenceManager.Instance.recipeHoldingParent,
            CardPersistenceManager.Instance.cauldronOutputParent
        );

        // ✅ SAVE WHILE OLD SCENE IS STILL ACTIVE
        CardPersistenceManager.Instance.SaveAllCards();


        InventoryDebug.Dump(
            "AFTER SaveAllCards",
            CardPersistenceManager.Instance.playerInventoryParent,
            CardPersistenceManager.Instance.ingredientsInventoryParent,
            CardPersistenceManager.Instance.recipeHoldingParent,
            CardPersistenceManager.Instance.cauldronOutputParent
        );

        yield return null;

        InventoryDebug.Dump(
            "AFTER yield (before LoadSceneAsync)",
            CardPersistenceManager.Instance.playerInventoryParent,
            CardPersistenceManager.Instance.ingredientsInventoryParent,
            CardPersistenceManager.Instance.recipeHoldingParent,
            CardPersistenceManager.Instance.cauldronOutputParent);

        // ✅ THEN load new scene
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // ✅ THEN unload old gameplay scenes
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name != "_Systems" && s.name != sceneName)
                yield return SceneManager.UnloadSceneAsync(s);
        }

        IsTransitioning = false;
    }
}