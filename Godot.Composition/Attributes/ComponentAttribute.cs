using System;

namespace Godot.Composition;

public class ComponentAttribute : Attribute
{
    public Type ParentType
    {
        get;
        set;
    }

    public ComponentAttribute(Type parentType)
    {
        ParentType = parentType;
    }
}
