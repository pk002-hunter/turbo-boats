Shader "TurboBoats/StylizedCel"
{
    Properties
    {
        _Color ("Paint Tint", Color) = (1,1,1,1)
        _MainTex ("Painted Albedo", 2D) = "white" {}
        _BumpMap ("Surface Brush Detail", 2D) = "bump" {}
        _BumpScale ("Bump Strength", Range(0, 1)) = 0.2
        _MetallicGlossMap ("Surface Finish", 2D) = "white" {}
        _ShadowBand ("Shadow Band", Range(0, 1)) = 0.30
        _MidBand ("Midtone Band", Range(0, 1)) = 0.68
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            half _BumpScale;
            sampler2D _MetallicGlossMap;
            half _ShadowBand;
            half _MidBand;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
                UNITY_FOG_COORDS(5)
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.positionWS = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.normalWS = UnityObjectToWorldNormal(input.normal);
                output.tangentWS = UnityObjectToWorldDir(input.tangent.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangent.w;
                output.uv = input.uv;
                UNITY_TRANSFER_FOG(output, output.position);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                if (dot(input.tangentWS, input.tangentWS) > 0.01)
                {
                    float3 tangentNormal = UnpackNormal(tex2D(_BumpMap, TRANSFORM_TEX(input.uv, _BumpMap)));
                    tangentNormal.xy *= _BumpScale;
                    float3x3 tangentToWorld = float3x3(
                        normalize(input.tangentWS),
                        normalize(input.bitangentWS),
                        normalWS);
                    normalWS = normalize(mul(tangentNormal, tangentToWorld));
                }

                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz - input.positionWS * _WorldSpaceLightPos0.w);
                half lambert = saturate(dot(normalWS, lightDirection));
                half band = lambert > _MidBand ? 1.0h : (lambert > _ShadowBand ? 0.78h : 0.56h);
                half3 ambient = max(ShadeSH9(half4(normalWS, 1.0h)), 0.12h);
                half3 lighting = ambient * 0.52h + _LightColor0.rgb * (band * 0.68h);

                fixed4 paint = tex2D(_MainTex, TRANSFORM_TEX(input.uv, _MainTex)) * _Color;
                half finish = tex2D(_MetallicGlossMap, input.uv).a;
                half3 viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
                half highlight = pow(saturate(dot(reflect(-lightDirection, normalWS), viewDirection)), 28.0h);
                paint.rgb = paint.rgb * lighting + highlight * finish * _LightColor0.rgb * 0.18h;
                UNITY_APPLY_FOG(input.fogCoord, paint);
                return paint;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
