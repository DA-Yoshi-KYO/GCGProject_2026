Shader "Custom/PostProcess/GradientLURD"
{
    // 対角線位置(diag)と時間(progress)を1つの連続値(t)にまとめ、
    // LeftUpColor(最初) -> MiddleColor -> RightDownColor -> 実際のゲーム画面(最後) の
    // 4色グラデーションを対角線に敷き詰めて、そのまま斜めにスクロールさせる。
    // gradientWidthが大きいほど4色が画面上に同時に並んで見え、
    // 小さいほど1色ずつパキッと切り替わる。
    // CSV_GradientLURD.cs がセットするプロパティ名をそのまま使うため、C#側の変更は不要。
    Properties
    {
        _BlitTexture("Blit Texture", 2D) = "white" {}
        _RightDownColor("Right Down Color (Start)", Color) = (0,0,0,1)
        _MiddleColor("Middle Color", Color) = (1,1,0,1)
        _LeftUpColor("Left Up Color", Color) = (1,1,1,1)
        _MaxTimeFloat("Max Time (合計秒数)", Float) = 5
        _UseCustomProgress("Use Custom Progress", Float) = 0
        _CustomProgress("Custom Progress", Range(0,1)) = 0
        _GradientWidth("Gradient Width (大きいほど3色同時に見える)", Range(0.05, 3)) = 1
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

            half4 _RightDownColor;
            half4 _MiddleColor;
            half4 _LeftUpColor;
            float _MaxTimeFloat;
            float _UseCustomProgress;
            float _CustomProgress;
            float _GradientWidth;

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
                float2 uv = IN.uv;
                half4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);

                // 左上(0)→右下(1)の対角線座標
                float diag = saturate((uv.x + (1.0 - uv.y)) * 0.5);

                // 全体でMaxTimeFloat秒(CustomProgress使用時は0〜1で全体を表す)
                float autoProgress = saturate(_Time.y / max(_MaxTimeFloat, 0.0001));
                float progress = _UseCustomProgress > 0.5 ? saturate(_CustomProgress) : autoProgress;

                float width = max(_GradientWidth, 0.0001);

                // bandStartが-widthから1まで動き、対角線をグラデーション帯が斜めに通過していく
                float bandStart = progress * (1.0 + width) - width;

                // t: 1=まだ帯が来ていない(LeftUp/最初) 〜 0=帯が通過済み(実画面)
                // 順番: LeftUp(白,最初) -> Middle(黄) -> RightDown(黒) -> 実画面(最後)
                float t = saturate((diag - bandStart) / width);

                half3 tint;
                if (t > 2.0 / 3.0)
                    tint = lerp(_MiddleColor.rgb, _LeftUpColor.rgb, (t - 2.0 / 3.0) * 3.0);
                else if (t > 1.0 / 3.0)
                    tint = lerp(_RightDownColor.rgb, _MiddleColor.rgb, (t - 1.0 / 3.0) * 3.0);
                else
                    tint = lerp(sceneColor.rgb, _RightDownColor.rgb, t * 3.0);

                return half4(tint, 1.0);
            }
            ENDHLSL
        }
    }
}
