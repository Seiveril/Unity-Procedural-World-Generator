using UnityEngine;

/// <summary>
/// Utility class to generate a falloff map for terrain generation.
/// </summary>
public static class FalloffGenerator
{
    /// <summary>
    /// Generates a falloff map based on specified parameters.
    /// </summary>
    /// <param name="_size">Size of the map (both width and height).</param>
    /// <param name="falloffStart">Distance at which falloff begins.</param>
    /// <param name="falloffEnd">Distance at which falloff ends.</param>
    /// <returns>2D array representing the falloff map.</returns>
    public static float[,] GenerateFalloffMap(int _size, float falloffStart, float falloffEnd)
    {
        Vector2Int size = new Vector2Int(_size, _size);

        float[,] map = new float[size.x, size.y];

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2 position = new Vector2(
                    (float)x / size.x * 2 - 1,
                    (float)y / size.y * 2 - 1
                );

                float value = Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.y));

                if (value < falloffStart)
                {
                    map[x, y] = 1;
                }
                else if (value > falloffEnd)
                {
                    map[x, y] = 0;
                }
                else
                {
                    map[x, y] = Mathf.SmoothStep(1, 0, Mathf.InverseLerp(falloffStart, falloffEnd, value));
                }
            }
        }

        return map;
    }
}