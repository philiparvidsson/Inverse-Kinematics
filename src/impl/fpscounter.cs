namespace InverseKinematics.Impl {

using Core;

public sealed class FpsCounter: EcsSystem {
    private string mOrigTitle;

    private int mNumDraws;

    private float mInvUpdatesPerSec;

    private float mUpdTimer;

    public FpsCounter(float updatesPerSec=10.0f) {
        mInvUpdatesPerSec = 1.0f/updatesPerSec;
    }

    public override void Init() {
        base.Init();

        mOrigTitle = Program.Inst.Window.Title;
    }

    public override void Cleanup() {
        base.Cleanup();

        Program.Inst.Window.Title = mOrigTitle;
    }

    public override void Draw(float t, float dt) {
        base.Draw(t, dt);

        mNumDraws++;

        mUpdTimer -= dt;
        if (mUpdTimer <= 0.0f) {
            var numDrawsPerSec = mNumDraws / mInvUpdatesPerSec;
            Program.Inst.Window.Title = $"{mOrigTitle} (fps: {numDrawsPerSec:0.00})";

            mNumDraws = 0;
            mUpdTimer = mInvUpdatesPerSec;
        }
    }
}

}
