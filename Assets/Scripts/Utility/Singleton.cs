using UnityEngine;

public abstract class Singleton<T> where T : Singleton<T>, new()
{
    private static T _instance;

    public Singleton()
    {
        if (_instance != null)
        {
            Debug.LogError("Singleton already exists");
        }

        _instance = (T)this;
    }

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }
            return _instance;
        }
    }

    public virtual void SelfDelete()
    {
        if (_instance != null)
        {
            _instance = null;
        }
    }
}

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T _instance;

    public virtual void Awake()
    {
        if (_instance == this)
            return;
        else if (_instance != null)
            Debug.LogError("Singleton already exists " + _instance.name);

        _instance = this as T;
    }

    public static T instance
    {
        
        get
        {
            if ( _instance == null)
            {
                GameObject obj = new GameObject(typeof(T).ToString());

                _instance = obj.AddComponent<T>();
            }

            return _instance;
        }
    }

    public static bool HasInstance()
    {
        return (_instance != null);
    }

    public virtual void SelfDelete()
    {
        if (_instance != null)
        {
            DestroyImmediate(gameObject);
            _instance = null;
        }
    }
}



