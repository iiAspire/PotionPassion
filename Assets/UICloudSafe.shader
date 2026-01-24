Shader "UI/CloudSafe"
{
    Properties
    {
        _MainTex ("Noise", 2D) = "white" {}
        _Speed ("Speed", Float) = 0.05
        _Strength ("Strength", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Speed;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv + float2(_Time.y * _Speed, 0);
                float n = tex2D(_MainTex, uv).r;
                return fixed4(1,1,1, n * _Strength * 0.6);
            }
            ENDCG
        }
    }
}