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
    float4 pos : POSITION0;
};

struct VSOutput {
    float4 pos : POSITION0;
    float4 screenPos : TEXCOORD0;
};

void ps_main(in VSOutput x, out PSOutput r) {
    r.color = float4(0.0, 0.0, 0.0, 1.0);
}

void vs_main(in VSInput x, out VSOutput r) {
    float4 modelPos = mul(x.pos   , Model);
    float4 viewPos  = mul(modelPos, View );

    r.pos = mul(viewPos, Proj);
    r.screenPos = mul(viewPos, Proj);
}

technique Technique1 {
    pass Pass0 {
        PixelShader  = compile ps_3_0 ps_main();
        VertexShader = compile vs_3_0 vs_main();
    }
}
