Shader "Custom/CrystalCrystalGrow"
{
    Properties
    {
        _BaseColor("Crystal Color", Color) = (0.39, 0.7, 1.0, 1.0)
        _GradiendColorY("GCY", Color) = (0.0, 0.0, 0.0, 1.0)
        _DarkColor("Dark Color", Color) = (0.0, 0.28, 0.55, 1.0)
        _StartZ("StartZ", Float) = 0.0
        _EndZ("EndZ", Float) = 1.0
        _Progress("Progress", Range(0.0, 1.0)) = 0.0
        _Size("Size", Float) = 1.0
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
            float4 _GradiendColorY;
            float4 _DarkColor;
            float _StartZ;
            float _EndZ;
            float _Progress;
            float _Size;

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv3 : TEXCOORD2;
                float2 uv4 : TEXCOORD3;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float darkness : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                float range = _EndZ - _StartZ;
                if (abs(range) < 0.0001) range = 1.0;


                o.darkness = saturate((v.positionOS.z - _StartZ) / range);
                
                float delay = v.uv3.x;
                float duration = v.uv3.y;         
                float2 position = v.uv4;

                float t = saturate((_Progress - delay) / max(0.0001, duration));

                // 2. —жимаем вершины в точку 'position' при t = 0 (_Progress = delay)
                v.positionOS.xy = lerp(position.xy, v.positionOS.xy, t);

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;

                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                return lerp(_BaseColor, _DarkColor, i.darkness) * lerp(_GradiendColorY, _BaseColor, i.uv.y);
            }
           
            ENDHLSL
        }
    }
}
