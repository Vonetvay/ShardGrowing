Shader "Custom/PaintShader"
{
    Properties
    {
        _SelfTexture2D ("Self Texture", 2D) = "black" {}
        _PaintPosition ("Center", Vector) = (0.5, 0.5, 0, 0)
        _BrushSize ("Brush Size", Float) = 0.05
        _BrushColor ("Brush Color", Color) = (1, 0, 0, 1)
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCustomRenderTexture.cginc"

            float4 _PaintPosition;
            float _BrushSize;
            float4 _BrushColor;

            // Свои чистые структуры, чтобы не зависеть от версий движка
            struct MyAttributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct MyVaryings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            MyVaryings vert(MyAttributes v)
            {
                MyVaryings o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(MyVaryings i) : SV_Target
            {
                // Извлекаем UV-координаты текущего пикселя
                float2 uv = i.uv;

                // Считываем предыдущее состояние текстуры из встроенной переменной _SelfTexture2D
                fixed4 col = tex2D(_SelfTexture2D, uv);

                // Вычисляем расстояние до кисти
                float dist = distance(uv, _PaintPosition.xy);

                if (dist < _BrushSize)
                {
                    // Создаем мягкие края для кисти
                    float mask = 1.0 - smoothstep(0.0, _BrushSize, dist);
                    
                    // Смешиваем цвета с учетом прозрачности кисти
                    col = lerp(col, _BrushColor, mask * _BrushColor.a);
                }

                return col;
            }
            ENDCG
        }
    }
}
