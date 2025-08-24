using UnityEngine;

/// <summary>
/// Simple MonoBehaviour singleton base. Used for manager classes.
/// Ensures only one instance exists and provides global access via I.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    /// <summary>
    /// Global access to the singleton instance.
    /// If no instance exists, tries to find one in the scene or creates a new GameObject.
    /// </summary>
    public static T I
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Ensures only one instance exists in the scene. Destroys duplicates.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null) { _instance = this as T; }
        else if (_instance != this) Destroy(gameObject);
    }
}
