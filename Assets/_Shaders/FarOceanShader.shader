Shader "Custom/FarOceanShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		//Tags { "RenderType"="Opaque" }

		//Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		Tags{ "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }

		//ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		//Blend OneMinusDstColor One
		//Blend DstColor Zero
		//Blend One OneMinusSrcAlpha
		//Blend One One

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			//fixed4 c = fixed4(IN.uv_MainTex.x, IN.uv_MainTex.y, 1.0f, 1.0f);

			o.Albedo = c.rgb;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			float2 uvOffset = fixed2(0.5f, 0.5f) - IN.uv_MainTex;
			float dist = sqrt(uvOffset.x * uvOffset.x + uvOffset.y * uvOffset.y);

			if (dist <= 0.065f)
			{
				o.Albedo = fixed3(1.0f, 0.0f, 0.0f);
				o.Alpha = 0.0f;
			}

			clip(o.Alpha - 0.1f);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
