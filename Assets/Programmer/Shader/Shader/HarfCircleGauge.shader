Shader "Custom/HalfCircleGauge"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _FillAmount("FillAmount", Range(0,1)) = 0.5
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _FillAmount;
            fixed4 _Color;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float2 p = float2(i.uv.x - 0.5, (1 - i.uv.y) - 1);
                float angle = (atan2(p.y, p.x) + 3.14159) / 3.14159;
                col.a *= step(angle, _FillAmount);
                return col;
            }
            ENDCG
        }
    }
}
