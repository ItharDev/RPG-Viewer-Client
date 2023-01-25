
// internal variables

// game lightmaps

sampler2D _GameTexture1; float4 _GameRect1; float4 _GameColor1; float _GameRotation1;
sampler2D _GameTexture2; float4 _GameRect2; float4 _GameColor2; float _GameRotation2;
sampler2D _GameTexture3; float4 _GameRect3; float4 _GameColor3; float _GameRotation3;
sampler2D _GameTexture4; float4 _GameRect4; float4 _GameColor4; float _GameRotation4;

// scene lightmaps
sampler2D _SceneTexture1; float4 _SceneRect1; float4 _SceneColor1; float _SceneRotation1;
sampler2D _SceneTexture2; float4 _SceneRect2; float4 _SceneColor2; float _SceneRotation2;
sampler2D _SceneTexture3; float4 _SceneRect3; float4 _SceneColor3; float _SceneRotation3;
sampler2D _SceneTexture4; float4 _SceneRect4; float4 _SceneColor4; float _SceneRotation4;

// day lighting variables

float _Day_Direction;
float _Day_Height;
float _Day_Alpha;

float _OffScreen;

sampler2D _NormalMap;

// public values

void SL2D_Value_DayAlpha_float(out float color)
{
    color = _Day_Alpha;
}

void SL2D_Value_DayHeight_float(out float color)
{
    color = _Day_Height;
}

void SL2D_Value_DayDirection_float(out float color)
{
    color = _Day_Direction;
}

void SL2D_Value_Color_1_float(out float4 color)
{
    color = max(_GameColor1, _SceneColor1);
}

void SL2D_Value_Color_2_float(out float4 color)
{
    color = max(_GameColor2, _SceneColor2);
}

void SL2D_Value_Color_3_float(out float4 color)
{
    color = max(_GameColor3, _SceneColor3);
}

void SL2D_Value_Color_4_float(out float4 color)
{
    color = max(_GameColor4, _SceneColor4);
}

// internal api

float3 internal_GameLightmap(int id, float2 texcoord)
{
    switch(id)
    {
        case 1:
            return(tex2D (_GameTexture1, texcoord).rgb);

        case 2:
            return(tex2D (_GameTexture2, texcoord).rgb);

        case 3:
            return(tex2D (_GameTexture3, texcoord).rgb);

        case 4:
            return(tex2D (_GameTexture4, texcoord).rgb);
    }

    return(tex2D (_GameTexture1, texcoord).rgb);
}

float4 internal_GameRect(int id)
{
    switch(id)
    {
        case 1:
            return(_GameRect1);

        case 2:
            return(_GameRect2);

        case 3:
            return(_GameRect3);

        case 4:
            return(_GameRect4);
    }

    return(float4(0, 0, 0, 0));
}

float internal_GameRotation(int id)
{
    switch(id)
    {
        case 1:
            return(_GameRotation1);

        case 2:
            return(_GameRotation2);

        case 3:
            return(_GameRotation3);

        case 4:
            return(_GameRotation4);
    }

    return(0);
}

float3 internal_SceneLightmap(int id, float2 texcoord)
{
    switch(id)
    {
        case 1:
            return(tex2D (_SceneTexture1, texcoord).rgb);

        case 2:
            return(tex2D (_SceneTexture2, texcoord).rgb);

        case 3:
            return(tex2D (_SceneTexture3, texcoord).rgb);

        case 4:
            return(tex2D (_SceneTexture4, texcoord).rgb);
    }

    return(tex2D (_SceneTexture1, texcoord).rgb);
}

float4 internal_SceneRect(int id)
{
    switch(id)
    {
        case 1:
            return(_SceneRect1);

        case 2:
            return(_SceneRect2);

        case 3:
            return(_SceneRect3);

        case 4:
            return(_SceneRect4);
    }

    return(float4(0, 0, 0, 0));
}

float internal_SceneRotation(int id)
{
    switch(id)
    {
        case 1:
            return(_SceneRotation1);

        case 2:
            return(_SceneRotation2);

        case 3:
            return(_SceneRotation3);

        case 4:
            return(_SceneRotation4);
    }

    return(0);
}

float2 internal_WorldToLocal(float2 pos, float rotation)
{
    float c = cos(-rotation);
    float s = sin(-rotation);

    float x = pos.x;
    float y = pos.y;

    pos.x = x * c - y * s;
    pos.y = x * s + y * c;

    return(pos);
}

bool internal_InCamera(float2 pos, float2 rectSize)
{
    float2 rectPos = -rectSize * 0.5;

    return !(pos.x < rectPos.x || pos.x > rectPos.x + rectSize.x || pos.y < rectPos.y || pos.y > rectPos.y + rectSize.y);
}

// private api (game pass + scene pass)

float4 SL2D_Pass(float4 color, int id, float2 world)
{
    float4 rect = internal_GameRect(id);
    float rotation = internal_GameRotation(id);
    float2 cameraSize = float2(rect.z, rect.w);
    float2 localPosition = internal_WorldToLocal(world - float2(rect.x, rect.y), rotation);
    bool draw = (rect.z > 0) * internal_InCamera(localPosition, cameraSize);

    color.rgb = lerp(color.rgb, internal_GameLightmap(id, (localPosition + cameraSize / 2) / cameraSize), draw);

    rect = internal_SceneRect(id);
    rotation = internal_SceneRotation(id);
    cameraSize = float2(rect.z, rect.w);
    localPosition = internal_WorldToLocal(world - float2(rect.x, rect.y), rotation);
    draw = (rect.z > 0) * internal_InCamera(localPosition, cameraSize);

    color.rgb = lerp(color.rgb, internal_SceneLightmap(id, (localPosition + cameraSize / 2) / cameraSize), draw);

    return color;
}

float4 SL2D_Pass_Light(int id, float2 world)
{
    float4 color = float4(0, 0, 0, 1);
    color.rgb = lerp(0, 1, _OffScreen);

    color = SL2D_Pass(color, id, world);

    return color;
}

float4 SL2D_Pass_Depth(int id, float2 world)
{
    float2 cameraSize, localPosition;
    float depth = 0;

    float4 rect = internal_GameRect(id);

    float rotation = internal_GameRotation(id);

    if (rect.z > 0)
    {
        float2 cameraSize = float2(rect.z, rect.w);
        float2 localPosition = internal_WorldToLocal(world - float2(rect.x, rect.y), rotation);

        if (internal_InCamera(localPosition, cameraSize))
        {
            depth = internal_GameLightmap(id, (localPosition + cameraSize / 2) / cameraSize).r;
        }
    }

    rect = internal_SceneRect(id);

    rotation = internal_SceneRotation(id);

    if (rect.z > 0)
    {
        float2 cameraSize = float2(rect.z, rect.w);
        float2 localPosition = internal_WorldToLocal(world - float2(rect.x, rect.y), rotation);

        if (internal_InCamera(localPosition, cameraSize))
        {
            depth = internal_SceneLightmap(id, (localPosition + cameraSize / 2) / cameraSize).r;
        }
    }

    depth = depth * 255 - 101;

    return(depth);
}

// public api (SL2D_Light_ID) (4 game + 4 scene lightmaps)

void SL2D_Light_float(float2 world, out float4 color)
{
    color = float4(0, 0, 0, 1);
    color.rgb = lerp(0, 1, _OffScreen);
    
    color = SL2D_Pass(color, 1, world);
    color = SL2D_Pass(color, 2, world);
    color = SL2D_Pass(color, 3, world);
    color = SL2D_Pass(color, 4, world);
}

void SL2D_Depth_float(float2 world, float depth, float strength, out float4 color)
{
    color = lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(1, world)));
    color = min(color, lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(2, world))));
    color = min(color, lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(3, world))));
    color = min(color, lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(4, world))));
}

void SL2D_Light_1_float(float2 world, out float4 color) { color = SL2D_Pass_Light(1, world); }
void SL2D_Light_2_float(float2 world, out float4 color) { color = SL2D_Pass_Light(2, world); }
void SL2D_Light_3_float(float2 world, out float4 color) { color = SL2D_Pass_Light(3, world); }
void SL2D_Light_4_float(float2 world, out float4 color) { color = SL2D_Pass_Light(4, world); }

void SL2D_Depth_1_float(float2 world, float depth, float strength, out float4 color) { color = lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(1, world))); }
void SL2D_Depth_2_float(float2 world, float depth, float strength, out float4 color) { color = lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(2, world))); }
void SL2D_Depth_3_float(float2 world, float depth, float strength, out float4 color) { color = lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(3, world))); }
void SL2D_Depth_4_float(float2 world, float depth, float strength, out float4 color) { color = lerp(float4(1, 1, 1, 1), float4(1 - strength, 1 - strength, 1 - strength, 1), step(depth, SL2D_Pass_Depth(4, world))); }

// blend api

void SL2D_Blend_FogOfWar_float(float4 incolor, float4 lightmap, out float4 outcolor)
{
    outcolor = incolor;

    outcolor.a = min(outcolor.a * ((lightmap.r + lightmap.g + lightmap.b) / 3), 1);
}

void SL2D_Blend_Lit_float(float4 incolor, float lit, out float4 outcolor)
{
    outcolor = lerp(incolor, float4(1, 1, 1, 1), 1 - lit);
}

// misc api

float4 SL2D_Pass_LightAdd(int id, float2 world)
{
    float2 cameraSize, localPosition;
    float4 color = float4(1, 1, 1, 1);

    float4 rect = internal_GameRect(id);

    float rotation = internal_GameRotation(id);

    if (rect.z > 0)
    {
        float2 cameraSize = float2(rect.z, rect.w);
        float2 localPosition = internal_WorldToLocal(world - float2(rect.x, rect.y), rotation);

        if (internal_InCamera(localPosition, cameraSize))
        {
            color.rgb = internal_GameLightmap(id, (localPosition + cameraSize / 2) / cameraSize).rgb + float3(1, 1, 1);
        }
    }

    return(color);
}

void SL2D_Light2_float(float2 world, out float4 color)
{
    color = SL2D_Pass_LightAdd(2, world);
}

void SL2D_DayBump_float(float2 uv, float intensity, float specular, out float4 color)
{
    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, uv)).xyz;

    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), unity_WorldToObject).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float lightHorizontal = cos(_Day_Direction);
    float lightVertical = sin(_Day_Direction);
    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -0.75f * _Day_Height * 2));

    float normalDotLight = dot(normalDirection, lightDirection);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float spec = intensity * pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, spec, specular);
    }

    float diffuseLevel = intensity * max(0.0f, normalDotLight);

    float3 diffuseAndSpecular = diffuseLevel + specularLevel;

    color = float4(diffuseAndSpecular, 1);
}

void SL2D_Bump_float(float2 world, float2 uv, float delta, float depth, out float4 color)
{
    float4 lightPixelLeft = SL2D_Pass_Light(3, world + float2(-delta, 0));
    float4 lightPixelRight = SL2D_Pass_Light(3, world + float2(delta, 0));

    float4 lightPixelUp = SL2D_Pass_Light(3, world + float2(0, delta));
    float4 lightPixelDown = SL2D_Pass_Light(3, world + float2(0, -delta));

    float lightHorizontal = (lightPixelRight.r - lightPixelLeft.r) * depth;
    float lightVertical = (lightPixelUp.r - lightPixelDown.r) * depth;

    float3 lightDirection = normalize(float3(lightHorizontal, lightVertical, -1));

    float3 normalDirection = UnpackNormal(tex2D(_NormalMap, uv)).xyz;
    
    normalDirection = float3(mul(float4(normalDirection.xyz, 1.0f), unity_WorldToObject).xyz);
    normalDirection.z *= -1;
    normalDirection = normalize(normalDirection);

    float normalDotLight = dot(normalDirection, lightDirection);

    float diffuseLevel = max(0.0f, normalDotLight);

    float specularLevel = 0.0f;

    if (normalDotLight > 0.0f)
    {
        float specular = pow(max(0.0, dot(reflect(-lightDirection, normalDirection), float3(0.0f, 0.0f, -1.0f))), 10);
        
        specularLevel = lerp(0, specular, 0.5); // _Specular
    }

    float3 diffuseAndSpecular = diffuseLevel + specularLevel; 

    float4 result = float4(diffuseAndSpecular, 1);

    float l = (lightPixelLeft.r + lightPixelRight.r + lightPixelUp.r + lightPixelDown.r) * 0.25;
    float4 lightPixel = result * l + 1;

    // lightPixel = lerp(lightPixel, float4(1, 1, 1, 1), 1 - _Lit);

    color = lightPixel;
}


/*
void SL2D_Depth_float(float2 world, float depth, float strength, out float4 color)
{
    float depthmap = SL2D_Pass_Depth(1, world);

    float depthExist = step(depth, depthmap); 

    float lightPass = SL2D_Pass_Light(2, world).r;

    float output = min(1 - strength + lightPass, 1);

    color = lerp(float4(1, 1, 1, 1), float4(output, output, output, 1), depthExist);
}
*/
