using UnityEngine;

/// <summary>
/// Simple MonoBehaviour singleton base. Used for managers.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;
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

    protected virtual void Awake()
    {
        if (_instance == null) { _instance = this as T; DontDestroyOnLoad(gameObject); }
        else if (_instance != this) Destroy(gameObject);
    }
}