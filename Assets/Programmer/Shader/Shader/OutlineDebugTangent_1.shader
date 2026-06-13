Shader "Custom/URP/OutlineDebug_1"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "DebugNormal"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 color      : TEXCOORD0;
            };

            // MODE: 0=元法線, 1=Tangent.xyz（ベイク済みスムース法線）
            #define DEBUG_MODE 1

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                #if DEBUG_MODE == 0
                    // 元の法線を色として表示 ([-1,1] -> [0,1])
                    o.color = input.normalOS * 0.5 + 0.5;
                #else
                    // Tangentに書いたスムース法線を色として表示
                    o.color = input.tangentOS.xyz * 0.5 + 0.5;
                #endif
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(i.color, 1.0);
            }
            ENDHLSL
        }
    }
}
