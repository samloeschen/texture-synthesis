Shader "Unlit/Display"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 palette( float t, float3 a, float3 b, float3 c, float3 d )
            {
                return a + b*cos( 6.28318*(c*t+d) );
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // return tex2D(_MainTex, i.uv).r;
                float2 s = tex2D(_MainTex, i.uv).rg;
                float3 color = lerp(float3(1.0, 1.0, 1.0), float3(1.0, 0.0, 0.0), s.y); 
                return float4(color * s.x, s.x);

            }
            ENDCG
        }
    }
}
