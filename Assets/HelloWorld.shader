Shader "Custom/HelloWorld"
{
    Properties
    {
        _FirstColor("First Color", Color) = (1, 1, 1, 1)
        _SecondColor("Second Color", Color) = (1, 1, 1, 1)
        _Speed("AnimateSpeed", Float) = 2.0
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

            float4 _FirstColor;
            float4 _SecondColor;
            float _Speed;

            struct appdata
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                float wave = sin(_Time.y * 3.0 + v.positionOS.x) * 0.2;
                v.positionOS.y += wave;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_TARGET
            {
                float timeFactor = _Time.y * _Speed;

                return lerp(_SecondColor, _FirstColor, frac(timeFactor));
            }

            ENDHLSL
        }
    }
}
