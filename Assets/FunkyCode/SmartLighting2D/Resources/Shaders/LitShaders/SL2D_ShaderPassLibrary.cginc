// lightmaps

#ifdef SL2D_PASS_1
    sampler2D _GameTexture1; float4 _GameRect1; float _GameRotation1;
    sampler2D _SceneTexture1; float4 _SceneRect1; float _SceneRotation1;
#endif

#ifdef SL2D_PASS_2
    sampler2D _GameTexture2; float4 _GameRect2; float _GameRotation2;
    sampler2D _SceneTexture2; float4 _SceneRect2; float _SceneRotation2;
#endif

#ifdef SL2D_PASS_3
    sampler2D _GameTexture3; float4 _GameRect3; float _GameRotation3;
    sampler2D _SceneTexture3; float4 _SceneRect3; float _SceneRotation3;
#endif

#ifdef SL2D_PASS_4
    sampler2D _GameTexture4; float4 _GameRect4; float _GameRotation4;
    sampler2D _SceneTexture4; float4 _SceneRect4; float _SceneRotation4;
#endif

#ifdef SL2D_PASS_5
    sampler2D _GameTexture5; float4 _GameRect5; float _GameRotation5;
    sampler2D _SceneTexture5; float4 _SceneRect5; float _SceneRotation5;
#endif

#ifdef SL2D_PASS_6
    sampler2D _GameTexture6; float4 _GameRect6; float _GameRotation6;
    sampler2D _SceneTexture6; float4 _SceneRect6; float _SceneRotation6;
#endif

#ifdef SL2D_PASS_7
    sampler2D _GameTexture7; float4 _GameRect7; float _GameRotation7;
    sampler2D _SceneTexture7; float4 _SceneRect7; float _SceneRotation7;
#endif

#ifdef SL2D_PASS_8
    sampler2D _GameTexture8; float4 _GameRect8; float _GameRotation8;
    sampler2D _SceneTexture8; float4 _SceneRect8; float _SceneRotation8;
#endif

sampler2D _Render_GameTexture; float4 _Render_GameRect; float _Render_GameRotation;
sampler2D _Render_SceneTexture; float4 _Render_SceneRect; float _Render_SceneRotation;
sampler2D _Render_NoneTexture;

float _OcclusionOffset;
float _OcclusionDepth;
float _Specular;

float _OffScreen;

sampler2D _NormalMap;

///// core functions /////

float2 TransformToCamera(float2 pos, float rotation)
{
    float c = cos(-rotation);
    float s = sin(-rotation);

    float x = pos.x;
    float y = pos.y;

    pos.x = x * c - y * s;
    pos.y = x * s + y * c;

    return(pos);
}

bool InCamera (float2 pos, float2 rectSize)
{
    float2 rectPos = -rectSize * 0.5;

    return !(pos.x < rectPos.x || pos.x > rectPos.x + rectSize.x || pos.y < rectPos.y || pos.y > rectPos.y + rectSize.y);
}

///// lightmap color functions /////

float3 SL2D_Light(float2 worldPos) // all lightmaps
{ 
    float rotation, draw;
    float2 cameraSize, localPosition, uv;

    float3 color = lerp(0, 1, _OffScreen);
    
    float4 rect;

    // _Render_GameTexture = _Render_NoneTexture;

    #ifdef SL2D_PASS_1

        _Render_GameTexture = _GameTexture1;
        _Render_GameRect = _GameRect1;
        _Render_GameRotation = _GameRotation1;

        _Render_SceneTexture = _SceneTexture1;
        _Render_SceneRect = _SceneRect1;
        _Render_SceneRotation = _SceneRotation1;

    #endif

    #ifdef SL2D_PASS_2

        _Render_GameTexture = _GameTexture2;
        _Render_GameRect = _GameRect2;
        _Render_GameRotation = _GameRotation2;

        _Render_SceneTexture = _SceneTexture2;
        _Render_SceneRect = _SceneRect2;
        _Render_SceneRotation = _SceneRotation2;

    #endif

    #ifdef SL2D_PASS_3

        _Render_GameTexture = _GameTexture3;
        _Render_GameRect = _GameRect3;
        _Render_GameRotation = _GameRotation3;

        _Render_SceneTexture = _SceneTexture3;
        _Render_SceneRect = _SceneRect3;
        _Render_SceneRotation = _SceneRotation3;

    #endif

    #ifdef SL2D_PASS_4

        _Render_GameTexture = _GameTexture4;
        _Render_GameRect = _GameRect4;
        _Render_GameRotation = _GameRotation4;

        _Render_SceneTexture = _SceneTexture4;
        _Render_SceneRect = _SceneRect4;
        _Render_SceneRotation = _SceneRotation4;

    #endif

    
    #ifdef SL2D_PASS_5

        _Render_GameTexture = _GameTexture5;
        _Render_GameRect = _GameRect5;
        _Render_GameRotation = _GameRotation5;

        _Render_SceneTexture = _SceneTexture5;
        _Render_SceneRect = _SceneRect5;
        _Render_SceneRotation = _SceneRotation5;

    #endif

    #ifdef SL2D_PASS_6

        _Render_GameTexture = _GameTexture6;
        _Render_GameRect = _GameRect6;
        _Render_GameRotation = _GameRotation6;

        _Render_SceneTexture = _SceneTexture6;
        _Render_SceneRect = _SceneRect6;
        _Render_SceneRotation = _SceneRotation6;

    #endif

    #ifdef SL2D_PASS_7

        _Render_GameTexture = _GameTexture7;
        _Render_GameRect = _GameRect7;
        _Render_GameRotation = _GameRotation7;

        _Render_SceneTexture = _SceneTexture7;
        _Render_SceneRect = _SceneRect7;
        _Render_SceneRotation = _SceneRotation7;

    #endif

    #ifdef SL2D_PASS_8

        _Render_GameTexture = _GameTexture8;
        _Render_GameRect = _GameRect8;
        _Render_GameRotation = _GameRotation8;

        _Render_SceneTexture = _SceneTexture8;
        _Render_SceneRect = _SceneRect8;
        _Render_SceneRotation = _SceneRotation8;

    #endif

    // game view

    rect = _Render_GameRect;   
    rotation = _Render_GameRotation;
    cameraSize = float2(rect.z, rect.w);
    localPosition = TransformToCamera(worldPos - float2(rect.x, rect.y), rotation);
    draw = step(0, rect.z) * InCamera(localPosition, cameraSize);
    uv = (localPosition + cameraSize / 2) / cameraSize;
    color = lerp(color, tex2Dproj (_Render_GameTexture, float4(uv.x, uv.y, 1, 1)), draw);

    // scene view

    rect = _Render_SceneRect;
    rotation = _Render_SceneRotation;
    cameraSize = float2(rect.z, rect.w);
    localPosition = TransformToCamera(worldPos - float2(rect.x, rect.y), rotation);
    draw = step(0, rect.z) * InCamera(localPosition, cameraSize);
    uv = (localPosition + cameraSize / 2) / cameraSize;
    color = lerp(color, tex2Dproj (_Render_SceneTexture, float4(uv.x, uv.y, 1, 1)), draw);

    return(color);
}

float SL2D_FogOfWar(float2 worldPos)
{
    float3 lightPixel = SL2D_Light(worldPos);

    return min(1, (lightPixel.r + lightPixel.g + lightPixel.b) / 3);
}

float3 SL2D_Blend_Lit(float3 input, float lit) 
{
    return lerp(input, float3(1, 1, 1), 1 - lit);
}

float3 SL2D_Bump(float2 worldPos, float2 texcoord)
{
    float delta = _OcclusionOffset;

    // light pixel
    float3 lightPixelLeft = SL2D_Light(worldPos + float2(-delta, 0));  
    float3 lightPixelRight = SL2D_Light(worldPos + float2(delta, 0));

    float3 lightPixelUp = SL2D_Light(worldPos + float2(0, delta));
    float3 lightPixelDown = SL2D_Light(worldPos + float2(0, -delta));

    lightPixelLeft.r = (lightPixelLeft.r +  lightPixelLeft.g + lightPixelLeft.b) / 3;
    lightPixelRight.r = (lightPixelRight.r + lightPixelRight.g + lightPixelRight.b) / 3;

    lightPixelUp.r = (lightPixelUp.r + lightPixelUp.g + lightPixelUp.b) / 3;
    lightPixelDown.r = (lightPixelDown.r + lightPixelDown.g + lightPixelDown.b) / 3;

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

    float3 l = (lightPixelLeft + lightPixelRight + lightPixelUp + lightPixelDown) * 0.25;
    float3 lightPixel = result * l;

    return lightPixel;
}

float3 SL2D_Depth(float2 worldPos)
{
    float depth = SL2D_Light(worldPos).r;

    return(depth * 255 - 101);
}