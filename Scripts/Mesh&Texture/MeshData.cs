using UnityEngine;

/// <summary>
/// Class representing mesh data for generating and manipulating meshes.
/// </summary>
public class MeshData
{
    /// <summary>
    /// Array of vertices defining the geometry of the mesh.
    /// </summary>
    private Vector3[] vertices;

    /// <summary>
    /// Array of triangles defining the triangle connections of the mesh.
    /// </summary>
    private int[] triangles;

    /// <summary>
    /// Array of UV coordinates for texture mapping on the mesh.
    /// </summary>
    private Vector2[] uvs;

    /// <summary>
    /// Array of vertices defining the border geometry of the mesh.
    /// </summary>
    private Vector3[] outOfMeshVertices;
    
    /// <summary>
    /// Array of baked normals.
    /// </summary>
    private Vector3[] bakedNormals;

    /// <summary>
    /// Array of triangles defining the triangle connections of the border geometry.
    /// </summary>
    private int[] outOfMeshTriangles;

    /// <summary>
    /// Index tracking the current triangle being added to the mesh.
    /// </summary>
    private int triangleIndex;

    /// <summary>
    /// Index tracking the current triangle being added to the border geometry.
    /// </summary>
    private int outOfMeshTriangleIndex;
    
    /// <summary>
    /// If true, use flat shading.
    /// </summary>
    private bool useFlatShading;

    /// <summary>
    /// Constructor for MeshData class.
    /// </summary>
    /// <param name="numVertsPerLine">Number of vertices per line.</param>
    /// <param name="skipIncrement">Skip vertices based on the level of detail.</param>
    /// <param name="useFlatShading">If true, use flat shading.</param>
    public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeConnectionVerts = (skipIncrement - 1) * (numVertsPerLine - 5)/skipIncrement * 4;
        int numMainVertsPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
        int numMainVerts = numMainVertsPerLine * numMainVertsPerLine;
        
        vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVerts + numMainVerts];
        uvs = new Vector2[vertices.Length];

        int numMeshEdgeTriangles = 8*(numVertsPerLine - 4);
        int numMainTriangles = (numMainVertsPerLine - 1) * (numMainVertsPerLine - 1) * 2;
        
        triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];
        
        outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
        outOfMeshTriangles = new int[24 * (numVertsPerLine-2)];
    }

    /// <summary>
    /// Adds a vertex to the mesh data.
    /// </summary>
    /// <param name="vertexPosition">Position of the vertex.</param>
    /// <param name="uv">UV coordinate of the vertex.</param>
    /// <param name="vertexIndex">Index of the vertex.</param>
    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            // Add vertex to border vertices array
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            // Add vertex to main vertices array
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    /// <summary>
    /// Adds a triangle to the mesh data.
    /// </summary>
    /// <param name="a">Index of vertex A.</param>
    /// <param name="b">Index of vertex B.</param>
    /// <param name="c">Index of vertex C.</param>
    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            // Add triangle to border triangles array
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
            outOfMeshTriangleIndex += 3;
        }
        else
        {
            // Add triangle to main triangles array
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }
    
    /// <summary>
    /// Bake the mesh normals.
    /// </summary>
    private void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    /// <summary>
    /// Calculates vertex normals for the mesh.
    /// </summary>
    /// <returns>An array of vertex normals.</returns>
    Vector3[] CalculateNormals()
    {
        // Initialize array to hold vertex normals
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        // Calculate normals for triangles
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            // Calculate surface normal and add to vertex normals
            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Calculate normals for border triangles
        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            // Calculate surface normal and add to vertex normals (if not a border vertex)
            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
                vertexNormals[vertexIndexA] += triangleNormal;
            if (vertexIndexB >= 0)
                vertexNormals[vertexIndexB] += triangleNormal;
            if (vertexIndexC >= 0)
                vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Normalize vertex normals
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    /// <summary>
    /// Calculates the surface normal for a triangle defined by vertex indices.
    /// </summary>
    /// <param name="indexA">Index of vertex A.</param>
    /// <param name="indexB">Index of vertex B.</param>
    /// <param name="indexC">Index of vertex C.</param>
    /// <returns>The surface normal of the triangle.</returns>
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        // Determine vertices based on whether they are border vertices
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        // Calculate and return surface normal
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
    
    /// <summary>
    /// Process the mesh.
    /// </summary>
    public void ProcessMesh() {
        if (useFlatShading) {
            FlatShading ();
        } else {
            BakeNormals ();
        }
    }
    
    /// <summary>
    /// Flat shade the mesh.
    /// </summary>
    void FlatShading() {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            flatShadedVertices [i] = vertices [triangles [i]];
            flatShadedUvs [i] = uvs [triangles [i]];
            triangles [i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }

    /// <summary>
    /// Creates a Unity Mesh object from the mesh data.
    /// </summary>
    /// <returns>A Unity Mesh object generated from the mesh data.</returns>
    public Mesh CreateMesh()
    {
        // Create new Mesh object
        Mesh mesh = new Mesh();

        // Assign vertices, triangles, UVs, and normals to the Mesh object
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        if (useFlatShading) {
            mesh.RecalculateNormals ();
        } else {
            mesh.normals = bakedNormals;
        }

        // Return the created Mesh object
        return mesh;
    }
}
