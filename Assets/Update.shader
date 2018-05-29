Shader "Sensel/Visualizer"
{
    Properties
    {
        _MainTex("", 2D) = ""
    }
    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    Buffer<float> _ForceBuffer;
    float2 _BufferDims;

    half4 Fragment(v2f_img input) : SV_Target
    {
        float4 dd = float4(1, 1, -1, 0) * _MainTex_TexelSize.xyxy * 30;
        half4 hp =
            tex2D(_MainTex, input.uv - dd.xy) * 1 +
            tex2D(_MainTex, input.uv - dd.wy) * 2 +
            tex2D(_MainTex, input.uv - dd.zy) * 1 +
            tex2D(_MainTex, input.uv + dd.zw) * 2 +
            tex2D(_MainTex, input.uv        ) * 4 +
            tex2D(_MainTex, input.uv + dd.xw) * 2 +
            tex2D(_MainTex, input.uv + dd.zy) * 1 +
            tex2D(_MainTex, input.uv + dd.wy) * 2 +
            tex2D(_MainTex, input.uv + dd.xy) * 1;
        hp *= 0.99 / 16;

        float2 uv = input.uv;
        uv.y = 1 - uv.y;
        uv *= _BufferDims;
        float2 iuv = (int2)uv;
        float2 duv = uv - iuv;

        int idx = iuv.y * _BufferDims.x + iuv.x;

        float p00 = _ForceBuffer[idx];
        float p01 = _ForceBuffer[idx + 1];
        float p10 = _ForceBuffer[idx + _BufferDims.x];
        float p11 = _ForceBuffer[idx + _BufferDims.x + 1];

        float p = lerp(lerp(p00, p01, duv.x), lerp(p10, p11, duv.x), duv.y);
        float p2 = saturate(max(hp.a, saturate(p * 0.05)));

        float cr = sin(p2 * 8.73 + _Time.y * 5.31) * 0.5 + 0.5;
        float cg = sin(p2 * 7.31 + _Time.y * 4.78) * 0.5 + 0.5;
        float cb = sin(p2 * 5.11 + _Time.y * 3.53) * 0.5 + 0.5;

        return half4(saturate(half3(cr, cg, cb) * saturate(p2 * 2)), p2);
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
