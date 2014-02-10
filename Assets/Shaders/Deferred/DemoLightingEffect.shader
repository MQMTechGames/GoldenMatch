Shader "Custom/DemoLightingEffect" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_color ("Main Color", Color) = (1, 1, 1, 1)
		_fresnelColor ("Fresnel Color", Color) = (1, 1, 0, 1)
		_fresnelPow("Fresnel Power", Float) = 3.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass
		{
			Tags { "LightMode" = "Always" }
		
			Fog { Mode Off }
			ZTest LEqual
			Cull Back
			Lighting Off
		
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			sampler2D _MainTex;
			fixed4 _color;
			
			fixed4 _fresnelColor;
			float _fresnelPow;

			struct Tv2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float3 vPos : TEXCOORD1;
				float3 wPos : TEXCOORD2;
				float4 color : COLOR;
				
				float fresnelFactor : TEXCOORD3;
			};

			void vert( in appdata_full v
					 , out Tv2f o)
			{	
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy);

				o.vPos = v.vertex.xyz;
				o.wPos = mul(_Object2World, v.vertex).xyz;

				o.color = v.color;

				// Calculate fresnel
				float3 normal = normalize(v.normal);
				float3 dirToCamera = normalize(_WorldSpaceCameraPos.xyz - o.wPos.xyz);
				float3 wNormal = mul(_Object2World, float4(v.normal.xyz, 0.0f));
				
				o.fresnelFactor = pow(1 - dot(dirToCamera, wNormal), _fresnelPow);
			}

			fixed4 frag(in Tv2f i
					) : COLOR
			{
				half4 color = tex2D (_MainTex, i.uv);
				color = i.color;

				color += _fresnelColor * i.fresnelFactor;

				return color;
			}

		ENDCG
		}
	}
	
	FallBack "Diffuse"
}
