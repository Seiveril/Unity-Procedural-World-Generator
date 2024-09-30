using UnityEngine;

/// <summary>
/// Class responsible for displaying maps using textures and meshes.
/// </summary>
public class MapPreview : MonoBehaviour
{
    
    [Header("Scene Preview")]
    [Tooltip("Reference to the renderer used for displaying textures.")]
    public Renderer previewTextureRender;
    
    [Tooltip("Reference to the mesh filter used for displaying meshes.")]
    public MeshFilter previewMeshFilter;
    
    [Tooltip("The mesh preview material.\nUse the same as the map material.")]
    public Material terrainMaterial;
    
    [Space]
    
    [Header("DrawMode")]
    [Tooltip("Current draw mode.\nEach mode provides a different visual representation of the map data.")]
    public DrawMode drawMode;
    
    [Range(0, MeshSettings.numSupportedLODs -1)]
    [Tooltip("Level of detail for the editor preview.\nRanges from 0 (highest detail) to 4 (lowest detail).\nAffects the complexity and performance of the preview mesh.")]
    public int editorPreviewLOD;
    
    [Space]
    
    [Header("Settings Scriptable Objects")]
    [Tooltip("Scriptable object that contains the world height map (noise map) settings.")]
    public HeightMapSettings heightMapSettings;
    
    [Tooltip("Scriptable object that contains the chunks mesh settings.")]
    public MeshSettings meshSettings;
    
    [Tooltip("Scriptable object that contains the world texture settings.")]
    public TextureData textureData;
    
    [Space]
    
    [Header("Preview Update")]
    [Tooltip("Enable or disable auto-update.\nWhen enabled, changes in parameters automatically regenerate the map in the editor.")]
    public bool autoUpdate;
    
    private void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Draws a texture on the specified texture renderer.
    /// </summary>
    /// <param name="texture">Texture to be drawn.</param>
    private void DrawTexture(Texture2D texture)
    {
        previewTextureRender.sharedMaterial.mainTexture = texture;
        //previewTextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) /2.5f;
    }

    /// <summary>
    /// Draws a mesh using the provided mesh data.
    /// </summary>
    /// <param name="meshData">Mesh data used to create the mesh.</param>
    private void DrawMesh(MeshData meshData)
    {
        previewMeshFilter.sharedMesh = meshData.CreateMesh();
    }
    
    /// <summary>
    /// Draws the map in the editor based on the selected draw mode.
    /// </summary>
    public void DrawMapInEditor() {
        
        textureData.ApplyToMat(terrainMaterial);

        textureData.UpdateMeshHeight(terrainMaterial,heightMapSettings.minHeight, heightMapSettings.maxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine,meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine, heightMapSettings.falloffStart, heightMapSettings.falloffEnd), 0, 1)));
        }
        else if (drawMode == DrawMode.MeshAndNoise)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
    }
    
    /// <summary>
    /// Update the map in the editor.
    /// </summary>
    private void UpdateMapInEditor()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= UpdateMapInEditor;
            meshSettings.OnValuesUpdated += UpdateMapInEditor;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= UpdateMapInEditor;
            heightMapSettings.OnValuesUpdated += UpdateMapInEditor;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= UpdateMapInEditor;
            textureData.OnValuesUpdated += UpdateMapInEditor;
        }
    }
#endif
}