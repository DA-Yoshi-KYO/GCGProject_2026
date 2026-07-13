/*
+=====================================
 ファイル名 : SH_JumpUpFade.shader
 概要     : Jump時にUVを上方向へ移動しながらFadeするEffectShader
 作者     : ヨシモト リョウ
 履歴     : 2026/07/13 新規作成
=====================================+
*/

Shader "Unlit/SH_JumpUpFade"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _MainColor ("Main Color", Color) = (1, 1, 1, 1)

        _Brightness ("Brightness", Range(0, 20)) = 2
        _Alpha ("Alpha", Range(0, 1)) = 1

        [Header(Jump Animation)]
        _ScrollDistance ("Up Scroll Distance", Float) = 1
        _FadeStart ("Fade Start", Range(0, 1)) = 0
        _FadePower ("Fade Power", Range(0.1, 8)) = 1

        [Header(Vertical Edge Fade)]
        _BottomSoftness ("Bottom Softness", Range(0.001, 0.5)) = 0.05
        _TopSoftness ("Top Softness", Range(0.001, 0.5)) = 0.05

        [HideInInspector] _Progress ("Progress", Range(0, 1)) = 0
        [HideInInspector] _EffectTime ("Effect Time", Float) = 0
        [HideInInspector] _EffectPlay ("Effect Play", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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
                float _ScrollDistance;
                float _FadeStart;
                float _FadePower;
                float _BottomSoftness;
                float _TopSoftness;
                float _Progress;
                float _EffectTime;
                float _EffectPlay;
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
                float2 baseUV : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes f_Input)
            {
                Varyings f_Output;

                f_Output.positionHCS = TransformObjectToHClip(f_Input.positionOS.xyz);
                f_Output.baseUV = TRANSFORM_TEX(f_Input.uv, _MainTex);
                f_Output.color = f_Input.color;

                return f_Output;
            }

            half4 Frag(Varyings f_Input) : SV_Target
            {
                float f_Progress = saturate(_Progress);

                // UVをマイナス方向へずらすことで、模様は画面上で上方向へ移動します。
                float2 v2_ScrollUV = f_Input.baseUV;
                v2_ScrollUV.y -= f_Progress * _ScrollDistance;

                // UV範囲外を透明にして、Repeat設定でも模様がループしないようにします。
                float f_InsideY =
                    step(0.0, v2_ScrollUV.y) *
                    step(v2_ScrollUV.y, 1.0);

                half4 h4_MainTexture = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    v2_ScrollUV);

                // 元UVの上下端を柔らかくします。
                float f_BottomMask = smoothstep(
                    0.0,
                    max(_BottomSoftness, 0.001),
                    f_Input.baseUV.y);

                float f_TopMask = 1.0 - smoothstep(
                    1.0 - max(_TopSoftness, 0.001),
                    1.0,
                    f_Input.baseUV.y);

                float f_EdgeMask = f_BottomMask * f_TopMask;

                // FadeStartまでは1、以降は0へFadeします。
                float f_FadeRate = saturate(
                    (f_Progress - _FadeStart) /
                    max(1.0 - _FadeStart, 0.0001));

                float f_Fade = pow(
                    saturate(1.0 - f_FadeRate),
                    max(_FadePower, 0.1));

                float f_FinalAlpha =
                    h4_MainTexture.a *
                    _MainColor.a *
                    f_Input.color.a *
                    _Alpha *
                    f_InsideY *
                    f_EdgeMask *
                    f_Fade;

                clip(f_FinalAlpha - 0.001);

                float3 v3_FinalColor =
                    h4_MainTexture.rgb *
                    _MainColor.rgb *
                    f_Input.color.rgb *
                    _Brightness;

                return half4(v3_FinalColor, f_FinalAlpha);
            }
            ENDHLSL
        }
    }
}
