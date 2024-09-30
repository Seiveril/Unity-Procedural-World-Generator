using UnityEngine;

/// <summary>
/// Static class responsible for generating mesh data based on a terrain height map.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Generates a mesh for terrain based on the provided height map and settings.
    /// </summary>
    /// <param name="heightMap">The 2D array of height values used to generate the mesh.</param>
    /// <param name="meshSettings">Settings for the mesh generation including number of vertices per line, world size, etc.</param>
    /// <param name="levelOfDetail">Level of detail for the mesh; higher values result in lower detail and vice versa.</param>
    /// <returns>Returns a MeshData object containing the generated mesh data.</returns>
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        // Determine the increment to skip vertices based on the level of detail
        int skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numVertsPerLine = meshSettings.numVertsPerLine;

        // Define the top-left corner of the mesh in world space
        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        // Initialize MeshData object
        MeshData meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

        // Create a map to track vertex indices
        int[][] vertexIndicesMap = new int[numVertsPerLine][];

        // Initialize each row of the vertex index map
        for (int i = 0; i < numVertsPerLine; i++)
        {
            vertexIndicesMap[i] = new int[numVertsPerLine];
        }

        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        // Assign vertex indices and handle special cases like out-of-mesh vertices
        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 &&
                                       ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (isOutOfMeshVertex)
                {
                    vertexIndicesMap[x][y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if (!isSkippedVertex)
                {
                    vertexIndicesMap[x][y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        // Generate vertices and triangles for the mesh
        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 &&
                                       ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                if (!isSkippedVertex)
                {
                    bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 &&
                                        !isOutOfMeshVertex && !isMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) &&
                                                   !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndicesMap[x][y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                    float height = heightMap[x, y];

                    // Handle edge connection vertices by interpolating between main vertices
                    if (isEdgeConnectionVertex)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                        int dstToMainVertexB = skipIncrement - dstToMainVertexA;
                        float dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

                        float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];

                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }

                    // Add the vertex to the mesh data
                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                    // Create triangles for the mesh if not on an edge connection vertex
                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

                        int a = vertexIndicesMap[x][y];
                        int b = vertexIndicesMap[x + currentIncrement][y];
                        int c = vertexIndicesMap[x][y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement][y + currentIncrement];
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                }
            }
        }

        // Finalize and return the generated mesh data
        meshData.ProcessMesh();

        return meshData;
    }
}
