Shader "Custom/Terrain" // Define a custom shader named "Custom/Terrain"
{
    Properties
    {
        // A texture property for the shader, named "testTexture"
        // It is a 2D texture, with a default value of "white"
        testTexture("Texture", 2D) = "white"{}
        
        // A float property named "testScale" to control the scaling of the texture
        testScale("Scale", Float) = 1
    }

    SubShader
    {
        // Define the type of rendering. "Opaque" is for non-transparent objects
        Tags { "RenderType"="Opaque" }

        // Set the level of detail (LOD) of the shader, which helps control the complexity of the shader
        LOD 200

        CGPROGRAM // Start the CG program block

        // Specify that this is a surface shader using the Standard Physically Based Rendering (PBR) model
        // Full forward shadows means it supports shadows in the forward rendering path
        #pragma surface surf Standard fullforwardshadows

        // Set the shader target to model 3.0, enabling nicer lighting features
        #pragma target 3.0

        // Define some constant variables for layer handling and precision
        // Maximum number of layers allowed for terrain textures
        const static int maxLayerCount = 8;
        // A small epsilon value to avoid precision issues
        const static float epsilon = 1E-4;  

        // Declare variables to control the layers of the terrain
        // Number of layers in use
        int layerCount;
        // Array of base colors for each layer
        float3 baseColours[maxLayerCount];
        // Array of starting heights for each layer
        float baseStartHeights[maxLayerCount];
        // Array controlling how much the layers blend with each other
        float baseBlends[maxLayerCount];
        // Array controlling the strength of each base color
        float baseColourStrength[maxLayerCount];
        // Array controlling the texture scale for each layer
        float baseTextureScale[maxLayerCount]; 

        // Variables to define the height range of the terrain
        // Minimum height of the terrain
        float minHeight;
        // Maximum height of the terrain
        float maxHeight; 

        // Sampler for the texture provided by the user
        sampler2D testTexture; // Test texture (2D)
        float testScale; // Scaling factor for the test texture

        // Declare a 2D texture array (array of textures) for the base layers
        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        // Struct defining the input to the surface shader
        struct Input 
        {
             // The world position of the fragment
            float3 worldPos;
            // The world normal at that fragment
            float3 worldNormal; 
        };

        // Custom inverse lerp function to normalize a value between a and b
        float inverseLerp(float a, float b, float value)
        {
            // saturate ensures the result is clamped between 0 and 1
            return saturate((value - a) / (b - a)); 
        }

        // Triplanar mapping function to apply textures based on the three world axes
        float3 tirplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex)
        {
            // Scale the world position by the given scale factor
            float3 scaledWorldPos = worldPos / scale;
                
            // Sample the texture from the 2D texture array using the y-z plane projection
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;

            // Sample the texture from the 2D texture array using the x-z plane projection
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;

            // Sample the texture from the 2D texture array using the x-y plane projection
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            
            // Return the weighted sum of the three projections based on the blend axes
            return xProjection + yProjection + zProjection;
        }

        // Main surface function that defines the color and properties of the surface
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate the height percentage relative to min and max height, adding epsilon to avoid precision issues
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y + epsilon);

            // Calculate the blending axes based on the absolute value of the world normal
            float3 blendAxes = abs(IN.worldNormal);

            // Normalize the blend axes so that their sum equals 1
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
            
            // Iterate over each layer in the terrain
            for(int i = 0; i < layerCount; i++)
            {
                // Calculate the strength of the current layer based on its blend range and the current height
                float drawStrenght = inverseLerp(-baseBlends[i] / 2 - epsilon, baseBlends[i] / 2, (heightPercent - baseStartHeights[i]));

                // Multiply the base color of the current layer by its strength
                float3 baseColour = baseColours[i] * baseColourStrength[i];

                // Get the texture color using triplanar mapping, blending it with the base color
                float3 textureColour = tirplanar(IN.worldPos, baseTextureScale[i], blendAxes, i) * (1 - baseColourStrength[i]); 
                
                // Combine the existing color with the new layer color based on the drawing strength
                o.Albedo = (o.Albedo * (1 - drawStrenght)) + ((baseColour + textureColour) * drawStrenght);
            }
        }
        // End of the CG program block
        ENDCG
    }
    // Fallback shader in case this shader is unsupported, uses a basic diffuse shader
    FallBack "Diffuse"
}
