Shader "Custom/Effect/CS_HitEffect_Additive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Brightness ("Brightness", Float) = 2.0
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "UnlitAdditive"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _TintColor;
                float _Brightness;
                float _Alpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half3 finalColor = texColor.rgb * _TintColor.rgb * _Brightness * input.color.rgb;
                half finalAlpha = texColor.a * _TintColor.a * _Alpha * input.color.a;

                return half4(finalColor * finalAlpha, finalAlpha);
            }
            ENDHLSL
        }
    }
}
