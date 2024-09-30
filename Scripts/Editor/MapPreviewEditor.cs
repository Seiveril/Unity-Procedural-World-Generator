using UnityEngine;
using UnityEditor;

// Custom editor for the MapGenerator component
[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    
    // Override the default inspector GUI
    public override void OnInspectorGUI()
    {
        
        // Cast the target to MapGenerator type
        MapPreview mapPrev = (MapPreview)target;

        if (DrawDefaultInspector())
        {
            if (mapPrev.autoUpdate)
            {
                mapPrev.DrawMapInEditor();
            }
        }
        
        // Add a button to trigger map update
        if (GUILayout.Button("Update Map"))
        {
            mapPrev.DrawMapInEditor();
        }
    }
    
}