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

    public static Mesh Sphere(float radius, int m=24, int n=24) {
        // http://wiki.unity3d.com/index.php/ProceduralPrimitives

        var r = radius;

        var verts = new VertexPositionColorNormal[(m+1) * (n+2)];

        verts[0] = new VertexPositionColorNormal{ Position = r*Vector3.Up };

        for (var i = 0; i < m; i++) {
            var a = (float)Math.PI * (float)(i+1)/(m+1);
            var cosa = (float)Math.Cos(a);
            var sina = (float)Math.Sin(a);

            for (var j = 0; j <= n; j++) {
                var b = 2.0f*(float)Math.PI * (float)(j == n ? 0 : j) / n;
                var cosb = (float)Math.Cos(b);
                var sinb = (float)Math.Sin(b);

                var pos = new Vector3(sina * cosb, cosa, sina * sinb) * r;
                verts[j + i * (n+1) + 1] = new VertexPositionColorNormal {
                    Position = pos,
                };
            }
        }

        verts[verts.Length - 1] = new VertexPositionColorNormal{ Position = r*Vector3.Down };

        for (var i = 0; i < verts.Length; i++) {
            var norm = verts[i].Position;
            norm.Normalize();
            verts[i].Normal = norm;
        }

        var numTris = 2*verts.Length;
        var numIndices = 3*numTris;

        var indices = new int[numIndices];

        var idx = 0;
        for (var j = 0; j < n; j++) {
            indices[idx++] = j + 1;
            indices[idx++] = j + 2;
            indices[idx++] = 0;
        }

        for (var i = 0; i < m - 1; i++) {
            for (var j = 0; j < n; j++) {
                var c = j + i * (n+1) + 1;
                var x = c + n + 1;

                indices[idx++] = c;
                indices[idx++] = x + 1;
                indices[idx++] = c + 1;
                indices[idx++] = c;
                indices[idx++] = x;
                indices[idx++] = x + 1;
            }
        }

        for (var j = 0; j < n; j++) {
            indices[idx++] = verts.Length - 1;
            indices[idx++] = verts.Length - (j + 2) - 1;
            indices[idx++] = verts.Length - (j + 1) - 1;
        }

        return new Mesh(verts, indices);
    }

    public static Mesh Bone(float thickness, float length, float ratio=0.15f) {
        var t = 0.5f*thickness;
        var l = length;
        var r = ratio*l;

        var verts = new List<VertexPositionColorNormal>();
        var indices = new List<int>();

        Action<Vector3, Vector3, Vector3> tri =
            (p0, p1, p2) => {
                var n = Vector3.Cross(p1 - p0, p2 - p1);
                verts.Add(new VertexPositionColorNormal { Position = p0, Normal = n });
                verts.Add(new VertexPositionColorNormal { Position = p1, Normal = n });
                verts.Add(new VertexPositionColorNormal { Position = p2, Normal = n });
                indices.Add(indices.Count);
                indices.Add(indices.Count);
                indices.Add(indices.Count);
            };

        tri(Vector3.Zero, new Vector3(-t, r,  t), new Vector3( t, r,  t));
        tri(Vector3.Zero, new Vector3( t, r,  t), new Vector3( t, r, -t));
        tri(Vector3.Zero, new Vector3( t, r, -t), new Vector3(-t, r, -t));
        tri(Vector3.Zero, new Vector3(-t, r, -t), new Vector3(-t, r,  t));

        var p = l*Vector3.Up;
        tri(p, new Vector3( t, r,  t), new Vector3(-t, r,  t));
        tri(p, new Vector3( t, r, -t), new Vector3( t, r,  t));
        tri(p, new Vector3(-t, r, -t), new Vector3( t, r, -t));
        tri(p, new Vector3(-t, r,  t), new Vector3(-t, r, -t));

        return new Mesh(verts.ToArray(), indices.ToArray());
    }

    public static Mesh Disc(float radius, float height, int numSegments=32, bool flipNormals=false) {
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
