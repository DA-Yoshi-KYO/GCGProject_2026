Shader "Custom/AllPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        // C#からの入力データ
        _UseGrayscale ("Use Grayscale", Int) = 0
        _UseInvert ("Use Invert", Int) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Name "PostProcess"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ===== 入力 =====
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            int _UseGrayscale;
            int _UseInvert;

            // ===== 入力データ =====
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            // ===== 出力データ =====
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ===== 頂点シェーダ =====
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            // ===== ポストプロセス用関数 =====
            // グレースケールをかけます
            // ※テスト用関数
            half4 Effect_Grayscale(half4 col)
            {
                float g = dot(col.rgb, float3(0.3, 0.59, 0.11));
                return float4(g, g, g, 1);
            }

            // 色を反転させます
            // ※テスト用関数
            half4 Effect_Invert(half4 col)
            {
                return float4(1 - col.rgb, 1);
            }

            // ===== フラグメント =====
            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // 元画面
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // ポストプロセスの重ねがけ
                if (_UseGrayscale == 1)
                    col = Effect_Grayscale(col);

                if (_UseInvert == 1)
                    col = Effect_Invert(col);

                return col;
            }

            ENDHLSL
        }
    }
}
