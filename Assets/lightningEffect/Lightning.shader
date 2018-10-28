/*
 * 距離に応じて_NoiseAmplitudeと楕円半径を変えてやり，
 * 距離が遠ければノイズが大きく楕円が大きく，
 * 距離が短ければノイズが小さく楕円が小さいというように
 *
 */
Shader "Unlit/Lightning"
{
	Properties
	{
        [HDR] _Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

        //Pass {
			CGPROGRAM

            //#pragma surface surf Standard nolightmap
			//#pragma vertex vert
            #pragma surface surf Standard vertex:vert nolightmap

			//#pragma fragment frag

			// make fog work
			//#pragma multi_compile_fog
            #pragma target 3.0
			
			#include "UnityCG.cginc"
            #include "SimplexNoise2D.cginc"
            //#include "ClassicNoise3D.cginc"

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR; 
            };
            struct Input { fixed4 color : COLOR; }; //追加

            float _Throttle;
            float _Seed;
            float2 _Interval;
            float2 _Length;
            float3 _Point0;
            float3 _Point1;
            float2 _NoiseFrequency;
            float2 _NoiseMotion;
            float2 _NoiseAmplitude;
            float _Distance;
            float3 _Asis0;
            float3 _Asis1;
            float3 _Asis2;
			float3 _FingerNormal;
			float _EllipseBend;

            float4 _Color;
            int _VertexNum;

			/*
			 * オイラー角（ラジアン）を回転行列に変換
			 */
			float4x4 eulerAnglesToRotationMatrix(float3 angles)
			//float3x3 eulerAnglesToRotationMatrix(float3 angles)
			{
				float ch = cos(angles.y); float sh = sin(angles.y); // heading
				float ca = cos(angles.z); float sa = sin(angles.z); // attitude
				float cb = cos(angles.x); float sb = sin(angles.x); // bank

				// Ry-Rx-Rz (Yaw Pitch Roll)
				return float4x4(
					ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
					cb * sa, cb * ca, -sb, 0,
					-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
					0, 0, 0, 1
				);
				/*
				// Ry-Rx-Rz (Yaw Pitch Roll)
				return float3x3(
					ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb,
					cb * sa, cb * ca, -sb,
					-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb
				);
				*/
			}


            // pseudo random number generator
            float nrand01(float seed, float salt)
            {
                float2 uv = float2(seed, salt);
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // vertex intensity function
            float intensity(float seed)
            {
                return (nrand01(seed, 4) < _Throttle) * nrand01(seed, 5) - 0.01;
            }

            // displacement function (今はclassic noise)
            float displace(float p, float t, float offs) {
                float2 np1 = float2(_NoiseFrequency.x * p + offs, t * _NoiseMotion.x);
                float2 np2 = float2(_NoiseFrequency.y * p + offs, t * _NoiseMotion.y);
                return snoise(np1) * _NoiseAmplitude.x + snoise(np2) * _NoiseAmplitude.y;
            }

            float middle_point(float3 p0, float3 p1) {
                return p0 + (p1 - p0) / 2;
            }

            float ellipse_y(float x) {
                // 楕円形にする
                float3 p0 = _Point0;
                float3 p1 = _Point1;
                float half_dis = distance(p0, p1) / 2.0;
                float a = half_dis;
                float b = half_dis * _EllipseBend; //円の大きさを決める
                float y = b * sqrt(half_dis*half_dis - x * x / a * a);
                return y;
            }

			//v2f vert (uint id : SV_VertexID)
            void vert(inout appdata_full v)
			{
                //float pp01 = (float)id / _VertexNum; // position on the line segment [0-1]
                float pp01 = v.vertex.x; 
                float seed = (v.vertex.y + _Seed) * 131.1; // random seed
				_NoiseAmplitude = _NoiseAmplitude * _Distance * 0.12; //距離に応じてNoiseを大きくする

                //
                // ピカピカさせる（電気の法則っぽい）
                //
                // interval (length of cycle)
                float interval = lerp(_Interval.x, _Interval.y, nrand01(seed, 0));
                //_Time.x:t/20[秒] _Time.y:t[秒] _Time.z:2×t[秒] _Time.w:3×t[秒] 
                float t = _Time.y;          // absolute time
                float tpi = t / interval;
                float tp01 = frac(tpi);     // time parameter [0-1]
                float cycle = floor(tpi);   // cycle count
                // modify the random seed with the cycle count
                seed += fmod(cycle, 9973) * 3.174;

                //
                // 光を飛ばす感じ
                //
                // modify pp01 with the bolt length parameter
                float bolt_len = lerp(_Length.x, _Length.y, nrand01(seed, 1));
                pp01 = lerp(tp01, pp01, bolt_len);

                float d0 = displace(pp01 * _Distance, t, seed *  13.45);
                float d1 = displace(pp01 * _Distance, t, seed * -21.73);
                //float3 pos = lerp(_Point0, _Point1, pp01) + d0 * _Asis1 + d1 * _Asis2;
				float decrease = float3(0.2, 0.2, 0.2);
                float3 pos = lerp(_Point0, _Point1, pp01);
                pos += decrease * d0 + decrease * d1;

				float power_y = ellipse_y(pos.x);
				pos += power_y * _FingerNormal;

                /*
                float timex = _Time.y * _Speed * 0.1365143f;
                float timey = _Time.y * _Speed * 1.21688f;
                float timez = _Time.y * _Speed * 2.5564f;
                float x = cnoise(float3(timex + pos.x, timex + pos.y, timex + pos.z));
                float y = cnoise(float3(timey + pos.x, timey + pos.y, timey + pos.z));
                float z = cnoise(float3(timez + pos.x, timez + pos.y, timez + pos.z));
                float3 offset = float3(x, y, z);
                pos+=offset;
                */


                // vertex position
                //v.vertex.xyz = lerp(p0, p1, pp01);

                // vertex color (indensity)
                //v.color = _Color * intensity(seed);

                v.vertex.xyz = pos;
                //v.color = _Color;
                v.color = _Color * intensity(seed);
                //return o;
			}

            void surf(Input IN, inout SurfaceOutputStandard o) {
                clip(IN.color.r);
                o.Emission = IN.color.rgb;
            }
			ENDCG
		//}
	}
}
