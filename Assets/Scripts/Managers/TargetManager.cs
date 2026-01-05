using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get; private set; }


    [SerializeField]
    private List<Transform> zTargetables = new List<Transform>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        // Subscribe to target registration events
        EventManager.OnZTargetRegister += RegisterTarget;
        EventManager.OnZTargetUnregister += UnregisterTarget;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        EventManager.OnZTargetRegister -= RegisterTarget;
        EventManager.OnZTargetUnregister -= UnregisterTarget;
    }

    /// <summary>
    /// Registers a transform as a valid Z-target
    /// </summary>
    public void RegisterTarget(Transform target)
    {
        if (target != null && !zTargetables.Contains(target))
        {
            zTargetables.Add(target);
            Debug.Log("target Aquired");
        }
    }

    /// <summary>
    /// Unregisters a transform from being a valid Z-target
    /// </summary>
    public void UnregisterTarget(Transform target)
    {
        if (target != null && zTargetables.Contains(target))
        {
            zTargetables.Remove(target);
        }
    }

    /// <summary>
    /// Gets all currently registered Z-targetable transforms
    /// </summary>
    public List<Transform> GetZTargetables()
    {
        // Clean up any null references (destroyed objects)
        zTargetables.RemoveAll(t => t == null);
        return zTargetables;
    }

    /// <summary>
    /// Clears all registered targets (useful for scene transitions)
    /// </summary>
    public void ClearAllTargets()
    {
        zTargetables.Clear();
    }

    /// <summary>
    /// Gets the count of currently registered targets
    /// </summary>
    public int GetTargetCount()
    {
        zTargetables.RemoveAll(t => t == null);
        return zTargetables.Count;
    }
}
