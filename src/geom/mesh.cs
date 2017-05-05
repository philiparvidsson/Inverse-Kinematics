namespace InverseKinematics.Geom {

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Mesh {
    public VertexPositionColorNormal[] Verts { get; }
    public int[] Indices { get; }

    public Mesh(VertexPositionColorNormal[] verts, int[] indices) {
        Verts   = verts;
        Indices = indices;
    }

    public BoundingBox Aabb() {
        var posInf = float.PositiveInfinity;
        var negInf = float.NegativeInfinity;

        var min = new Vector3(posInf, posInf, posInf);
        var max = new Vector3(negInf, negInf, negInf);

        foreach (var v in Verts) {
            min.X = (float)Math.Min(min.X, v.Position.X);
            min.Y = (float)Math.Min(min.Y, v.Position.Y);
            min.Z = (float)Math.Min(min.Z, v.Position.Z);

            max.X = (float)Math.Max(max.X, v.Position.X);
            max.Y = (float)Math.Max(max.Y, v.Position.Y);
            max.Z = (float)Math.Max(max.Z, v.Position.Z);
        }

        return new BoundingBox(min, max);
    }

    public GpuMesh Gpu(GraphicsDevice gfxDevice) {
        return GpuMesh.FromMesh(gfxDevice, this);
    }

    public Mesh FlipTris() {
        FlipNormals();

        for (var i = 0; i < Indices.Length; i += 3) {
            var t = Indices[i+1];
            Indices[i+1] = Indices[i+2];
            Indices[i+2] = t;
        }

        return this;
    }

    public Mesh FlipNormals() {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Normal *= -1.0f;
        }

        return this;
    }

    public Mesh Color(Color color) {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Color = color;
        }

        return this;
    }

    public Mesh Transform(Matrix m) {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Position = Vector3.Transform(Verts[i].Position, m);
            var n = Vector4.Transform(new Vector4(Verts[i].Normal, 0.0f) , m);
            Verts[i].Normal   = new Vector3(n.X, n.Y, n.Z);
        }

        return this;
    }

    public Mesh Translate(Vector3 d) {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Position += d;
        }

        return this;
    }
}

}
