
Shader "1UP/Magic Outline/NenUnlit"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGBA)", 2D) = "white" { }
		_MulColor("Intensity", Range(0.0, 5)) = 1
		//_LightColor("Ramp Color",Color) = (1,1,1,1)
		//_LightDir("LightDir",Vector) = (1,1,1,1)
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" {}

		[Header(Outline Properties)]
		[Space(10)]
		_Color2("Outline Color", Color) = (1,0.56,0,1)
		_ColorR("Extra Outline Color", Color) = (0.95,1,0,1)
		_Brightness("Extra Outline Brightness", Range(0.5, 3)) = 2
		_Edge("Extra Outline Edge", Range(0.0, 1)) = 0.1
		_RimPower("Extra Outline Power", Range(0.01, 10.0)) = 3.0

		_Outline("Outline width", Range(0.002, 5)) = 0.128
		_OutlineZ("Outline Z", Range(-0.16, 1)) = 0.06

		[Header(Noise Properties)]
		[Space(10)]
		_Offset("Noise Opacity", Range(0.01, 10.0)) = 10
		_NoiseTex("Noise Texture", 2D) = "white" { }
		_Scale("Noise Scale", Range(0.0, 0.2)) = 0.003
		_SpeedX("Speed X", Range(-10, 10)) = 10.0
		_SpeedY("Speed Y", Range(-10, 10)) = 10.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Opaque" }

		Blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			ZWrite On
			Ztest LEqual
			Cull Off 

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			struct appdata 
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f 
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 normal : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _Ramp;
			float3 _LightDir;
			float _MulColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;

				float3 lightDir = normalize(_LightDir);

				half d = dot (i.normal, lightDir.xyz)*0.5 + 0.5;

				half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;

				half4 c;
				c.rgb = col.rgb * ramp * _MulColor * 2;
				c.a = saturate(col.a);
				return c;
			}
			ENDCG
		}

		
		Pass
		{
			Name "OUTLINE"
			Tags{ "LightMode" = "Always" }
			Cull Back
			ZWrite Off
			//ColorMask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
		
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
				float3 viewDir : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
			};

			uniform float _Outline;
			uniform float _OutlineZ;
			uniform float _RimPower;

			sampler2D _NoiseTex;
			float _Scale, _Offset, _Edge;
			uniform float4 _Color2, _ColorR;
			float _Brightness, _SpeedX, _SpeedY;

			v2f vert(appdata v) 
			{
				v2f o;

				/*
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal ));
				float2 offset = TransformViewToProjection(norm.xy);
				o.pos.xy += offset * _Outline * o.pos.z;
				o.pos.z += _OutlineZ;
				o.normalDir = normalize(mul(float4(v.normal, 0), unity_WorldToObject).xyz);
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				UNITY_TRANSFER_FOG(o, o.pos);
				*/

				float3 norm = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				float2 offset = TransformViewToProjection(norm.xy);

				float3 vertexOutline = v.vertex.xyz + v.normal * _Outline;;				

				o.pos = UnityObjectToClipPos(float4(vertexOutline, 1));

				//o.pos.xy += offset * _Outline * o.pos.z;
				o.pos.xy += offset * _Outline;  
				o.pos.z += _OutlineZ;

				o.normalDir = normalize(mul(float4(v.normal, 0), unity_WorldToObject).xyz);

				UNITY_TRANSFER_FOG(o, o.pos);

				return o;
			} 

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = float2(i.pos.x* _Scale - (_Time.x * _SpeedX), i.pos.y * _Scale - (_Time.x * _SpeedY)); 
				float4 text = tex2D(_NoiseTex, uv); 
				float4 rim = pow(saturate(dot(i.viewDir, i.normalDir)), _RimPower ) ;
				rim -= text; 
				float4 texturedRim = saturate(rim.a * _Offset); 
				float4 extraRim = (saturate((_Edge + rim.a) * _Offset) - texturedRim) * _Brightness ;
				float4 result = (_Color2 * texturedRim) + (_ColorR * extraRim);
				UNITY_APPLY_FOG(i.fogCoord, result);
				return result;
			}
			ENDCG
		}
		
	}
}