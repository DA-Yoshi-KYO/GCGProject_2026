Shader "Custom/SH_Toon"
{
    Properties
    {
        [MainColor]
        _Color("Base Color", Color) = (1, 1, 1, 1)

        [MainTexture]
        _MainTex("Base Texture", 2D) = "white" {}

        [Header(Toon Shadow)]
        _ShadowColor("Shadow Color", Color) = (0.55, 0.55, 0.65, 1)
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0.001, 0.5)) = 0.05

        [Header(Specular)]
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSize("Specular Size", Range(0.001, 1)) = 0.1
        _SpecularSoftness("Specular Softness", Range(0.001, 0.5)) = 0.02
        _SpecularIntensity("Specular Intensity", Range(0, 2)) = 0.5

        [Header(Rim Light)]
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0.1, 10)) = 3.0
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.5
        _RimSmoothness("Rim Smoothness", Range(0.001, 0.5)) = 0.1
        _RimIntensity("Rim Intensity", Range(0, 2)) = 0.3

        [Header(Ambient)]
        _AmbientColor("Ambient Color", Color) = (0.15, 0.15, 0.2, 1)
        _AmbientIntensity("Ambient Intensity", Range(0, 2)) = 0.3
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
            #pragma multi_compile_instancing

            // Main Light Shadow
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // Fog
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;

                half4 _Color;

                half4 _ShadowColor;
                half _ShadowThreshold;
                half _ShadowSmoothness;

                half4 _SpecularColor;
                half _SpecularSize;
                half _SpecularSoftness;
                half _SpecularIntensity;

                half4 _RimColor;
                half _RimPower;
                half _RimThreshold;
                half _RimSmoothness;
                half _RimIntensity;

                half4 _AmbientColor;
                half _AmbientIntensity;
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
                float4 shadowCoord : TEXCOORD4;

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
                output.normalWS =
                    NormalizeNormalPerVertex(normalInputs.normalWS);

                output.uv =
                    TRANSFORM_TEX(input.uv, _MainTex);

                output.fogFactor =
                    ComputeFogFactor(positionInputs.positionCS.z);

                output.shadowCoord =
                    TransformWorldToShadowCoord(positionInputs.positionWS);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 textureColor =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv);

                half4 baseColor =
                    textureColor * _Color;

                half3 normalWS =
                    NormalizeNormalPerPixel(input.normalWS);

                half3 viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(input.positionWS);

                //-----------------------------------
                // Main Light
                //-----------------------------------
                Light mainLight =
                    GetMainLight(
                        inputData.shadowCoord
                    );
                
                half3 toonDiffuse =
                    CalculateToonDiffuse(
                        brdfData,
                        mainLight,
                        inputData.normalWS
                    );
                
                half3 directSpecular =
                    CalculatePBRDirectSpecular(
                        brdfData,
                        mainLight,
                        inputData.normalWS,
                        inputData.viewDirectionWS
                    );
                
                //-----------------------------------
                // Additional Lights
                // Point Light / Spot Light
                //-----------------------------------
                #ifdef _ADDITIONAL_LIGHTS
                
                uint additionalLightCount =
                    GetAdditionalLightsCount();
                
                for (uint lightIndex = 0u;
                     lightIndex < additionalLightCount;
                     ++lightIndex)
                {
                    Light additionalLight =
                        GetAdditionalLight(
                            lightIndex,
                            inputData.positionWS
                        );
                
                    toonDiffuse +=
                        CalculateToonDiffuse(
                            brdfData,
                            additionalLight,
                            inputData.normalWS
                        );
                
                    directSpecular +=
                        CalculatePBRDirectSpecular(
                            brdfData,
                            additionalLight,
                            inputData.normalWS,
                            inputData.viewDirectionWS
                        );
                }
                
                #endif

                // URPの影を反映
                half shadowAttenuation =
                    mainLight.shadowAttenuation;

                half distanceAttenuation =
                    mainLight.distanceAttenuation;

                half lightAttenuation =
                    shadowAttenuation * distanceAttenuation;

                toonLighting *= lightAttenuation;

                // 影色とベース色を切り替え
                half3 shadowColor =
                    baseColor.rgb * _ShadowColor.rgb;

                half3 litColor =
                    baseColor.rgb * mainLight.color;

                half3 diffuseColor =
                    lerp(
                        shadowColor,
                        litColor,
                        toonLighting);

                // 環境光
                half3 ambientColor =
                    baseColor.rgb *
                    _AmbientColor.rgb *
                    _AmbientIntensity;

                // トゥーンハイライト
                half3 halfDirectionWS =
                    normalize(
                        lightDirectionWS +
                        viewDirectionWS);

                half NdotH =
                    saturate(
                        dot(normalWS, halfDirectionWS));

                half specularThreshold =
                    1.0h - _SpecularSize;

                half specularMask =
                    smoothstep(
                        specularThreshold - _SpecularSoftness,
                        specularThreshold + _SpecularSoftness,
                        NdotH);

                // 暗い側にはハイライトを出しにくくする
                specularMask *= toonLighting;

                half3 specularColor =
                    _SpecularColor.rgb *
                    specularMask *
                    _SpecularIntensity *
                    mainLight.color;

                // リムライト
                half rimBase =
                    1.0h -
                    saturate(
                        dot(normalWS, viewDirectionWS));

                half rimValue =
                    pow(rimBase, _RimPower);

                half rimMask =
                    smoothstep(
                        _RimThreshold - _RimSmoothness,
                        _RimThreshold + _RimSmoothness,
                        rimValue);

                half3 rimColor =
                    _RimColor.rgb *
                    rimMask *
                    _RimIntensity;

                half3 finalColor =
                    diffuseColor +
                    ambientColor +
                    specularColor +
                    rimColor;

                finalColor =
                    MixFog(
                        finalColor,
                        input.fogFactor);

                return half4(
                    finalColor,
                    baseColor.a);
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
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

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
                ShadowVaryings output = (ShadowVaryings)0;

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

                    float3 lightDirectionWS =
                        _LightDirection;

                #endif

                float4 positionCS =
                    TransformWorldToHClip(
                        ApplyShadowBias(
                            positionWS,
                            normalWS,
                            lightDirectionWS));

                #if UNITY_REVERSED_Z

                    positionCS.z =
                        min(
                            positionCS.z,
                            UNITY_NEAR_CLIP_VALUE);

                #else

                    positionCS.z =
                        max(
                            positionCS.z,
                            UNITY_NEAR_CLIP_VALUE);

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
