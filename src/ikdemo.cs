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
            var t  = Target;
            var p0 = (i > 0) ? mBones[i - 1].FinalPos() : mBones[i].Pos;
            var p1 = mBones[i].FinalPos();

            var a = p1 - p0;
            var b = t  - p0;

            a.Normalize();
            b.Normalize();

            var theta = (float)Math.Acos(Vector3.Dot(a, b));
            var axis  = Vector3.Cross(a, b);

            mBones[i].Rot = Quaternion.Slerp(mBones[i].Rot,
                                             Quaternion.CreateFromAxisAngle(axis, (i+1)*0.2f),
                                             dt);
        }
    }

    public void AddBone(float length, Color? color=null, Vector3? pos=null) {
        color = color ?? Color.Black;

        IKBone parent = null;

        if (mBones.Count > 0) {
            parent = mBones[mBones.Count - 1];
        }

        var bone = CreateBone(0.04f, length, color.Value);

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

        AddSystems(new FpsCounter(updatesPerSec: 10),
                   new World(),
        ikSolver = new IKSolver(),
                   new Renderer());

        base.Init();

        ikbc = ikSolver.CreateBoneChain();

        ikbc.AddBone(0.1f, Color.Red, Vector3.Zero);
        ikbc.AddBone(0.1f, Color.Green);
        ikbc.AddBone(0.1f, Color.Blue);
        ikbc.AddBone(0.1f, Color.Yellow);
        ikbc.AddBone(0.1f, Color.Green);
        ikbc.AddBone(0.1f, Color.Blue);
        ikbc.AddBone(0.1f, Color.Yellow);

        ikbc.Target = new Vector3(-0.2f, 0.0f, 0.5f);

        var g = Program.Inst.GraphicsDevice;
        AddEntity(new EcsEntity(
                                new CMesh {
                                    Mesh = MeshGen.Box(0.1f, 0.1f, 0.1f).Color(Color.Black).Gpu(g)
                                },
                                cp = new CPos {
                                    Pos = ikbc.Target
                                }));
    }

    public override void Draw(float t, float dt) {
        var mouse = Mouse.GetState();

        ikbc.Target = new Vector3(mouse.X, 0.0f, mouse.Y)*0.01f;
        cp.Pos = ikbc.Target;

        base.Draw(t, dt);
    }
}

}
