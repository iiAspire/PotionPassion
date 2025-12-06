Shader "UI/Horizontal4ColorGradient"
{
    Properties
    {
        _Color1 ("Color 1 (Night)", Color) = (0,0,0.1,1)
        _Color2 ("Color 2 (Dawn)",  Color) = (0.3,0.4,0.6,1)
        _Color3 ("Color 3 (Day)",   Color) = (0.4,0.7,1,1)
        _Color4 ("Color 4 (Dusk)",  Color) = (0.2,0.3,0.5,1)

        _Stop1 ("Night→Dawn %", Range(0,1)) = 0.10
        _Stop2 ("Dawn→Day %",  Range(0,1)) = 0.20
        _Stop3 ("Day→Dusk %",  Range(0,1)) = 0.80
        _Stop4 ("Dusk→Night %", Range(0,1)) = 0.90
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Lighting Off Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _Color4;

            float _Stop1;
            float _Stop2;
            float _Stop3;
            float _Stop4;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x;

                float p1 = _Stop1;
                float p2 = _Stop2;
                float p3 = _Stop3;
                float p4 = _Stop4;

                if (x < p1)
                {
                    float t = x / p1;
                    return lerp(_Color1, _Color2, t);
                }
                else if (x < p2)
                {
                    float t = (x - p1) / (p2 - p1);
                    return lerp(_Color2, _Color3, t);
                }
                else if (x < p3)
                {
                    return _Color3;
                }
                else if (x < p4)
                {
                    float t = (x - p3) / (p4 - p3);
                    return lerp(_Color3, _Color4, t);
                }
                else
                {
                    float t = (x - p4) / (1.0 - p4);
                    return lerp(_Color4, _Color1, t);
                }
            }
            ENDCG
        }
    }
}