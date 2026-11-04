Shader "Custom/SquareToHouse"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _WallHeight("Wall Height Ratio", Range(0, 1)) = 0.7 // ¬ысота, где заканчиваютс€ стены и начинаетс€ крыша
        _RoofSharpness("Roof Sharpness", Range(0, 1)) = 1.0 // Ќасколько сильно сужаетс€ крыша к пику
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline" 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry" 
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _BaseColor;
            float _WallHeight;
            float _RoofSharpness;

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0; // »спользуем UV квадрата (0..1)
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                // 1. Ѕерем текущие локальные координаты вершины
                float3 pos = v.positionOS.xyz;

                // 2. ћаги€ создани€ крыши на основе UV:
                // v.uv.y Ч это высота нашей вершины от 0 (низ) до 1 (верх).
                // ≈сли вершина находитс€ выше уровн€ стен (_WallHeight), мы начинаем сужать еЄ к центру.
                if (v.uv.y > _WallHeight)
                {
                    // ¬ычисл€ем, насколько далеко вершина продвинулась в зону крыши (от 0.0 до 1.0)
                    float roofProgress = (v.uv.y - _WallHeight) / (1.0 - _WallHeight);

                    // ÷ентр нашего квадрата по оси X в дол€х UV равен 0.5.
                    // Ќаходим рассто€ние от текущей координаты X (в дол€х v.uv.x) до центра (0.5)
                    float directionToCenter = 0.5 - v.uv.x;

                    // —двигаем вершины X к центру (0.5) пропорционально высоте крыши
                    // ”множение на исходный шаг позиции (pos.x) сохранит пропорции меша
                    pos.x += directionToCenter * roofProgress * _RoofSharpness * (pos.x / (v.uv.x - 0.5 + 0.0001));
                    
                    // јльтернативный, более чистый способ сжати€ краев, если пивот меша строго в центре (0,0):
                    // „ем выше крыша (roofProgress -> 1), тем сильнее координата X множитс€ на (1 - roofProgress)
                    float shrink = 1.0 - (roofProgress * _RoofSharpness);
                    pos.x *= shrink;
                }

                // ѕереводим трансформированные координаты домика в пространство экрана
                o.positionCS = TransformObjectToHClip(pos);
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
