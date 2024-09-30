using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    // Override the default inspector GUI
    public override void OnInspectorGUI()
    {

        UpdatableData data = (UpdatableData)target;
        
        if (DrawDefaultInspector())
        {
            if (data.autoUpdate)
            {
                data.NotifyOfUpdatedValues();
                EditorUtility.SetDirty(target);
            }
        }
        
        // Add a button labeled "Generate" to trigger map generation
        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}