using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Generation/Mesh Settings")]

public class MeshSettings : UpdatableData
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;

    public static readonly int[] supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
    
    [Space]
    [Header("Mesh Settings")]
    
    [Tooltip("The scale of the chunk mesh.")]
    public float meshScale;
    
    [Tooltip("The size of a chunk.")]
    [Range(0, numSupportedChunkSizes-1)]
    public int chunkSizeIndex;
    
    [Space]
    
    [Tooltip("Enable or disable the use of flat shading.")]
    public bool useFlatShading;
    
    [Tooltip("The size of a flat shaded chunk.")]
    [Range(0, numSupportedFlatShadedChunkSizes-1)]
    public int flatShadedChunkSizeIndex;
    
    /// <summary>
    /// Number of vertices per line of a mesh rendered at LOD = 0. <br />
    /// Includes the 2 extra vertices that are excluded from the final mesh, but used for calculating the normals.
    /// </summary>
    public int numVertsPerLine {
        get
        {
            return supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] + 5;
        }
    }

    public float meshWorldSize
    {
        get
        {
            return (numVertsPerLine - 3) * meshScale;
        }
    }
    
}
