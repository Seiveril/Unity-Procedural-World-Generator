using UnityEngine;

[System.Serializable]
public struct Layer
{
    [Tooltip("Drag the texture here.\nMake sure the texture is set to 'Read/Write Enabled' in the texture settings.")]
    public Texture2D texture;

    [Tooltip("Optional color tint applied to the texture.\nThis allows you to adjust the texture's color without modifying the texture itself.")]
    public Color tint;

    [Tooltip("Controls how much the tint color will affect the texture's original colors.\nA value of 0 means no tint, while a value of 1 means full tint.")]
    [Range(0, 1)]
    public float tintStrength;

    [Tooltip("The vertical position on the mesh where this texture starts to appear.\nValues range from 0 (bottom of the mesh) to 1 (top of the mesh).")]
    [Range(0, 1)]
    public float startHeight;

    [Tooltip("Defines how smoothly this texture blends with the next one.\nHigher values create smoother transitions between textures, while lower values create sharper transitions.")]
    [Range(0, 1)]
    public float blendStrength;

    [Tooltip("The scale of the texture on the mesh.\nLarger values make the texture appear larger and more stretched, while smaller values make the texture appear smaller and more repeated.")]
    public float textureScale;
}