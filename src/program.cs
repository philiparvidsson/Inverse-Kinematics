namespace InverseKinematics {

using System;
using System.Collections.Generic;

using Core;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public sealed class Program: Game {
    private readonly Stack<Scene> mScenes = new Stack<Scene>();

    private static Program sInst;

    public GraphicsDeviceManager GfxDevMgr { get; }

    public static Program Inst => sInst;

    public Scene Scene => (mScenes.Count > 0) ? mScenes.Peek() : null;

    public Program(string title, Scene scene) {
        sInst = this;

        GfxDevMgr = new GraphicsDeviceManager(this);
        GfxDevMgr.PreparingDeviceSettings += (sender, e) => {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        };

        mScenes.Push(scene);

        IsMouseVisible = true;
        Window.Title = title;
    }

    protected override void Initialize() {
        base.Initialize();

        Scene.Init();
    }

    protected override void Update(GameTime gameTime) {
        var scene = Scene;
        if (scene != null) {
            var t  = (float)gameTime.  TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            scene.Update(t, dt);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        var scene = Scene;
        if (scene != null) {
            var t  = (float)gameTime.  TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            scene.Draw(t, dt);
        }

        base.Draw(gameTime);
    }

    [STAThread]
    private static void Main(string[] args) {
        using (var p = new Program("Inverse Kinematics", new IKDemo())) {
            p.Run();
        }
    }
}

}
