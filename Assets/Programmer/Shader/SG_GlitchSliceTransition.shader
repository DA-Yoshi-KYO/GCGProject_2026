Shader "Custom/SG_GlitchSliceTransition"
{
    // ポストプロセス版:
    //  ・_BlitTexture = 現在(遷移後)のカメラ映像。常にライブで動いている。
    //  ・_TexA        = 遷移開始の瞬間にフリーズさせた「遷移前」のスナップショット。
    // _Progress を 0→1 にアニメーションさせると、Aの凍結画面がスライス状に
    // グリッチしながら剥がれ、下の生きた映像(現在のカメラ)が現れる。
    Properties
    {
        _TexA ("Frozen Texture (遷移前スナップショット)", 2D) = "white" {}

        _Progress ("Progress (0=遷移前, 1=遷移後)", Range(0,1)) = 0

        _SliceCount ("Slice Count (帯の本数)", Range(4, 200)) = 40
        _SliceRandomness ("Slice Timing Randomness (帯ごとの切替タイミングのばらつき)", Range(0,1)) = 0.6

        _JitterAmount ("Jitter Amount (横ズレ量)", Range(0, 0.5)) = 0.08
        _JitterFrequency ("Jitter Frequency (揺れの速さ)", Range(0, 60)) = 20

        _RGBSplit ("RGB Split Amount (色収差量)", Range(0, 0.05)) = 0.01

        _Seed ("Random Seed", Float) = 0

        // 通常はスクリプト(GlitchSliceRendererFeature)が Time.time を毎フレーム渡すが、
        // マテリアル単体でPlayモード無しに見た目を確認したい場合は手動で動かせるようにしておく
        _Time_Y ("Manual Time (確認用。実行中はスクリプトが上書き)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "GlitchSlicePost"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Blit.hlsl は TEXTURE2D_X などのマクロを Core.hlsl 側の定義に依存しているため、
            // 必ず Core.hlsl を先にインクルードすること
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_TexA);
            SAMPLER(sampler_TexA);

            float _Progress;
            float _SliceCount;
            float _SliceRandomness;
            float _JitterAmount;
            float _JitterFrequency;
            float _RGBSplit;
            float _Seed;
            float _Time_Y; // C#側から Time.time を渡す(URP post-processでは _Time が使えない場合があるため)

            float hash(float n)
            {
                return frac(sin(n * 12.9898 + _Seed * 78.233) * 43758.5453);
            }

            float4 SampleWithGlitch(TEXTURE2D_PARAM(tex, samp), float2 uv, float offsetX, float rgbSplit)
            {
                float2 uvR = saturate(uv + float2(offsetX + rgbSplit, 0));
                float2 uvG = saturate(uv + float2(offsetX, 0));
                float2 uvB = saturate(uv + float2(offsetX - rgbSplit, 0));

                float r = SAMPLE_TEXTURE2D(tex, samp, uvR).r;
                float4 gba = SAMPLE_TEXTURE2D(tex, samp, uvG);
                float b = SAMPLE_TEXTURE2D(tex, samp, uvB).b;

                return float4(r, gba.g, b, gba.a);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float sliceIndex = floor(uv.y * _SliceCount);
                float sliceRand = hash(sliceIndex);

                float threshold = sliceRand * _SliceRandomness
                                 + (sliceIndex / max(_SliceCount - 1, 1)) * (1 - _SliceRandomness);

                float switched = step(threshold, _Progress);

                float edgeDist = abs(_Progress - threshold);
                float burst = saturate(1.0 - edgeDist * 6.0);
                float jitterNoise = (hash(sliceIndex * 3.17 + floor(_Time_Y * _JitterFrequency)) - 0.5) * 2.0;
                float offsetX = jitterNoise * _JitterAmount * burst;
                float rgbSplitAmt = _RGBSplit * (0.3 + burst);

                // A = 凍結した「遷移前」画面
                float4 colA = SampleWithGlitch(TEXTURE2D_ARGS(_TexA, sampler_TexA), uv, offsetX, rgbSplitAmt);

                // B = 今まさに描画されている「遷移後」のライブ映像
                float2 uvBr = saturate(uv + float2(offsetX + rgbSplitAmt, 0));
                float2 uvBg = saturate(uv + float2(offsetX, 0));
                float2 uvBb = saturate(uv + float2(offsetX - rgbSplitAmt, 0));
                float br = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uvBr).r;
                float4 bgba = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uvBg);
                float bb = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uvBb).b;
                float4 colB = float4(br, bgba.g, bb, bgba.a);

                float4 col = lerp(colA, colB, switched);
                col.rgb += burst * 0.15;

                return col;
            }
            ENDHLSL
        }
    }
}
