Shader "Unlit/TestUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
		ColorMask 0        

        Pass
        {
		//ZWrite Off
		//ZTest Greater
		/*Stencil
		{	
		Ref 10
		Comp Always
		Pass Zero
		}*/

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;               
            };

            struct v2f
            {                
                float4 vertex : SV_POSITION;
            };


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {	
                return 0.3;
            }
            ENDCG
        }
    }
}
