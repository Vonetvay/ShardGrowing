Shader "Custom/VoronoiEdges" {
    Properties {
        _CellDensity ("Cell Density", Float) = 10.0
        _AngleOffset ("Angle Offset", Range(0.0, 5.0)) = 1.0
        _EdgeThickness ("Edge Thickness", Range(0.0, 0.2)) = 0.02
        _EdgeSmoothness ("Edge Smoothness", Range(0.001, 0.1)) = 0.01
        [HDR] _EdgeColor ("Edge Color", Color) = (0.0, 1.0, 1.0, 1.0)
        _CellColor ("Cell Color", Color) = (0.1, 0.1, 0.15, 1.0)
    }
    SubShader {
        Tags { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry" 
        }
        LOD 100

        Pass {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Исправлено: подключена правильная базовая библиотека URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            float _CellDensity;
            float _AngleOffset;
            float _EdgeThickness;
            float _EdgeSmoothness;
            float4 _EdgeColor;
            float4 _CellColor;

            float2 hash2(float2 p, float offset) {
                float2 m = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(m) * 43758.5453) * offset;
            }

            float GetVoronoiEdgeDistance(float2 uv) {
                float2 g = floor(uv);
                float2 f = frac(uv);
                
                float2 mg, mr;
                float md = 8.0;
                
                // Первый проход: ищем ближайшую точку ячейки
                for (int y = -1; y <= 1; y++) {
                    for (int x = -1; x <= 1; x++) {
                        float2 lattice = float2(x, y);
                        float2 offset = hash2(g + lattice, _AngleOffset);
                        float2 r = lattice + offset - f;
                        float d = dot(r, r);

                        if (d < md) {
                            md = d;
                            mr = r;
                            mg = lattice;
                        }
                    }
                }

                // Второй проход: считаем расстояние до граней (алгоритм Иньиго Килеза)
                md = 8.0;
                for (int y2 = -2; y2 <= 2; y2++) {
                    for (int x2 = -2; x2 <= 2; x2++) {
                        float2 lattice = mg + float2(x2, y2);
                        float2 offset = hash2(g + lattice, _AngleOffset);
                        float2 r = lattice + offset - f;

                        if (dot(mr - r, mr - r) > 0.00001) {
                            // Проекция на срединный перпендикуляр между центрами ячеек
                            float d = dot(0.5 * (mr + r), normalize(r - mr));
                            md = min(md, d);
                        }
                    }
                }
                return md;
            }

            Varyings vert(Attributes input) {
                Varyings output;
                // Исправлено: используются стандартные функции трансформации URP
                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInputs.positionCS;
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target {
                float2 scaledUV = input.uv * _CellDensity;
                float distanceToEdge = GetVoronoiEdgeDistance(scaledUV);
                
                // Исправлено: инвертирован smoothstep для правильного наложения маски граней
                float edgeMask = 1.0 - smoothstep(_EdgeThickness, _EdgeThickness + _EdgeSmoothness, distanceToEdge);
                return lerp(_CellColor, _EdgeColor, edgeMask);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
