Shader "Custom/PostProcess/ColorTint"
{
    // FullScreenRendererPass(CustomRenderPassFeature)から
    // _BlitTexture / _BlitScaleBias 経由で描画される想定のフルスクリーンシェーダー
    Properties
    {
        _BlitTexture("Blit Texture", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,0,0,1)
        _Intensity("Intensity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _BlitScaleBias;

            half4 _TintColor;
            float _Intensity;

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings OUT;
                float2 uv      = float2((id << 1) & 2, id & 2);
                OUT.positionCS = float4(uv * 2.0 - 1.0, 0, 1);
                OUT.uv         = uv * _BlitScaleBias.xy + _BlitScaleBias.zw;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);

                // intensity=0で元の画面のまま、1で完全に単色(_TintColor)へ
                half3 tinted = lerp(sceneColor.rgb, _TintColor.rgb, saturate(_Intensity));

                return half4(tinted, sceneColor.a);
            }
            ENDHLSL
        }
    }
}
