Shader "Custom/RGBStrip"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _StripColor ("Strip Color", Color) = (1,0,0,1)
        _StripWidth ("Strip Width", Range(0.01,1)) = 0.15
        _Brightness ("Brightness", Range(0,4)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _StripColor;
        float _StripWidth;
        float _Brightness;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            // Rotate a strip of _StripColor around the Y axis
            float3 center = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
            float3 toFrag = IN.worldPos - center;
            float angle = atan2(toFrag.z, toFrag.x) / (2.0 * 3.14159265) + 0.5;
            float dist = abs(frac(angle - frac(_Time.y * 0.2) + 0.5) - 0.5) * 2.0;
            float strip = 1.0 - smoothstep(_StripWidth - 0.05, _StripWidth + 0.05, dist);
            o.Albedo = lerp(c.rgb, _StripColor.rgb, strip * 0.5);
            o.Emission = _StripColor.rgb * strip * _Brightness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
