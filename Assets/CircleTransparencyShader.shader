Shader "Custom/CircleTransparencyShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TransparencyFactor ("Transparency Factor", Float) = 0.9
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _TransparencyFactor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture color
                fixed4 col = tex2D(_MainTex, i.uv);

                // Calculate the distance from the center
                float dist = distance(i.uv, float2(0.5, 0.5));

                // Adjust alpha based on the transparency factor
                if(dist < _TransparencyFactor || dist > 0.48)
                {
                    col.a = 0;
                }

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
