using UnityEngine;

/// <summary>
/// Class representing a Level of Detail (LOD) mesh.
/// </summary>
public class LODMesh
{
    /// <summary>
    /// The mesh associated with this LOD.
    /// </summary>
    public Mesh mesh;

    /// <summary>
    /// Indicates if a mesh request has been made.
    /// </summary>
    public bool hasRequestedMesh;

    /// <summary>
    /// Indicates if the mesh data has been received and set.
    /// </summary>
    public bool hasMesh;

    /// <summary>
    /// Level of detail index.
    /// </summary>
    private int lod;
    
    /// <summary>
    /// Callback function to be called when mesh is updated.
    /// </summary>
    public event System.Action updateCallback;

    /// <summary>
    /// Constructor for LODMesh class.
    /// </summary>
    /// <param name="lod">Level of detail index.</param>
    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    /// <summary>
    /// Callback method called when mesh data is received.
    /// </summary>
    /// <param name="meshDataObject">Mesh data received.</param>
    private void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        // Invoke the update callback to notify that mesh has been updated
        updateCallback();
    }

    /// <summary>
    /// Requests mesh data based on provided map data.
    /// </summary>
    /// <param name="heightMap">Map data used to request mesh.</param>
    /// <param name="meshSettings">Mesh settings used to request mesh.</param>
    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        MultiThreading.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}