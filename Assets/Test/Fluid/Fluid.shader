// StableFluids - A GPU implementation of Jos Stam's Stable Fluids on Unity
// https://github.com/keijiro/StableFluids

Shader "Hidden/StableFluids"
{
    Properties
    {
        _MainTex("", 2D) = ""
        _VelocityField("", 2D) = ""
    }

    CGINCLUDE

    #define MAX_FORCES 16

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    sampler2D _VelocityField;

    float2 _ForceOrigins[MAX_FORCES];
    float _ForceExponent;

    half4 frag_advect(v2f_img i) : SV_Target
    {
        // Time parameters
        float time = _Time.y;
        float deltaTime = unity_DeltaTime.x;

        // Aspect ratio coefficients
        float2 aspect = float2(_MainTex_TexelSize.y * _MainTex_TexelSize.z, 1);
        float2 aspect_inv = float2(_MainTex_TexelSize.x * _MainTex_TexelSize.w, 1);

        // Color advection with the velocity field
        float2 delta = tex2D(_VelocityField, i.uv).xy * aspect_inv * deltaTime;
        float color = tex2D(_MainTex, i.uv - delta).x;

        // Dye (injection color)
        float3 dye = 1;//saturate(sin(time * float3(2.72, 5.12, 4.98)) + 0.5);

        for (uint idx = 0; idx < MAX_FORCES; idx++)
        {
            // Blend dye with the color from the buffer.
            float2 pos = (i.uv - 0.5) * aspect;
            float amp = exp(-_ForceExponent / 4 * distance(_ForceOrigins[idx], pos));
            color = max(color, amp * 4);
        }
        color *= 0.995;

        return color;
    }

    half4 frag_render(v2f_img i) : SV_Target
    {
        half color = tex2D(_MainTex, i.uv).r;

        // Mixing channels up to get slowly changing false colors
        half3 rgb = sin(float3(2.43, 3.43, 3.84) * color +
                        float3(1.12, 1.33, 0.94) * _Time.y) * 0.5 + 0.5;

        rgb *= smoothstep(0.01, 0.1, color);

        return half4(GammaToLinearSpace(rgb), 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_advect
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_render
            ENDCG
        }
    }
}
