Shader "Unlit/SH_CryRengerCircle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [HDR]_ColorA ("Color A", Color) = (0.1, 0.8, 0.2, 1.0)
        [HDR]_ColorB ("Color B", Color) = (0.7, 1.0, 0.7, 1.0)

        _GlowPower ("Glow Power", Range(0, 10)) = 3
        _Alpha ("Alpha", Range(0, 1)) = 1
        _BlackCut ("Black Cut", Range(0, 1)) = 0.05

        _RotationSpeed ("Rotation Speed", Float) = 1
        _ColorBlendPower ("Color Blend Power", Range(0.1, 5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
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
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _ColorA;
            fixed4 _ColorB;

            float _GlowPower;
            float _Alpha;
            float _BlackCut;
            float _RotationSpeed;
            float _ColorBlendPower;

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
                //-----------------------------------
                // 中心基準でUV回転
                //-----------------------------------
                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);
                float2 d = uv - center;

                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);

                float2 rotUV;
                rotUV.x = d.x * c - d.y * s;
                rotUV.y = d.x * s + d.y * c;
                rotUV += center;

                //-----------------------------------
                // テクスチャ取得
                //-----------------------------------
                fixed4 texColor = tex2D(_MainTex, rotUV);

                // 白いほど表示、黒いほど透過
                float rawMask = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

                float safeBlackCut = min(_BlackCut, 0.999);
                float mask = saturate((rawMask - safeBlackCut) / max(1.0 - safeBlackCut, 0.0001));

                //-----------------------------------
                // 2色ブレンド
                // 薄い部分 -> ColorA
                // 濃い部分 -> ColorB
                //-----------------------------------
                float colorLerp = pow(mask, _ColorBlendPower);
                fixed3 blendColor = lerp(_ColorA.rgb, _ColorB.rgb, colorLerp);

                //-----------------------------------
                // Particle System側の色も反映
                //-----------------------------------
                float particleAlpha = i.color.a;

                fixed4 col;
                col.rgb = blendColor * _GlowPower * mask * _Alpha * particleAlpha * i.color.rgb;
                col.a   = mask * _Alpha * particleAlpha;

                return col;
            }
            ENDCG
        }
    }
}
