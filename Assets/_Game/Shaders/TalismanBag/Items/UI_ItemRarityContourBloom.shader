Shader "TalismanBag/UI/ItemRarityContourBloom"
{
    Properties
    {
        [PerRendererData] _MainTex ("Authoritative Sprite Texture", 2D) = "white" {}
        _Color ("Vertex Intensity", Color) = (1, 1, 1, 1)
        _BandColorA ("Band Color A", Color) = (0.85, 0.16, 0.08, 1)
        _BandColorB ("Band Color B", Color) = (1, 0.45, 0.08, 1)
        _HighlightColor ("Moving Highlight", Color) = (1, 0.84, 0.35, 1)
        _OutlineRadiusTexels ("Outline Radius (Texels)", Range(0.25, 32)) = 15
        _InnerEdgeStrength ("Inner Edge Strength", Range(0, 1)) = 0.08
        _HaloStrength ("Soft Halo Strength", Range(0, 2)) = 0.2
        _BandOpacity ("Band Opacity", Range(0, 3)) = 1
        _Brightness ("Brightness", Range(0, 4)) = 1
        _BodyEmissionStrength ("Whole Body Emission", Range(0, 2)) = 0
        _BodyWarmth ("Whole Body Warmth", Range(0, 1)) = 0.24
        _GlowFalloff ("Glow Falloff", Range(0.1, 2)) = 0.58
        _FlowSpeed ("Flow Speed", Range(0.01, 2)) = 0.16
        _FlowScale ("Flow Scale", Range(0.25, 8)) = 2.6
        _AngularBlend ("Angular Phase Blend", Range(0, 1)) = 0.78
        _NoiseAmount ("Procedural Noise", Range(0, 1)) = 0.08
        _HighlightStrength ("Highlight Strength", Range(0, 4)) = 1.35
        [NoScaleOffset] _GradientLut ("Replaceable Gradient LUT", 2D) = "white" {}
        [NoScaleOffset] _NoiseTexture ("Replaceable Noise Texture", 2D) = "white" {}
        [NoScaleOffset] _RingTexture ("Replaceable Ring Modulator", 2D) = "white" {}
        _UseGradientLut ("Use Gradient LUT", Float) = 0
        _UseNoiseTexture ("Use Noise Texture", Float) = 0
        _UseRingTexture ("Use Ring Modulator", Float) = 0
        [HideInInspector] _SrcBlend ("Source Blend", Float) = 5
        [HideInInspector] _DstBlend ("Destination Blend", Float) = 10

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend [_SrcBlend] [_DstBlend]
        ColorMask [_ColorMask]

        Pass
        {
            Name "ItemRarityContourBloom"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 objectPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _GradientLut;
            sampler2D _NoiseTexture;
            sampler2D _RingTexture;
            fixed4 _TextureSampleAdd;
            fixed4 _Color;
            fixed4 _BandColorA;
            fixed4 _BandColorB;
            fixed4 _HighlightColor;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _ClipRect;
            float _OutlineRadiusTexels;
            float _InnerEdgeStrength;
            float _HaloStrength;
            float _BandOpacity;
            float _Brightness;
            float _BodyEmissionStrength;
            float _BodyWarmth;
            float _GlowFalloff;
            float _FlowSpeed;
            float _FlowScale;
            float _AngularBlend;
            float _NoiseAmount;
            float _HighlightStrength;
            float _UseGradientLut;
            float _UseNoiseTexture;
            float _UseRingTexture;

            v2f vert(appdata_t v)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.worldPosition = v.vertex;
                output.vertex = UnityObjectToClipPos(output.worldPosition);
                output.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                output.objectPosition = v.vertex.xy;
                output.color = v.color * _Color;
                return output;
            }

            fixed SampleAlpha(float2 uv)
            {
                return saturate((tex2D(_MainTex, uv) + _TextureSampleAdd).a);
            }

            float ProceduralNoise(float2 value)
            {
                return frac(sin(dot(value, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 radius = _MainTex_TexelSize.xy * _OutlineRadiusTexels;
                float2 diagonal = radius * 0.70710678;
                fixed4 sourceSample = tex2D(_MainTex, input.texcoord);
                fixed centerAlpha = saturate((sourceSample + _TextureSampleAdd).a);

                fixed n0 = SampleAlpha(input.texcoord + float2(radius.x, 0));
                fixed n1 = SampleAlpha(input.texcoord + float2(-radius.x, 0));
                fixed n2 = SampleAlpha(input.texcoord + float2(0, radius.y));
                fixed n3 = SampleAlpha(input.texcoord + float2(0, -radius.y));
                fixed n4 = SampleAlpha(input.texcoord + diagonal);
                fixed n5 = SampleAlpha(input.texcoord - diagonal);
                fixed n6 = SampleAlpha(input.texcoord + float2(diagonal.x, -diagonal.y));
                fixed n7 = SampleAlpha(input.texcoord + float2(-diagonal.x, diagonal.y));

                fixed maxNeighbour = max(
                    max(max(n0, n1), max(n2, n3)),
                    max(max(n4, n5), max(n6, n7)));
                fixed minNeighbour = min(
                    min(min(n0, n1), min(n2, n3)),
                    min(min(n4, n5), min(n6, n7)));
                fixed averageNeighbour =
                    (n0 + n1 + n2 + n3 + n4 + n5 + n6 + n7) * 0.125;

                fixed neighbourReach = saturate(maxNeighbour - centerAlpha);
                fixed contourCoverage = smoothstep(0.018, 0.24, neighbourReach);
                fixed outerBand = saturate(max(
                    neighbourReach * 1.35,
                    contourCoverage * 0.86));
                fixed innerEnergy = saturate(centerAlpha - minNeighbour)
                    * _InnerEdgeStrength;
                fixed band = saturate(outerBand + innerEnergy);
                fixed haloSignal = pow(
                    saturate(averageNeighbour * (1 - centerAlpha)),
                    max(_GlowFalloff, 0.1));
                fixed halo = saturate(
                    haloSignal * _HaloStrength
                    + contourCoverage * (1 - centerAlpha)
                    * _HaloStrength * 0.12);

                float2 centered = input.objectPosition;
                float2 localDirection =
                    centered / max(length(centered), 0.0001);
                float angularPhase = atan2(centered.y, centered.x)
                    * 0.15915494 + 0.5;
                float directionalPhase = dot(
                    localDirection,
                    float2(0.78, 0.36)) * 0.5 + 0.5;
                float fallbackNoise =
                    ProceduralNoise(floor(input.texcoord * 48.0));
                float textureNoise = tex2D(
                    _NoiseTexture,
                    input.texcoord * 3.0
                    + float2(_Time.y * 0.015, 0)).r;
                float noise = lerp(
                    fallbackNoise,
                    textureNoise,
                    saturate(_UseNoiseTexture));
                float phase = frac(
                    lerp(directionalPhase, angularPhase, _AngularBlend)
                    * _FlowScale
                    - _Time.y * _FlowSpeed
                    + (noise - 0.5) * _NoiseAmount);

                float gradientWave = 0.5 + 0.5 * sin(phase * 6.2831853);
                float segmentDistance =
                    abs(frac(phase + 0.5) - 0.5) * 2.0;
                float brightSegment = pow(
                    saturate(1.0 - segmentDistance),
                    3.2);
                float brightTail = pow(
                    saturate(1.0 - segmentDistance),
                    1.35) * 0.34;
                float ringModulator = lerp(
                    1.0,
                    tex2D(_RingTexture, float2(phase, 0.5)).r,
                    saturate(_UseRingTexture));
                brightSegment *= ringModulator;
                brightTail *= ringModulator;

                fixed3 proceduralPalette = lerp(
                    _BandColorA.rgb,
                    _BandColorB.rgb,
                    0.34 + gradientWave * 0.18);
                fixed3 lutPalette = tex2D(
                    _GradientLut,
                    float2(phase, 0.5)).rgb;
                fixed3 palette = lerp(
                    proceduralPalette,
                    lutPalette,
                    saturate(_UseGradientLut));
                float highlightMix = saturate(
                    brightSegment * _HighlightStrength * 0.8
                    + brightTail);
                palette = lerp(
                    palette,
                    _HighlightColor.rgb,
                    highlightMix);
                palette += _HighlightColor.rgb
                    * brightSegment
                    * _HighlightStrength
                    * 0.32;

                fixed outlineSignal = saturate(
                    band * _BandOpacity + halo);
                fixed bodySignal = saturate(
                    centerAlpha * _BodyEmissionStrength);
                fixed3 bodyTint = lerp(
                    fixed3(1, 1, 1),
                    _BandColorB.rgb,
                    saturate(_BodyWarmth));
                fixed3 bodyEmission = saturate(sourceSample.rgb) * bodyTint;
                bodyEmission += _HighlightColor.rgb
                    * saturate(_BodyWarmth)
                    * 0.12;
                fixed combinedSignal = max(
                    outlineSignal + bodySignal,
                    0.0001);
                fixed3 combinedColor =
                    (palette * _Brightness * outlineSignal
                    + bodyEmission * bodySignal)
                    / combinedSignal;
                fixed alpha = saturate(
                    max(outlineSignal, bodySignal)
                    * input.color.a);
                fixed4 color = fixed4(
                    combinedColor * input.color.rgb,
                    alpha);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(
                    input.worldPosition.xy,
                    _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
