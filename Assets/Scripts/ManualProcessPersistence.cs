using UnityEngine;
using System.Collections.Generic;

public class ManualProcessPersistence : MonoBehaviour
{
    public static ManualProcessPersistence Instance;
    private Dictionary<ProcessingTool, ManualProcessState> activeProcesses = new Dictionary<ProcessingTool, ManualProcessState>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save(ManualProcessState state)
    {
        activeProcesses[state.tool] = state;
        //Debug.Log($"💾 ManualProcessPersistence.Save: tool={state.tool}, cardID={state.cardRuntimeID}, remaining={state.remainingTime}");
    }

    // 👇 NEW: Peek without consuming
    public ManualProcessState Peek(ProcessingTool tool)
    {
        activeProcesses.TryGetValue(tool, out var state);
        return state;
    }

    // 👇 UPDATED: Only consume (clear) the state
    public void Consume(ProcessingTool tool)
    {
        if (activeProcesses.ContainsKey(tool))
        {
            //Debug.Log($"✅ ManualProcessPersistence.Consume: cleared state for {tool}");
            activeProcesses.Remove(tool);
        }
    }

    public bool HasSavedProcess(ProcessingTool tool)
    {
        bool has = activeProcesses.ContainsKey(tool);
        //Debug.Log($"ManualProcessPersistence.HasSavedProcess({tool}) = {has}");
        return has;
    }
}