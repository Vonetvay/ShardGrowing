Shader "Custom/EarthCrystalShader"
{
    Properties
    {
        _Progress("Progress", Range(0.0, 1.0)) = 1.0

        _EdgeColor("EdgeColor", Color) = (0.588, 0.294, 0.0, 1.0)
        _CenterColor("CenterColor", Color) = (0.635, 0.396, 0.243, 1.0)
        _BackgroundBlackout("BackgroundBlackout", Color) = (0.3, 0.3, 0.3, 1.0)
        _GradientRadius ("Gradient Radius", Range(0.1, 1.0)) = 0.5
        _GradientHardness ("Gradient Hardness", Range(0.0, 5.0)) = 1.0

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
                float darkness : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            half4 _EdgeColor;
            half4 _CenterColor;
            float _GradientRadius;
            float _GradientHardness;
            half4 _BackgroundBlackout;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(CrystalAttributes IN)
            {
                Varyings o;

                // 1. Получаем измененную позицию из общего файла
                float3 grownPositionOS = CalculateCrystalGrowth(IN);

                o.darkness = saturate(grownPositionOS.z);

                // 2. Переносим трансформацию и UV сюда, в уникальный шейдер:
                o.positionHCS = TransformObjectToHClip(grownPositionOS.xyz);
                o.uv = IN.uv;

                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * (1.0, 1.0, 1.0, 1.0);

                half2 offset = abs(IN.uv - half2(0.5, 0.5));

                // 1. Вычисляем расстояние от текущей текстурной координаты до центра UV (0.5, 0.5)
                half dist = max(offset.x, offset.y);

                // 2. Нормализуем это расстояние относительно радиуса и применяем жесткость/интенсивность
                float t = saturate((dist * 2.0 / _GradientRadius) * _GradientHardness);

                // 3. Линейно интерполируем цвета: в центре (t=0) будет _CenterColor, на краях (t=1) — _EdgeColor
                half4 finalColor = lerp(_CenterColor, _EdgeColor, t);

                return color.a * finalColor * lerp((1.0, 1.0, 1.0, 1.0), _BackgroundBlackout, IN.darkness);
            }
            ENDHLSL
        }
    }
}
