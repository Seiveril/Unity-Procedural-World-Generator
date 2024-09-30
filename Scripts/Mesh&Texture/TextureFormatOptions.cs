using UnityEngine;

/// <summary>
/// Enumeration of available texture format options. 
/// Each option corresponds to a specific texture format used by Unity's TextureFormat class.
/// </summary>
public enum TextureFormatOptions
{
    /// <summary>
    /// RGB565 texture format, 16-bit color format with 5 bits for red, 6 bits for green, and 5 bits for blue.
    /// </summary>
    RGB565 = TextureFormat.RGB565,

    /// <summary>
    /// ARGB32 texture format, 32-bit color format with 8 bits for each of the alpha, red, green, and blue channels.
    /// </summary>
    ARGB32 = TextureFormat.ARGB32,

    /// <summary>
    /// RGBA32 texture format, 32-bit color format with 8 bits for each of the red, green, blue, and alpha channels.
    /// </summary>
    RGBA32 = TextureFormat.RGBA32,

    /// <summary>
    /// RGB24 texture format, 24-bit color format with 8 bits for each of the red, green, and blue channels.
    /// </summary>
    RGB24 = TextureFormat.RGB24
}