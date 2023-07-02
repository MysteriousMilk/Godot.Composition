namespace Godot.Composition;

/// <summary>
/// Interface for a component node.
/// </summary>
public interface IComponent : INode
{
    /// <summary>
    /// Resolves component reference dependencies specified for this component.
    /// </summary>
    void ResolveDependencies();
}
