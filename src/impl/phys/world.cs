namespace InverseKinematics.Impl.Phys {

using Core;

using Microsoft.Xna.Framework;

public sealed class World: EcsSystem {

    public BoundingBox Bounds { get; set; } = new BoundingBox(-1.0f*Vector3.One, 1.0f*Vector3.One);

    public Vector3 Gravity { get; set; } = new Vector3(0.0f, -9.82f, 0.0f);

    public override void Update(float t, float dt) {
        base.Update(t, dt);

        foreach (var e in Program.Inst.Scene.GetEntities<CVel>()) {

            var pos = e.GetComponent<CPos>();
            var vel = e.GetComponent<CVel>();

            vel.Vel += dt*Gravity;

            pos.Pos += dt*vel.Vel;
            System.Console.WriteLine(pos.Pos);

            var aabb = e.GetComponent<CAabb>();
            if (aabb != null) {
                var wsaabb = new BoundingBox(pos.Pos + aabb.Aabb.Min, pos.Pos + aabb.Aabb.Max);

                if (wsaabb.Min.X < Bounds.Min.X) {
                    pos.Pos.X = Bounds.Min.X - aabb.Aabb.Min.X;
                    vel.Vel.X *= -1.0f;
                }

                if (wsaabb.Max.X > Bounds.Max.X) {
                    pos.Pos.X = Bounds.Max.X - aabb.Aabb.Max.X;
                    vel.Vel.X *= -1.0f;
                }

                if (wsaabb.Min.Y < Bounds.Min.Y) {
                    pos.Pos.Y = Bounds.Min.Y - aabb.Aabb.Min.Y;
                    vel.Vel.Y *= -1.0f;
                }

                if (wsaabb.Max.Y > Bounds.Max.Y) {
                    pos.Pos.Y = Bounds.Max.Y - aabb.Aabb.Max.Y;
                    vel.Vel.Y *= -1.0f;
                }

                if (wsaabb.Min.Z < Bounds.Min.Z) {
                    pos.Pos.Z = Bounds.Min.Z - aabb.Aabb.Min.Z;
                    vel.Vel.Z *= -1.0f;
                }

                if (wsaabb.Max.Z > Bounds.Max.Z) {
                    pos.Pos.Z = Bounds.Max.Z - aabb.Aabb.Max.Z;
                    vel.Vel.Z *= -1.0f;
                }
            }
        }
    }

}

}
