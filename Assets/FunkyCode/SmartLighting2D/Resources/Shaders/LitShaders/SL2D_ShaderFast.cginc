// custom property values
float _Lit;

// game lightmaps
sampler2D _GameTexture1; float4 _GameRect1; float _GameRotation1;
sampler2D _GameTexture2; float4 _GameRect2; float _GameRotation2;
sampler2D _GameTexture3; float4 _GameRect3; float _GameRotation3;
sampler2D _GameTexture4; float4 _GameRect4; float _GameRotation4;
sampler2D _GameTexture5; float4 _GameRect5; float _GameRotation5;
sampler2D _GameTexture6; float4 _GameRect6; float _GameRotation6;
sampler2D _GameTexture7; float4 _GameRect7; float _GameRotation7;
sampler2D _GameTexture8; float4 _GameRect8; float _GameRotation8;

// normal map & bump mapping
sampler2D _NormalMap;

float _LightIntensity;
float _Specular;

float _OcclusionLit;

float _OcclusionOffset;
float _OcclusionDepth;

float2 SL2D_FAST_LIGHTMAP_UV(float2 worldPos, float4 rect, float rotation)
{ 
    float2 cameraSize = float2(rect.z, rect.w);

    float c = cos(-rotation);
    float s = sin(-rotation);

    float x = worldPos.x - rect.x;
    float y = worldPos.y - rect.y;

    float2 localPosition = float2(x * c - y * s, x * s + y * c);

    return (localPosition + cameraSize / 2) / cameraSize;
}

float2 SL2D_FAST_LIGHTMAP_UV_1(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect1, _GameRotation1);
}

float2 SL2D_FAST_LIGHTMAP_UV_2(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect2, _GameRotation2);
}

float2 SL2D_FAST_LIGHTMAP_UV_3(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect3, _GameRotation3);
}

float2 SL2D_FAST_LIGHTMAP_UV_4(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect4, _GameRotation4);
}

float2 SL2D_FAST_LIGHTMAP_UV_5(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect5, _GameRotation5);
}

float2 SL2D_FAST_LIGHTMAP_UV_6(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect6, _GameRotation6);
}

float2 SL2D_FAST_LIGHTMAP_UV_7(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect7, _GameRotation7);
}

float2 SL2D_FAST_LIGHTMAP_UV_8(float2 worldPos)
{ 
    return SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect8, _GameRotation8);
}

/*
float3 SL2D_FAST_PASS_1(float2 worldPos)
{ 
    return tex2D (_GameTexture1, SL2D_FAST_LIGHTMAP_UV(worldPos, _GameRect1, _GameRotation1));
}*/

float3 SL2D_FAST_PASS_1(float2 lightmapUV)
{   
    return tex2Dproj (_GameTexture1, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_2(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture2, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_3(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture3, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_4(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture4, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_5(float2 lightmapUV)
{   
    return tex2Dproj (_GameTexture5, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_6(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture6, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_7(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture7, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

float3 SL2D_FAST_PASS_8(float2 lightmapUV)
{ 
    return tex2Dproj (_GameTexture8, float4(lightmapUV.x, lightmapUV.y, 1, 1)) * (0.25 > ((lightmapUV.x - 0.5) * (lightmapUV.x - 0.5))) * (0.25 > ((lightmapUV.y - 0.5) * (lightmapUV.y - 0.5)));
}

// change to blend function?

float3 SL2D_FAST_PASS_LIT_1(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_1(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_2(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_2(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_3(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_3(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_4(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_4(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_5(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_5(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_6(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_6(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_7(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_7(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_PASS_LIT_8(float2 worldPos)
{
    return lerp(SL2D_FAST_PASS_8(worldPos), float3(1, 1, 1), 1 - _Lit);
}

float3 SL2D_FAST_BUMP_PASS_1(float2 lightmapUV, float2 texcoord, float2 worldPosition)
{
    float delta = _OcclusionOffset / _GameRect1.z;

    // light pixel
    float3 lightPixelLeft = SL2D_FAST_PASS_1(lightmapUV + float2(-delta, 0));
    float3 lightPixelRight = SL2D_FAST_PASS_1(lightmapUV + float2(delta, 0));

    float3 lightPixelUp = SL2D_FAST_PASS_1(lightmapUV + float2(0, delta));
    float3 lightPixelDown = SL2D_FAST_PASS_1(lightmapUV + float2(0, -delta));

    float lightHorizontal = (lightPixelRight.r - lightPixelLeft.r) * _OcclusionDepth;
    float lightVertical = (lightPixelUp.r - lightPixelDown.r) * _OcclusionDepth;

    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -1));

    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, texcoord)).xyz;

    float4x4 worldMatrix = unity_WorldToObject;
    // worldMatrix[0].x = 1;
    // worldMatrix[1].y = 1;
    worldMatrix[2].z = 1;
    //  worldMatrix[3].w = 1;

    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), worldMatrix).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float normalDotLight = dot(normalDirection, lightDirection);

    float diffuseLevel = max(0.0f, normalDotLight);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float specular = pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, specular, _Specular);
    }

    float3 diffuseAndSpecular = diffuseLevel + specularLevel; 

    float3 result = diffuseAndSpecular;

    float l = (lightPixelLeft + lightPixelRight + lightPixelUp + lightPixelDown) * 0.25;
    float3 lightPixel = result * l;

    return lightPixel;
}

float3 SL2D_FAST_BUMP_PASS_2(float2 worldPos, float2 texcoord)
{
    float delta = _OcclusionOffset / _GameRect1.z;

    // light pixel
    float3 lightPixelLeft = SL2D_FAST_PASS_2(worldPos + float2(-delta, 0));
    float3 lightPixelRight = SL2D_FAST_PASS_2(worldPos + float2(delta, 0));

    float3 lightPixelUp = SL2D_FAST_PASS_2(worldPos + float2(0, delta));
    float3 lightPixelDown = SL2D_FAST_PASS_2(worldPos + float2(0, -delta));

    float lightHorizontal = (lightPixelRight.r - lightPixelLeft.r) * _OcclusionDepth;
    float lightVertical = (lightPixelUp.r - lightPixelDown.r) * _OcclusionDepth;

    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -1));

    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, texcoord)).xyz;

    float4x4 worldMatrix = unity_WorldToObject;
    // worldMatrix[0].x = 1;
    // worldMatrix[1].y = 1;
    worldMatrix[2].z = 1;
    //  worldMatrix[3].w = 1;
    
    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), worldMatrix).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float normalDotLight = dot(normalDirection, lightDirection);

    float diffuseLevel = max(0.0f, normalDotLight);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float specular = pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, specular, _Specular);
    }

    float3 diffuseAndSpecular = diffuseLevel + specularLevel; 

    float3 result = diffuseAndSpecular;

    float l = (lightPixelLeft + lightPixelRight + lightPixelUp + lightPixelDown) * 0.25;
    float3 lightPixel = result * l;

    return lightPixel;
}

float3 SL2D_FAST_BUMP_PASS_3(float2 worldPos, float2 texcoord)
{
    float delta = _OcclusionOffset / _GameRect1.z;

    // light pixel
    float3 lightPixelLeft = SL2D_FAST_PASS_3(worldPos + float2(-delta, 0));
    float3 lightPixelRight = SL2D_FAST_PASS_3(worldPos + float2(delta, 0));

    float3 lightPixelUp = SL2D_FAST_PASS_3(worldPos + float2(0, delta));
    float3 lightPixelDown = SL2D_FAST_PASS_3(worldPos + float2(0, -delta));

    float lightHorizontal = (lightPixelRight.r - lightPixelLeft.r) * _OcclusionDepth;
    float lightVertical = (lightPixelUp.r - lightPixelDown.r) * _OcclusionDepth;

    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -1));

    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, texcoord)).xyz;

    float4x4 worldMatrix = unity_WorldToObject;
    // worldMatrix[0].x = 1;
    // worldMatrix[1].y = 1;
    worldMatrix[2].z = 1;
    //  worldMatrix[3].w = 1;
    
    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), worldMatrix).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float normalDotLight = dot(normalDirection, lightDirection);

    float diffuseLevel = max(0.0f, normalDotLight);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float specular = pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, specular, _Specular);
    }

    float3 diffuseAndSpecular = diffuseLevel + specularLevel; 

    float3 result = diffuseAndSpecular;

    float l = (lightPixelLeft + lightPixelRight + lightPixelUp + lightPixelDown) * 0.25;
    float3 lightPixel = result * l; 

    return lightPixel;
}

float3 SL2D_FAST_BUMP_PASS_4(float2 worldPos, float2 texcoord)
{
    float delta = _OcclusionOffset / _GameRect1.z;

    // light pixel
    float3 lightPixelLeft = SL2D_FAST_PASS_4(worldPos + float2(-delta, 0));
    float3 lightPixelRight = SL2D_FAST_PASS_4(worldPos + float2(delta, 0));

    float3 lightPixelUp = SL2D_FAST_PASS_4(worldPos + float2(0, delta));
    float3 lightPixelDown = SL2D_FAST_PASS_4(worldPos + float2(0, -delta));

    float lightHorizontal = (lightPixelRight.r - lightPixelLeft.r) * _OcclusionDepth;
    float lightVertical = (lightPixelUp.r - lightPixelDown.r) * _OcclusionDepth;

    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -1));

    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, texcoord)).xyz;

    float4x4 worldMatrix = unity_WorldToObject;
    // worldMatrix[0].x = 1;
    // worldMatrix[1].y = 1;
    worldMatrix[2].z = 1;
    //  worldMatrix[3].w = 1;
    
    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), worldMatrix).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float normalDotLight = dot(normalDirection, lightDirection);

    float diffuseLevel = max(0.0f, normalDotLight);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float specular = pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, specular, _Specular);
    }

    float3 diffuseAndSpecular = diffuseLevel + specularLevel; 

    float3 result = diffuseAndSpecular;

    float l = (lightPixelLeft + lightPixelRight + lightPixelUp + lightPixelDown) * 0.25;
    float3 lightPixel = result * l; 

    return lightPixel;
}