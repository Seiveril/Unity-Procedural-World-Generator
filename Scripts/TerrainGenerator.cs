using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates and manages terrain chunks based on viewer position for endless terrain generation.
/// </summary>
public class TerrainGenerator : Singleton<TerrainGenerator>
{
    [Space]
    [Header("Mesh Collider Settings")]
    [Tooltip("Which element of the 'Detail Levels' list is used for the mesh collider of the chunk.")]
    public int colliderLODIndex;

    [Tooltip("Chunk within this threshold (distance from the nearest border to the player) will have a mesh collider.")]
    public float colliderGenerationDT;
    
    [Space]
    [Tooltip("List of levels of detail.\nRanges from 0 (highest detail) to 4 (lowest detail).\nAffects the complexity of the mesh and performance.")]
    public LODInfo[] detailLevels;
    
    [Header("Player Reference")]
    [Tooltip("Player Reference (Transform)")]
    public Transform player;
    
    [Space, Header("Map Material")]
    [Tooltip("Map Material (Apply Terrain Shader)")]
    public Material mapMaterial;
    
    [Space, Header("Scriptable Objects")]
    [Tooltip("Scriptable object that contains the world height map (noise map) settings.")]
    public HeightMapSettings heightMapSettings;

    [Tooltip("Scriptable object that contains the chunks mesh settings.")]
    public MeshSettings meshSettings;

    [Tooltip("Scriptable object that contains the world texture settings.")]
    public TextureData textureSettings;

    [Tooltip("Array of every spawnable that has to be instantiated in the world map.")]
    public Spawnable[] spawnables;
    
    /// <summary>
    /// Distance threshold for triggering chunk updates based on viewer movement.
    /// </summary>
    private const float viewerMoveThresholdForChunkUpdate = 25f;

    /// <summary>
    /// Squared distance threshold for chunk updates, avoids computing square root.
    /// </summary>
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    
    /// <summary>
    /// Size of each terrain chunk.
    /// </summary>
    private float meshWorldSize;

    /// <summary>
    /// Number of visible chunks around the viewer.
    /// </summary>
    private int chunksVisibleInViewDst;

    /// <summary>
    /// Number of the chunk, 1 at the start.
    /// </summary>
    private int chunkNum = 1;

    /// <summary>
    /// Current position of the viewer on the terrain.
    /// </summary>
    private Vector2 viewerPosition;

    /// <summary>
    /// Previous position of the viewer, used for detecting movement.
    /// </summary>
    private Vector2 viewerPositionOld;

    /// <summary>
    /// Dictionary mapping chunk coordinates to TerrainChunk objects.
    /// </summary>
    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    /// <summary>
    /// List of terrain chunks that were visible in the last update.
    /// </summary>
    private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        textureSettings.ApplyToMat(mapMaterial);
        textureSettings.UpdateMeshHeight(mapMaterial,heightMapSettings.minHeight, heightMapSettings.maxHeight);
        
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(player.position.x, player.position.z);

        //If the player moved
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    /// <summary>
    /// Updates the visibility and generation of terrain chunks based on viewer position.
    /// </summary>
    private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoord = new HashSet<Vector2>();
        
        
        // Disable chunks that were visible in the last update
        for (int i = visibleTerrainChunks.Count -1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoord.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        // Calculate current chunk coordinates based on viewer's position
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        // Iterate through visible chunks and update or create them
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (!alreadyUpdatedChunkCoord.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(chunkNum, viewedChunkCoord, heightMapSettings, meshSettings, spawnables, detailLevels, colliderLODIndex, colliderGenerationDT, transform,player, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        chunkNum++;
                        newChunk.OnVisibilityChanged += OnterrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update the visibility of a chunk.
    /// </summary>
    /// <param name="chunk">Reference to the chunk.</param>
    /// <param name="isVisible">Boolean for the visibility.</param>
    private void OnterrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}
