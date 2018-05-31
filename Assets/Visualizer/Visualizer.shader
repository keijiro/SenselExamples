Shader "Hidden/Sensel/Visualizer"
{
    Properties
    {
        _MainTex("", 2D) = ""
        _InputTex("", 2D) = ""
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "SimplexNoise3D.hlsl"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    sampler2D _InputTex;

    half3 Palette(half p)
    {
        float3 x = p * float3(7.73, 9.31, 8.11);
        x += _Time.y * float3(2.31, 3.78, 2.54);
        return sin(x) * 0.5 + 0.5;
    }

    half4 frag_img(v2f_img input) : SV_Target
    {
        float2 uv_n = input.uv * 4;
        float t_n = _Time.y * 0.7;
        float amp_n = 0.02;

        float2 uv = input.uv;
        uv.x += snoise(float3(uv_n, t_n - 11)) * amp_n;
        uv.y += snoise(float3(t_n + 13, uv_n)) * amp_n;

        half p0 = tex2D(_MainTex, uv).a * 0.99;
        half p1 = saturate(tex2D(_InputTex, input.uv).r * 2);
        half p2 = max(p0, p1);

        return half4(Palette(p2) * saturate(p2 * 2), p2);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_img
            ENDCG
        }
    }
}
