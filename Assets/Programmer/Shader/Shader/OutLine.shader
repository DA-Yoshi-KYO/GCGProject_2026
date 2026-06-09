Shader "Custom/URP/Outline"
{
    Properties
    {
        // アウトラインの太さ（ワールド空間スケール）
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.005
        // アウトラインの色
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        // 距離によるアウトライン細さの補正（0=補正なし、1=完全補正）
        _DistanceFade ("Distance Fade", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        // URP の Opaque キューに乗せる
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        // ─────────────────────────────────────────
        // Pass : Back-face Outline
        // ─────────────────────────────────────────
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            // 裏面だけ描画（表面はメインのマテリアルが担う）
            Cull Front
            // デプスバッファへ書き込み（Z-fighting 対策で少しオフセット）
            ZWrite On
            ZTest LEqual

            // アウトラインは不透明なので Alpha Blend 不要
            Blend Off

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag

            // URP Core ライブラリ
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SRP Batcher 対応：マテリアルプロパティをここにまとめる
            CBUFFER_START(UnityPerMaterial)
                float  _OutlineWidth;
                float4 _OutlineColor;
                float  _DistanceFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
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

                // ワールド空間で法線方向に押し出す
                // → スケール変化に強く、どんなモデルでも均一な太さになる
                VertexPositionInputs posInputs    = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   normalInputs = GetVertexNormalInputs(input.normalOS);

                float3 normalWS = normalize(normalInputs.normalWS);

                // カメラ距離に応じてアウトライン幅を補正（遠くても太さを維持したい場合は係数を上げる）
                float  dist     = length(posInputs.positionVS); // ビュー空間での距離
                float  fadeMul  = lerp(1.0, dist * 0.1, _DistanceFade);
                float  width    = _OutlineWidth * fadeMul;

                float3 posWS    = posInputs.positionWS + normalWS * width;
                output.positionCS = TransformWorldToHClip(posWS);

                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, 1.0);
            }

            ENDHLSL
        }
    }

    // URP 非対応環境へのフォールバック（エラー回避）
    FallBack Off
}
