/*
+=====================================
 ファイル名 : SH_EffectTemplate.shader
 概要     : Particle用エフェクトテンプレートShader
 内容     : 透過PNGをデフォルトで透過 / 色変更 / 明るさ / Alpha / UVスクロール / 回転 / Noise歪み
=====================================+
*/

Shader "Unlit/SH_EffectTemplate"
{
    Properties
    {
        [MainTexture] _MainTex ("Main Texture", 2D) = "white" {}
        [HDR] _MainColor ("Main Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 20)) = 1
        _Alpha ("Alpha", Range(0, 1)) = 1

        [Header(UV Animation)]
        _ScrollSpeedX ("Scroll Speed X", Float) = 0
        _ScrollSpeedY ("Scroll Speed Y", Float) = 0
        _RotationSpeed ("Rotation Speed", Float) = 0
        _RotationCenterX ("Rotation Center X", Float) = 0.5
        _RotationCenterY ("Rotation Center Y", Float) = 0.5

        [Header(Noise Distortion)]
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _NoiseStrength ("Noise Strength", Range(0, 0.5)) = 0
        _NoiseScrollSpeedX ("Noise Scroll Speed X", Float) = 0
        _NoiseScrollSpeedY ("Noise Scroll Speed Y", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            float4 _MainColor;
            float _Brightness;
            float _Alpha;

            float _ScrollSpeedX;
            float _ScrollSpeedY;
            float _RotationSpeed;
            float _RotationCenterX;
            float _RotationCenterY;

            float _NoiseStrength;
            float _NoiseScrollSpeedX;
            float _NoiseScrollSpeedY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
                fixed4 color : COLOR;
            };

            float2 RotateUV(float2 f_uv, float f_angle, float2 f_center)
            {
                float s = sin(f_angle);
                float c = cos(f_angle);

                f_uv -= f_center;

                float2 rotatedUV;
                rotatedUV.x = f_uv.x * c - f_uv.y * s;
                rotatedUV.y = f_uv.x * s + f_uv.y * c;

                rotatedUV += f_center;

                return rotatedUV;
            }

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);

                float angle = _Time.y * _RotationSpeed;
                float2 center = float2(_RotationCenterX, _RotationCenterY);

                uv = RotateUV(uv, angle, center);
                uv += float2(_ScrollSpeedX, _ScrollSpeedY) * _Time.y;

                float2 noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                noiseUV += float2(_NoiseScrollSpeedX, _NoiseScrollSpeedY) * _Time.y;

                o.uv = uv;
                o.noiseUV = noiseUV;
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 noiseTex = tex2D(_NoiseTex, i.noiseUV);

                float2 distortion = (noiseTex.rg - 0.5) * _NoiseStrength;
                float4 mainTex = tex2D(_MainTex, i.uv + distortion);

                float finalAlpha =
                    mainTex.a *
                    _MainColor.a *
                    i.color.a *
                    _Alpha;

                clip(finalAlpha - 0.001);

                float3 finalColor =
                    mainTex.rgb *
                    _MainColor.rgb *
                    i.color.rgb *
                    _Brightness;

                return float4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
