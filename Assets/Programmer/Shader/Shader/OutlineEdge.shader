Shader "Custom/OutlineEdge"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (0.03, 0.03, 0.03, 1)
        _OutlineWidth ("Outline Width (px)", Range(0, 10)) = 2
        _EmissionIntensity ("Emission Intensity", Range(1.0, 30.0)) = 6.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            float4 _MaskTex_TexelSize;

            half4 _OutlineColor;
            float _OutlineWidth;
            float _EmissionIntensity;

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            // フルスクリーン三角形(頂点バッファ不要)
            Varyings vert(uint id : SV_VertexID)
            {
                Varyings OUT;
                float2 uv = float2((id << 1) & 2, id & 2);
                OUT.positionCS = float4(uv * 2 - 1, 0, 1);
                OUT.uv = uv;
                return OUT;
            }

            // マスクシェーダー側
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float4 col = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineColor);
                float2 texel = _MaskTex_TexelSize.xy * _OutlineWidth;
                float2 uv = IN.uv;

                half c  = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r;
                half n  = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2(0,  texel.y)).r;
                half s  = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv - float2(0,  texel.y)).r;
                half e  = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2(texel.x, 0)).r;
                half w  = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv - float2(texel.x, 0)).r;
                half ne = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2( texel.x,  texel.y)).r;
                half nw = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2(-texel.x,  texel.y)).r;
                half se = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2( texel.x, -texel.y)).r;
                half sw = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv + float2(-texel.x, -texel.y)).r;

                half neighborMax = max(max(max(n, s), max(e, w)), max(max(ne, nw), max(se, sw)));
                half edge = saturate(neighborMax - c); // 自分が背景で周囲に物体があるほど強い

                col.rgb = _OutlineColor.rgb * _EmissionIntensity; // Bloom反応用にHDR化

                return half4(col.rgb, 1); // alphaはシルエット判定に、rgbは色情報に使う
            }
            ENDHLSL
        }
    }
}
