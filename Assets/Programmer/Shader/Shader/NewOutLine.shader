Shader "Custom/NewOutline"
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
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
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
            Varyings vert(appdata v)
            {
                Varyings o;

                // 頂点カラーにベイクされた法線を格納（タンジェント空間）
                float3 smoothNormalTS = v.color.xyz * 2 - 1;

                // オブジェクト空間の情報
                float3 normalOS = v.normal;
                float3 tangentOS = v.tangent.xyz;
                float3 binormalOS = cross(normalOS, tangentOS) * v.tangent.w * unity_WorldTransformParams.w;

                // オブジェクト空間 → タンジェント空間 の変換行列
                float3x3 objectToTangentMatrix = float3x3(tangentOS.xyz, binormalOS, normalOS);
                // タンジェント空間 → オブジェクト空間 の変換行列
                float3x3 tangentToObjectMatrix = transpose(objectToTangentMatrix);

                // タンジェント空間のベクトルをオブジェクト空間に変換
                float3 smoothNormalOS = mul(tangentToObjectMatrix, smoothNormalTS);

                // 頂点座標をスムース法線の方向に押し出す
                float3 vertexOS = v.vertex.xyz + smoothNormalOS * _OutlineWidth;
                o.positionCS = UnityObjectToClipPos(vertexOS);

                return o;
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
