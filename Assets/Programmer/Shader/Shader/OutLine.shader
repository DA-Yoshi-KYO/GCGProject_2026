Shader "Custom/URP/Outline"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0.0, 10.0)) = 1.5
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry+1"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front      // 裏面のみ描画（Back-face法に戻す）
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 tangentOS  : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings OutlineVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Tangentのスムース法線（向きは元法線に揃え済み）
                float3 smoothNormalOS = normalize(input.tangentOS.xyz);

                // ビュー空間へ変換
                float3 smoothNormalVS = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, smoothNormalOS));

                // クリップ空間の頂点位置
                float4 posCS = TransformObjectToHClip(input.positionOS.xyz);

                // クリップ空間でスクリーン固定サイズ押し出し
                float4 normalCS  = mul(UNITY_MATRIX_P, float4(smoothNormalVS, 0.0));
                float2 normalNDC = normalize(normalCS.xy);

                float width = _OutlineWidth * 0.001;
                posCS.xy += normalNDC * posCS.w * width;

                output.positionCS = posCS;
                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
