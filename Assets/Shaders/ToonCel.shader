Shader "Custom/ToonCel"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _ShadeColor ("Shadow Color", Color) = (0.45,0.42,0.6,1)
        _Threshold ("Shadow Threshold", Range(-1,1)) = 0.1
        _Softness ("Edge Softness", Range(0.001,0.3)) = 0.03
        _SpecColor2 ("Highlight Color", Color) = (1,1,1,1)
        _SpecSize ("Highlight Size", Range(0,1)) = 0.12
        _RimColor ("Rim Color", Color) = (0.8,0.9,1,1)
        _RimAmount ("Rim Amount", Range(0,1)) = 0.35
        _OutlineColor ("Outline Color", Color) = (0.05,0.04,0.08,1)
        _OutlineWidth ("Outline Width (m)", Range(0,0.05)) = 0.012
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor, _ShadeColor, _SpecColor2, _RimColor, _OutlineColor;
            half _Threshold, _Softness, _SpecSize, _RimAmount, _OutlineWidth;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ToonForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct A { float4 pos : POSITION; float3 nrm : NORMAL; float2 uv : TEXCOORD0; };
            struct V { float4 posCS : SV_POSITION; float2 uv : TEXCOORD0; float3 nrmWS : TEXCOORD1; float3 posWS : TEXCOORD2; };

            V vert(A i)
            {
                V o;
                o.posWS = TransformObjectToWorld(i.pos.xyz);
                o.posCS = TransformWorldToHClip(o.posWS);
                o.nrmWS = TransformObjectToWorldNormal(i.nrm);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                float3 n = normalize(i.nrmWS);
                float3 v = normalize(GetWorldSpaceViewDir(i.posWS));
                Light l = GetMainLight(TransformWorldToShadowCoord(i.posWS));

                half ndl = dot(n, l.direction);
                half lit = smoothstep(_Threshold, _Threshold + _Softness, ndl) * l.shadowAttenuation;
                half3 shade = lerp(_ShadeColor.rgb, 1.0, lit);
                half3 col = albedo * shade * l.color;

                float3 h = normalize(l.direction + v);
                half spec = smoothstep(1.0 - _SpecSize - 0.01, 1.0 - _SpecSize, pow(saturate(dot(n, h)), 2.0)) * lit;
                col += _SpecColor2.rgb * spec * 0.35;

                half rim = 1.0 - saturate(dot(n, v));
                rim = smoothstep(1.0 - _RimAmount, 1.0 - _RimAmount + 0.05, rim) * saturate(ndl + 0.5);
                col += _RimColor.rgb * rim * 0.4;

                col += albedo * SampleSH(n) * 0.25;
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct A { float4 pos : POSITION; float3 nrm : NORMAL; };
            struct V { float4 posCS : SV_POSITION; };
            V vert(A i)
            {
                V o;
                float3 w = TransformObjectToWorld(i.pos.xyz) + TransformObjectToWorldNormal(i.nrm) * _OutlineWidth;
                o.posCS = TransformWorldToHClip(w);
                return o;
            }
            half4 frag(V i) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
