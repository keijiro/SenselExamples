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

    float4 _ForceOrigins[MAX_FORCES];

    half4 frag_advect(v2f_img input) : SV_Target
    {
        float2 uv = input.uv;

        // Time parameters
        float time = _Time.y;
        float deltaTime = unity_DeltaTime.x;

        // Aspect ratio coefficients
        float2 aspect = float2(_MainTex_TexelSize.y * _MainTex_TexelSize.z, 1);
        float2 aspect_inv = float2(_MainTex_TexelSize.x * _MainTex_TexelSize.w, 1);

        // Color advection with the velocity field
        float2 delta = tex2D(_VelocityField, uv).xy * aspect_inv * deltaTime;
        float color = tex2D(_MainTex, uv - delta).x;

        // Input from force origins
        float force = 0;
        for (uint i = 0; i < MAX_FORCES; i++)
        {
            float dist = distance((uv - 0.5) * aspect, _ForceOrigins[i].xy);
            force += max(0, _ForceOrigins[i].z - dist);
        }

        // Change the color where the input force is over the threshold.
        float dye = sin(_Time * 49) + 1.01;
        color = lerp(color, dye, smoothstep(0, 0.05, force));

        return color;
    }

    half4 frag_render(v2f_img inpu) : SV_Target
    {
        half color = tex2D(_MainTex, inpu.uv).r;

        // Mixing channels up to get slowly changing false colors
        half3 rgb = sin(float3(11.43, 13.43, 13.43) * color +
                        float3( 1.12,  1.33,  1.13) * _Time.y);
        rgb = rgb * float3(0.2, 0.5, 0.5) + float3(0.8, 0.5, 0.5);

        // Near-zero cut off
        rgb *= smoothstep(0, 0.01, color);

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
