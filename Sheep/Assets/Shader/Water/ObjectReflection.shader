// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ObjectReflection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TintColor("_TintColor",color) = (0,0,0,0)
		_BlendMinValue("BlendMinValue", Float ) = 0
		_BlendMaxValue("BlendMaxValue", Float) = 0
		_WaterLevel("WaterLevel", Float) = 0
		_WaterDeepClippingPlane("WaterDeepClippingPlane", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

		Cull Front
		Blend SrcAlpha OneMinusSrcAlpha
		//ZWrite On
		//ZTest Greater
		//ZTest Less
		/*Stencil{
			Ref 10
			Comp Equal
			Pass replace
			ZFail replace
		}*/

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;	
                float4 uv : TEXCOORD0;
				float4 worldvertex : TEXCOORD1;
            };

            sampler2D _MainTex;
			float4 _TintColor;
			float _BlendMinValue;
			float _BlendMaxValue;
			float _WaterLevel;
			float _WaterDeepClippingPlane;

			float InverseLerp(float min, float max, float value)
			{
				//if (abs(max - min) < 0.0001) return min; 
				float f = (value - min) / (max - min);
				return saturate(f);
			}

			float3 lerp(float3 a, float3 b, float w)
			{
				float f = a + w * (b - a);
				return saturate(f);
			}

            v2f vert (appdata v)
            {
                v2f o;
				//v.vertex.y += _WaterLevel; 
				//v.vertex.y *= -1;
				//v.vertex.y += _WaterLevel;
				//float 4 worldvertex =
				o.worldvertex = mul(unity_ObjectToWorld, v.vertex);

				float d = o.worldvertex.y - v.vertex.y - _WaterLevel;				
				v.vertex.y*=-1;
				v.vertex.y -= (2*d);
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv;
				o.uv.z = o.worldvertex.y - _WaterLevel > -0.1 ? 1 : 0;
                return o; 
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture				 

				float NormalizedH = InverseLerp(_WaterLevel, _WaterDeepClippingPlane, i.worldvertex.y);

				float _BlendeValue = lerp(_BlendMinValue, _BlendMaxValue, NormalizedH);


                fixed4 col = tex2D(_MainTex, i.uv);
				col = col * (1 - _BlendMinValue) + _BlendMinValue * _TintColor;


				//col = fixed4(_BlendeValue,0,0,1);

                //return col;
				return float4(col.rgb, i.uv.z);
            }				

            ENDCG
        }
    }
}
