// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Outline" {
	Properties{
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineWidth("Outline Width", Range(0, 0.05)) = 0.005
		_FillColor("Fill Color", Color) = (1, 1, 1, 0.1)
	}

	CGINCLUDE
	ENDCG

	SubShader{
		Tags{ "Queue" = "Transparent" }

		//Pass {
		//	Name "BASE"
		//	Cull Back
		//	Blend Zero One

		//	// uncomment this to hide inner details:
		//	//Offset -8, -8

		//	SetTexture[_OutlineColor] {
		//		ConstantColor(0,0,0,0)
		//		Combine constant
		//	}
		//}

		Pass {
			Name "Fill"
			Tags{ "LightMode" = "Always" }
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata {
				float4 vertex : POSITION;
			};

			struct VertData {
				float4 pos : POSITION;
			};

			uniform fixed4 _FillColor;
			
			VertData vert(appdata v) {
				VertData o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(VertData i) : COLOR {
				return _FillColor;
			}
			ENDCG
		}

		Pass {
			Name "Outline"
			Tags{ "LightMode" = "Always" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct VertData {
				float4 pos : POSITION;
			};
			
			uniform float _OutlineWidth;
			uniform fixed4 _OutlineColor;
			
			VertData vert(appdata v) {
				// expand mesh
				VertData o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(normal.xy);

				o.pos.xy += offset * pow(o.pos.z, 0.8) * _OutlineWidth;
				return o;
			}

			fixed4 frag(VertData i) : COLOR {
				return _OutlineColor;
			}
			ENDCG
		}
	}

	Fallback "None"
}