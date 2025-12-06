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
        activeProcess = state;
    }

    public ManualProcessState Consume()
    {
        var s = activeProcess;
        activeProcess = null;
        return s;
    }

    public bool HasSavedProcess => activeProcess != null;
}