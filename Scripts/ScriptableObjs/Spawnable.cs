using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Generation/Spawnable")]
public class Spawnable : ScriptableObject
{
    [Header("Make sure the prefab is marked as Addressable!")]
    [Space]
    [Header("Prefab path")]
    [Tooltip("Copy here the prefab path after it being marked as Addressable.")]
    public string prefabPath;
    
    [Space]
    [Header("Water settings")]
    [Space(-10)]
    [Header("(leave blank if the prefab is not water)")]
    [Space(5)]
    [Tooltip("True if the prefab is water.")]
    public bool isWater;
    [Tooltip("The height at which the water will be generated.\nThe prefab will spawn at the center of the chunks with LOD 0, at the custom height.\nMake sure the density is set to 1.")]
    public float waterWorldHeight;
    
    [Space]
    
    [Header("Prefab Variation Settings")]
    [Tooltip("How many prefab will try to spawn per chunk.")]
    public int density;
    [Tooltip("The minimum world height for the prefab to spawn.")]
    public float minWorldHeight;
    [Tooltip("The maximum world height for the prefab to spawn.")]
    public float maxWorldHeight;
    [Tooltip("The rotation towards the mesh normal.")]
    [Range(0, 1)] public float rotateTowardsNormal;
    [Tooltip("The prefab rotation range.")]
    public Vector2 rotationRange;
    [Tooltip("The minimum prefab scale.")]
    public Vector3 minScale;
    [Tooltip("The maximum prefab scale.")]
    public Vector3 maxScale;

}
