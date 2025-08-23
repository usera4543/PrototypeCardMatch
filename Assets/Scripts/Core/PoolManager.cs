using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PoolEntry
{
    public string key;
    public GameObject prefab;
    public int initialSize = 10;
    public bool expandable = true;
}

public class PoolManager : Singleton<PoolManager>
{
    [Header("Pools")]
    public PoolEntry[] pools;

    Dictionary<string, ObjectPool> poolMap = new Dictionary<string, ObjectPool>();

    protected override void Awake()
    {
        base.Awake();
        foreach (var entry in pools)
        {
            if (entry.prefab == null) continue;
            poolMap[entry.key] = new ObjectPool(entry.prefab, entry.initialSize, transform, entry.expandable);
        }
    }

    public GameObject Get(string key)
    {
        if (!poolMap.ContainsKey(key)) return null;
        return poolMap[key].Get();
    }

    public void Return(string key, GameObject go)
    {
        if (!poolMap.ContainsKey(key)) { Destroy(go); return; }
        poolMap[key].Return(go);
    }
}