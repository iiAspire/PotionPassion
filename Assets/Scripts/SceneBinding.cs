using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneBinding : MonoBehaviour
{
    [Header("Drop Parents (ACTUAL card parents)")]
    [SerializeField] Transform ingredientsGridContent;
    [SerializeField] Transform playerGridContent;
    [SerializeField] Transform recipeGridContent;
    [SerializeField] Transform cauldronOutput;

    void Start()
    {
        var persistence = CardPersistenceManager.Instance;
        if (persistence == null)
            return;

        persistence.BindSceneParents(
            playerGridContent,
            ingredientsGridContent,
            recipeGridContent,
            cauldronOutput
        );

        persistence.LoadAllCards();
    }

    //void OnDisable()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    if (mode != LoadSceneMode.Single)
    //        return;

    //    var persistence = CardPersistenceManager.Instance;
    //    if (persistence == null)
    //        return;

    //    persistence.BindSceneParents(
    //        playerGridContent,
    //        ingredientsGridContent,
    //        recipeGridContent,
    //        cauldronOutput
    //    );

    //    persistence.LoadAllCards();
    //}
}