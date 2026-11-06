Shader "Crystal/BlueCrystalShader"
{
    Properties
    {
        _Alpha("Alpha", Range(0.0, 1.0)) = 1.0
        _BaseColor("Crystal Color", Color) = (0.39, 0.7, 1.0, 1.0)
        _GradiendColorY("GCY", Color) = (0.0, 0.0, 0.0, 1.0)
        _DarkColor("Dark Color", Color) = (0.0, 0.28, 0.55, 1.0)
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

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
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings o;

                // 1. Получаем измененную позицию из общего файла
                //float3 grownPositionOS = CalculateCrystalGrowth(IN);

                // 2. Переносим трансформацию и UV сюда, в уникальный шейдер:
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                o.uv = IN.uv;

                o.darkness = saturate(IN.positionOS.z);

                return o;
            }

            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float SmoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
    
                // Плавное интерполирование (Smoothstep)
                float2 u = f * f * (3.0 - 2.0 * f);
    
                // Микшируем значения 4-х соседних точек
                return lerp(lerp(hash(i + float2(0.0, 0.0)), hash(i + float2(1.0, 0.0)), u.x),
                            lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return lerp(_BaseColor, _DarkColor, IN.darkness) * lerp(_GradiendColorY, _BaseColor, pow(IN.uv.y, 1.5)) * (1.0, 1.0, 1.0, _Alpha);
            }
            ENDHLSL
        }
    }
}
