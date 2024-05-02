Shader "Custom/LayeredTextureShader"
{
    Properties
    {
        _MainTex1 ("Texture 1", 2D) = "white" {}
        _MainTex2 ("Texture 2", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex1;
            sampler2D _MainTex2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv1 = v.uv1;
                o.uv2 = v.uv2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color1 = tex2D(_MainTex1, i.uv1);
                fixed4 color2 = tex2D(_MainTex2, i.uv2);
                
                return color1 * color1.a + color2 * (1 - color1.a);
            }
            ENDCG
        }
    }
}