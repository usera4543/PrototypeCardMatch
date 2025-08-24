using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple object pool for reusing GameObjects to reduce runtime instantiation overhead.
/// Objects are created upfront and can optionally expand if more are requested than available.
/// </summary>
public class ObjectPool
{
    // Prefab to instantiate objects from
    readonly GameObject prefab;

    // Optional parent transform to organize pooled objects in the hierarchy
    readonly Transform parent;

    // Stack to store inactive objects for reuse
    readonly Stack<GameObject> stack = new Stack<GameObject>();

    // If true, pool can create new objects when none are available
    readonly bool expandable;

    /// <summary>
    /// Constructor for the object pool.
    /// </summary>
    /// <param name="prefab">Prefab to instantiate objects from.</param>
    /// <param name="initialSize">Number of objects to create initially.</param>
    /// <param name="parent">Optional parent for pooled objects in hierarchy.</param>
    /// <param name="expandable">Whether the pool can expand beyond initial size.</param>
    public ObjectPool(GameObject prefab, int initialSize, Transform parent = null, bool expandable = true)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.expandable = expandable;

        // Pre-populate the pool with inactive objects
        for (int i = 0; i < initialSize; i++)
            stack.Push(CreateNew());
    }

    /// <summary>
    /// Creates a new GameObject instance from the prefab and sets it inactive.
    /// </summary>
    /// <returns>Newly instantiated, inactive GameObject.</returns>
    GameObject CreateNew()
    {
        var go = Object.Instantiate(prefab, parent);
        go.SetActive(false); // Pool-managed objects start inactive
        return go;
    }

    /// <summary>
    /// Retrieves an object from the pool.
    /// If none are available and expandable is true, creates a new one.
    /// </summary>
    /// <returns>Active GameObject from the pool, or null if none available and not expandable.</returns>
    public GameObject Get()
    {
        GameObject go = (stack.Count > 0) ? stack.Pop() : (expandable ? CreateNew() : null);

        if (go == null) return null;

        go.SetActive(true); // Activate object before returning
        return go;
    }

    /// <summary>
    /// Returns a GameObject to the pool for reuse.
    /// Calls OnReturnToPool() if the object implements IPoolable.
    /// </summary>
    /// <param name="go">GameObject to return to the pool.</param>
    public void Return(GameObject go)
    {
        // Optional callback for objects implementing IPoolable interface
        var poolable = go.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();

        go.SetActive(false);  // Deactivate before returning to pool
        stack.Push(go);       // Add back to pool
    }
}
