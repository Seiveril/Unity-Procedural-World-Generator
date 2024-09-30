using UnityEngine;

/// <summary>
/// Static class responsible for generating textures from color maps or height maps.
/// </summary>
public static class TextureGenerator
{
    /// <summary>
    /// Generates a Texture2D from a given color map.
    /// </summary>
    /// <param name="colourMap">Array of colors representing the texture.</param>
    /// <param name="width">Width of the texture.</param>
    /// <param name="height">Height of the texture.</param>
    /// <returns>Generated Texture2D object.</returns>
    private static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        // Create new Texture2D object
        Texture2D texture = new Texture2D(width, height);

        // Set texture filtering and wrapping modes
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Set pixels of the texture from the provided color map and apply changes
        texture.SetPixels(colourMap);
        texture.Apply();

        // Return the generated Texture2D object
        return texture;
    }

    /// <summary>
    /// Generates a Texture2D from a given height map.
    /// </summary>
    /// <param name="heightMap">2D array representing the height map.</param>
    /// <returns>Generated Texture2D object.</returns>
    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        // Get dimensions of the height map
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        // Initialize array to hold colors for the texture
        Color[] colourMap = new Color[width * height];

        // Populate color map based on height map values
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Lerp between black and white based on height map value
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue,heightMap.values[x,y]));
            }
        }

        // Generate and return Texture2D from the color map
        return TextureFromColourMap(colourMap, width, height);
    }
}
