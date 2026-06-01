Shader "Custom/HoverOutline"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        // Standard alpha blending for 2D sprites
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            // Unity automatically populates this with texture dimensions
            float4 _MainTex_TexelSize; 

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                // Multiply vertex color (from SpriteRenderer) with the material tint
                output.color = input.color * _BaseColor;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample the base sprite color
                float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                // For precise pixel art, keep _OutlineWidth at exactly 1
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;

                // 4-way orthogonal neighbors
                float alphaUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, texelSize.y)).a;
                float alphaDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -texelSize.y)).a;
                float alphaLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, 0)).a;
                float alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, 0)).a;

                // 4-way diagonal neighbors
                float alphaUpLeft    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, texelSize.y)).a;
                float alphaUpRight   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, texelSize.y)).a;
                float alphaDownLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-texelSize.x, -texelSize.y)).a;
                float alphaDownRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(texelSize.x, -texelSize.y)).a;

                // Find the highest alpha value among all 8 neighbors
                float maxOrthogonal = max(max(alphaUp, alphaDown), max(alphaLeft, alphaRight));
                float maxDiagonal = max(max(alphaUpLeft, alphaUpRight), max(alphaDownLeft, alphaDownRight));
                float maxNeighborAlpha = max(maxOrthogonal, maxDiagonal);

                // Flag pixel if it's currently transparent but adjacent to an opaque pixel
                float isOutline = saturate(maxNeighborAlpha - baseColor.a);

                // Blend between base color and outline color
                float4 finalColor = lerp(baseColor, _OutlineColor, isOutline);
                
                // Combine alphas so the outline respects the overall sprite alpha
                finalColor.a = max(baseColor.a, maxNeighborAlpha) * input.color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}