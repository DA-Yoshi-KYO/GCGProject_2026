Shader "Custom/OutlineMask"
{
    Properties
    {
        [HDR] _OutlineColor      ("Outline Color",      Color)        = (1, 1, 1, 1)
        _OutlineWidth            ("Outline Width (px)", Range(0, 10)) = 2
        _EmissionIntensity       ("Emission Intensity", Range(1, 30)) = 6.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "OutlineMask"
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SRPBatcher 対応: CBUFFER で受け取る（MPBはこちら経由で渡る）
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _EmissionIntensity;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // RGB = HDR色, A = 太さ(0〜1に正規化)
                return half4(_OutlineColor.rgb * _EmissionIntensity, _OutlineWidth / 10.0);
            }
            ENDHLSL
        }
    }
}
