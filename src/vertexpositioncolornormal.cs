namespace InverseKinematics {

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

[DataContract]
[StructLayout(LayoutKind.Sequential, Pack=1)]
public struct VertexPositionColorNormal : IVertexType {
    [DataMember]
    public Vector3 Position;

    [DataMember]
    public Color Color;

    [DataMember]
    public Vector3 Normal;

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new [] {
        new VertexElement(0 , VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color  , VertexElementUsage.Color   , 0),
        new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal  , 0)
    });

    public VertexPositionColorNormal(Vector3 position, Color color, Vector3 normal) {
        Position = position;
        Color    = color;
        Normal   = normal;
    }

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}

}
