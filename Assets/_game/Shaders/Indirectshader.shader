Shader "Unlit/Indirect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct StaticInstanceData{
                float3 AABBCenter;
                float3 AABBExtents;
                float4x4 trs;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;

            StructuredBuffer<StaticInstanceData> instanceBuffer;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(instanceBuffer[v.instanceID].trs, v.vertex);
                o.vertex = UnityObjectToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= _Color;
                return col;
            }
            ENDCG
        }
    }
}
