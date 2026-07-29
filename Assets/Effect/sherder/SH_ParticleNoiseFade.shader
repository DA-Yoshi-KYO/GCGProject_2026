Shader "Custom/Particle/SH_ParticleNoiseFade"
{
    Properties
    {
        [Header(Main)]
        _MainTex ("Main Mask Texture", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (1,1,1,1)

        [Header(Noise)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _NoiseTiling ("Noise Tiling", Vector) = (1,1,0,0)

        [Header(Fade)]
        _NoiseStrength ("Noise Strength", Range(0, 2)) = 1.0
        _FadePower ("Fade Power", Range(0.1, 5.0)) = 1.0

        [Header(Noise Contrast)]
        _NoiseMin ("Noise Min", Range(0, 1)) = 0.0
        _NoiseMax ("Noise Max", Range(0, 1)) = 1.0

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
            Name "ParticleNoiseFade"

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

                float _NoiseStrength;
                float _FadePower;

                float _NoiseMin;
                float _NoiseMax;

                float _MaskPower;

            CBUFFER_END


            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _MainTex);

                output.color = input.color;

                return output;
            }


            half4 Frag(Varyings input) : SV_Target
            {
                // ==================================================
                // Particle本体マスク
                // 左側の白黒Texture
                // ==================================================

                half4 mainSample =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv);

                float mainMask =
                    pow(
                        saturate(mainSample.r),
                        _MaskPower);


                // ==================================================
                // Noise
                // ==================================================

                float2 noiseUV =
                    input.uv * _NoiseTiling.xy;

                float noise =
                    SAMPLE_TEXTURE2D(
                        _NoiseTex,
                        sampler_NoiseTex,
                        noiseUV).r;


                // ==================================================
                // Noiseの白黒差を調整
                //
                // Noise画像がほぼ白なので、
                // _NoiseMin / _NoiseMaxで使用範囲を広げる
                // ==================================================

                noise =
                    saturate(
                        (noise - _NoiseMin)
                        /
                        max(
                            0.001,
                            _NoiseMax - _NoiseMin));


                // ==================================================
                // Particle寿命
                //
                // Color Over Lifetime
                //
                // Alpha 1 → 0
                //
                // progress
                //
                // 0 → 1
                // ==================================================

                float progress =
                    1.0 - input.color.a;

                progress =
                    saturate(progress);


                // ==================================================
                // 通常Fade
                //
                // 寿命終了へ向かって
                // 滑らかに全体を透明にする
                // ==================================================

                float lifeFade =
                    pow(
                        saturate(1.0 - progress),
                        _FadePower);


                // ==================================================
                // Noise Fade
                //
                // 最初
                // noiseInfluence = 1
                //
                // ↓
                //
                // 寿命が進むほど
                // Noise TextureそのものがAlphaへ混ざる
                // ==================================================

                float noiseAmount =
                    saturate(
                        progress * _NoiseStrength);

                float noiseInfluence =
                    lerp(
                        1.0,
                        noise,
                        noiseAmount);


                // ==================================================
                // Noiseを少し強調
                //
                // 黒い部分ほど先に薄くなる
                // 白い部分ほど残る
                // ==================================================

                float noiseFade =
                    noiseInfluence * lifeFade;


                // ==================================================
                // 最終Alpha
                // ==================================================

                float finalAlpha =
                    mainMask
                    * noiseFade
                    * _Color.a;


                // clipは使わない。
                //
                // 前のShaderではclipによって
                // PixelがON/OFFになり、
                // パッパッと消える原因にもなっていた。
                // ==================================================


                // ==================================================
                // Color
                // ==================================================

                float3 finalColor =
                    _Color.rgb
                    * input.color.rgb;

                return half4(
                    finalColor,
                    saturate(finalAlpha));
            }

            ENDHLSL
        }
    }
}
