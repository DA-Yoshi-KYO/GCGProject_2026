Shader "Custom/DebugPostProcess"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }

        Pass
        {
            Name "PostProcessTest"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 🔥 Blitter用テクスチャ
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _Intensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 Frag (Varyings i) : SV_Target
            {
                // 元画像取得
                half4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);

                // わかりやすい色変化（赤強調）
                col.rgb = lerp(col.rgb, float3(1, 0, 0), 1.0f);

                return col;
            }

            ENDHLSL
        }
    }
}
