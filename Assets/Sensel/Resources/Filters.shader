Shader "Hidden/Sensel/Filters"
{
    Properties
    {
        _MainTex("", 2D) = ""
        _Sensitivity("", Float) = 0.2
        _Alpha("", Float) = 1
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    float _Sensitivity;
    float _Alpha;

    half4 frag_prefilter(v2f_img input) : SV_Target
    {
        float2 uv = input.uv;
#if UNITY_UV_STARTS_AT_TOP
        uv.y = 1 - uv.y;
#endif
        return tex2D(_MainTex, uv).r * _Sensitivity;
    }

    half4 frag_downsample(v2f_img input) : SV_Target
    {
        float4 duv = float4(1, 1, -1, 0) * _MainTex_TexelSize.xyxy * 2.5;

        half s =
            tex2D(_MainTex, input.uv - duv.xy).r     +
            tex2D(_MainTex, input.uv - duv.wy).r * 2 +
            tex2D(_MainTex, input.uv - duv.zy).r     +
            tex2D(_MainTex, input.uv + duv.zw).r * 2 +
            tex2D(_MainTex, input.uv         ).r * 4 +
            tex2D(_MainTex, input.uv + duv.xw).r * 2 +
            tex2D(_MainTex, input.uv + duv.zy).r     +
            tex2D(_MainTex, input.uv + duv.wy).r * 2 +
            tex2D(_MainTex, input.uv + duv.xy).r;

        return half4(s / 16, 0, 0, 1);
    }

    half4 frag_upsample(v2f_img input) : SV_Target
    {
        return half4(tex2D(_MainTex, input.uv).r, 0, 0, _Alpha);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_prefilter
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_downsample
            ENDCG
        }
        Pass
        {
            Blend SrcAlpha SrcAlpha
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_upsample
            ENDCG
        }
    }
}
