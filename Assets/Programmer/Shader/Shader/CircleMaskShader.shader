// フォルダの位置(マテリアルからShaderを探す時の階層に使用)
Shader "Custom/CircleMask"
{
    Properties
    {
        // ここに使用するプロパティを記述
        // 変数名(インスペクターの表示名, 型) = デフォルト値
        // 例 _Color("Color", Color) = (1,1,1,1)     // カラーパレット
        // 例 _Texture("Texture", 2D) = "white" {}   // 画像 白テクスチャをデフォルトとして使用 {}は追加オプション無し
        // 例 _Alpha("FillAmount", Range(0,1)) = 0.5 // 0~1の範囲指定のfloat
        // 例 _Speed("Speed", Float) = 0.5           // 自由指定のfloat

        [Toggle] _InColorBoolean("InColorBoolean", Float) = 1
        _ScaleFloat("Scale", Float) = 0.5
        _PosVector2("Pos", Vector) = (0.5, 0.5, 0, 0)
        _MaskColor("MaskColor", Color) =  (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }   // 透明度を使うのでOpaqueではなくTransparentを使用する  
        Cull Off    // 両面描画
        Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド
        ZWrite Off  // Z書き込み無し

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 使用する変数
            // プロパティの値を持ってきたい時はその変数名と一致させる
            // half4 _Color;   // half4...中精度のfloat4
            // sampler2D _Texture;  // sampler2D...テクスチャの格納先
            // float _Alpha;
            // float _Speed;
            float _InColorBoolean;
            float _ScaleFloat;
            float2 _PosVector2;
            float4 _MaskColor;


            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            // VS
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // PS
            // 主にこちらを記述する
            half4 frag(v2f input) : SV_Target
            {
                half4 color = half4(1,1,1,1);

                float2 uvXY = input.uv;
                float2 distance = length(uvXY - _PosVector2);

                float scale = _ScaleFloat - 0.05;

                float step = smoothstep(scale, _ScaleFloat, distance);

                float alpha = 0.0;

                if(_InColorBoolean == 0)
                {
                    alpha = step;
                }
                else
                {
                    alpha = 1.0 - step;
                }
                
                color.xyz = _MaskColor.xyz;

                color.w = alpha;

                return color;
            }
            ENDCG
        }
    }
}
