Shader "Unlit/Distortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			struct v2f {
				float4 position : SV_POSITION;
				float4 screenPosition : TEXCOORD0;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				//convert the vertex positions from object space to clip space so they can be rendered
				o.position = UnityObjectToClipPos(v.vertex);
				o.screenPosition = ComputeScreenPos(o.position);
				return o;
			}

			//the fragment shader
			fixed4 frag(v2f i) : SV_TARGET{
				/*float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
				float aspect = _ScreenParams.x / _ScreenParams.y;
				textureCoordinate.x = textureCoordinate.x * aspect;
				textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);*/

				float x = i.position.x / _ScreenParams.x;
				float y = i.position.y / _ScreenParams.x;

				fixed4 col = tex2D(_MainTex, i.screenPosition);
				//col *= _Color;



				//col = i.position.x/ _ScreenParams.x;


				return col;
			}
            ENDCG
        }
    }
}
