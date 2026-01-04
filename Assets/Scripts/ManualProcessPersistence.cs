using UnityEngine;

public class ManualProcessPersistence : MonoBehaviour
{
    public static ManualProcessPersistence Instance;
    ManualProcessState activeProcess; // only one at a time (by design)

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(ManualProcessState state)
    {
        Debug.Log($"💾 ManualProcessPersistence.Save: tool={state.tool}, cardID={state.cardRuntimeID}, remaining={state.remainingTime}");
        activeProcess = state;
    }

    // 👇 NEW: Peek without consuming
    public ManualProcessState Peek()
    {
        return activeProcess;
    }

    // 👇 UPDATED: Only consume (clear) the state
    public void Consume()
    {
        Debug.Log($"✅ ManualProcessPersistence.Consume: cleared state for {activeProcess?.tool}");
        activeProcess = null;
    }

    public bool HasSavedProcess
    {
        get
        {
            bool has = activeProcess != null;
            Debug.Log($"ManualProcessPersistence.HasSavedProcess = {has} (tool={activeProcess?.tool})");
            return has;
        }
    }
}