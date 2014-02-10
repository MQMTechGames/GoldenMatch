Shader "Custom/DemoShaderEffect" 
{
	Properties 
	{
		_time ("_time", Float) = 0.5
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
			CGPROGRAM
			#pragma vertex vert_img_n
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float _time;

			void vert_img_n ( in appdata_img v
							, out half2 uv : TEXCOORD0
							, out float4 pos : POSITION )
			{
				pos = mul (UNITY_MATRIX_MVP, v.vertex);
				uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord.xy);
			}

			fixed4 frag (				 
					 	  in half2 uv : TEXCOORD0
						, in float4 pos : POSITION
			) : COLOR
			{	
				fixed4 original = tex2D(_MainTex, uv);

				fixed4 color = fixed4(
					  sin(_time)
					, cos(_time)
					, 1
					, 1
				);

				return original * color;
			}

			ENDCG
		}
	}

	Fallback off
}
