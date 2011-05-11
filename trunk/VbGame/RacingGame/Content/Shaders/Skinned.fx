

float4x4 viewProj         : ViewProjection;
float4x4 world            : World;

float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {0.65f, 0.65f, 0.39f}; // Normalized by app. FxComposer still uses inverted stuff

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor = {0.67f, 0.67f, 0.67f, 1.0f};

float4 diffuseColor = {1.0f, 1.0f, 1.0f, 1.0f};

// Texture and samplers
texture diffuseTexture;
sampler diffuseTextureSampler = sampler_state
{
    Texture = <diffuseTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
};


#define SKINNED_EFFECT_MAX_BONES   59

float4x3 Bones[SKINNED_EFFECT_MAX_BONES];

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    float4 pos : Position;
    float3 Normal   : NORMAL;
    float2 TexCoord : TEXCOORD0;
    int4   Indices  : BLENDINDICES0;
    float4 Weights  : BLENDWEIGHT0;
};

// vertex shader output structure
struct VertexOutput
{
    float4 pos          : POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float3 normal     : TEXCOORD1;
};

//----------------------------------------------------


void Skin(inout VertexInput vin)
{
    float4x3 skinning = 0;

    for (int i = 0; i < 4; i++)
    {
        skinning += Bones[vin.Indices[i]] * vin.Weights[i];
    }

    vin.pos.xyz = mul(vin.pos, skinning);
    vin.Normal = mul(vin.Normal, (float3x3)skinning);
}
float4 TransformPosition(float3 pos)
{
    return mul(mul(float4(pos.xyz, 1), world), viewProj);
}

float3 CalcNormalVector(float3 nor)
{
    return normalize(mul(nor, (float3x3)world));
}


// Vertex shader function
VertexOutput VS_Diffuse(VertexInput In)
{
    VertexOutput Out = (VertexOutput) 0; 
    Skin(In);
    
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.TexCoord;
	Out.normal = CalcNormalVector(In.Normal);
	
    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse(VertexOutput In) : COLOR
{
    // Grab texture data
    float4 diffuseTexture = tex2D(diffuseTextureSampler, In.diffTexCoord);
    
    float3 normalVector = normalize(CalcNormalVector(In.normal));
	
	float3 ld = mul(lightDir, (float3x3)world);
    // Compute the angle to the light
    float bump = saturate(dot(normalVector, ld));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    return diffuseTexture * ambDiffColor;
}

// Same for ps20 to show up in 3DS Max.
technique Diffuse20
{
    pass P0
    {
        VertexShader = compile vs_2_0 VS_Diffuse();
        PixelShader  = compile ps_2_0 PS_Diffuse();
    }
}

