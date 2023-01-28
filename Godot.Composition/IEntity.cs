using System.Collections.Generic;

namespace Godot.Composition
{
    public interface IEntity : INode
    {
        /// <summary>
        /// Checks to see if the <see cref="Entity"/> contains a component specified by type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IComponent"/> to check for.</typeparam>
        /// <returns>True if the <see cref="Entity"/> has the <see cref="IComponent"/>, False otherwise.</returns>
        bool HasComponent<T>() where T : Node;

        /// <summary>
        /// Gets the component specified by type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IComponent"/> to check for.</typeparam>
        /// <returns>
        /// Null if the <see cref="Entity"/> does not have the <see cref="IComponent"/>, 
        /// otherwise, the <see cref="IComponent"/>.
        /// </returns>
        T GetComponent<T>() where T : Node;

        /// <summary>
        /// Gets the component specified by type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IComponent"/> to check for.</typeparam>
        /// <param name="name">The name of the component/node in the scene tree.</param>
        /// <returns>
        /// Null if the <see cref="Entity"/> does not have the <see cref="IComponent"/>, 
        /// otherwise, the <see cref="IComponent"/>.
        /// </returns>
        T GetComponentByName<T>(string name) where T : Node;

        /// <summary>
        /// Enumerates all components attached to the <see cref="Entity"/>.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IComponent"/>s.</returns>
        IEnumerable<IComponent> Components();
    }
}