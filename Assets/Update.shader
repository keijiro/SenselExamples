Shader "Sensel/Visualizer"
{
    Properties
    {
        _MainTex("", 2D) = ""
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "SimplexNoise3D.hlsl"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    Buffer<float> _ForceBuffer;
    int2 _BufferDims;

    half SampleBuffer(float2 uv)
    {
#if UNITY_UV_STARTS_AT_TOP
        uv.y = 1 - uv.y;
#endif
        uv *= _BufferDims;

        int2 uv_i = (int2)uv;
        float2 delta = uv - uv_i;

        int index = uv_i.y * _BufferDims.x + uv_i.x;

        float s00 = _ForceBuffer[index];
        float s01 = _ForceBuffer[index + 1];
        float s10 = _ForceBuffer[index + _BufferDims.x];
        float s11 = _ForceBuffer[index + _BufferDims.x + 1];

        return lerp(lerp(s00, s01, delta.x), lerp(s10, s11, delta.x), delta.y);
    }

    half3 Palette(half p)
    {
        float3 x = p * float3(3.73, 4.31, 3.11);
        x += _Time.y * float3(2.31, 3.78, 2.54);
        return sin(x) * 0.5 + 0.5;
    }

    half4 Fragment(v2f_img input) : SV_Target
    {
        float2 uv_n = input.uv * 4;
        float t_n = _Time.y * 0.7;
        float amp_n = 0.02;

        float2 uv = input.uv;
        uv.x += snoise(float3(uv_n, t_n - 11)) * amp_n;
        uv.y += snoise(float3(t_n + 13, uv_n)) * amp_n;

        half p0 = tex2D(_MainTex, uv).a * 0.99;
        half p1 = SampleBuffer(input.uv) * 0.05;
        half p2 = max(p0, p1);

        return half4(Palette(p2) * saturate(p2 * 2), p2);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Fragment
            ENDCG
        }
    }
}
