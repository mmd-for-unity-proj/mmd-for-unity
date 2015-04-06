Shader "Custom/NewShader" {
	Properties {
		_Ambient ("Ambient", Color) = (1,1,1,1)
		_Diffuse ("Diffuse", Color) = (1,1,1,1)
		_Specular ("Specular", Color) = (1,1,1,1)
		_Specularity ("Specularity", Float) = 1.0
		_Transparency ("Transparency", Range(0.0, 1.0)) = 1.0
		_Texture ("Main Texture", 2D) = "white" {}
		_ToonTexture ("Toon Texture", 2D) = "white" {}
		_SphereMap ("Sphere Map", 2D) = "black" {}
		_SelfShadow ("Self Shadow", Int) = 1
		_SphereAdd ("Sphere Add", Int) = 1
		_EdgeFlag ("Edge Flag", Int) = 1
		_BackCull ("BackFace Culling", Int) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _Texture;
		sampler2D _ToonTexture;
		sampler2D _SphereMap;
		float4 _Ambient;
		float4 _Diffuse;
		float4 _Specular;
		float _Specularity;
		float _Transparency;
		int _SelfShadow;
		int _SphereAdd;
		int _EdgeFlag;
		int _BackCull;

		struct Input {
			float2 uv_Texture;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {

			fixed4 c = tex2D (_Texture, IN.uv_Texture) * _Ambient;
			o.Albedo = c.rgb;
			o.Alpha = c.a;

		}
		ENDCG
	} 
	FallBack "Diffuse"
}
