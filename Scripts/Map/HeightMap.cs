/// <summary>
/// Represents a height map containing height values along with minimum and maximum height information.
/// </summary>
public struct HeightMap
{
    /// <summary>
    /// A 2D array of float values representing the heights at different points in the map.
    /// </summary>
    public readonly float[,] values;
    
    /// <summary>
    /// The minimum height value found in the height map.
    /// </summary>
    public readonly float minValue;
    
    /// <summary>
    /// The maximum height value found in the height map.
    /// </summary>
    public readonly float maxValue;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HeightMap"/> struct with the specified values, minimum, and maximum height.
    /// </summary>
    /// <param name="values">A 2D array of float values representing the heights in the height map.</param>
    /// <param name="minValue">The minimum height value in the height map.</param>
    /// <param name="maxValue">The maximum height value in the height map.</param>
    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}