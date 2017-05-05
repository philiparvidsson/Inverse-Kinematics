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
    // making a finite-state-machine of this class to also make it put balls into box lol
    public EcsEntity HeldBall { get; set; }
    public Vector3 DropPos1 { get; set; }
    public Vector3 DropPos2 { get; set; }

    internal readonly List<IKBone> mBones = new List<IKBone>();

    public Vector3 Target { get; set; }

    public EcsEntity BestBall { get; set; }

    public void Think() {
            BestBall = null;
            var minDist = 100000.0f;
            var p1 = mBones[0].Pos;

            foreach (var e in Program.Inst.Scene.GetEntities<CBall>()) {
                if (e.HasComponent<CIKIgnore>()) {
                    continue;
                }

                var p2 = e.GetComponent<CPos>().Pos;

                if ((p2 - p1).Length() < minDist) {
                    minDist = (p2 - p1).Length();
                    BestBall = e;
                }
            }

            if (HeldBall != null) {
                var sv = HeldBall.GetComponent<CSort>().SortVal;

                     if (sv == 0) Target = DropPos1;
                else if (sv == 1) Target = DropPos2;
            }
            else if (BestBall != null) {
                Target = BestBall.GetComponent<CPos>().Pos;
            }

    }

    public bool StepSolve(float dt) {
        var ret = false;

        for (var i = mBones.Count - 1; i >= 0; i--) {
            var p0 = (i > 0) ? mBones[i - 1].FinalPos() : mBones[i].Pos;
            var p1 = mBones[i].FinalPos();
            var t  = Target;

            var a = p1 - p0;
            var b = t  - p0;

            var bLength = b.Length();
            var aLength = a.Length();

            var cl = (t - p1).Length();
            if (cl < 0.1f) {
                ret |= (i == mBones.Count - 1);
            }

            a.Normalize();
            b.Normalize();

            var theta = (float)Math.Acos(Vector3.Dot(a, b));
            var axis  = Vector3.Cross(a, b);

            if (axis.Length() < 0.001f) {
                continue;
            }

            // Don't know why this gives worse result. Should already be normalized since a and b
            // are...?
            //axis.Normalize();



            //Console.WriteLine(axis + ", " + theta);

            if (bLength < aLength) {
                mBones[i].CompAngle += (MathHelper.ToRadians(10.0f) + theta) * ((aLength/bLength) - 1.0f);
            }

            //mBones[i].CompAngle -= 30.0f*mBones[i].CompAngle*dt;
            mBones[i].CompAngle *= 0.5f;


            var r = 0.0f;
            var qq = 1;
            for (var j = i; j < mBones.Count; j++) {
                r += mBones[j].CompAngle*qq;
                qq++;
            }

            mBones[i].Rot = Quaternion.Slerp(
                                mBones[i].Rot,
                                Quaternion.CreateFromAxisAngle(axis, (i+1)*0.4f-r),
                                dt);
        }

        return ret;
    }

    public void AddBone(float length, Color? color=null, Vector3? pos=null, float thickness=0.06f) {
        color = color ?? Color.Black;

        IKBone parent = null;

        if (mBones.Count > 0) {
            parent = mBones[mBones.Count - 1];
        }

        var bone = CreateBone(thickness, length, color.Value);

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
                                        new CShadow{}
                                        ));

        return bone;
    }
}

public class IKBone {
    public float CompAngle { get; set; }
    public IKBone Parent { get; set; }

    public float Length { get; set; }

    public Vector3 Pos { get; set; }

    public Quaternion BaseRot { get; set; } = Quaternion.Identity;
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
        var rot = BaseRot * Rot;

        if (Parent != null) {
            rot *= Parent.FinalRot();
        }

        return rot;
    }
}

public class CIKIgnore: EcsComponent {

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


            ikbc.Think();

            var first = true;
            for (var i = 0; i < 10; i++) {
                if (ikbc.StepSolve(0.1f*dt) && first) {
                    first = false;
                    if (ikbc.HeldBall != null && !ikbc.HeldBall.HasComponent<CVel>()) {
                        var rnd = new System.Random();
                        ikbc.HeldBall.AddComponent(new CVel { Vel = ((float)rnd.NextDouble()- 0.5f) * Vector3.One*0.5f });

                        ikbc.HeldBall = null;
                    }
                    else if (ikbc.BestBall != null) {
                        if (ikbc.HeldBall == null) {
                            ikbc.HeldBall = ikbc.BestBall;
                            ikbc.HeldBall.RemoveComponent<CVel>();
                            ikbc.HeldBall.AddComponent(new CIKIgnore());
                        }
                    }
                }
            }

            if (ikbc.HeldBall != null) {
                ikbc.HeldBall.GetComponent<CPos>().Pos = ikbc.mBones[ikbc.mBones.Count - 1].FinalPos();
            }
        }

        foreach (var e in p.Scene.GetEntities<CIKBone>()) {
            var ikBone = e.GetComponent<CIKBone>();
            var pos    = e.GetComponent<CPos>();
            var rot    = e.GetComponent<CRot>();

            if (pos != null) {
                if (ikBone.Bone.Parent != null) {
                    pos.Pos = ikBone.Bone.Parent.FinalPos();
                }
                else {
                    pos.Pos = ikBone.Bone.Pos;
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
    CVel cv;

    private void CreateIKChain(IKSolver ikSolver, Quaternion rot) {
        ikbc = ikSolver.CreateBoneChain();

        var t = 0.2f;
        ikbc.AddBone(0.3f, Color.Yellow, new Vector3(0.0f, 0.5f, 0.0f), t);
        for (var i = 0; i < 2; i++) {
            t *= 0.8f;
            ikbc.AddBone(0.3f, Color.Yellow, null, t);
            t *= 0.8f;
            ikbc.AddBone(0.3f, Color.Yellow, null, t);

        }

        ikbc.AddBone(0.3f, new Color(0.0f, 0.0f, 1.0f));

        ikbc.mBones[0].BaseRot = rot;

        ikbc.DropPos1 = new Vector3(-0.9f, 1.1f, 0.8f);
        ikbc.DropPos2 = new Vector3( 0.9f, 1.1f, 0.8f);
    }

    public override void Init() {
        IKSolver ikSolver;
        World world;

        AddSystems(new FpsCounter(updatesPerSec: 10),
           world = new World(),
        ikSolver = new IKSolver(),
                   new Renderer());

        base.Init();

        CreateIKChain(ikSolver, Quaternion.Identity);
        CreateIKChain(ikSolver, Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.ToRadians(30.0f)));
        CreateIKChain(ikSolver, Quaternion.CreateFromAxisAngle(Vector3.Backward, MathHelper.ToRadians(-30.0f)));

        InitScene(Vector3.Zero);

        world.Bounds = new BoundingBox(-5.0f*new Vector3(1.0f, 0.0f, 1.0f), 5.0f*Vector3.One);
    }

    int mNumSpawns = 0;
    public override void Draw(float t, float dt) {
        mSpawnTimer -= dt;
        if (mNumSpawns < 5*15 && mSpawnTimer < 0.0f) {
            SpawnBall();
            mSpawnTimer = 0.2f;
            mNumSpawns++;
        }

        base.Draw(t, dt);
    }


    public void CreateConveyor() {
        var g = Program.Inst.GraphicsDevice;

        var width = 0.6f;
        var length = 6.0f;
        var height = 0.1f;
        var thickness = 0.1f;
        var pos = new Vector3(2.0f, 0.5f, -1.0f);
        var color = new Color(0.2f, 0.2f, 0.2f);

        var box1Transform = Matrix.CreateScale(new Vector3(length, height, thickness))
                          * Matrix.CreateTranslation(pos + 0.5f*width*Vector3.Backward + 0.5f*height*Vector3.Up);
        var box1Mesh      = MeshGen.Box().Transform(box1Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb   { Aabb = box1Mesh.Aabb() },
                                new CMesh   { Mesh = box1Mesh.Gpu(g) }));

        var box2Transform = Matrix.CreateScale(new Vector3(thickness, height, width))
                          * Matrix.CreateTranslation(pos + 0.5f*length*Vector3.Right + 0.5f*height*Vector3.Up);
        var box2Mesh      = MeshGen.Box().Transform(box2Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box2Mesh.Aabb() },
                                new CMesh { Mesh = box2Mesh.Gpu(g) } ));

        var box3Transform = Matrix.CreateScale(new Vector3(length, height, thickness))
                          * Matrix.CreateTranslation(pos + 0.5f*width*Vector3.Forward + 0.5f*height*Vector3.Up);
        var box3Mesh      = MeshGen.Box().Transform(box3Transform).Color(color);
         AddEntity(new EcsEntity(new CAabb { Aabb = box3Mesh.Aabb() },
                                 new CMesh { Mesh = box3Mesh.Gpu(g) } ));

        var box4Transform = Matrix.CreateScale(new Vector3(thickness, height, width))
                          * Matrix.CreateTranslation(pos + 0.5f*length*Vector3.Left + 0.5f*height*Vector3.Up);
        var box4Mesh      = MeshGen.Box().Transform(box4Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box4Mesh.Aabb() },
                                new CMesh { Mesh = box4Mesh.Gpu(g) } ));

        var box5Transform = Matrix.CreateScale(new Vector3(length, pos.Y, width))
                          * Matrix.CreateTranslation(pos + (-0.5f*pos.Y + 0.01f)*Vector3.Up);
        var box5Mesh      = MeshGen.Box().Transform(box5Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box5Mesh.Aabb() },
                                new CMesh { Mesh = box5Mesh.Gpu(g) } ));
    }


    private void CreateContainer(Vector3 pos, Color color) {
        var g = Program.Inst.GraphicsDevice;

        var size = 0.8f;
        var height = 0.5f;
        var thickness = 0.1f;

        var box1Transform = Matrix.CreateScale(new Vector3(size, height, thickness))
                          * Matrix.CreateTranslation(pos + 0.5f*size*Vector3.Backward + 0.5f*height*Vector3.Up);
        var box1Mesh      = MeshGen.Box().Transform(box1Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb   { Aabb = box1Mesh.Aabb() },
                                new CMesh   { Mesh = box1Mesh.Gpu(g) }));

        var box2Transform = Matrix.CreateScale(new Vector3(thickness, height, size))
                          * Matrix.CreateTranslation(pos + 0.5f*size*Vector3.Right + 0.5f*height*Vector3.Up);
        var box2Mesh      = MeshGen.Box().Transform(box2Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box2Mesh.Aabb() },
                                new CMesh { Mesh = box2Mesh.Gpu(g) } ));

        var box3Transform = Matrix.CreateScale(new Vector3(size, height, thickness))
                          * Matrix.CreateTranslation(pos + 0.5f*size*Vector3.Forward + 0.5f*height*Vector3.Up);
        var box3Mesh      = MeshGen.Box().Transform(box3Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box3Mesh.Aabb() },
                                new CMesh { Mesh = box3Mesh.Gpu(g) } ));

        var box4Transform = Matrix.CreateScale(new Vector3(thickness, height, size))
                          * Matrix.CreateTranslation(pos + 0.5f*size*Vector3.Left + 0.5f*height*Vector3.Up);
        var box4Mesh      = MeshGen.Box().Transform(box4Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box4Mesh.Aabb() },
                                new CMesh { Mesh = box4Mesh.Gpu(g) } ));

        var box5Transform = Matrix.CreateScale(new Vector3(size, thickness, size))
                          * Matrix.CreateTranslation(pos + (-0.5f*thickness + 0.01f)*Vector3.Up);
        var box5Mesh      = MeshGen.Box().Transform(box5Transform).Color(color);
        AddEntity(new EcsEntity(new CAabb { Aabb = box5Mesh.Aabb() },
                                new CMesh { Mesh = box5Mesh.Gpu(g) } ));
    }

    private void InitScene(Vector3 containerPos) {
        var g = Program.Inst.GraphicsDevice;

        var roomSize = 10.0f;

        var floorColor     = Color.DarkGray;
        var floorTransform = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f))
                           * Matrix.CreateScale(roomSize*Vector3.One);
        var floorMesh      = MeshGen.Quad().Transform(floorTransform).Color(floorColor);
        AddEntity(new EcsEntity(new CMesh { Mesh = floorMesh.Gpu(g) }));

        var roomColor     = Color.White;
        var roomTransform = Matrix.CreateScale(10.0f*Vector3.One);
        var roomMesh      = MeshGen.Box().FlipNormals().Transform(roomTransform).Color(roomColor);
        AddEntity(new EcsEntity(new CMesh { Mesh = roomMesh.Gpu(g) }));

        var stoolColor     = Color.Blue;
        var stoolTransform = Matrix.CreateScale(0.5f*Vector3.One) * Matrix.CreateTranslation(0.25f*Vector3.Up);
        var stoolMesh      = MeshGen.Box(0.6f, 1.0f, 0.6f).FlipNormals().Transform(stoolTransform).Color(stoolColor);
        AddEntity(new EcsEntity(new CAabb { Aabb = stoolMesh.Aabb() },
                                new CMesh { Mesh = stoolMesh.Gpu(g) }));

        CreateContainer(new Vector3(-0.9f, 0.0f, 1.0f), Color.Red);
        CreateContainer(new Vector3( 0.9f, 0.0f, 1.0f), Color.White);
        CreateConveyor();
    }

    private static GpuMesh[] sBallMeshes;
    private static BoundingBox sBallAabb;
    private float mSpawnTimer;

    private void SpawnBall() {
        if (sBallMeshes == null) {

            var cols = new [] {
                new Color(1.0f, 0.0f, 0.0f),
                new Color(1.0f, 1.0f, 1.0f),
            };

            var ballMeshes = new List<GpuMesh>();

            for (var i = 0; i < cols.Length; i++) {
                var g = Program.Inst.GraphicsDevice;
                var m = MeshGen.Sphere(0.1f).Color(cols[i % cols.Length]);
                ballMeshes.Add(m.Gpu(g));
                sBallAabb = m.Aabb();
            }

            sBallMeshes = ballMeshes.ToArray();
        }

        var rnd = new Random();

        var vx = -3.0f - (float)rnd.NextDouble()*1.0f;
        var vy = (float)rnd.NextDouble() - 1.4f;
        var vz = ((float)rnd.NextDouble() - 0.5f)*0.5f;

        var num = rnd.Next(sBallMeshes.Length);

        Program.Inst.Scene.AddEntity(new EcsEntity(
                                new CMesh {
                                    Mesh = sBallMeshes[num]
                                },
                                new CAabb{ Aabb=sBallAabb },
                                new CBall { Radius = 0.1f },
                                new CVel{ Vel = new Vector3(vx, vy, vz) },
                                new CShadow{},
                                new CSort { SortVal = 1-num },
                                new CPos {
                                    Pos = new Vector3(4.8f, 0.7f, -1.0f)
                                }));

    }

}

}
