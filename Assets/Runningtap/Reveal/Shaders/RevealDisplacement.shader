// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Runningtap/Reveal/Displacement" {
	Properties
	{
		_Tess ("Tessellation", Range(1,32)) = 4
		_Displacement ("Displacement", Range(-1.0, 1.0)) = 0.3
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (1,1,1,1)
		_Splat ("SplatMap", 2D) = "black" {}
		_SplatRemap ("SplatOffset", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 300

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessDistance 
		#pragma target 4.6
		#include "Tessellation.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2; 
        };

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _Splat)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color2)
			UNITY_DEFINE_INSTANCED_PROP(float4, _SplatRemap)
			UNITY_DEFINE_INSTANCED_PROP(half, _Tess)
			UNITY_DEFINE_INSTANCED_PROP(half, _Displacement )
		UNITY_INSTANCING_BUFFER_END(Props) 

		void disp (inout appdata v) 
        {
			float2 WUV = (mul (unity_ObjectToWorld, v.vertex).xz - _SplatRemap.xy)/ _SplatRemap.zw;

            float d = tex2Dlod(_Splat, float4(WUV, 0, 0)).r * _Displacement;
            v.vertex.xyz += v.normal * d;
        }

		float4 tessDistance (appdata v0, appdata v1, appdata v2)
		{
            float minDist = 10.0;
            float maxDist = 50.0;
            return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float2 WUV = (IN.worldPos.xz - _SplatRemap.xy)/ _SplatRemap.zw;
			 
			half mask = tex2Dlod(_Splat, float4(WUV, 0, 0)).r;
			fixed4 c =   tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c2 =  tex2D(_MainTex, IN.uv_MainTex) * _Color2;
			
			float4 mix = lerp(c, c2, mask);
			//o.Alpha = saturate(mix);
			o.Albedo = mix.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}