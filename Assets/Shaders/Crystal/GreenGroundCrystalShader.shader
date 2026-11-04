Shader "Crystal/BlueCrystalShader"
{
    Properties
    {
        _Progress("Progress", Range(0.0, 1.0)) = 1.0
        _BaseColor("Crystal Color", Color) = (0.18, 0.89, 0.58, 1.0)
        _GradiendColorY("GCY", Color) = (0.0, 0.0, 0.0, 1.0)
        _DarkColor("Dark Color", Color) = (0.07, 0.35, 0.23, 1.0)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

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
                float darkness : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GradiendColorY;
                float4 _DarkColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(CrystalAttributes IN)
            {
                Varyings o;

                // 1. Получаем измененную позицию из общего файла
                float3 grownPositionOS = CalculateCrystalGrowth(IN);

                // 2. Переносим трансформацию и UV сюда, в уникальный шейдер:
                o.positionHCS = TransformObjectToHClip(grownPositionOS.xyz);
                o.uv = IN.uv;

                o.darkness = saturate(grownPositionOS.z);

                return o;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return lerp(_BaseColor, _DarkColor, IN.darkness) * lerp(_GradiendColorY, _BaseColor, IN.uv.y);
            }
            ENDHLSL
        }
    }
}
