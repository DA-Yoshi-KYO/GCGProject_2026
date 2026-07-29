Shader "Custom/Particle/SH_ParticleFireColumn"
{
    Properties
    {
        //========================================
        // Main
        //========================================

        [Header(Main)]
        _MainTex ("Fire Mask Texture", 2D) = "white" {}

        [HDR]
        _Color ("Fire Color", Color) = (1.0, 0.15, 0.01, 1.0)

        _EmissionStrength (
            "Emission Strength",
            Range(0.0, 20.0)
        ) = 5.0


        //========================================
        // Noise
        //========================================

        [Header(Noise)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        _NoiseTiling (
            "Noise Tiling",
            Vector
        ) = (1.0, 1.0, 0.0, 0.0)

        _NoiseSpeed (
            "Noise Speed",
            Vector
        ) = (0.0, -1.0, 0.0, 0.0)

        _DistortionStrength (
            "Distortion Strength",
            Range(0.0, 0.2)
        ) = 0.03

        _NoiseInfluence (
            "Noise Influence",
            Range(0.0, 1.0)
        ) = 0.35


        //========================================
        // Fire
        //========================================

        [Header(Fire)]
        _MaskPower (
            "Mask Power",
            Range(0.1, 5.0)
        ) = 1.0

        _CoreBrightness (
            "Core Brightness",
            Range(0.0, 5.0)
        ) = 1.5

        _EdgeSoftness (
            "Edge Softness",
            Range(0.1, 3.0)
        ) = 1.0
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

        // 炎用Additive
        Blend SrcAlpha One

        ZWrite Off
        Cull Off

        Pass
        {
            Name "FireColumn"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            //========================================
            // Input
            //========================================

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


            //========================================
            // Texture
            //========================================

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);


            CBUFFER_START(UnityPerMaterial)

                float4 _MainTex_ST;

                float4 _Color;

                float _EmissionStrength;

                float4 _NoiseTiling;
                float4 _NoiseSpeed;

                float _DistortionStrength;
                float _NoiseInfluence;

                float _MaskPower;
                float _CoreBrightness;
                float _EdgeSoftness;

            CBUFFER_END


            //========================================
            // Vertex
            //========================================

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz);

                output.uv =
                    TRANSFORM_TEX(
                        input.uv,
                        _MainTex);

                output.color =
                    input.color;

                return output;
            }


            //========================================
            // Fragment
            //========================================

            half4 Frag(Varyings input) : SV_Target
            {
                //====================================
                // Noise UV
                // 上方向へNoiseを流す
                //====================================

                float2 noiseUV =
                    input.uv *
                    _NoiseTiling.xy;

                noiseUV +=
                    _Time.y *
                    _NoiseSpeed.xy;


                float noise =
                    SAMPLE_TEXTURE2D(
                        _NoiseTex,
                        sampler_NoiseTex,
                        noiseUV).r;


                //====================================
                // 炎TextureをNoiseで歪ませる
                //====================================

                float2 distortedUV =
                    input.uv;

                float distortion =
                    (noise - 0.5) *
                    _DistortionStrength;

                distortedUV.x += distortion;
                distortedUV.y += distortion * 0.5;


                //====================================
                // Fire Mask
                //====================================

                float mainMask =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        distortedUV).r;

                mainMask =
                    pow(
                        saturate(mainMask),
                        _MaskPower);


                //====================================
                // Noiseを炎の形に混ぜる
                //====================================

                float noiseMask =
                    lerp(
                        1.0,
                        noise,
                        _NoiseInfluence);

                float fireMask =
                    mainMask *
                    noiseMask;


                //====================================
                // Particle Color
                //====================================

                float3 particleColor =
                    input.color.rgb;


                //====================================
                // 中心を強く発光
                //====================================

                float coreMask =
                    pow(
                        saturate(mainMask),
                        _EdgeSoftness);

                float coreBrightness =
                    1.0 +
                    coreMask *
                    _CoreBrightness;


                //====================================
                // HDR Fire Color
                //====================================

                float3 finalColor =
                    _Color.rgb *
                    particleColor *
                    _EmissionStrength *
                    coreBrightness;


                //====================================
                // Alpha
                // Particle Color over Lifetime対応
                //====================================

                float finalAlpha =
                    fireMask *
                    _Color.a *
                    input.color.a;


                return half4(
                    finalColor,
                    saturate(finalAlpha));
            }

            ENDHLSL
        }
    }
}
