using UnityEngine;

/// <summary>
/// Static class responsible for generating height maps based on noise and settings.
/// </summary>
public static class HeightMapGenerator
{
    /// <summary>
    /// Generates a height map using noise, settings, and sample center.
    /// </summary>
    /// <param name="width">The width of the height map.</param>
    /// <param name="height">The height of the height map.</param>
    /// <param name="settings">Settings used for generating the height map, including noise settings and height curve.</param>
    /// <param name="sampleCenter">The center position for sampling the noise map.</param>
    /// <returns>A HeightMap object containing the generated height values, minimum, and maximum height values.</returns>
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        // Generate a noise map based on the given dimensions, noise settings, and sample center.
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);

        // Copy the provided height curve to ensure thread safety and avoid modifying the original curve.
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.HeightCurve.keys);

        // Initialize variables to track the minimum and maximum values in the height map.
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Generate a falloff map which reduces height values near the edges based on specified start and end points.
        float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(width, settings.falloffStart, settings.falloffEnd);

        // Process each value in the height map.
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Evaluate the height value using the height curve and apply height multiplier.
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.HeightMultiplier;

                // If falloff is enabled, apply the falloff map to adjust height values.
                if (settings.useFalloff)
                {
                    values[i, j] *= falloffMap[i, j];
                }

                // Update the minimum and maximum height values encountered.
                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        // Return a HeightMap object with the processed height values, minimum, and maximum height values.
        return new HeightMap(values, minValue, maxValue);
    }
}
