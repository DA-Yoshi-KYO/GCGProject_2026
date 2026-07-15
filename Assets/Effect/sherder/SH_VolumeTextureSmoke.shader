Shader "Custom/Effect/SH_VolumeTextureSmoke"
{
    Properties
    {
        [Header(Volume Texture)]
        _VolumeAtlasCurrent(
            "Volume Atlas Current",
            2D
        ) = "black" {}

        _VolumeAtlasNext(
            "Volume Atlas Next",
            2D
        ) = "black" {}

        _FrameBlend(
            "Frame Blend",
            Range(0.0, 1.0)
        ) = 0.0

        [Header(Smoke)]
        [HDR]
        _SmokeColor(
            "Smoke Color",
            Color
        ) = (0.92549, 0.768627, 0.266667, 1.0)

        _Density(
            "Density",
            Range(0.0, 30.0)
        ) = 8.0

        _DensityCutoff(
            "Density Cutoff",
            Range(0.0, 1.0)
        ) = 0.01

        _GlobalAlpha(
            "Global Alpha",
            Range(0.0, 1.0)
        ) = 1.0

        [Header(Raymarch)]
        _StepCount(
            "Raymarch Step Count",
            Range(8.0, 128.0)
        ) = 48.0

        [Header(Atlas)]
        _TilesX(
            "Tiles X",
            Float
        ) = 6.0

        _TilesY(
            "Tiles Y",
            Float
        ) = 6.0

        _SliceCount(
            "Slice Count",
            Float
        ) = 36.0

        [Toggle]
        _FlipAtlasY(
            "Flip Atlas Y",
            Float
        ) = 1.0

        [Toggle]
        _FlipSliceDirection(
            "Flip Slice Direction",
            Float
        ) = 0.0

        [Toggle]
        _FlipSliceV(
            "Flip Slice V",
            Float
        ) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "VolumeSmoke"

            // RGBをAlpha乗算済みで返すためPremultiplied Alpha。
            Blend One OneMinusSrcAlpha

            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_VolumeAtlasCurrent);
            SAMPLER(sampler_VolumeAtlasCurrent);

            TEXTURE2D(_VolumeAtlasNext);
            SAMPLER(sampler_VolumeAtlasNext);

            // Unityが各Textureに対して自動設定する値。
            float4 _VolumeAtlasCurrent_TexelSize;
            float4 _VolumeAtlasNext_TexelSize;

            CBUFFER_START(UnityPerMaterial)

                float4 _SmokeColor;

                float _FrameBlend;

                float _Density;
                float _DensityCutoff;
                float _GlobalAlpha;
                float _StepCount;

                float _TilesX;
                float _TilesY;
                float _SliceCount;

                float _FlipAtlasY;
                float _FlipSliceDirection;
                float _FlipSliceV;

            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            /// <summary>
            /// Vertex Shaderです。
            /// </summary>
            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.positionOS =
                    input.positionOS.xyz;

                return output;
            }

            /// <summary>
            /// 0に近い方向成分による除算エラーを防ぎます。
            /// </summary>
            float3 GetSafeDirection(
                float3 direction)
            {
                float3 directionSign =
                    step(0.0, direction) *
                    2.0 -
                    1.0;

                return directionSign *
                    max(
                        abs(direction),
                        0.00001
                    );
            }

            /// <summary>
            /// 指定SliceのAtlas上のUVを計算します。
            /// </summary>
            float2 GetAtlasUV(
                float2 sliceUV,
                float sliceIndex,
                float2 atlasTexelSize)
            {
                float tilesX =
                    max(
                        floor(_TilesX + 0.5),
                        1.0
                    );

                float tilesY =
                    max(
                        floor(_TilesY + 0.5),
                        1.0
                    );

                float2 tileCount =
                    float2(
                        tilesX,
                        tilesY
                    );

                // Bilinearで隣のTileへにじまないよう、
                // Tile内部へ半Pixel分だけ寄せます。
                float2 tileMargin =
                    0.5 *
                    atlasTexelSize *
                    tileCount;

                sliceUV = clamp(
                    sliceUV,
                    tileMargin,
                    1.0 - tileMargin
                );

                if (_FlipSliceV > 0.5)
                {
                    sliceUV.y =
                        1.0 - sliceUV.y;
                }

                float tileX =
                    fmod(
                        sliceIndex,
                        tilesX
                    );

                float tileY =
                    floor(
                        sliceIndex /
                        tilesX
                    );

                if (_FlipAtlasY > 0.5)
                {
                    tileY =
                        tilesY -
                        1.0 -
                        tileY;
                }

                return (
                    float2(
                        tileX,
                        tileY
                    ) +
                    sliceUV
                ) /
                tileCount;
            }

            /// <summary>
            /// 現在フレームの指定Sliceを取得します。
            /// </summary>
            float SampleCurrentAtlasSlice(
                float2 sliceUV,
                float sliceIndex)
            {
                float2 atlasUV =
                    GetAtlasUV(
                        sliceUV,
                        sliceIndex,
                        _VolumeAtlasCurrent_TexelSize.xy
                    );

                return SAMPLE_TEXTURE2D_LOD(
                    _VolumeAtlasCurrent,
                    sampler_VolumeAtlasCurrent,
                    atlasUV,
                    0.0
                ).r;
            }

            /// <summary>
            /// 次フレームの指定Sliceを取得します。
            /// </summary>
            float SampleNextAtlasSlice(
                float2 sliceUV,
                float sliceIndex)
            {
                float2 atlasUV =
                    GetAtlasUV(
                        sliceUV,
                        sliceIndex,
                        _VolumeAtlasNext_TexelSize.xy
                    );

                return SAMPLE_TEXTURE2D_LOD(
                    _VolumeAtlasNext,
                    sampler_VolumeAtlasNext,
                    atlasUV,
                    0.0
                ).r;
            }

            /// <summary>
            /// 現在と次フレームを補間してSliceを取得します。
            /// </summary>
            float SampleAtlasSlice(
                float2 sliceUV,
                float sliceIndex)
            {
                float currentDensity =
                    SampleCurrentAtlasSlice(
                        sliceUV,
                        sliceIndex
                    );

                float nextDensity =
                    SampleNextAtlasSlice(
                        sliceUV,
                        sliceIndex
                    );

                return lerp(
                    currentDensity,
                    nextDensity,
                    saturate(_FrameBlend)
                );
            }

            /// <summary>
            /// 3D位置からVolume Densityを取得します。
            /// Houdini側でUp AxisをYにしているため、
            /// YをSlice方向として使用します。
            /// </summary>
            float SampleVolume(
                float3 volumePosition)
            {
                volumePosition =
                    saturate(volumePosition);

                float tilesX =
                    max(
                        floor(_TilesX + 0.5),
                        1.0
                    );

                float tilesY =
                    max(
                        floor(_TilesY + 0.5),
                        1.0
                    );

                float maximumSliceCount =
                    tilesX *
                    tilesY;

                float sliceCount =
                    clamp(
                        floor(
                            _SliceCount +
                            0.5
                        ),
                        1.0,
                        maximumSliceCount
                    );

                float slicePosition =
                    volumePosition.y *
                    max(
                        sliceCount - 1.0,
                        0.0
                    );

                if (_FlipSliceDirection > 0.5)
                {
                    slicePosition =
                        sliceCount -
                        1.0 -
                        slicePosition;
                }

                float sliceIndex0 =
                    floor(slicePosition);

                float sliceIndex1 =
                    min(
                        sliceIndex0 + 1.0,
                        sliceCount - 1.0
                    );

                float sliceBlend =
                    frac(slicePosition);

                // 1Slice内はX・Z平面です。
                float2 sliceUV =
                    float2(
                        volumePosition.x,
                        volumePosition.z
                    );

                float density0 =
                    SampleAtlasSlice(
                        sliceUV,
                        sliceIndex0
                    );

                float density1 =
                    SampleAtlasSlice(
                        sliceUV,
                        sliceIndex1
                    );

                return lerp(
                    density0,
                    density1,
                    sliceBlend
                );
            }

            /// <summary>
            /// Cube内部をレイマーチして煙を描画します。
            /// </summary>
            half4 Frag(
                Varyings input) : SV_Target
            {
                float3 cameraPositionOS =
                    TransformWorldToObject(
                        _WorldSpaceCameraPos
                    );

                float3 rayDirectionOS =
                    normalize(
                        input.positionOS -
                        cameraPositionOS
                    );

                // Cube表面から少しだけ内部へ移動します。
                float3 rayOriginOS =
                    input.positionOS +
                    rayDirectionOS *
                    0.0005;

                float3 safeDirection =
                    GetSafeDirection(
                        rayDirectionOS
                    );

                float3 inverseDirection =
                    rcp(safeDirection);

                float3 distanceToMinimum =
                    (
                        -0.5 -
                        rayOriginOS
                    ) *
                    inverseDirection;

                float3 distanceToMaximum =
                    (
                        0.5 -
                        rayOriginOS
                    ) *
                    inverseDirection;

                float3 farDistance =
                    max(
                        distanceToMinimum,
                        distanceToMaximum
                    );

                float exitDistance =
                    min(
                        farDistance.x,
                        min(
                            farDistance.y,
                            farDistance.z
                        )
                    );

                if (exitDistance <= 0.0)
                {
                    discard;
                }

                int stepCount =
                    (int)clamp(
                        floor(
                            _StepCount +
                            0.5
                        ),
                        8.0,
                        128.0
                    );

                float stepLength =
                    exitDistance /
                    max(
                        (float)stepCount,
                        1.0
                    );

                float3 samplePositionOS =
                    rayOriginOS +
                    rayDirectionOS *
                    stepLength *
                    0.5;

                float3 accumulatedColor =
                    float3(
                        0.0,
                        0.0,
                        0.0
                    );

                float accumulatedAlpha =
                    0.0;

                float globalAlpha =
                    saturate(_GlobalAlpha) *
                    saturate(_SmokeColor.a);

                [loop]
                for (int i = 0; i < 128; i++)
                {
                    if (i >= stepCount)
                    {
                        break;
                    }

                    float3 volumePosition =
                        samplePositionOS +
                        0.5;

                    if (
                        any(
                            volumePosition <
                            0.0
                        ) ||
                        any(
                            volumePosition >
                            1.0
                        )
                    )
                    {
                        break;
                    }

                    float density =
                        SampleVolume(
                            volumePosition
                        );

                    density =
                        max(
                            density -
                            _DensityCutoff,
                            0.0
                        );

                    float sampleAlpha =
                        1.0 -
                        exp(
                            -density *
                            _Density *
                            stepLength
                        );

                    sampleAlpha *=
                        globalAlpha;

                    float remainingAlpha =
                        1.0 -
                        accumulatedAlpha;

                    // Premultiplied Color。
                    accumulatedColor +=
                        remainingAlpha *
                        _SmokeColor.rgb *
                        sampleAlpha;

                    accumulatedAlpha +=
                        remainingAlpha *
                        sampleAlpha;

                    if (accumulatedAlpha >= 0.98)
                    {
                        break;
                    }

                    samplePositionOS +=
                        rayDirectionOS *
                        stepLength;
                }

                return half4(
                    accumulatedColor,
                    accumulatedAlpha
                );
            }

            ENDHLSL
        }
    }

    Fallback Off
}
