using UnityEngine;

/// <summary>
/// Utility class for generating procedural noise maps using Perlin noise. <br/>
/// This class provides a method to create 2D noise maps, which can be used for terrain generation, texture creation, and other procedural content.<br/>
/// It supports multiple octaves, customizable noise settings, and normalization modes.
/// </summary>
public static class Noise
{
    /// <summary>
    /// Generates a 2D noise map based on specified parameters such as map dimensions, noise settings, and the position of the sample center.<br/>
    /// The resulting noise map can be normalized either globally or locally.
    /// </summary>
    /// <param name="mapWidth">The width of the noise map in pixels.</param>
    /// <param name="mapHeight">The height of the noise map in pixels.</param>
    /// <param name="settings">An instance of <see cref="NoiseSettings"/> that contains parameters like scale, octaves, and persistence.</param>
    /// <param name="sampleCentre">The center point of the sample area for the noise generation, affecting offset calculations.</param>
    /// <returns>A 2D array of floats representing the generated noise map.</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        // Initialize the 2D array to store the noise map
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Create a random number generator based on the provided seed
        System.Random prng = new System.Random(settings.seed);

        // Array to store offsets for each octave
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        // Variables to calculate maximum possible height
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        // Loop through octaves to calculate offsets and maximum possible height
        for (int i = 0; i < settings.octaves; i++)
        {
            // Generate random offsets for each octave
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            // Accumulate amplitude for each octave to calculate maximum possible height
            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence; // Decrease amplitude with each octave
        }

        // Variables to track local minimum and maximum noise heights
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // Calculate half dimensions of the map
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Loop through all pixels in the noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                // Calculate noise value for each octave and accumulate height
                for (int i = 0; i < settings.octaves; i++)
                {
                    // Calculate sample coordinates for current octave
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.noiseScale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.noiseScale * frequency;

                    // Generate Perlin noise value and scale to range [-1, 1]
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Accumulate noise height with amplitude
                    noiseHeight += perlinValue * amplitude;

                    // Decrease amplitude and increase frequency for next octave
                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }

                // Track local min and max noise heights
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                // Store the calculated noise height in the noise map
                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    // Normalize to range [0, 1] based on max possible height
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (settings.normalizeMode == NormalizeMode.Local)
        {
            // Normalize the noise map based on chosen normalization mode
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // Normalize to range [0, 1] based on local min and max heights
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        // Return the generated noise map
        return noiseMap;
    }
}
