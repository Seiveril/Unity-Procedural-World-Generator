using UnityEngine;

/// <summary>
/// A generic Singleton class that ensures only one instance of a MonoBehaviour-derived class exists. <br/>
/// The instance can be accessed globally through the `Instance` property.<br/>
/// Optionally, the singleton can persist across scene loads if `singleInstance` is set to true.
/// </summary>
/// <typeparam name="T">The type of the MonoBehaviour-derived class that will be used as a Singleton.</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// The Instance of class T available from everywhere.
    /// </summary>
    public static T Instance { get; private set; } = null;

    /// <summary>
    /// Boolean to determine if the singleton should persist across scenes.
    /// </summary>
    [Tooltip("If true, the instance will persist across scenes.")]
    [SerializeField] protected bool singleInstance;

    /// <summary>
    /// Sets the single instance of the class when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        SetSingleInstance();
    }

    /// <summary>
    /// Sets the single instance of the class when the object becomes enabled and active.
    /// </summary>
    protected virtual void OnEnable()
    {
        SetSingleInstance();
    }

    /// <summary>
    /// Resets the instance to null when the application is quitting.
    /// </summary>
    private void OnApplicationQuit()
    {
        Instance = null;
    }

    /// <summary>
    /// Sets the single instance of the class, ensuring only one instance exists.
    /// </summary>
    private void SetSingleInstance()
    {
        //There is already an instance
        if (Instance == this)
        {
            return;
        }
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        //Find all the instances of type T in the scenes and save them into an array
        T[] instances = FindObjectsOfType<T>();

        //More than one instance is found in the scene
        if(instances.Length > 1)
        {
            Debug.LogError("Multiple instances of " + typeof(T).Name + " found");
            
            //Set this as instance. The others will be destroyed in their Awake
            Instance = this as T;
        }
        //Only one instance of type T is found. Set the instance.
        else if(instances.Length == 1)
        {
            Instance = instances[0];
        }

        if (singleInstance)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
