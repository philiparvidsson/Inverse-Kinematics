namespace InverseKinematics.Impl.Phys {

using System;

using Core;
using Impl.Phys;

using Microsoft.Xna.Framework;

public sealed class World: EcsSystem {

    public BoundingBox Bounds { get; set; } = new BoundingBox(-1.0f*Vector3.One, 1.0f*Vector3.One);

    public Vector3 Gravity { get; set; } = new Vector3(0.0f, -9.82f, 0.0f);

    public override void Update(float t, float dt) {
        base.Update(t, dt);

        var restitutionCoeff = 0.5f;

        foreach (var e in Program.Inst.Scene.GetEntities<CVel>()) {

            var pos = e.GetComponent<CPos>();
            var vel = e.GetComponent<CVel>();

            if (!e.HasComponent<CIKIgnore>()) {
                if (pos.Pos.Y > 0.3f) {
                    vel.Vel += new Vector3(-0.05f, 0.0f, 0.001f)*dt;
                }
                else {
                    var r = (Vector3.Zero - pos.Pos) * new Vector3(1.0f, 0.0f, 1.0f);
                    r.Normalize();
                    vel.Vel += 0.02f*r*dt;
                }
            }

            vel.Vel += dt*Gravity;
            vel.Vel -= 0.5f*vel.Vel*dt;


            pos.Pos += dt*vel.Vel;
            //System.Console.WriteLine(pos.Pos);

            var aabb = e.GetComponent<CAabb>();
            if (aabb != null) {
                var wsaabb = new BoundingBox(pos.Pos + aabb.Aabb.Min, pos.Pos + aabb.Aabb.Max);

                if (wsaabb.Min.X < Bounds.Min.X) {
                    pos.Pos.X = Bounds.Min.X - aabb.Aabb.Min.X;
                    vel.Vel.X *= -restitutionCoeff;
                }

                if (wsaabb.Max.X > Bounds.Max.X) {
                    pos.Pos.X = Bounds.Max.X - aabb.Aabb.Max.X;
                    vel.Vel.X *= -restitutionCoeff;
                }

                if (wsaabb.Min.Y < Bounds.Min.Y) {
                    pos.Pos.Y = Bounds.Min.Y - aabb.Aabb.Min.Y;
                    vel.Vel.Y *= -restitutionCoeff;
                }

                if (wsaabb.Max.Y > Bounds.Max.Y) {
                    pos.Pos.Y = Bounds.Max.Y - aabb.Aabb.Max.Y;
                    vel.Vel.Y *= -restitutionCoeff;
                }

                if (wsaabb.Min.Z < Bounds.Min.Z) {
                    pos.Pos.Z = Bounds.Min.Z - aabb.Aabb.Min.Z;
                    vel.Vel.Z *= -restitutionCoeff;
                }

                if (wsaabb.Max.Z > Bounds.Max.Z) {
                    pos.Pos.Z = Bounds.Max.Z - aabb.Aabb.Max.Z;
                    vel.Vel.Z *= -restitutionCoeff;
                }
            }


            var ball1 = e.GetComponent<CBall>();

            if (ball1 != null) {
                foreach (var e2 in Program.Inst.Scene.GetEntities<CBall>()) {
                    if (e2.ID <= e.ID) {
                        continue;
                    }

                    var ball2 = e2.GetComponent<CBall>();
                    var p1 = pos;
                    var p2 = e2.GetComponent<CPos>();
                    var vel2 = e2.GetComponent<CVel>();

                    if (vel2 == null) {
                        continue;
                    }

                    var minDist = ball1.Radius + ball2.Radius;

                    var n = p1.Pos - p2.Pos;

                    if (n.LengthSquared() >= minDist*minDist) {
                        // Not colliding.
                        continue;
                    }

                    var d = n.Length();
                    n.Normalize();

                    var i1 = Vector3.Dot(vel.Vel, n);
                    var i2 = Vector3.Dot(vel2.Vel, n);

                    if (i1 > 0.0f && i2 < 0.0f) {
                        // Moving away from each other, so don't bother with collision.
                        // TODO: We could normalize n after this check, for better performance.
                        continue;
                    }

                    var p  = (2.0f*(i2 - i1))*0.5f;

                    d = (minDist - d)*0.5f; // Mass adjusted penetration distance

                    p1.Pos += n*d*1.0f;
                    vel.Vel += n*p*1.0f;

                    p2.Pos -= n*d*1.0f;
                    vel2.Vel -= n*p*1.0f;

                    var c = 0.5f*(p1.Pos + p2.Pos);


                }

                foreach (var e2 in Program.Inst.Scene.GetEntities<CAabb>()) {
                    if (e2.ID == e.ID) {
                        continue;
                    }

                    var aabb2 = e2.GetComponent<CAabb>();
                    if (aabb2 != null) {
                        var p1 = pos.Pos;//e2.GetComponent<CPos>()?.Pos ?? Vector3.Zero;
                        var p2 = p1;

                        p2.X = Math.Min(aabb2.Aabb.Max.X, Math.Max(p2.X, aabb2.Aabb.Min.X));
                        p2.Y = Math.Min(aabb2.Aabb.Max.Y, Math.Max(p2.Y, aabb2.Aabb.Min.Y));
                        p2.Z = Math.Min(aabb2.Aabb.Max.Z, Math.Max(p2.Z, aabb2.Aabb.Min.Z));

                        //System.Console.WriteLine(p2);

                        var d = p2 - p1;
                        var r2 = d.LengthSquared();

                        var minR2 = ball1.Radius*ball1.Radius;
                        //System.Console.WriteLine(p2);

                        if (r2 >= minR2) {
                            continue;
                        }

                        // coll normal
                        d.Normalize();

                        if (Vector3.Dot(vel.Vel, d) < 0.0f) {
                            // moving away
                            continue;
                        }

                        vel.Vel += d * Vector3.Dot(d, Vector3.Reflect(vel.Vel, d)) * (1.0f + restitutionCoeff);
                        pos.Pos -= d * ((float)Math.Sqrt(minR2) - (float)Math.Sqrt(r2));
                    }
                }
            }
        }
    }

}

}
