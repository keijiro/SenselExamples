Shader "Sensel/Visualizer"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    Buffer<float> _ForceBuffer;
    float2 _BufferDims;

    struct Varyings
    {
        float4 vertex : SV_Position;
        float2 uv : TEXCOORD0;
    };

    Varyings Vertex(float4 vertex : POSITION, float2 uv : TEXCOORD0)
    {
        Varyings output;
        output.vertex = UnityObjectToClipPos(vertex);
        output.uv = uv;
        output.uv.y = 1 - output.uv.y;
        return output;
    }

    half4 Fragment(Varyings input) : SV_Target
    {
        float2 uv = round(input.uv * _BufferDims);
        uint idx = (uint)(uv.y * _BufferDims.x + uv.x);
        return saturate(_ForceBuffer[idx] * 0.1);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
