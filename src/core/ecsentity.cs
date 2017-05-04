namespace InverseKinematics.Core {

using System;
using System.Collections.Generic;

public sealed class EcsEntity {
    private readonly Dictionary<Type, EcsComponent> mComponents =
        new Dictionary<Type, EcsComponent>();

    private static int sNextID = 1;

    public int ID { get; }

    public EcsEntity() {
        ID = sNextID++;
    }

    public EcsEntity(params EcsComponent[] components) : this() {
        AddComponents(components);
    }

    public void AddComponent(EcsComponent component) {
        mComponents.Add(component.GetType(), component);
    }

    public void AddComponents(params EcsComponent[] components) {
        foreach (var component in components) {
            AddComponent(component);
        }
    }

    public EcsComponent GetComponent(Type type) {
        EcsComponent component;
        mComponents.TryGetValue(type, out component);
        return component;
    }

    public T GetComponent<T>() where T: EcsComponent {
        return (T)GetComponent(typeof (T));
    }

    public bool HasComponent(Type type) {
        return mComponents.ContainsKey(type);
    }

    public bool HasComponent<T>() where T: EcsComponent {
        return HasComponent(typeof (T));
    }

    public bool RemoveComponent(Type type) {
        return mComponents.Remove(type);
    }

    public bool RemoveComponent<T>() where T: EcsComponent {
        return RemoveComponent(typeof (T));
    }
}

}
