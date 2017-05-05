uniform extern float4x4 Model;
uniform extern float4x4 Proj;
uniform extern float4x4 View;

static const int NUM_LIGHTS = 3;

static const float4 Lights[NUM_LIGHTS] = {
    float4(-2.0, 2.5, 1.0,  1.0),
    float4( 2.0, 2.0, 0.0,  1.0),
    float4( 0.0, 2.0, 0.5,  1.0),
};

static const float3 CamPos = float3(0.9, 1.6, 2.2);

struct PSOutput {
    float4 color : SV_TARGET;
};

struct VSInput {
    float4 pos      : POSITION0;
    float2 texCoord : TEXCOORD0;
    float4 color    : COLOR;
    float3 normal   : NORMAL0;
};

struct VSOutput {
    float4 screenPos : POSITION0;
    float3 worldPos  : TEXCOORD1;
    float2 texCoord  : TEXCOORD0;
    float3 normal    : TEXCOORD2;
    float4 color     : COLOR;
};

void ps_main(in VSOutput x, out PSOutput r) {
    float amb = 0.2;

    r.color = float4(amb*x.color.rgb, 1.0);

    for (int i = 0; i < NUM_LIGHTS; i++) {
        float3 l = normalize(Lights[i].xyz - x.worldPos);
        float3 n = normalize(x.normal);
        float3 rn = reflect(-l, n);
        float3 v = normalize(CamPos - x.worldPos);
        float3 vl = length(CamPos - x.worldPos);
        float  k = 50.0;
        float  d = length(Lights[i].xyz - x.worldPos);
        float  j = Lights[i].w/(d*d);

        float3 col = x.color.rgb;
        float  dif = max(0.0, dot(l, n));
        float  spe = pow(max(0.0, dot(rn, v)), k);

        float3 c = 0.0;

        c += dif*col;
        c += spe;
        c *= j;
        c = sqrt(c);

        r.color += float4(c, 1.0);
    }
}

void vs_main(in VSInput x, out VSOutput r) {
    float4 modelPos = mul(x.pos   , Model);
    float4 viewPos  = mul(modelPos, View );

    r.screenPos = mul(viewPos, Proj);
    r.worldPos  = modelPos.xyz;
    r.texCoord  = x.texCoord;
    r.normal    = mul(float4(x.normal.xyz, 0.0), Model).xyz;
    r.color     = x.color;
}

technique Technique1 {
    pass Pass0 {
        PixelShader  = compile ps_3_0 ps_main();
        VertexShader = compile vs_3_0 vs_main();
    }
}
