namespace InverseKinematics {

using System;
using System.Collections.Generic;

using Core;
using Geom;
using Impl;
using Impl.Gfx;
using Impl.Phys;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public sealed class CIKBone: EcsComponent {
    public IKBone Bone;
}

public class IKBoneChain {
    private readonly List<IKBone> mBones = new List<IKBone>();

    public Vector3 Target { get; set; }

    public void StepSolve(float dt) {
        for (var i = mBones.Count - 1; i >= 0; i--) {
            var p0 = (i > 0) ? mBones[i - 1].FinalPos() : mBones[i].Pos;
            var p1 = mBones[i].FinalPos();
            var t  = Target;

            var a = p1 - p0;
            var b = t  - p0;

            var bLength = b.Length();
            var aLength = a.Length();

            a.Normalize();
            b.Normalize();

            var theta = (float)Math.Acos(Vector3.Dot(a, b));
            var axis  = Vector3.Cross(a, b);



            //Console.WriteLine(axis + ", " + theta);

            if (bLength < aLength) {
                mBones[i].CompAngle += (MathHelper.ToRadians(10.0f) + theta) * ((aLength/bLength) - 1.0f);
            }

            mBones[i].CompAngle -= 20.0f*mBones[i].CompAngle*dt;


            var r = 0.0f;
            var qq = 1;
            for (var j = i; j < mBones.Count; j++) {
                r += mBones[j].CompAngle*qq;
                qq++;
            }
            mBones[i].Rot = Quaternion.Lerp(
                                mBones[i].Rot,
                                Quaternion.CreateFromAxisAngle(axis, (i+1)*0.4f-r),
                                dt);
        }
    }

    public void AddBone(float length, Color? color=null, Vector3? pos=null) {
        color = color ?? Color.Black;

        IKBone parent = null;

        if (mBones.Count > 0) {
            parent = mBones[mBones.Count - 1];
        }

        var bone = CreateBone(0.06f, length, color.Value);

        if (parent == null) {
            bone.Pos = pos.Value;
        }
        else {
            bone.Parent = parent;
        }

        mBones.Add(bone);
    }

    private IKBone CreateBone(float thickness, float length, Color color) {
        var bone = new IKBone(length, null);

        var p = Program.Inst;
        var g = p.GraphicsDevice;

        var mesh = MeshGen.Box(thickness, length, thickness)
                          .Translate(0.5f*length*Vector3.Up)
                          .Color(color);

        p.Scene.AddEntity(new EcsEntity(new CMesh   { Mesh = mesh.Gpu(g) },
                                        new CIKBone { Bone = bone },
                                        new CPos    { },
                                        // new CVel  { },
                                        new CRot { },
                                        new CAabb   { Aabb = mesh.Aabb() }));

        return bone;
    }
}

public class IKBone {
    public float CompAngle { get; set; }
    public IKBone Parent { get; set; }

    public float Length { get; set; }

    public Vector3 Pos { get; set; }

    public Quaternion Rot { get; set; } = Quaternion.Identity;

    public IKBone() {
    }

    public IKBone(float length, IKBone parent) {
        Length = length;
        Parent = parent;
    }

    public Vector3 FinalPos() {
        var pos = Pos;
        var rot = FinalRot();

        if (Parent != null) {
            pos += Parent.FinalPos();
        }

        var transf = Matrix.CreateFromQuaternion(rot);
        pos += Length * Vector3.Transform(Vector3.Up, transf);

        return pos;
    }

    public Quaternion FinalRot() {
        var rot = Rot;

        if (Parent != null) {
            rot *= Parent.FinalRot();
        }

        return rot;
    }
}

public sealed class IKSolver: EcsSystem {
    private readonly List<IKBoneChain> mIKChains = new List<IKBoneChain>();

    public IKBoneChain CreateBoneChain() {
        var ikbc = new IKBoneChain();

        mIKChains.Add(ikbc);

        return ikbc;
    }

    public override void Update(float t, float dt) {
        var p = Program.Inst;

        foreach (var ikbc in mIKChains) {
            ikbc.StepSolve(dt);
        }

        foreach (var e in p.Scene.GetEntities<CIKBone>()) {
            var ikBone = e.GetComponent<CIKBone>();
            var pos    = e.GetComponent<CPos>();
            var rot    = e.GetComponent<CRot>();

            if (pos != null) {
                if (ikBone.Bone.Parent != null) {
                    pos.Pos = ikBone.Bone.Parent.FinalPos();
                }
            }

            if (rot != null) {
                rot.Rot = ikBone.Bone.FinalRot();
            }

        }
    }
}

public sealed class IKDemo: Scene {
    IKBoneChain ikbc;
    CPos cp;
    public override void Init() {
        IKSolver ikSolver;
        World world;

        AddSystems(new FpsCounter(updatesPerSec: 10),
           world = new World(),
        ikSolver = new IKSolver(),
                   new Renderer());

        base.Init();

        world.Bounds = new BoundingBox(-5.0f*new Vector3(1.0f, 0.0f, 1.0f), 5.0f*Vector3.One);

        ikbc = ikSolver.CreateBoneChain();

        ikbc.AddBone(0.3f, new Color(1.0f, 0.0f, 0.0f), Vector3.Zero);
        for (var i = 0; i < 2; i++) {
            ikbc.AddBone(0.3f, new Color(0.0f, 1.0f, 0.0f));
            ikbc.AddBone(0.3f, new Color(1.0f, 1.0f, 0.0f));
        }

        ikbc.AddBone(0.3f, new Color(0.0f, 0.0f, 1.0f));

        ikbc.Target = new Vector3(-0.2f, 0.0f, 0.5f);

        var g = Program.Inst.GraphicsDevice;
        var box = MeshGen.Box(0.1f, 0.1f, 0.1f).Color(Color.Black);
        AddEntity(new EcsEntity(
                                new CMesh {
                                    Mesh = box.Gpu(g)
                                },
                                new CAabb{ Aabb=box.Aabb() },
                                new CVel{},
                                cp = new CPos {
                                    Pos = ikbc.Target
                                }));

        var floorTransform = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f))
                           * Matrix.CreateScale(10.0f*Vector3.One);
        var floor = MeshGen.Quad().Transform(floorTransform).Color(Color.White);
        AddMeshEntity(floor);
    }

    private void AddMeshEntity(Mesh mesh) {
        var g = Program.Inst.GraphicsDevice;
        AddEntity(new EcsEntity(new CMesh { Mesh = mesh.Gpu(g) }));
    }

    public override void Draw(float t, float dt) {
        var kb = Keyboard.GetState();

        Vector3 d = Vector3.Zero;
        if (kb.IsKeyDown(Keys.Left)) {
            d += Vector3.Left;
        }
        if (kb.IsKeyDown(Keys.Right)) {
            d += Vector3.Right;
        }
        if (kb.IsKeyDown(Keys.A)) {
            d += Vector3.Down;
        }
        if (kb.IsKeyDown(Keys.Q)) {
            d += Vector3.Up;
        }
        if (kb.IsKeyDown(Keys.Down)) {
            d += Vector3.Backward;
        }
        if (kb.IsKeyDown(Keys.Up)) {
            d += Vector3.Forward;
        }

        cp.Pos += d*dt;
        ikbc.Target = cp.Pos;

        base.Draw(t, dt);
    }
}

}
