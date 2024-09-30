using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Generation/Heightmap Settings")]
public class HeightMapSettings : UpdatableData
{
    [Space]
    [Header("Noise Settings")]
    public NoiseSettings noiseSettings;
    
    [Space]
    [Header("Falloff Settings")]
    
    [Tooltip("Enable or disable the use of falloff map.\nWhen enabled, the falloff map is applied to smooth out the terrain transitions.")]
    public bool useFalloff;
    
    [Range(0, 1)]
    [Tooltip("Start value for the falloff map.\nDetermines at what height the falloff effect begins.\nFalloff maps create smoother transitions between different terrain types.")]
    public float falloffStart;
    
    [Range(0, 1)]
    [Tooltip("End value for the falloff map.\nDetermines at what height the falloff effect ends.\nEnsures a smooth transition and blending of terrain features.")]
    public float falloffEnd;
    
    [Space]
    [Header("HeightMap Settings")]
    
    [Tooltip("Multiplier for the height of the mesh.\nAffects the vertical scale of the generated terrain.\nHigher values create taller terrain, while lower values create flatter terrain.")]
    public float HeightMultiplier;
    
    [Tooltip("Animation curve for the height of the mesh.\nDefines how the height values are interpolated, allowing for more complex terrain shapes.")]
    public AnimationCurve HeightCurve;
    
    public float minHeight
    {
        get
        {
            return HeightMultiplier * HeightCurve.Evaluate(0);
        }
    }
    
    public float maxHeight
    {
        get
        {
            return HeightMultiplier * HeightCurve.Evaluate(1);
        }
    }
    
#if UNITY_EDITOR
    
    /// <summary>
    /// Validates the input values to ensure they are within acceptable ranges.
    /// </summary>
    private void OnValidate()
    {
        noiseSettings.ValidateValues();
        
        if (HeightMultiplier < 0)
        {
            HeightMultiplier = 0;
        }
        if (falloffEnd <falloffStart)
        {
            falloffEnd = falloffStart;
        }
    }
    
#endif
}
