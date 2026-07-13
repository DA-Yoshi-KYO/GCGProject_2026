/*
+=====================================
 ファイル名 : SH_JumpShockwave_EffectWarpMeshOneSmallExpand.shader
 概要     : Effect_Warp_Mesh用。
            円周上にTextureを表示し、Shader内で徐々に拡大しながらFadeするJump衝撃波Shader
 作者     : ヨシモト リョウ
 履歴     : 2026/07/13 新規作成
=====================================+
*/

Shader "Unlit/SH_JumpShockwave_EffectWarpMeshOneSmallExpand"
{
    Properties
    {
        [MainTexture]
        _MainTex ("Main Texture", 2D) = "black" {}

        [HDR]
        _MainColor ("Main Color", Color) = (1, 1, 1, 1)

        _Brightness ("Brightness", Range(0.0, 20.0)) = 3.0
        _Alpha ("Alpha", Range(0.0, 1.0)) = 1.0

        [Header(Position On Ring)]
        _ArcCenter ("Arc Center", Range(0.0, 1.0)) = 0.25
        _ArcWidth ("Arc Width", Range(0.01, 1.0)) = 0.18

        [Header(Height On Mesh)]
        _HeightCenter ("Height Center", Float) = 0.0
        _HeightSize ("Height Size", Range(0.01, 3.0)) = 0.25

        [Header(Shader Expand)]
        _StartExpandScale ("Start Expand Scale", Range(0.01, 3.0)) = 0.45
        _EndExpandScale ("End Expand Scale", Range(0.01, 5.0)) = 1.35
        _ExpandPower ("Expand Power", Range(0.1, 5.0)) = 1.0

        [Header(Texture Adjust)]
        _TextureOffsetX ("Texture Offset X", Range(-1.0, 1.0)) = 0.0
        _TextureOffsetY ("Texture Offset Y", Range(-1.0, 1.0)) = 0.0
        _TextureScaleX ("Texture Scale X", Range(0.1, 5.0)) = 1.0
        _TextureScaleY ("Texture Scale Y", Range(0.1, 5.0)) = 1.0

        [Header(Black Background Mask)]
        _BlackCut ("Black Cut", Range(0.0, 1.0)) = 0.02
        _BlackCutSoftness ("Black Cut Softness", Range(0.001, 0.5)) = 0.04

        [Header(Animation)]
        _UVUpMove ("UV Up Move", Range(-2.0, 2.0)) = 0.1
        _FadeStart ("Fade Start", Range(0.0, 1.0)) = 0.15
        _FadePower ("Fade Power", Range(0.1, 8.0)) = 2.0

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

            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -1, -1

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

                float _ArcCenter;
                float _ArcWidth;

                float _HeightCenter;
                float _HeightSize;

                float _StartExpandScale;
                float _EndExpandScale;
                float _ExpandPower;

                float _TextureOffsetX;
                float _TextureOffsetY;
                float _TextureScaleX;
                float _TextureScaleY;

                float _BlackCut;
                float _BlackCutSoftness;

                float _UVUpMove;
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
                float3 positionOS : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes f_Input)
            {
                Varyings f_Output;

                f_Output.positionHCS =
                    TransformObjectToHClip(f_Input.positionOS.xyz);

                f_Output.positionOS =
                    f_Input.positionOS.xyz;

                f_Output.color =
                    f_Input.color;

                return f_Output;
            }

            half4 Frag(Varyings f_Input) : SV_Target
            {
                float f_Progress =
                    saturate(_Progress);

                //-----------------------------------
                // Fadeと同じProgressで拡大率を作る
                //-----------------------------------
                float f_ExpandRate =
                    pow(
                        f_Progress,
                        max(_ExpandPower, 0.1));

                float f_ExpandScale =
                    lerp(
                        _StartExpandScale,
                        _EndExpandScale,
                        f_ExpandRate);

                //-----------------------------------
                // 円周方向の角度を0～1にする
                //-----------------------------------
                float f_Angle =
                    atan2(f_Input.positionOS.z, f_Input.positionOS.x);

                float f_Angle01 =
                    (f_Angle + PI) / (2.0 * PI);

                //-----------------------------------
                // ArcCenterを中心に、ArcWidthぶんだけ表示する
                // ここにf_ExpandScaleを掛けるので横幅が徐々に広がる
                //-----------------------------------
                float f_Diff =
                    frac(f_Angle01 - _ArcCenter + 0.5) - 0.5;

                float f_ArcHalfWidth =
                    max((_ArcWidth * f_ExpandScale) * 0.5, 0.0001);

                float f_U =
                    (f_Diff / f_ArcHalfWidth) * 0.5 + 0.5;

                //-----------------------------------
                // 高さ方向もf_ExpandScaleで徐々に広がる
                //-----------------------------------
                float f_V =
                    ((f_Input.positionOS.y - _HeightCenter) /
                    max(_HeightSize * f_ExpandScale, 0.0001)) + 0.5;

                float2 v2_UV =
                    float2(f_U, f_V);

                //-----------------------------------
                // Texture調整
                //-----------------------------------
                v2_UV.x =
                    ((v2_UV.x - 0.5) / max(_TextureScaleX, 0.0001)) + 0.5;

                v2_UV.y =
                    ((v2_UV.y - 0.5) / max(_TextureScaleY, 0.0001)) + 0.5;

                v2_UV.x += _TextureOffsetX;
                v2_UV.y += _TextureOffsetY;

                //-----------------------------------
                // 上方向移動
                //-----------------------------------
                v2_UV.y -= f_Progress * _UVUpMove;

                //-----------------------------------
                // TextureのTiling / Offset
                //-----------------------------------
                v2_UV =
                    v2_UV * _MainTex_ST.xy +
                    _MainTex_ST.zw;

                //-----------------------------------
                // UV範囲外を消す
                //-----------------------------------
                float f_InsideMask =
                    step(0.0, v2_UV.x) *
                    step(v2_UV.x, 1.0) *
                    step(0.0, v2_UV.y) *
                    step(v2_UV.y, 1.0);

                //-----------------------------------
                // Texture取得
                //-----------------------------------
                half4 h4_Texture =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        v2_UV);

                //-----------------------------------
                // 黒背景を透明扱い
                //-----------------------------------
                float f_Luminance =
                    max(
                        h4_Texture.r,
                        max(h4_Texture.g, h4_Texture.b));

                float f_TextureMask =
                    smoothstep(
                        _BlackCut,
                        _BlackCut + max(_BlackCutSoftness, 0.001),
                        f_Luminance);

                //-----------------------------------
                // Fade
                //-----------------------------------
                float f_FadeStart =
                    min(_FadeStart, 0.9999);

                float f_FadeRate =
                    saturate(
                        (f_Progress - f_FadeStart) /
                        max(1.0 - f_FadeStart, 0.0001));

                float f_Fade =
                    pow(
                        saturate(1.0 - f_FadeRate),
                        max(_FadePower, 0.1));

                //-----------------------------------
                // 最終色
                //-----------------------------------
                float f_Strength =
                    f_TextureMask *
                    f_InsideMask *
                    f_Fade *
                    _Alpha *
                    _MainColor.a *
                    f_Input.color.a;

                float3 v3_FinalColor =
                    h4_Texture.rgb *
                    _MainColor.rgb *
                    f_Input.color.rgb *
                    _Brightness *
                    f_Strength;

                return half4(v3_FinalColor, 1.0);
            }

            ENDHLSL
        }
    }
}
