using UnityEngine;

public class UpdatableData : ScriptableObject
{
    [Header("Preview Update")]
    [Tooltip("Enable or disable auto-update.\nWhen enabled, changes in parameters automatically regenerate the map in the editor.")]
    public bool autoUpdate;
    
    public event System.Action OnValuesUpdated;

#if UNITY_EDITOR
    
    /// <summary>
    /// Notifies listeners that the values have been updated.
    /// This method is used in the Unity Editor to invoke the OnValuesUpdated event.
    /// </summary>
    public void NotifyOfUpdatedValues()
    {
        // Unsubscribe the method from the update event to avoid multiple calls
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        
        // Invoke the event if there are any listeners
        OnValuesUpdated?.Invoke();
    }
    
#endif
}
