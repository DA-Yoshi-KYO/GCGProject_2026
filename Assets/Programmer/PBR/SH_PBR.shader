Shader "Custom/SH_PBR"
{
    Properties
    {
        [MainColor]
        _Color("Color", Color) = (1, 1, 1, 1)

        [MainTexture]
        _MainTex("Albedo", 2D) = "white" {}

        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
        }

        LOD 300

        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #pragma target 3.0

            // GPU Instancing
            #pragma multi_compile_instancing

            // URP Shadow
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // URP Additional Lights
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // Lightmap / Fog
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Glossiness;
                half _Metallic;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS    : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                half fogFactor    : TEXCOORD3;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 baseMap =
                    SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                half4 baseColor = baseMap * _Color;

                // PBRへ渡す表面情報
                SurfaceData surfaceData = (SurfaceData)0;

                surfaceData.albedo = baseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.specular = half3(0.0h, 0.0h, 0.0h);
                surfaceData.smoothness = _Glossiness;
                surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
                surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
                surfaceData.occlusion = 1.0h;
                surfaceData.alpha = baseColor.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;

                // PBR計算に必要な入力情報
                InputData inputData = (InputData)0;

                inputData.positionWS = input.positionWS;
                inputData.normalWS =
                    NormalizeNormalPerPixel(input.normalWS);

                inputData.viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(input.positionWS);

                inputData.shadowCoord =
                    TransformWorldToShadowCoord(input.positionWS);

                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = VertexLighting(
                    input.positionWS,
                    inputData.normalWS);

                inputData.bakedGI = SampleSH(inputData.normalWS);
                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);

                inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);

                half4 color =
                    UniversalFragmentPBR(inputData, surfaceData);

                color.rgb = MixFog(color.rgb, inputData.fogCoord);

                return color;
            }

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"

            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _LightDirection;
            float3 _LightPosition;

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                float3 normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS =
                        normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(
                    ApplyShadowBias(
                        positionWS,
                        normalWS,
                        lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z =
                        min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z =
                        max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;

                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                return 0;
            }

            ENDHLSL
        }
    }
}
