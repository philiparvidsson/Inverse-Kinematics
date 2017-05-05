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

    public Mesh Color(Color color) {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Color = color;
        }

        return this;
    }

    public Mesh Transform(Matrix m) {
        for (var i = 0; i < Verts.Length; i++) {
            Verts[i].Position = Vector3.Transform(Verts[i].Position, m);
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
