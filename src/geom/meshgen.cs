namespace InverseKinematics.Geom {

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public static class MeshGen {
    public static Mesh Combine(params Mesh[] meshes) {
        var verts   = new List<VertexPositionColorNormal>();
        var indices = new List<int>();

        foreach (var mesh in meshes) {
            var n = verts.Count;
            verts.AddRange(mesh.Verts);
            foreach (var i in mesh.Indices) {
                indices.Add(i + n);
            }
        }

        return new Mesh(verts.ToArray(), indices.ToArray());
    }

    public static Mesh Disc(float radius, float height, int numSegments=8, bool flipNormals=false) {
        var h = 0.5f*height;
        var r = radius;

        Func<Vector3, Vector3, VertexPositionColorNormal> v =
            (p, n) => new VertexPositionColorNormal { Position = p, Normal = n };

        var tops  = new List<Mesh>();
        var sides = new List<Mesh>();
        var bottoms = new List<Mesh>();

        for (var i = 0; i < numSegments; i++) {
            var side = Quad(1.0f, 2.0f*h);

            var a = 2.0f*Math.PI*(float)i/numSegments;
            var b = 2.0f*Math.PI*(float)((i + 1) % numSegments)/numSegments;

            var cosa = (float)Math.Cos(a);
            var sina = (float)Math.Sin(a);
            var cosb = (float)Math.Cos(b);
            var sinb = (float)Math.Sin(b) ;

            var verts = new [] {
                v(new Vector3(0.0f  , h,   0.0f), Vector3.Up),
                v(new Vector3(r*cosa, h, r*sina), Vector3.Up),
                v(new Vector3(r*cosb, h, r*sinb), Vector3.Up),
            };

            var indices = new [] {
                0, 1, 2,
                2, 3, 0
            };

            var top = new Mesh(verts, indices);

            side.Verts[0].Position.X = side.Verts[3].Position.X =  r*cosa;
            side.Verts[0].Position.Z = side.Verts[3].Position.Z = -r*sina;
            side.Verts[0].Normal.X = side.Verts[3].Normal.X =  cosa;
            side.Verts[0].Normal.Z = side.Verts[3].Normal.Z = -sina;

            side.Verts[1].Position.X = side.Verts[2].Position.X =  r*cosb;
            side.Verts[1].Position.Z = side.Verts[2].Position.Z = -r*sinb;
            side.Verts[1].Normal.X = side.Verts[2].Normal.X =  cosb;
            side.Verts[1].Normal.Z = side.Verts[2].Normal.Z = -sinb;

            verts = new [] {
                v(new Vector3(0.0f  , -h,   0.0f), Vector3.Down),
                v(new Vector3(r*cosb, -h, r*sinb), Vector3.Down),
                v(new Vector3(r*cosa, -h, r*sina), Vector3.Down),
            };

            indices = new [] {
                0, 1, 2,
                2, 3, 0
            };

            var bottom = new Mesh(verts, indices);

            tops.Add(top);
            sides.Add(side);
            bottoms.Add(bottom);
        }

        return Combine(Combine(tops.ToArray()),
                       Combine(sides.ToArray()),
                       Combine(bottoms.ToArray()));
    }

    public static Mesh Quad(float width=1.0f, float height=1.0f, bool flipNormals=false) {
        var w = 0.5f*width;
        var h = 0.5f*height;

        Func<Vector3, Vector3, VertexPositionColorNormal> v =
            (p, n) => new VertexPositionColorNormal { Position = p, Normal = n };

        var verts = new [] {
            v(new Vector3(-w,  h, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            v(new Vector3( w,  h, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            v(new Vector3( w, -h, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
            v(new Vector3(-w, -h, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
        };

        var indices = new [] {
            0, 1, 2,
            2, 3, 0
        };

        if (flipNormals) {
            for (var i = 0; i < verts.Length; i++) {
                verts[i].Normal *= -1.0f;
            }
        }

        return new Mesh(verts, indices);
    }

    public static Mesh Box(float width      = 1.0f,
                           float height     = 1.0f,
                           float depth      = 1.0f,
                           bool flipNormals = false)
    {
        var w = width;
        var h = height;
        var d = depth;

        var top    = Quad(w, d);
        var front  = Quad(w, h);
        var right  = Quad(d, h);
        var back   = Quad(w, h);
        var left   = Quad(d, h);
        var bottom = Quad(w, d);

        for (var i = 0; i < 4; i++) {
            top.Verts[i].Position.Z = -top.Verts[i].Position.Y;
            top.Verts[i].Position.Y = 0.5f*h;
            top.Verts[i].Normal = Vector3.Up;

            front.Verts[i].Position.Z = 0.5f*d;
            front.Verts[i].Normal = Vector3.Backward;

            right.Verts[i].Position.Z = -right.Verts[i].Position.X;
            right.Verts[i].Position.X = 0.5f*w;
            right.Verts[i].Normal = Vector3.Right;

            back.Verts[i].Position.X = -back.Verts[i].Position.X;
            back.Verts[i].Position.Z = -0.5f*d;
            back.Verts[i].Normal = Vector3.Forward;

            left.Verts[i].Position.Z = left.Verts[i].Position.X;
            left.Verts[i].Position.X = -0.5f*w;
            left.Verts[i].Normal = Vector3.Left;

            bottom.Verts[i].Position.Z = bottom.Verts[i].Position.Y;
            bottom.Verts[i].Position.Y = -0.5f*h;
            bottom.Verts[i].Normal = Vector3.Down;
        }

        var box = Combine(top, front, right, back, left, bottom);

        if (flipNormals) {
            for (var i = 0; i < box.Verts.Length; i++) {
                box.Verts[i].Normal *= -1.0f;
            }
        }

        return box;
    }
}

}
