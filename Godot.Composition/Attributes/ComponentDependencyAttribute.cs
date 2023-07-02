using System;

namespace Godot.Composition;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ComponentDependencyAttribute : Attribute
{
    public Type ComponentType
    {
        get;
        set;
    }

    public ComponentDependencyAttribute(Type componentType)
    {
        ComponentType = componentType;
    }
}
