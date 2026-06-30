// フォルダの位置(マテリアルからShaderを探す時の階層に使用)
Shader "Custom/TextureMask"
{
    Properties
    {
        // ここに使用するプロパティを記述
        // 変数名(インスペクターの表示名, 型) = デフォルト値
        // 例 _Color("Color", Color) = (1,1,1,1)     // カラーパレット
        // 例 _Texture("Texture", 2D) = "white" {}   // 画像 白テクスチャをデフォルトとして使用 {}は追加オプション無し
        // 例 _Alpha("FillAmount", Range(0,1)) = 0.5 // 0~1の範囲指定のfloat
        // 例 _Speed("Speed", Float) = 0.5           // 自由指定のfloat

        _CurrentScaleFloat("CurrentScale", Float) = 0.5
        _ScaleFloat("Scale", Float) = 1.0
        _MaskTexture("Texture", 2D) = "white"{}
        _AlphaScaleFloat("AlphaScale", Float) = 0.5
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
            float _CurrentScaleFloat;
            float _ScaleFloat;
            sampler2D _MaskTexture;
            float _AlphaScaleFloat;
            float4 _MaskColor;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float4 screenPos:TEXCOORD1;};

            // VS
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            // PS
            // 主にこちらを記述する
            half4 frag(v2f input) : SV_Target
            {
                half4 color = half4(1,1,1,1);

                float2 screenPosXY = input.screenPos.xy;

                float2 subtractValue = float2(screenPosXY.x, screenPosXY.y) - float2(0.5, 0.5);

                float maxScale = 20;

                float clampScaleFloat = clamp(_CurrentScaleFloat, 0.0, maxScale) * _ScaleFloat;

                float2 multiplayValue = float2(subtractValue.x, subtractValue.y) * float2(clampScaleFloat, clampScaleFloat);

                float2 addValue = float2(multiplayValue.x, multiplayValue.y) + float2(0.5, 0.5);

                float2 uvXY = clamp(addValue,float2(0.0, 0.0), float2(1.0, 1.0));

                float multiplayAlpha = 0.0f;

                if(clampScaleFloat < maxScale)
                {
                    float4 textureColor = tex2D(_MaskTexture, uvXY);

                    multiplayAlpha = textureColor.w * _AlphaScaleFloat;
         
                    textureColor.xyz = _MaskColor.xyz;
                    color.xyz = textureColor.xyz;
                }
                else
                {
                    multiplayAlpha = 1.0 - _AlphaScaleFloat;
                   
                    color.xyz = _MaskColor.xyz;
                }

                float alpha = 0.0;

                alpha = 1.0 - multiplayAlpha;

                color.w = alpha;

                return color;
            }
            ENDCG
        }
    }
}
