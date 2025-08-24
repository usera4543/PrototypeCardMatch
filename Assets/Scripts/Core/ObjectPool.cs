using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple object pool that returns the objects from the pool
/// </summary>
public class ObjectPool
{
    readonly GameObject prefab;
    readonly Transform parent;
    readonly Stack<GameObject> stack = new Stack<GameObject>();
    readonly bool expandable;

    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null, bool expandable = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.expandable = expandable;
        for (int i = 0; i < initialSize; i++) stack.Push(CreateNew());
    }

    GameObject CreateNew()
    {
        var go = Object.Instantiate(prefab, parent);
        go.SetActive(false);
        // Pool-managed objects inactive initially
        return go;
    }

    public GameObject Get()
    {
        GameObject go = (stack.Count > 0) ? stack.Pop() : (expandable ? CreateNew() : null);
        if (go == null) return null;
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        var poolable = go.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();
        go.SetActive(false);
        stack.Push(go);
    }
}