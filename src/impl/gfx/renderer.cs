namespace InverseKinematics.Impl.Gfx {

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Renderer: EcsSystem {
    public Effect DefEffect { get; set; }

    private RenderTarget2D mShadowMap;

    private SpriteBatch mSB;

    public Camera Cam { get; set; } = new Camera();

    public override void Init() {
        /*Program.Inst.GraphicsDevice.RasterizerState = new RasterizerState {
            CullMode = CullMode.None
        };*/

        Cam.Proj = Matrix.CreatePerspectiveFieldOfView(fieldOfView       : MathHelper.ToRadians(80.0f),
                                                       aspectRatio       : Program.Inst.GraphicsDevice.Viewport.AspectRatio,
                                                       nearPlaneDistance : 0.1f,
                                                       farPlaneDistance  : 100.0f);

        Cam.Pos = new Vector3(0.9f, 1.6f, 2.2f);
        Cam.Target = new Vector3(0.0f, 0.6f, 0.3f);


        mShadowMap = new RenderTarget2D(Program.Inst.GraphicsDevice, Program.Inst.GfxDevMgr.PreferredBackBufferWidth, Program.Inst.GfxDevMgr.PreferredBackBufferHeight);

        mSB = new SpriteBatch(Program.Inst.GraphicsDevice);
    }

    public override void Draw(float t, float dt) {
        base.Draw(t, dt);


        var p = Program.Inst;
        var g = p.GraphicsDevice;

        g.BlendState = BlendState.AlphaBlend;

        g.Clear(Color.CornflowerBlue);

        var w = Program.Inst.GfxDevMgr.PreferredBackBufferWidth;
        var h = Program.Inst.GfxDevMgr.PreferredBackBufferHeight;

        DefEffect = Program.Inst.Content.Load<Effect>("adsmat");
        DefEffect.Parameters["Proj"].SetValue(Cam.Proj);
        DefEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(Cam.Pos, Cam.Target, Vector3.Up));

        foreach (var e in p.Scene.GetEntities<CMesh>()) {
            var mesh = e.GetComponent<CMesh>();

            g.SetVertexBuffer(mesh.Mesh.Vbo);
            g.Indices = mesh.Mesh.Ibo;

            var model = Matrix.Identity;

            var rot = e.GetComponent<CRot>();
            if (rot != null) {
                model *= Matrix.CreateFromQuaternion(rot.Rot);
            }

            var pos = e.GetComponent<CPos>();
            if (pos != null) {
                model *= Matrix.CreateTranslation(pos.Pos);
            }

            DefEffect.Parameters["Model"].SetValue(model);

            foreach (var pass in DefEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            g.DrawIndexedPrimitives(primitiveType  : PrimitiveType.TriangleList,
                                    baseVertex     : 0,
                                    startIndex     : 0,
                                    primitiveCount : mesh.Mesh.Ibo.IndexCount / 3);
        }

        DefEffect = Program.Inst.Content.Load<Effect>("shadowmat");
        DefEffect.Parameters["Proj"].SetValue(Cam.Proj);
        DefEffect.Parameters["View"].SetValue(Matrix.CreateLookAt(Cam.Pos, Cam.Target, Vector3.Up));

        foreach (var e in p.Scene.GetEntities<CMesh>()) {
            var mesh = e.GetComponent<CMesh>();
            if (!e.HasComponent<CShadow>()) {
                continue;
            }

            var shadow = e.GetComponent<CShadow>();

            g.SetVertexBuffer(mesh.Mesh.Vbo);
            g.Indices = mesh.Mesh.Ibo;

            var model = Matrix.Identity;

            var rot = e.GetComponent<CRot>();
            if (rot != null) {
                model *= Matrix.CreateFromQuaternion(rot.Rot);
            }

            model *= Matrix.CreateScale(new Vector3(1.1f, 0.0f, 1.1f));

            var pos = e.GetComponent<CPos>();
            if (pos != null) {
                model *= Matrix.CreateTranslation(pos.Pos);
            }

            model *= Matrix.CreateScale(new Vector3(1.0f, 0.0f, 1.0f))
                   * Matrix.CreateTranslation(0.0f, shadow.Y, 0.0f);

            DefEffect.Parameters["Model"].SetValue(model);

            foreach (var pass in DefEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }
;

            g.DrawIndexedPrimitives(primitiveType  : PrimitiveType.TriangleList,
                                    baseVertex     : 0,
                                    startIndex     : 0,
                                    primitiveCount : mesh.Mesh.Ibo.IndexCount / 3);
        }

    }

}

}
