using UnityEngine;

/// <summary>
/// Structure holding level of detail and threshold.
/// </summary>
[System.Serializable]
public struct LODInfo
{
    [Tooltip("Level of detail.\nRanges from 0 (highest detail) to 4 (lowest detail).\nAffects the complexity of the mesh and performance.")]
    [Range(0, MeshSettings.numSupportedLODs-1)]
    public int lod;
    
    [Tooltip("Distance threshold for visibility.\nHigher values result in more chunks being visible.")]
    public float visibleDistanceThreshold;

    /// <summary>
    /// The visibility distance threshold squared.
    /// </summary>
    public float sqrVisibleDstThreshold
    {
        get
        {
            return visibleDistanceThreshold * visibleDistanceThreshold;
        }
    }

}