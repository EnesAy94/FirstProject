#ifndef GLITCH_FUNCTIONS_INCLUDED
#define GLITCH_FUNCTIONS_INCLUDED

// Simple pseudo-random noise
float random(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

void GlitchEffect_float(float2 UV, float Time, float GlitchStrength, float GlitchRate, float2 Resolution, out float2 OutUV, out float3 ColorOffset)
{
    // 1. Time-based quantization for "jerky" movement
    float timeStep = floor(Time * GlitchRate);
    
    // 2. Generate random values based on quantized time
    float2 noiseUV = float2(0.0, timeStep);
    float noiseVal = random(noiseUV);
    
    // 3. Threshold to determine if glitch should happen
    // If random value is above GlitchStrength, we skip glitch (interaction is inverted for intuitive control)
    // Let's make GlitchStrength 0..1 where 1 is constant glitch
    
    float trigger = step(1.0 - GlitchStrength, noiseVal);
    
    // 4. Calculate UV Jitter (Blocky horizontal displacement)
    // We use y-coordinate to create horizontal strips
    float splitCount = 10.0 + random(float2(timeStep, 1.0)) * 50.0; // Random horizontal strips
    float blockY = floor(UV.y * splitCount) / splitCount;
    
    // Random offset per strip
    float stripNoise = random(float2(blockY, timeStep));
    float shake = (stripNoise - 0.5) * 2.0; // -1 to 1
    
    // Apply jitter only if triggered
    float2 jitter = float2(shake * 0.1 * trigger, 0.0);
    
    OutUV = UV + jitter;
    
    // 5. Chromatic Aberration
    // Offset channels slightly different
    float chromAb = 0.02 * trigger * shake;
    ColorOffset = float3(chromAb, 0.0, -chromAb);
}

#endif
