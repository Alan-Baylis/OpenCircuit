
Shader "Voxel/Normal" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Sigma ("Blend Tightness", Range(0, 0.57)) = 0.5
		_Blend ("Blend Sharpness", Float) = 4
		_TexWallDif ("Diffuse Wall Texture", 2D) = "surface" {}
		_TexFlrDif ("Diffuse Floor Texture", 2D) = "surface" {}
		_TexCeilDif ("Diffuse Ceiling Texture", 2D) = "surface" {}
		_TexWallNorm ("Normal Wall Texture", 2D) = "surface" {}
		_TexFlrNorm ("Normal Floor Texture", 2D) = "surface" {}
		_TexCeilNorm ("Normal Ceiling Texture", 2D) = "surface" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surfaceFunc Standard fullforwardshadows vertex:vertexFunc

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _TexWallDif;
		sampler2D _TexFlrDif;
		sampler2D _TexCeilDif;
		sampler2D _TexWallNorm;
		sampler2D _TexFlrNorm;
		sampler2D _TexCeilNorm;
		float4 _TexWallDif_ST;
		float4 _TexFlrDif_ST;
		float4 _TexCeilDif_ST;
		float4 _TexWallNorm_ST;
		float4 _TexFlrNorm_ST;
		float4 _TexCeilNorm_ST;
		float _Sigma;
		float _Blend;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		
		struct Input {
			float4 pos;
			float3 b;
		};
		
		float4 computeDiffuse(Input i, float blendMag) {
			return (
				i.b.x *tex2D(_TexWallDif, i.pos.zy *_TexWallDif_ST.xy + _TexWallDif_ST.zw) +
				i.b.z *tex2D(_TexWallDif, i.pos.xy *_TexWallDif_ST.xy + _TexWallDif_ST.zw) +
				((i.b.y > 0)?i.b.y *tex2D(_TexFlrDif, i.pos.xz *_TexFlrDif_ST.xy + _TexFlrDif_ST.zw):
				-i.b.y *tex2D(_TexCeilDif, i.pos.zx *_TexCeilDif_ST.xy + _TexCeilDif_ST.zw))
				) /blendMag;
		}
		
		float4 computeHeight(Input i, float blendMag) {
			return (
				i.b.x *tex2D(_TexWallNorm, i.pos.zy *_TexWallNorm_ST.xy + _TexWallDif_ST.zw) +
				i.b.z *tex2D(_TexWallNorm, i.pos.xy *_TexWallNorm_ST.xy + _TexWallDif_ST.zw) +
				((i.b.y > 0)?i.b.y *tex2D(_TexFlrNorm, i.pos.xz *_TexFlrNorm_ST.xy + _TexFlrNorm_ST.zw):
				-i.b.y *tex2D(_TexCeilNorm, i.pos.zx *_TexCeilNorm_ST.xy + _TexCeilNorm_ST.zw))
				) /blendMag;
		}
		
		void vertexFunc(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
 			o.pos = v.vertex;
            
			o.b.x = pow(max(0, abs(v.normal.x) -_Sigma), _Blend);
			o.b.z = pow(max(0, abs(v.normal.z) -_Sigma), _Blend);
			o.b.y = pow(max(0, abs(v.normal.y) -_Sigma), _Blend);
			
			if (v.normal.y < 0) o.b.y = -o.b.y;
        }

		void surfaceFunc(Input IN, inout SurfaceOutputStandard o) {
			float bmag = IN.b.x +IN.b.z +abs(IN.b.y);
			float4 dif = computeDiffuse(IN, bmag) *_Color;
			float4 height = computeHeight(IN, bmag);
			
			o.Albedo = dif.rgb;
			o.Normal = UnpackNormal(height);
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
