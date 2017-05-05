namespace InverseKinematics.Impl.Gfx {

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Renderer: EcsSystem {
    public BasicEffect DefEffect { get; set; }

    public Camera Cam { get; set; } = new Camera();

    public override void Init() {
        Program.Inst.GraphicsDevice.RasterizerState = new RasterizerState {
            CullMode = CullMode.None
        };

        DefEffect = new BasicEffect(Program.Inst.GraphicsDevice);

        DefEffect.VertexColorEnabled = true;
        DefEffect.EnableDefaultLighting();

        Cam.Proj = Matrix.CreatePerspectiveFieldOfView(fieldOfView       : MathHelper.ToRadians(60.0f),
                                                       aspectRatio       : Program.Inst.GraphicsDevice.Viewport.AspectRatio,
                                                       nearPlaneDistance : 0.1f,
                                                       farPlaneDistance  : 100.0f);
    }

    public override void Draw(float t, float dt) {
        base.Draw(t, dt);


        var p = Program.Inst;
        var g = p.GraphicsDevice;

        g.Clear(Color.CornflowerBlue);

        DefEffect.View = Matrix.CreateLookAt(new Vector3(0.9f, 1.6f, 2.2f), new Vector3(0.0f, 0.0f, 0.3f), Vector3.Up);

        foreach (var e in p.Scene.GetEntities<CMesh>()) {
            var mesh = e.GetComponent<CMesh>();

            g.SetVertexBuffer(mesh.Mesh.Vbo);
            g.Indices = mesh.Mesh.Ibo;

            DefEffect.DiffuseColor = Vector3.One;
            DefEffect.SpecularColor = Vector3.One * 0.3f;
            DefEffect.World = Matrix.Identity;
            DefEffect.Projection = Cam.Proj;

            var rot = e.GetComponent<CRot>();
            if (rot != null) {
                DefEffect.World *= Matrix.CreateFromQuaternion(rot.Rot);
            }

            var pos = e.GetComponent<CPos>();
            if (pos != null) {
                DefEffect.World *= Matrix.CreateTranslation(pos.Pos);
            }

            foreach (var pass in DefEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }

            g.DrawIndexedPrimitives(primitiveType  : PrimitiveType.TriangleList,
                                    baseVertex     : 0,
                                    startIndex     : 0,
                                    primitiveCount : mesh.Mesh.Ibo.IndexCount / 3);
        }

        foreach (var e in p.Scene.GetEntities<CMesh>()) {
            var mesh = e.GetComponent<CMesh>();
            if (!e.HasComponent<CShadow>()) {
                continue;
            }

            g.SetVertexBuffer(mesh.Mesh.Vbo);
            g.Indices = mesh.Mesh.Ibo;

            DefEffect.DiffuseColor = Vector3.Zero;
            DefEffect.World = Matrix.Identity;
            DefEffect.Projection = Cam.Proj;

            var rot = e.GetComponent<CRot>();
            if (rot != null) {
                DefEffect.World *= Matrix.CreateFromQuaternion(rot.Rot);
            }

            var pos = e.GetComponent<CPos>();
            if (pos != null) {
                DefEffect.World *= Matrix.CreateTranslation(pos.Pos);
            }

            DefEffect.World *= Matrix.CreateScale(new Vector3(1.0f, 0.0f, 1.0f))
                             * Matrix.CreateTranslation(0.0f, 0.02f, 0.0f);

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
