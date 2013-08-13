// Lucifuges‚³‚ñ‚Ì’ñ‹Ÿ
Shader "MMD/HalfLambertOutline"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_Shininess("Specularity", Float) = 6
		_SpecularColor("Specular Color", Color) = (0,0,0,1)
		_AmbColor("Ambient Color", Color) = (0,0,0,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Range(0, 1)) = 0.2
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
			"RenderType"="Opaque"
		}
		
		Cull Off
		LOD 200
		
		CGPROGRAM
		#pragma surface surf HalfLambert addshadow dualforward
		
		half4 _Color;
		half _Shininess;
		half4 _SpecularColor;
		half4 _AmbColor;
		sampler2D _MainTex;
		
		struct Input
		{
			float2 uv_MainTex;
			half4 color : COLOR;
			half3 viewDir;
			half3 worldNormal;
		};
		
		half4 LightingHalfLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			half NdotL = dot(s.Normal, lightDir);
			half wrap = NdotL * 0.5 + 0.5;
			half3 lit = _LightColor0.rgb * (wrap * atten);
			return half4(s.Albedo * lit + s.Emission, s.Alpha);
		}
		
		void surf(Input IN, inout SurfaceOutput o)
		{
			
			half4 tex0 = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = _AmbColor.rgb + tex0.rgb * IN.color.rgb * _Color.rgb;
			o.Alpha = tex0.a * IN.color.a;
			half rim = pow(1 - dot(IN.worldNormal, normalize(IN.viewDir)), _Shininess);
			o.Emission = _SpecularColor.rgb * _SpecularColor.a * rim;
		}
		
		ENDCG
		
		// Outline Pass
		Pass
		{
			Cull Front
			Lighting Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			float4 _OutlineColor;
			float _OutlineWidth;
			
			struct v2f
			{
				half4 pos : SV_POSITION;
			};
			
			v2f vert(appdata_base v)
			{
				half4 pos = mul(UNITY_MATRIX_MVP, v.vertex);
				half width = 0.01 * _OutlineWidth;
				half4 edge_pos = v.vertex + pos.w * width * half4(v.normal, 0.0);
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, edge_pos);
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				return half4(_OutlineColor);
			}
			ENDCG
		}
		
	}
	Fallback "Specular"
}