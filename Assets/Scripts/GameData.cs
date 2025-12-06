using UnityEngine;
using System.Collections.Generic;
using static CardPersistenceManager;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public List<SpellCombo> failedBrews = new List<SpellCombo>();
    public List<SpellCombo> successfulBrews = new List<SpellCombo>();

    [System.Serializable]
    public class SavedCauldronBrew
    {
        public bool isBrewing;
        public string spellName;
        public double finishTimeUtcOa;
        public bool fireWasOn;
        public float totalBrewTime;
    }

    // Full runtime card states for persistence
    public List<SavedCardState> savedCards = new List<SavedCardState>();
    public List<CardData> cauldronOutputCards = new List<CardData>();
    public List<SavedPlanterState> savedPlanters = new();
    public SavedCauldronBrew savedCauldron;

    public bool testCardsSpawned = false;
    public bool initialRandomCardsSpawned = false;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}