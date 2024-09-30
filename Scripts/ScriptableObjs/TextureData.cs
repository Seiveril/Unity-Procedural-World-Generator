using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Terrain Generation/Texture Data")]
public class TextureData : UpdatableData
{
    [Space]
    [Header("Texture Settings")]
    [Tooltip("Adjust this parameter to match the resolution of the textures.\nAll the texture must have the same resolution in order to work.")]
    public TextureResolutionOptions textureResolution;
    
    [Tooltip("Choose the texture format.")]
    public TextureFormatOptions textureFormatOption;
    
    [Space]
    [Header("Texture Layers")]
    public Layer[] layers;
    
    private float savedMinHeight;
    private float savedMaxHeight;
    
    public void ApplyToMat(Material material)
    {
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScale", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray textureArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", textureArray);
        
        UpdateMeshHeight(material, savedMinHeight, savedMaxHeight);   
    }

    public void UpdateMeshHeight(Material mat, float minHeight, float maxHeight)
    {
        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;
        
        mat.SetFloat("minHeight", minHeight);
        mat.SetFloat("maxHeight", maxHeight);
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        int textureSize = (int)textureResolution;
        TextureFormat textureFormat = (TextureFormat)textureFormatOption;
        
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);

        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();

        return textureArray;
    }
}
