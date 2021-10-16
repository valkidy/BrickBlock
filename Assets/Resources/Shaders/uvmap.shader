Shader "Tool/UVMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent"  "RenderType"="Transparent" }
		
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
				float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 projectedPosition : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			int _FaceID;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);
				o.projectedPosition = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				half cutOff = step(abs(i.uv2.x - _FaceID), 1e-3);
				half dt = abs(sin(4.0 * _Time.y));
				half alpha = 0.3 + 0.7 * cutOff * dt;

				half4 color = tex2D(_MainTex, i.uv);
				// half4 color = (half4)1.0;
				color.a = alpha;
				return color;
            }
            ENDCG
        }
    }
}
