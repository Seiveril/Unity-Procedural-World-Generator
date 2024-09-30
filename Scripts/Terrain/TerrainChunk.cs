using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a chunk of terrain in the endless terrain system.
/// </summary>
public class TerrainChunk
{
    /// <summary>
    /// Event triggered when the visibility of the terrain chunk changes. <br/>
    /// Provides the chunk and a boolean indicating visibility status. 
    /// </summary>
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged; 
    
    /// <summary>
    /// Mesh object of the terrain chunk.
    /// </summary>
    private GameObject meshObject;

    /// <summary>
    /// Position of the terrain chunk in the grid.
    /// </summary>
    private Vector2 sampleCentre;
    
    /// <summary>
    /// The grid coordinates of the terrain chunk.
    /// </summary>
    public Vector2 coord;

    /// <summary>
    /// Bounds of the terrain chunk.
    /// </summary>
    private Bounds bounds;

    /// <summary>
    /// Mesh renderer for graphical rendering.
    /// </summary>
    private MeshRenderer meshRenderer;

    /// <summary>
    /// Mesh filter for handling mesh geometry.
    /// </summary>
    private MeshFilter meshFilter;

    /// <summary>
    /// Mesh collider for collisions.
    /// </summary>
    private MeshCollider meshCollider;

    /// <summary>
    /// Detail levels (LODInfo) available for the terrain chunk.
    /// </summary>
    private LODInfo[] detailLevels;

    /// <summary>
    /// Array of LOD meshes for the terrain chunk.
    /// </summary>
    private LODMesh[] lodMeshes;

    /// <summary>
    /// LOD of Mesh collider.
    /// </summary>
    private int colliderLODIndex;

    /// <summary>
    /// Generated map data for the terrain chunk.
    /// </summary>
    private HeightMap heightMap;

    /// <summary>
    /// Indicates if map data has been received successfully.
    /// </summary>
    private bool heightMapReceived;

    /// <summary>
    /// Tracks whether the mesh collider has been set for the chunk.
    /// </summary>
    private bool hasSetCollider;

    /// <summary>
    /// Tracks whether the collision mesh has been updated.
    /// </summary>
    private bool hasUpdatedCollider;

    /// <summary>
    /// Indicates whether the chunk has been filled with spawnable objects.
    /// </summary>
    private bool filled;

    /// <summary>
    /// Index of the previous level of detail (LOD) used for managing updates.
    /// Initialized to -1.
    /// </summary>
    private int previousLODIndex = -1;

    /// <summary>
    /// The current LOD index being used for the chunk.
    /// </summary>
    private int currentLOD;

    /// <summary>
    /// The unique number identifier for this chunk.
    /// </summary>
    private int chunkNum;

    /// <summary>
    /// The maximum view distance at which the terrain chunk is visible.
    /// </summary>
    private float maxViewDst;
    
    /// <summary>
    /// The distance threshold for generating the collider mesh.
    /// </summary>
    private float colliderGenerationDistanceThreshold;

    /// <summary>
    /// The settings that define the height map generation for this terrain chunk.
    /// </summary>
    private HeightMapSettings heightMapSettings;

    /// <summary>
    /// The settings that define the mesh geometry for this terrain chunk.
    /// </summary>
    private MeshSettings meshSettings;

    /// <summary>
    /// An array of spawnable objects that can be placed within the terrain chunk.
    /// </summary>
    private Spawnable[] spawnables;

    /// <summary>
    /// A list of instantiated GameObjects within this chunk, created from spawnables.
    /// </summary>
    private List<GameObject> chunkPrefabs = new List<GameObject>();
    
    /// <summary>
    /// The reference to the viewer's Transform, used to calculate the distance from the chunk.
    /// </summary>
    private Transform viewer;

    /// <summary>
    /// Initializes a new instance of the TerrainChunk class.
    /// </summary>
    /// <param name="chunkNum">The number of the chunk.</param>
    /// <param name="coord">"The coordinates of the chunk.</param>
    /// <param name="heightMapSettings">The heightmap settings.</param>
    /// <param name="meshSettings">The mesh settings.</param>
    /// <param name="spawnables">The array of spawnables.</param>
    /// <param name="detailLevels">The array of LOD.</param>
    /// <param name="colliderLODIndex">The index of the collider.</param>
    /// <param name="cGDT">The threshold for the generation of the colliders.</param>
    /// <param name="parent">The chunk parent.</param>
    /// <param name="viewer">The viewer reference.</param>
    /// <param name="material">The chunk mesh material.</param>
    public TerrainChunk(int chunkNum, Vector2 coord,HeightMapSettings heightMapSettings, MeshSettings meshSettings,Spawnable[] spawnables, LODInfo[] detailLevels, int colliderLODIndex, float cGDT, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        colliderGenerationDistanceThreshold = cGDT;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.spawnables = spawnables;
        this.viewer = viewer;
        this.chunkNum = chunkNum;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);


        meshObject = new GameObject("Chunk " + chunkNum);
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];

        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;

            if (i == colliderLODIndex || i == 0)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
            
        }
        
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }
    /// <summary>
    /// Gets the current position of the viewer as a Vector2, ignoring the y-axis.<br/>
    /// Used to calculate the distance from the viewer to the chunk. 
    /// </summary>
    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    /// <summary>
    /// Initiates the loading of the terrain chunk by requesting height map data.
    /// </summary>
    public void Load()
    {
        MultiThreading.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
    }

    /// <summary>
    /// Callback method that is invoked when height map data is received.<br/>
    /// Processes the data and updates the terrain chunk mesh.
    /// </summary>
    /// <param name="heightMapObject">Received map data.</param>
    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        
        UpdateTerrainChunk();
        
    }

    /// <summary>
    /// Fills the terrain chunk with spawnable objects based on the spawnables array.<br/>
    /// Ensures that objects are only spawned once.
    /// </summary>
    private void ChunkFill()
    {
        if (!filled)
        {
            filled = true;
            if (spawnables != null)
            {
                foreach (var spawnable in spawnables)
                {
                    for (int i = 0; i < spawnable.density; i++)
                    {
                        CraftPrefab(spawnable);
                    }
                } 
            }
        }
    }
    
    /// <summary>
    /// Asks the Prefab Placer class to instantiate the prefabs.
    /// </summary>
    /// <param name="spawnable">The spawnable to create.</param>
    private void CraftPrefab(Spawnable spawnable)
    {
        for (int i = 0; i < spawnable.density; i++)
        {
            PrefabPlacer.CreatePrefabs(spawnable, coord * meshSettings.meshWorldSize, meshObject.transform, chunkNum, meshSettings, OnPrefabCrafted);
        }
    }

    /// <summary>
    /// Add the object to the list.
    /// </summary>
    /// <param name="prefabObject">GameObject (prefab instantiated).</param>
    private void OnPrefabCrafted(object prefabObject)
    {
        if ((GameObject)prefabObject)
        {
            chunkPrefabs.Add((GameObject)prefabObject);
        }
    }

    /// <summary>
    /// Updates the terrain chunk based on viewer's distance and LOD.
    /// </summary>
    public void UpdateTerrainChunk()
    {

        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            bool wasVisible = IsVisible();

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                currentLOD = lodIndex;

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }

                if (currentLOD != 0)
                {
                    SetChunkPrefabsActive(false);
                }
                else
                {
                    if (filled)
                    {
                        SetChunkPrefabsActive(true);
                    }
                }
            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                
                if (OnVisibilityChanged != null)
                {
                    OnVisibilityChanged(this, visible);
                }
            }
        }
    }
    
    /// <summary>
    /// Activate or deactivate the prefabs based on the LOD.
    /// </summary>
    /// <param name="isActive">Boolean to activate or deactivate the prefabs.</param>
    private void SetChunkPrefabsActive(bool isActive)
    {
        foreach (GameObject prefab in chunkPrefabs)
        {
            prefab.SetActive(isActive);
        }
    }

    /// <summary>
    /// Updates the chunk collision mesh.
    /// </summary>
    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }
            
            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                    
                    if (!filled)
                    {
                        ChunkFill();
                    }
                    else
                    {
                        SetChunkPrefabsActive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets the visibility of the terrain chunk.
    /// </summary>
    /// <param name="visible">True to make the chunk visible, false to hide it.</param>
    private void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    /// <summary>
    /// Checks if the terrain chunk is currently visible.
    /// </summary>
    /// <returns>True if the chunk is visible, otherwise false.</returns>
    private bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}
