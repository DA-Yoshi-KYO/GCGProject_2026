Shader "Custom/Outline"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (0.03, 0.03, 0.03, 1)
        _OutlineWidth ("Outline Width", Range(0.0, 1.0)) = 1
        _EmissionIntensity ("Emission Intensity", Range(1.0, 30.0)) = 6.0
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Transparent"
            "Queue"          = "Geometry+1"
        }
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // GPU Instancing + MaterialPropertyBlock を有効化
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            // MaterialPropertyBlock で per-instance に上書きできるプロパティ
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(float4, _OutlineColor)
                UNITY_DEFINE_INSTANCED_PROP(float,  _OutlineWidth)
                UNITY_DEFINE_INSTANCED_PROP(float,  _EmissionIntensity)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                float outlineWidth = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineWidth);
                float3 positionOS = IN.positionOS.xyz;
                positionOS       += normalize(IN.normalOS) * (outlineWidth * 0.1f);
                float3 positionWS = TransformObjectToWorld(positionOS);
                OUT.positionCS    = TransformWorldToHClip(positionWS);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float4 color     = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineColor);
                float  intensity = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _EmissionIntensity);

                // RGB を intensity 倍にすることで輝度が 1.0 を超え、
                // URP の Bloom ポストプロセスが反応する HDR 出力になる
                color.rgb *= intensity;

                return half4(color.rgb, color.a);
            }
            ENDHLSL
        }
    }
}
