Shader "Custom/Particle/SH_ParticleNoiseDissolve"
{
    Properties
    {
        [Header(Main)]
        _MainTex ("Main Mask Texture", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (1,1,1,1)

        [Header(Dissolve Noise)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseTiling ("Noise Tiling", Vector) = (1,1,0,0)

        [Header(Dissolve)]
        _DissolveSoftness ("Dissolve Softness", Range(0.001, 0.3)) = 0.05
        _DissolveStart ("Dissolve Start", Range(0, 1)) = 0.0
        _DissolvePower ("Dissolve Power", Range(0.1, 5.0)) = 1.0

        [Header(Main Mask)]
        _MaskPower ("Mask Power", Range(0.1, 5.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ParticleNoiseDissolve"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;

                float4 _Color;

                float4 _NoiseTiling;

                float _DissolveSoftness;
                float _DissolveStart;
                float _DissolvePower;

                float _MaskPower;

            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv =
                    TRANSFORM_TEX(input.uv, _MainTex);

                output.color = input.color;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // =========================================
                // 左画像
                // Particle本体の形を取得
                // =========================================

                half4 mainSample =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv);

                // 今回の画像は白黒マスクなので
                // RGBのRをAlpha用マスクとして使用
                float mainMask =
                    pow(
                        saturate(mainSample.r),
                        _MaskPower);


                // =========================================
                // 右画像
                // Dissolve用Noise
                // =========================================

                float2 noiseUV =
                    input.uv * _NoiseTiling.xy;

                float noise =
                    SAMPLE_TEXTURE2D(
                        _NoiseTex,
                        sampler_NoiseTex,
                        noiseUV).r;


                // =========================================
                // Particle寿命からDissolve進行度を作成
                //
                // Color Over Lifetime
                //
                // Alpha 1
                // ↓
                // Alpha 0
                //
                // にすると
                //
                // dissolveProgress
                //
                // 0
                // ↓
                // 1
                //
                // になる
                // =========================================

                float dissolveProgress =
                    1.0 - input.color.a;

                dissolveProgress =
                    saturate(
                        (dissolveProgress - _DissolveStart)
                        /
                        max(
                            0.0001,
                            1.0 - _DissolveStart));

                dissolveProgress =
                    pow(
                        dissolveProgress,
                        _DissolvePower);


                // =========================================
                // Dissolve
                //
                // Progressが上がるほど
                // Noiseの暗い場所から徐々に消える
                // =========================================

                float threshold =
                    lerp(
                        -_DissolveSoftness,
                        1.0 + _DissolveSoftness,
                        dissolveProgress);

                float dissolveMask =
                    smoothstep(
                        threshold,
                        threshold + _DissolveSoftness,
                        noise);


                // =========================================
                // 最終Alpha
                // =========================================

                float finalAlpha =
                    mainMask
                    * dissolveMask
                    * _Color.a;


                // 完全に透明なPixelは描画しない
                clip(finalAlpha - 0.001);


                // =========================================
                // 最終Color
                //
                // Particle SystemのRGB Colorは使用するが
                // AlphaはDissolve制御に使っているため
                // ここでは掛けない
                // =========================================

                float3 finalColor =
                    _Color.rgb
                    * input.color.rgb;

                return half4(
                    finalColor,
                    finalAlpha);
            }

            ENDHLSL
        }
    }
}
