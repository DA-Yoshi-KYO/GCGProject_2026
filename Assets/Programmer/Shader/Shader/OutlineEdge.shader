Shader "Custom/OutlineEdge"
{
    // このマテリアルはグローバルなフォールバック値のみ持つ
    // 実際の色・太さは各オブジェクトのマスクRTから読む
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
                OUT.uv         = float2(uv.x, 1.0 - uv.y); // V反転でUV上下を修正
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 自分自身のマスク値（RGB=色, A=太さ）
                half4 self = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);

                // A チャンネルが 0 より大きければ「物体の内側」
                // 太さはピクセル数として A * 10.0 で元のWidth値に戻す
                float selfWidth = self.a * 10.0;

                // 8方向サンプリング：各方向の太さ(A)を収集
                // 隣接ピクセルの太さに応じてサンプリング距離を変える
                // → オブジェクトごとに異なる太さの輪郭を正確に出すため
                float2 offsets[8] = {
                    float2( 0,  1), float2( 0, -1),
                    float2( 1,  0), float2(-1,  0),
                    float2( 1,  1), float2(-1,  1),
                    float2( 1, -1), float2(-1, -1)
                };

                half  neighborAlphaMax = 0;
                half3 neighborColor    = 0;

                for (int i = 0; i < 8; i++)
                {
                    // 各隣接ピクセルを幅1pxでまずサンプル
                    float2 uvN = uv + offsets[i] * _MaskTex_TexelSize.xy;
                    half4  neighbor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvN);

                    // 隣接の太さ分だけ離れた場所もサンプルして輪郭幅を確保
                    float  neighborWidth = neighbor.a * 10.0;
                    float2 uvFar = uv + offsets[i] * _MaskTex_TexelSize.xy * neighborWidth;
                    half4  neighborFar = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvFar);

                    // より強い（太い）ほうを採用
                    half4 best = neighborFar.a > neighbor.a ? neighborFar : neighbor;

                    if (best.a > neighborAlphaMax)
                    {
                        neighborAlphaMax = best.a;
                        neighborColor    = best.rgb;
                    }
                }

                // 自分が背景(a=0)で、周囲に物体(a>0)がある → 輪郭ピクセル
                half edge = saturate(neighborAlphaMax - self.a);

                return half4(neighborColor, edge);
            }
            ENDHLSL
        }
    }
}
