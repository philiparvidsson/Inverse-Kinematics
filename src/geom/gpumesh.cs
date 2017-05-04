namespace InverseKinematics.Geom {

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class GpuMesh {
    public VertexBuffer Vbo { get; }

    public IndexBuffer Ibo { get; }

    private GpuMesh(VertexBuffer vbo, IndexBuffer ibo) {
        Vbo = vbo;
        Ibo = ibo;
    }

    public static GpuMesh FromMesh(GraphicsDevice gfxDevice, Mesh mesh) {
        var vbo = new VertexBuffer(gfxDevice,
                                   VertexPositionColorNormal.VertexDeclaration,
                                   mesh.Verts.Length,
                                   BufferUsage.None);

        vbo.SetData<VertexPositionColorNormal>(mesh.Verts);

        var ibo = new IndexBuffer(gfxDevice, typeof (int), mesh.Indices.Length, BufferUsage.None);

        ibo.SetData<int>(mesh.Indices);

        return new GpuMesh(vbo, ibo);
    }
}

}
