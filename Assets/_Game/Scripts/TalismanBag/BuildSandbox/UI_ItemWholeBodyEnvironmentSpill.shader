Shader "TalismanBag/UI/ItemWholeBodyEnvironmentSpill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Vertex Intensity", Color) = (1, 1, 1, 1)
        _InnerColor ("Inner Warm Light", Color) = (1, 0.68, 0.22, 1)
        _OuterColor ("Outer Vermilion Spill", Color) = (0.95, 0.18, 0.06, 1)
        _InnerRadius ("Inner Cutout Radius", Range(0.05, 0.35)) = 0.16
        _OuterRadius ("Outer Fade Radius", Range(0.3, 0.7)) = 0.52
        _SpillStrength ("Spill Strength", Range(0, 2)) = 1

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
            "CanUseSpriteAtlas" = "False"
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
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            Name "ItemWholeBodyEnvironmentSpill"

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _InnerColor;
            fixed4 _OuterColor;
            float4 _ClipRect;
            float _InnerRadius;
            float _OuterRadius;
            float _SpillStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 centered = IN.texcoord - 0.5;
                float distanceFromSource = length(centered);
                float innerOpen = smoothstep(
                    _InnerRadius * 0.45,
                    _InnerRadius,
                    distanceFromSource);
                float outerFade = 1.0 - smoothstep(
                    _InnerRadius,
                    _OuterRadius,
                    distanceFromSource);
                float spill = saturate(
                    innerOpen
                    * outerFade
                    * _SpillStrength);
                float warmCore = 1.0 - smoothstep(
                    _InnerRadius,
                    lerp(_InnerRadius, _OuterRadius, 0.58),
                    distanceFromSource);
                fixed3 spillColor = lerp(
                    _OuterColor.rgb,
                    _InnerColor.rgb,
                    saturate(warmCore));
                fixed4 color = fixed4(
                    spillColor * IN.color.rgb,
                    spill * IN.color.a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(
                    IN.worldPosition.xy,
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
