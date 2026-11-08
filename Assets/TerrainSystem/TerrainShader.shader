Shader "Custom/TerrainShader"
{
    Properties
    {
        _Mask("Mask", 2D) = "black" {}
        _MaskScale("MaskScale", Vector) = (150.0, 100.0, 0.0, 0.0)
        _FieldTex("FieldTexture", 2D) = "black" {}
        _FieldScale("FieldTextureScale", Vector) = (1.0, 1.0, 0.0, 0.0)
        _FieldColor("FieldBaseColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _GroundTex("GroundTexture", 2D) = "black" {}
        _GroundScale("GroundTextureScale", Vector) = (1.0, 1.0, 0.0, 0.0)
        _GroundColor("GroundBaseColor", Color) = (1.0, 1.0, 1.0, 1.0)
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
            };

            Texture2D _Mask;
            SamplerState sampler_Mask;
            Texture2D _FieldTex;
            SamplerState sampler_FieldTex;
            Texture2D _GroundTex;
            SamplerState sampler_GroundTex;
            Vector _MaskScale;
            Vector _FieldScale;
            Vector _GroundScale;
            half4 _FieldColor;
            half4 _GroundColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 maskColor = _Mask.Sample(sampler_Mask, i.uv);
                half4 fieldColor = _FieldTex.Sample(sampler_FieldTex, frac(i.uv * _MaskScale.xy / _FieldScale.xy)) * _FieldColor;
                half4 groundColor = _GroundTex.Sample(sampler_GroundTex, frac(i.uv * _MaskScale.xy / _GroundScale.xy)) * _GroundColor;
                half4 color = lerp(groundColor, fieldColor, maskColor.r);
                return color;
            }
            ENDHLSL
        }
    }
}
