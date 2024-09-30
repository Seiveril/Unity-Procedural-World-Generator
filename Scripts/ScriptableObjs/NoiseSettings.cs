using UnityEngine;

[System.Serializable]
public struct NoiseSettings
{
    
    [Tooltip("Scale of the noise map.\nLarger values zoom out the noise pattern, creating larger, more gradual features.\nSmaller values zoom in, creating smaller, more detailed features.")]
    public float noiseScale;
    
    [Range(0, 1)]
    [Tooltip("Persistence determines the amplitude of each octave relative to the previous one.\nLower values result in smoother noise, while higher values create rougher noise.")]
    public float persistence;
    
    [Range(1, 4)]
    [Tooltip("Lacunarity determines the frequency of each octave relative to the previous one.\nHigher values increase the frequency, resulting in more rapid changes in the noise pattern.")]
    public float lacunarity;
    
    [Range(1, 21)]
    [Tooltip("Number of octaves.\nHigher values result in more complex and detailed noise patterns.\nEach octave is a layer of noise.")]
    public int octaves;
    
    [Tooltip("Changing the seed will result in a different map.")]
    public int seed;
    
    [Tooltip("Offset for the noise map.\nShifts the noise pattern by the specified amount in the X and Y directions.\nUseful for creating variations of the same map without changing other parameters.")]
    public Vector2 offset;
    
    [Tooltip("Current normalize mode.\nDetermines how the generated noise values are normalized.")]
    public NormalizeMode normalizeMode;
    
    public void ValidateValues()
    {
        noiseScale = Mathf.Max(noiseScale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistence = Mathf.Clamp01(persistence);
    }

}
