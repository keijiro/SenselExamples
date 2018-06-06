Shader "Hidden/Cortina/Rain"
{
    Properties
    {
        _Color("", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Common.hlsl"

    half2 _NSpeed; // speed / (extent.z * 2)
    half2 _Length;
    half3 _Extent;
    half4 _Color;
    float4x4 _ObjectMatrix;
    float _LocalTime;

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        UNITY_FOG_COORDS(0)
    };

    Varyings Vertex(uint vid : SV_VertexID)
    {
        uint seed = (vid / 2) * 100;

        float spd = lerp(_NSpeed.x, _NSpeed.y, Random(seed++));
        float len = lerp(_Length.x, _Length.y, Random(seed++));

        float z = spd * _LocalTime;
        seed += 2 * (uint)z;
        float x = (Random(seed++) * 2 - 1) * _Extent.x;
        float y = (Random(seed++) * 2 - 1) * _Extent.y;
        z = (frac(z) - 0.5) * _Extent.z * 2;

        float4 pos = float4(x, y, z, 1);
        pos.z += len * ((vid & 1) - 0.5);

        Varyings output;
        output.vertex = UnityObjectToClipPos(mul(_ObjectMatrix, pos));
        UNITY_TRANSFER_FOG(output, output.vertex);
        return output;
    }

    half4 Fragment(Varyings input) : SV_Target
    {
        half4 c = _Color;
        UNITY_APPLY_FOG(input.fogCoord, c);
        return c;
    }

    ENDCG

    SubShader
    {
        Tags { "Queue"="Transparent" }
        Cull Off ZWrite Off
        Pass
        {
            Blend One One
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            ENDCG
        }
    }
}
