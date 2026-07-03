Shader "Custom/OutlineMask"
{
    Properties
    {
        [HDR] _OutlineColor      ("Outline Color",      Color)        = (1, 1, 1, 1)
        _OutlineWidth            ("Outline Width (px)", Range(0, 10)) = 6
        _EmissionIntensity       ("Emission Intensity", Range(1, 30)) = 6.0

        // 0 = LEqual(通常の深度テスト・遮蔽物越しは見えない)
        // 1 = Always(深度無視・オブジェクトを透過して常に見える)
        [Enum(Occlude,0,XRay,1)] _OutlineXRay ("Occlusion Mode", Float) = 0

        // _OutlineXRay を元に C# 側から実際の CompareFunction 値をセットする
        // (LEqual=4, Always=8)。シェーダー内では直接この値を ZTest に渡す。
        [HideInInspector] _ZTestMode ("ZTest Mode", Float) = 4
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "OutlineMask"
            ZWrite Off
            ZTest [_ZTestMode]   // マテリアルプロパティで LEqual/Always を切り替え
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
