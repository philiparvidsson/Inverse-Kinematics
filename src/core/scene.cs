namespace InverseKinematics.Core {

using System;
using System.Collections.Generic;

public abstract class Scene {
    private readonly Dictionary<int, EcsEntity> mEntities = new Dictionary<int, EcsEntity>();

    private readonly List<EcsSystem> mSystems = new List<EcsSystem>();

    public void AddEntity(EcsEntity entity) {
        mEntities.Add(entity.ID, entity);
    }

    public void AddSystem(EcsSystem system) {
        mSystems.Add(system);
    }

    public void AddSystems(params EcsSystem[] systems) {
        foreach (var system in systems) {
            AddSystem(system);
        }
    }

    public IEnumerable<EcsEntity> GetEntities(Type type) {
        foreach (var e in mEntities) {
            if (e.Value.HasComponent(type)) {
                yield return e.Value;
            }
        }
    }

    public IEnumerable<EcsEntity> GetEntities<T>() where T: EcsComponent {
        return GetEntities(typeof (T));
    }

    public virtual void Init() {
        foreach (var system in mSystems) {
            system.Init();
        }
    }

    public virtual void Cleanup() {
        foreach (var system in mSystems) {
            system.Cleanup();
        }
    }

    public virtual void Update(float t, float dt) {
        for (var i = 0; i < 2; i++) {
            foreach (var system in mSystems) {
                system.Update(t, dt*0.5f);
            }
        }
    }

    public virtual void Draw(float t, float dt) {
        foreach (var system in mSystems) {
            system.Draw(t, dt);
        }
    }
}

}
