using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a single pool entry for the PoolManager inspector setup.
/// </summary>
[Serializable]
public class PoolEntry
{
    [Tooltip("Unique key for this pool, e.g. 'Card' or 'Enemy'")]
    public string key;

    [Tooltip("Prefab to pool")]
    public GameObject prefab;

    [Tooltip("Initial number of objects to pre-spawn in the pool")]
    public int initialSize = 10;

    [Tooltip("If true, pool can grow dynamically when exhausted")]
    public bool expandable = true;
}

/// <summary>
/// Global pool manager singleton.
/// Handles pooling of multiple prefabs by string keys.
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    [Header("Pools Setup")]
    [Tooltip("Define all pools to be created on Awake")]
    public PoolEntry[] pools;

    // Map of pool key -> ObjectPool instance
    private readonly Dictionary<string, ObjectPool> poolMap = new Dictionary<string, ObjectPool>();

    /// <summary>
    /// Creates all the configured pools when the manager is initialized.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        foreach (var entry in pools)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning($"[PoolManager] Missing prefab for pool key '{entry.key}', skipping.");
                continue;
            }

            if (poolMap.ContainsKey(entry.key))
            {
                Debug.LogWarning($"[PoolManager] Duplicate pool key '{entry.key}', skipping.");
                continue;
            }

            poolMap[entry.key] = new ObjectPool(
                entry.prefab,
                entry.initialSize,
                transform, // Parent all pooled objects under PoolManager for hierarchy cleanliness
                entry.expandable
            );
        }
    }

    /// <summary>
    /// Get an object from the pool by key.
    /// Returns null if the key is invalid.
    /// </summary>
    public GameObject Get(string key)
    {
        if (!poolMap.TryGetValue(key, out var pool))
        {
            Debug.LogError($"[PoolManager] No pool found for key '{key}'");
            return null;
        }

        return pool.Get();
    }

    /// <summary>
    /// Return an object to the pool.
    /// If key doesn't exist, object is destroyed instead.
    /// </summary>
    public void Return(string key, GameObject go)
    {
        if (go == null) return;

        if (!poolMap.TryGetValue(key, out var pool))
        {
            Debug.LogWarning($"[PoolManager] No pool for key '{key}', destroying object instead.");
            Destroy(go);
            return;
        }

        pool.Return(go);
    }
}
