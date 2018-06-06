// Hash function from H. Schechter & R. Bridson, goo.gl/RXiKaH
uint Hash(uint s)
{
    s ^= 2747636419u;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    s ^= s >> 16;
    s *= 2654435769u;
    return s;
}

float Random(uint seed)
{
    return float(Hash(seed)) / 4294967295.0; // 2^32-1
}

half3 Hue2RGB(half h)
{
    h = frac(saturate(h)) * 6 - 2;
    half3 rgb = saturate(half3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)));
#ifndef UNITY_COLORSPACE_GAMMA
    rgb = GammaToLinearSpace(rgb);
#endif
    return rgb;
}
