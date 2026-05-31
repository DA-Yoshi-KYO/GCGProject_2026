Shader "Unlit/SH_magiccircle_down"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [HDR]_GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowPower ("Glow Power", Range(0, 10)) = 3
        _Alpha ("Alpha", Range(0, 1)) = 1
        _BlackCut ("Black Cut", Range(0, 1)) = 0.05
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

            fixed4 _GlowColor;
            float _GlowPower;
            float _Alpha;
            float _BlackCut;

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
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // 白黒画像から明るさを取る
                float mask = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

                // 黒い部分を消す
                mask = saturate((mask - _BlackCut) / (1.0 - _BlackCut));

                // Particle System の Color over Lifetime も反映
                float particleAlpha = i.color.a;

                // 最終的な光
                fixed4 col;
                col.rgb = _GlowColor.rgb * _GlowPower * mask * _Alpha * particleAlpha;
                col.a = mask * _Alpha * particleAlpha;

                return col;
            }
            ENDCG
        }
    }
}
