namespace InverseKinematics.Core {

public abstract class EcsSystem {
    public virtual void Init() {
    }

    public virtual void Cleanup() {
    }

    public virtual void Update(float t, float dt) {
    }

    public virtual void Draw(float t, float dt) {
    }

}

}
