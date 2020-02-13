uniform float _Time;
uniform float _DeltaTime;
uniform float2 _Mouse;
uniform float4 _Resolution; // resolution, texelSize


static const float PI = 3.1415926538;
static const float TWOPI = PI * 2;

float3 palette( float t, float3 a, float3 b, float3 c, float3 d )
{
    return a + b * cos(6.28318 * (c * t + d));
}


float2 rotate(float2 v, float theta) {
    float s = sin(theta);
    float c = cos(theta);
    return mul(v, float2x2(c, -s, s, c));
}


float unlerp(float a, float b, float t) {
    return (t - a) / (b - a);
}

float rand(float n){return frac(sin(n) * 43758.5453123);}
