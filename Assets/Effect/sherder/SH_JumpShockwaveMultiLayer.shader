/*
+=====================================
 ファイル名 : SH_JumpShockwaveMultiLayer.shader
 概要     : Jump衝撃波TextureをShader内で複数枚重ねて表示するEffectShader
 作者     : ヨシモト リョウ
 履歴     : 2026/07/13 新規作成
=====================================+
*/

Shader "Unlit/SH_JumpShockwaveMultiLayer"
{
    Properties
    {
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "black" {}

        [HDR]
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)

        _Brightness ("Brightness", Range(0.0, 20.0)) = 2.0
        _Alpha ("Alpha", Range(0.0, 1.0)) = 1.0

        [Header(Texture Black Background)]
        _BlackCut ("Black Cut", Range(0.0, 1.0)) = 0.05
        _BlackCutSoftness ("Black Cut Softness", Range(0.001, 0.5)) = 0.05

        [Header(Multi Layer)]
        _LayerCount ("Layer Count", Range(1.0, 8.0)) = 4.0
        _LayerScaleStep ("Layer Scale Step", Range(-0.5, 1.0)) = 0.18
        _LayerYOffset ("Layer Y Offset", Range(-1.0, 1.0)) = 0.08
        _LayerRotationStep ("Layer Rotation Step", Range(-180.0, 180.0)) = 0.0
        _LastLayerAlpha ("Last Layer Alpha", Range(0.0, 1.0)) = 0.35

        [Header(Animation)]
        _ScrollDistance ("UV Up Scroll Distance", Float) = 0.35
        _FadeStart ("Fade Start", Range(0.0, 1.0)) = 0.35
        _FadePower ("Fade Power", Range(0.1, 8.0)) = 1.0

        // CS_JumpEffectShaderOnlyから更新する値です。
        // Material上から手動確認できるように表示しています。
        _Progress ("Progress", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"

            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            // 黒背景Textureを加算表示するため、
            // 黒い部分は描画結果へ影響しません。
            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainColor;

                float _Brightness;
                float _Alpha;

                float _BlackCut;
                float _BlackCutSoftness;

                float _LayerCount;
                float _LayerScaleStep;
                float _LayerYOffset;
                float _LayerRotationStep;
                float _LastLayerAlpha;

                float _ScrollDistance;
                float _FadeStart;
                float _FadePower;
                float _Progress;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes f_Input)
            {
                Varyings f_Output;

                f_Output.positionHCS =
                    TransformObjectToHClip(f_Input.positionOS.xyz);

                f_Output.uv =
                    TRANSFORM_TEX(f_Input.uv, _MainTex);

                f_Output.color =
                    f_Input.color;

                return f_Output;
            }

            float2 RotateUV(
                float2 f_UV,
                float f_Radian)
            {
                float f_Sin = sin(f_Radian);
                float f_Cos = cos(f_Radian);

                float2 v2_CenteredUV =
                    f_UV - 0.5;

                float2 v2_RotatedUV;

                v2_RotatedUV.x =
                    (v2_CenteredUV.x * f_Cos) -
                    (v2_CenteredUV.y * f_Sin);

                v2_RotatedUV.y =
                    (v2_CenteredUV.x * f_Sin) +
                    (v2_CenteredUV.y * f_Cos);

                return v2_RotatedUV + 0.5;
            }

            half4 Frag(Varyings f_Input) : SV_Target
            {
                float f_Progress =
                    saturate(_Progress);

                int n_LayerCount =
                    clamp(
                        (int)round(_LayerCount),
                        1,
                        8);

                float f_FadeStart =
                    min(_FadeStart, 0.9999);

                float f_FadeRate =
                    saturate(
                        (f_Progress - f_FadeStart) /
                        max(1.0 - f_FadeStart, 0.0001));

                float f_GlobalFade =
                    pow(
                        saturate(1.0 - f_FadeRate),
                        max(_FadePower, 0.1));

                float3 v3_AccumulatedColor =
                    float3(0.0, 0.0, 0.0);

                for (int i = 0; i < 8; i++)
                {
                    if (i >= n_LayerCount)
                    {
                        break;
                    }

                    float f_LayerRate =
                        n_LayerCount <= 1
                            ? 0.0
                            : (float)i / (float)(n_LayerCount - 1);

                    //-----------------------------------
                    // Layerごとに大きさを変える
                    //-----------------------------------
                    float f_LayerScale =
                        max(
                            0.05,
                            1.0 + ((float)i * _LayerScaleStep));

                    float2 v2_LayerUV =
                        (f_Input.uv - 0.5) /
                        f_LayerScale +
                        0.5;

                    //-----------------------------------
                    // Layerごとに回転を変える
                    //-----------------------------------
                    float f_RotationRadian =
                        radians(
                            (float)i *
                            _LayerRotationStep);

                    v2_LayerUV =
                        RotateUV(
                            v2_LayerUV,
                            f_RotationRadian);

                    //-----------------------------------
                    // 同時に重ねつつ、位置だけ少しずらす
                    //-----------------------------------
                    v2_LayerUV.y -=
                        f_Progress *
                        _ScrollDistance;

                    v2_LayerUV.y -=
                        (float)i *
                        _LayerYOffset;

                    //-----------------------------------
                    // UV範囲外を無効化
                    //-----------------------------------
                    float f_InsideMask =
                        step(0.0, v2_LayerUV.x) *
                        step(v2_LayerUV.x, 1.0) *
                        step(0.0, v2_LayerUV.y) *
                        step(v2_LayerUV.y, 1.0);

                    half4 h4_TextureColor =
                        SAMPLE_TEXTURE2D(
                            _MainTex,
                            sampler_MainTex,
                            v2_LayerUV);

                    //-----------------------------------
                    // 黒背景Texture対応
                    // AlphaではなくRGBの明るさをMaskに使う
                    //-----------------------------------
                    float f_Luminance =
                        max(
                            h4_TextureColor.r,
                            max(
                                h4_TextureColor.g,
                                h4_TextureColor.b));

                    float f_TextureMask =
                        smoothstep(
                            _BlackCut,
                            _BlackCut +
                            max(_BlackCutSoftness, 0.001),
                            f_Luminance);

                    //-----------------------------------
                    // 奥のLayerほど薄くする
                    //-----------------------------------
                    float f_LayerAlpha =
                        lerp(
                            1.0,
                            _LastLayerAlpha,
                            f_LayerRate);

                    float f_FinalStrength =
                        f_TextureMask *
                        f_InsideMask *
                        f_LayerAlpha *
                        f_GlobalFade *
                        _Alpha *
                        _MainColor.a *
                        f_Input.color.a;

                    v3_AccumulatedColor +=
                        h4_TextureColor.rgb *
                        _MainColor.rgb *
                        f_Input.color.rgb *
                        f_FinalStrength;
                }

                v3_AccumulatedColor *=
                    _Brightness;

                return half4(
                    v3_AccumulatedColor,
                    1.0);
            }

            ENDHLSL
        }
    }
}
