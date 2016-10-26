Shader "Hypercube/Unlit Cutout"
{
    Properties {
        _MainTex ("Base (RGB) Transparency (A)", 2D) = "" {}
		_Color("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
    }
    SubShader {

	Cull [_Cull]

        Pass 
		{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

		CGPROGRAM
			#pragma vertex vert vertex:vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};
			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _Cutoff;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.vertexColor = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color * i.vertexColor;
				clip(col.a - _Cutoff);
				return col ;
			}
		ENDCG
        }
    }

	Fallback "Standard"
}
