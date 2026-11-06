Shader "Custom/AnimationTestShader"
{
    Properties
    {
        _Progress("Progress", Range(0.0, 1.0)) = 1.0
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/CrystalGrowth.compute"

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(CrystalAttributes IN)
            {
                Varyings o;

                // 1. Получаем измененную позицию из общего файла
                float3 grownPositionOS = NewCalculateCrystalGrowth(IN);

                // 2. Переносим трансформацию и UV сюда, в уникальный шейдер:
                o.positionHCS = TransformObjectToHClip(grownPositionOS.xyz);
                o.uv = IN.uv;

                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
