Shader "Unlit/SH_magiccircle_down_sweep"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}

        [HDR]_SweepColor ("Sweep Color", Color) = (0.4, 0.9, 1.0, 1)

        _Alpha ("Alpha", Range(0, 1)) = 1
        _BlackCut ("Black Cut", Range(0, 1)) = 0.05

        _BasePower ("Base Power", Range(0, 10)) = 0
        _SweepPower ("Sweep Power", Range(0, 20)) = 8

        _RotateSpeed ("Rotate Speed", Range(-5, 5)) = 0.8
        _SweepWidth ("Sweep Width", Range(0.01, 0.5)) = 0.08
        _SweepSoftness ("Sweep Softness", Range(0.001, 0.3)) = 0.08

        _NoiseScale ("Noise Scale", Range(0.1, 30)) = 8
        _NoiseSpeed ("Noise Speed", Range(-10, 10)) = 2
        _NoisePower ("Noise Power", Range(0, 1)) = 0.5

        _FlickerSpeed ("Flicker Speed", Range(0, 50)) = 18
        _FlickerPower ("Flicker Power", Range(0, 3)) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent+10"
            "IgnoreProjector"="True"
        }

        LOD 100

        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NoiseTex;

            fixed4 _SweepColor;

            float _Alpha;
            float _BlackCut;

            float _BasePower;
            float _SweepPower;

            float _RotateSpeed;
            float _SweepWidth;
            float _SweepSoftness;

            float _NoiseScale;
            float _NoiseSpeed;
            float _NoisePower;

            float _FlickerSpeed;
            float _FlickerPower;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y;

                // 元画像の白い部分だけ使う
                fixed4 texColor = tex2D(_MainTex, i.uv);
                float mask = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                mask = saturate((mask - _BlackCut) / (1.0 - _BlackCut));

                // UV中心から角度を取る
                float2 centerUV = i.uv - 0.5;
                float angle01 = atan2(centerUV.y, centerUV.x) / 6.2831853 + 0.5;

                // 回転する光の中心位置
                float sweepCenter = frac(time * _RotateSpeed);

                // 角度差を0〜0.5に丸める
                float angleDiff = abs(frac(angle01 - sweepCenter + 0.5) - 0.5);

                // 光の帯
                float sweep = 1.0 - smoothstep(_SweepWidth, _SweepWidth + _SweepSoftness, angleDiff);

                // ノイズで稲妻っぽいムラを作る
                float2 noiseUV = i.uv * _NoiseScale;
                noiseUV += float2(time * _NoiseSpeed, time * _NoiseSpeed * 0.35);

                float noise = tex2D(_NoiseTex, noiseUV).r;
                float noiseMask = smoothstep(0.35, 1.0, noise);
                noiseMask = lerp(1.0, noiseMask, _NoisePower);

                // ちらつき
                float flicker = sin(time * _FlickerSpeed + noise * 6.2831853) * 0.5 + 0.5;
                flicker = 1.0 + flicker * _FlickerPower;

                float particleAlpha = i.color.a;

                float glow = _BasePower;
                glow += sweep * _SweepPower * noiseMask * flicker;

                fixed4 col;
                col.rgb = _SweepColor.rgb * glow * mask * _Alpha * particleAlpha * i.color.rgb;
                col.a = mask * _Alpha * particleAlpha;

                return col;
            }
            ENDCG
        }
    }
}
