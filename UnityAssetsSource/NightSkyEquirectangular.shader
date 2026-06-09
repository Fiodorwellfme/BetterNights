Shader "Fiodor/Night Sky Equirectangular"
{
    Properties
    {
        _MainTex ("Night Sky Texture", 2D) = "black" {}
        _BackgroundTex ("Background Star Texture", 2D) = "black" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Float) = 1
        _Saturation ("Saturation", Float) = 1
        _BackgroundBrightness ("Background Brightness", Float) = 0
        _TodVisibility ("TOD Visibility", Float) = 1
        _HorizonFadeStartDegrees ("Horizon Fade Start Degrees", Float) = 0
        _HorizonFadeEndDegrees ("Horizon Fade End Degrees", Float) = 25
        _HorizonBrightnessMultiplier ("Horizon Brightness Multiplier", Range(0, 1)) = 0.25
        _HorizonFadeDebug ("Horizon Fade Debug", Float) = 0
        _HorizontalScale ("Horizontal Scale", Float) = 1
        _VerticalScale ("Vertical Scale", Float) = 1
        _BackgroundHorizontalScale ("Background Horizontal Scale", Float) = 1
        _BackgroundVerticalScale ("Background Vertical Scale", Float) = 1
        _HorizontalOffsetDegrees ("Horizontal Offset Degrees", Float) = 0
        _VerticalOffsetDegrees ("Vertical Offset Degrees", Float) = 0
        _BackgroundHorizontalOffsetDegrees ("Background Horizontal Offset Degrees", Float) = 0
        _BackgroundVerticalOffsetDegrees ("Background Vertical Offset Degrees", Float) = 0
        _MainBandEnabled ("Main Band Enabled", Float) = 0
        _MainBandCenterU ("Main Band Center U", Range(0, 1)) = 0.5
        _MainBandCenterV ("Main Band Center V", Range(0, 1)) = 0.5
        _MainBandWidth ("Main Band Width", Range(0.001, 1)) = 1
        _MainBandHeight ("Main Band Height", Range(0.001, 1)) = 0.5
        _MainClampToTransparent ("Main Clamp To Transparent", Float) = 1
        _MainHorizontalFade ("Main Horizontal Fade", Range(0, 0.5)) = 0.02
        _MainVerticalFade ("Main Vertical Fade", Range(0, 0.5)) = 0.02
        _YawDegrees ("Yaw Degrees", Float) = 0
        _PitchDegrees ("Pitch Degrees", Float) = 0
        _RollDegrees ("Roll Degrees", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background+10"
            "RenderType" = "Background"
            "IgnoreProjector" = "True"
        }

        Cull Front
        ZWrite Off
        ZTest LEqual
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _BackgroundTex;
            float4 _Tint;
            float _Brightness;
            float _Saturation;
            float _BackgroundBrightness;
            float _TodVisibility;
            float _HorizonFadeStartDegrees;
            float _HorizonFadeEndDegrees;
            float _HorizonBrightnessMultiplier;
            float _HorizonFadeDebug;
            float _HorizontalScale;
            float _VerticalScale;
            float _BackgroundHorizontalScale;
            float _BackgroundVerticalScale;
            float _HorizontalOffsetDegrees;
            float _VerticalOffsetDegrees;
            float _BackgroundHorizontalOffsetDegrees;
            float _BackgroundVerticalOffsetDegrees;
            float _MainBandEnabled;
            float _MainBandCenterU;
            float _MainBandCenterV;
            float _MainBandWidth;
            float _MainBandHeight;
            float _MainClampToTransparent;
            float _MainHorizontalFade;
            float _MainVerticalFade;
            float _YawDegrees;
            float _PitchDegrees;
            float _RollDegrees;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
                float3 worldDirection : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.direction = normalize(v.vertex.xyz);

                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldCenter = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                o.worldDirection = normalize(worldPosition - worldCenter);
                return o;
            }

            float3 RotateY(float3 direction, float degrees)
            {
                float angle = degrees * 0.01745329252;
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    direction.x * c - direction.z * s,
                    direction.y,
                    direction.x * s + direction.z * c);
            }

            float3 RotateX(float3 direction, float degrees)
            {
                float angle = degrees * 0.01745329252;
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    direction.x,
                    direction.y * c - direction.z * s,
                    direction.y * s + direction.z * c);
            }

            float3 RotateZ(float3 direction, float degrees)
            {
                float angle = degrees * 0.01745329252;
                float s = sin(angle);
                float c = cos(angle);

                return float3(
                    direction.x * c - direction.y * s,
                    direction.x * s + direction.y * c,
                    direction.z);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldDirection = normalize(i.worldDirection);
                float3 direction = worldDirection;
                direction = RotateY(direction, _YawDegrees);
                direction = RotateX(direction, _PitchDegrees);
                direction = RotateZ(direction, _RollDegrees);

                const float inverseTwoPi = 0.15915494309;
                const float inversePi = 0.31830988618;

                float u = atan2(direction.x, direction.z) * inverseTwoPi + 0.5;
                float v = asin(clamp(direction.y, -1.0, 1.0)) * inversePi + 0.5;

                float mainU = (u - 0.5) * max(_HorizontalScale, 0.0001) + 0.5;
                float mainV = (v - 0.5) * max(_VerticalScale, 0.0001) + 0.5;

                mainU += _HorizontalOffsetDegrees / 360.0;
                mainV += _VerticalOffsetDegrees / 180.0;

                float mainBandInside = 1.0;
                if (_MainBandEnabled > 0.5)
                {
                    float bandWidth = max(_MainBandWidth, 0.001);
                    float bandHeight = max(_MainBandHeight, 0.001);
                    float wrappedDeltaU = frac(u - _MainBandCenterU + 0.5) - 0.5;
                    float deltaV = v - _MainBandCenterV;
                    float halfBandWidth = bandWidth * 0.5;
                    float horizontalEdgeDistance = halfBandWidth - abs(wrappedDeltaU);
                    float horizontalFade = saturate(horizontalEdgeDistance / max(_MainHorizontalFade * bandWidth, 0.00001));
                    float horizontalInside = (bandWidth >= 0.999) ? 1.0 :
                        ((_MainHorizontalFade <= 0.00001) ? step(abs(wrappedDeltaU), halfBandWidth) : horizontalFade);

                    mainBandInside =
                        horizontalInside *
                        step(abs(deltaV), bandHeight * 0.5);

                    mainU = wrappedDeltaU / bandWidth + 0.5;
                    mainV = deltaV / bandHeight + 0.5;
                }

                float backgroundU = (u - 0.5) * max(_BackgroundHorizontalScale, 0.0001) + 0.5;
                float backgroundV = (v - 0.5) * max(_BackgroundVerticalScale, 0.0001) + 0.5;

                backgroundU += _BackgroundHorizontalOffsetDegrees / 360.0;
                backgroundV += _BackgroundVerticalOffsetDegrees / 180.0;

                // Do not emulate transparent padding with a huge mostly-empty texture:
                // compressed bundle size is not runtime cost, and transparent pixels still
                // consume VRAM/bandwidth once the texture is loaded. Instead, sample a
                // compact main texture safely and zero its alpha outside the intended band.
                float safeMainV = saturate(mainV);
                fixed4 mainColor = tex2D(_MainTex, float2(frac(mainU), safeMainV));
                fixed4 backgroundColor = tex2D(_BackgroundTex, float2(frac(backgroundU), frac(backgroundV)));

                float mainInsideV = 1.0;
                if (_MainClampToTransparent > 0.5)
                {
                    float fade = max(_MainVerticalFade, 0.00001);
                    float hardInside = step(0.0, mainV) * step(mainV, 1.0);
                    float bottomFade = smoothstep(0.0, fade, mainV);
                    float topFade = 1.0 - smoothstep(1.0 - fade, 1.0, mainV);
                    mainInsideV = (_MainVerticalFade <= 0.00001) ? hardInside : bottomFade * topFade;
                }

                mainColor.a *= mainInsideV * mainBandInside;

                float luminance = dot(mainColor.rgb, float3(0.2126, 0.7152, 0.0722));
                mainColor.rgb = lerp(luminance.xxx, mainColor.rgb, _Saturation);

                float altitudeDegrees = asin(clamp(dot(worldDirection, float3(0, 1, 0)), -1.0, 1.0)) * 57.2957795131;
                float horizonFade = smoothstep(
                    _HorizonFadeStartDegrees,
                    max(_HorizonFadeEndDegrees, _HorizonFadeStartDegrees + 0.001),
                    altitudeDegrees);
                float horizonVisibility = lerp(_HorizonBrightnessMultiplier, 1.0, horizonFade);

                fixed3 rgb = lerp(
                    backgroundColor.rgb * _BackgroundBrightness,
                    mainColor.rgb * _Brightness,
                    mainColor.a);

                rgb *= _Tint.rgb * _TodVisibility * horizonVisibility;

                if (_HorizonFadeDebug > 0.5)
                {
                    float dimRange = max(1.0 - _HorizonBrightnessMultiplier, 0.0001);
                    float debugMask = saturate((1.0 - horizonVisibility) / dimRange);
                    rgb = lerp(rgb, fixed3(1.0, 0.0, 1.0), debugMask * 0.75);
                }

                return fixed4(rgb, 1);
            }
            ENDCG
        }
    }
}
