Shader "Unlit/SH_magiccircle_UpUV"
{
    Properties
    {
        _MainTex ("Pattern Texture", 2D) = "white" {}
        _MaskTex ("Bottom Mask Texture", 2D) = "white" {}
        _TopMaskTex ("Top Mask Texture", 2D) = "white" {}

        [HDR]_InnerColor ("Inner Color", Color) = (1, 1, 1, 1)
        [HDR]_OuterColor ("Outer Color", Color) = (0.2, 0.6, 1, 1)

        _GlowPower ("Glow Power", Range(0, 10)) = 3
        _Alpha ("Alpha", Range(0, 1)) = 1
        _BlackCut ("Black Cut", Range(0, 1)) = 0.05

        _GradientMode ("Gradient Mode 0=Vertical 1=Radial", Range(0, 1)) = 1
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1

        _ScrollSpeed ("UV Scroll Speed", Float) = 1

        _MaskPower ("Bottom Mask Power", Range(0.1, 10)) = 1
        _MaskInvert ("Bottom Mask Invert 0=Normal 1=Invert", Range(0, 1)) = 0

        _TopMaskPower ("Top Mask Power", Range(0.1, 10)) = 1
        _TopMaskStrength ("Top Mask Strength", Range(0, 5)) = 1
        _TopAreaPower ("Top Area Power", Range(0.1, 10)) = 2
        _TopMaskInvert ("Top Mask Invert 0=Normal 1=Invert", Range(0, 1)) = 0
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

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            sampler2D _TopMaskTex;
            float4 _TopMaskTex_ST;

            fixed4 _InnerColor;
            fixed4 _OuterColor;

            float _GlowPower;
            float _Alpha;
            float _BlackCut;
            float _GradientMode;
            float _GradientPower;
            float _ScrollSpeed;

            float _MaskPower;
            float _MaskInvert;

            float _TopMaskPower;
            float _TopMaskStrength;
            float _TopAreaPower;
            float _TopMaskInvert;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //-----------------------------------
                // 1. メイン柄（猫柄）UVスクロール
                //-----------------------------------
                float2 mainUV = TRANSFORM_TEX(i.uv, _MainTex);
                mainUV.y = frac(mainUV.y - _Time.y * _ScrollSpeed);

                fixed4 patternTex = tex2D(_MainTex, mainUV);

                // 白いほど出す、黒いほど消す
                float patternRaw = dot(patternTex.rgb, float3(0.299, 0.587, 0.114));
                float safeBlackCut = min(_BlackCut, 0.999);
                float patternMask = saturate((patternRaw - safeBlackCut) / max(1.0 - safeBlackCut, 0.0001));

                // 必要なら _BlackCut を全体の出現量としても使う
                float reveal = saturate(1.0 - _BlackCut);
                patternMask *= reveal;

                //-----------------------------------
                // 2. 下側用マスク
                //-----------------------------------
                float2 bottomMaskUV = TRANSFORM_TEX(i.uv, _MaskTex);
                fixed4 bottomMaskTex = tex2D(_MaskTex, bottomMaskUV);

                float bottomMaskRaw = dot(bottomMaskTex.rgb, float3(0.299, 0.587, 0.114));

                // 0 = そのまま, 1 = 反転
                float bottomMask = lerp(bottomMaskRaw, 1.0 - bottomMaskRaw, _MaskInvert);
                bottomMask = pow(saturate(bottomMask), _MaskPower);

                //-----------------------------------
                // 3. いったん下側まで反映した最終マスク
                //-----------------------------------
                float finalMask = patternMask * bottomMask;

                //-----------------------------------
                // 4. 上側用マスク
                //-----------------------------------
                float2 topMaskUV = TRANSFORM_TEX(i.uv, _TopMaskTex);
                fixed4 topMaskTex = tex2D(_TopMaskTex, topMaskUV);

                float topMaskRaw = dot(topMaskTex.rgb, float3(0.299, 0.587, 0.114));

                // 0 = そのまま, 1 = 反転
                float topMask = lerp(topMaskRaw, 1.0 - topMaskRaw, _TopMaskInvert);
                topMask = pow(saturate(topMask), _TopMaskPower);

                // 上に行くほど効かせる
                float topArea = pow(saturate(i.uv.y), _TopAreaPower);

                // 下では 1.0（影響なし）、上では topMask を使う
                float topBlendMask = lerp(1.0, topMask, saturate(topArea * _TopMaskStrength));

                finalMask *= topBlendMask;

                //-----------------------------------
                // 5. 色グラデーション
                //-----------------------------------
                float verticalGradient = saturate(i.uv.y);

                float2 centerUV = i.uv - 0.5;
                float radialGradient = saturate(length(centerUV) * 2.0);

                // 0 = 縦, 1 = 中心から外
                float gradientValue = lerp(verticalGradient, radialGradient, saturate(_GradientMode));
                gradientValue = pow(saturate(gradientValue), _GradientPower);

                fixed4 gradientColor = lerp(_InnerColor, _OuterColor, gradientValue);

                //-----------------------------------
                // 6. Particle System の色・透明度を反映
                //-----------------------------------
                float particleAlpha = i.color.a;

                fixed4 col;
                col.rgb = gradientColor.rgb * _GlowPower * finalMask * _Alpha * particleAlpha * i.color.rgb;
                col.a   = finalMask * _Alpha * particleAlpha;

                return col;
            }
            ENDCG
        }
    }
}
