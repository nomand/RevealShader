Shader "Runningtap/Reveal/A2RGB" {
	Properties
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Splat ("SplatMap", 2D) = "black" {}
		_SplatRemap ("SplatOffset", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True"  "RenderType"="Transparent" }
		LOD 200
		
		Zwrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows alpha:fade
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MainTex)
			UNITY_DEFINE_INSTANCED_PROP(sampler2D, _Splat)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(float4, _SplatRemap)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o)
		{ 
			float2 WUV = (IN.worldPos.xz - _SplatRemap.xy)/ _SplatRemap.zw;
			 
			half mask = tex2Dlod(_Splat, float4(WUV, 0, 0)).r;
			fixed4 c =  tex2D(_MainTex, IN.uv_MainTex) * _Color;
			half mix = lerp(0, 1, mask);
			
			o.Albedo = c;
			o.Alpha = saturate(mix);
		}
		ENDCG
	}
	FallBack "Diffuse"
}