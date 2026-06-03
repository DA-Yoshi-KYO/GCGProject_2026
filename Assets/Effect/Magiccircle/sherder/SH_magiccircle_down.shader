Shader "Unlit/SH_magiccircle_down"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [HDR]_InnerColor ("Inner Color", Color) = (1, 1, 1, 1)
        [HDR]_OuterColor ("Outer Color", Color) = (0.2, 0.6, 1, 1)

        _GlowPower ("Glow Power", Range(0, 10)) = 3
        _Alpha ("Alpha", Range(0, 1)) = 1
        _BlackCut ("Black Cut", Range(0, 1)) = 0.05

        _GradientMode ("Gradient Mode 0=Vertical 1=Radial", Range(0, 1)) = 1
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1
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

            fixed4 _InnerColor;
            fixed4 _OuterColor;

            float _GlowPower;
            float _Alpha;
            float _BlackCut;
            float _GradientMode;
            float _GradientPower;

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

                // 白黒画像の白い部分だけをマスクにする
                float rawMask = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                
                // _BlackCut = 1 のときに割り算が壊れないようにする
                float safeBlackCut = min(_BlackCut, 0.999);
                
                // 元の白黒マスク
                float mask = saturate((rawMask - safeBlackCut) / max(1.0 - safeBlackCut, 0.0001));
                
                // _BlackCut 1 → 0 で、全体が 0 → 1 に出るようにする
                float reveal = saturate(1.0 - _BlackCut);
                mask *= reveal;

                // 縦グラデーション
                float verticalGradient = i.uv.y;

                // 中心から外側へのグラデーション
                float2 centerUV = i.uv - 0.5;
                float radialGradient = saturate(length(centerUV) * 2.0);

                // 0なら縦、1なら中心外側グラデーション
                float gradientValue = lerp(verticalGradient, radialGradient, _GradientMode);

                // グラデーションの寄り方を調整
                gradientValue = pow(saturate(gradientValue), _GradientPower);

                // 内側色 → 外側色
                fixed4 gradientColor = lerp(_InnerColor, _OuterColor, gradientValue);

                // Particle System側のColor over Lifetimeも反映
                float particleAlpha = i.color.a;

                fixed4 col;
                col.rgb = gradientColor.rgb * _GlowPower * mask * _Alpha * particleAlpha * i.color.rgb;
                col.a = mask * _Alpha * particleAlpha;

                return col;
            }
            ENDCG
        }
    }
}
