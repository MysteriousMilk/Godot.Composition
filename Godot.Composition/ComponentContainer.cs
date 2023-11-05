using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Godot.Composition;

/// <summary>
/// Container collection that maps component object references to their type for easy lookup.
/// </summary>
public class ComponentContainer : IEnumerable<IComponent>, ICollection<IComponent>
{
    private Dictionary<Type, List<WeakReference<IComponent>>> components;

    /// <summary>
    /// Number of components in the container.
    /// </summary>
    public int Count => components.Count;

    /// <summary>
    /// Indicates if the collection is read only or not.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ComponentContainer()
    {
        components = new Dictionary<Type, List<WeakReference<IComponent>>>();
    }

    /// <summary>
    /// Constructor which builds the container based on the given node.
    /// It searches the given node for components to add.
    /// </summary>
    /// <param name="entity">The entity node that contains component nodes.</param>
    public ComponentContainer(Node entity) : this()
    {
        AddEntityComponents(entity);
    }

    /// <summary>
    /// Finds all child component nodes for the given node and adds them to the container.
    /// </summary>
    /// <param name="entity">The entity node.</param>
    public void AddEntityComponents(Node entity)
    {
        foreach (var c in entity.FindDescendantNodesIf(n => n is IComponent).Cast<IComponent>())
            Add(c);
    }

    #region Components
    /// <summary>
    /// Checks to see if the <see cref="IEntity"/> contains a component specified by type T.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Component{T}"/> to check for.</typeparam>
    /// <returns>True if the <see cref="IEntity"/> has the <see cref="Component{T}"/>, False otherwise.</returns>
    public bool HasComponent<T>() where T : IComponent
    {
        var searchType = typeof(T);
        return components.ContainsKey(searchType);
    }

    /// <summary>
    /// Gets the component specified by type T.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Component{T}"/> to check for.</typeparam>
    /// <returns>
    /// Null if the <see cref="Entity"/> does not have the <see cref="Component{T}"/>, 
    /// otherwise, the <see cref="Component{T}"/>.
    /// </returns>
    public T GetComponent<T>() where T : IComponent
    {
        var searchType = typeof(T);
        T component = default(T);

        if (components.ContainsKey(searchType) && components[searchType].Count > 0)
        {
            if (components[searchType].First().TryGetTarget(out IComponent c))
                component = (T)c;
            else
                components.Remove(searchType);
        }

        return component;
    }

    /// <summary>
    /// Gets the component specified by type T.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Component{T}"/> to check for.</typeparam>
    /// <returns>
    /// Null if the <see cref="Entity"/> does not have the <see cref="Component{T}"/>, 
    /// otherwise, the <see cref="Component{T}"/>.
    /// </returns>
    public T GetComponentByName<T>(string name) where T : IComponent
    {
        var searchType = typeof(T);
        T component = default(T);

        if (components.ContainsKey(searchType) && components[searchType].Count > 0)
        {
            var match = components[searchType].FirstOrDefault(x =>
            {
                x.TryGetTarget(out IComponent c);
                return c.Name == name;
            });

            if (match != null && match.TryGetTarget(out IComponent c))
                component = (T)c;
        }

        return component;
    }

    /// <summary>
    /// Gets the component by a specified type.
    /// </summary>
    /// <param name="type">The type of component to get.</param>
    /// <returns>The first component of the given type, or null if no component in the conatiner matches the type.</returns>
    public IComponent GetComponentByType(Type type)
    {
        if (components.ContainsKey(type) && 
            components[type].Count > 0 &&
            components[type].First().TryGetTarget(out IComponent c))
        {
            return c;
        }
        return null;
    }
    #endregion

    #region IEnumerableT
    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<IComponent> GetEnumerator()
    {
        foreach (var componentList in components.Values)
        {
            foreach (var weakComponent in componentList)
            {
                if (weakComponent.TryGetTarget(out IComponent c))
                    yield return c;
            }
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var componentList in components.Values)
        {
            foreach (var weakComponent in componentList)
            {
                if (weakComponent.TryGetTarget(out IComponent c))
                    yield return c;
            }
        }
    }
    #endregion

    #region ICollectionT
    /// <summary>
    /// Adds an <see cref="IComponent"/> to the collection.
    /// </summary>
    /// <param name="item">The <see cref="IComponent"/> to add to the collection.</param>
    /// <exception cref="ArgumentException">Throws an <see cref="ArgumentException"/> if the item is not a <see cref="Node"/>.</exception>
    public void Add(IComponent item)
    {
        if (item is not Node)
            throw new ArgumentException("Component must be a node.", nameof(item));

        var key = item.GetType();

        if (!components.ContainsKey(key))
            components.Add(key, new List<WeakReference<IComponent>>());

        var weakRef = new WeakReference<IComponent>(item);
        components[key].Add(weakRef);

        // add lookup for interface types as well
        foreach (var interfaceType in key.GetInterfaces())
        {
            // don't add keyed entry for IComponent
            if (interfaceType == typeof(IComponent))
                continue;

            // don't add key entries for interfaces that don't inherit IComponent
            if (!interfaceType.InheritsOrImplements(typeof(IComponent)))
                continue;

            if (!components.ContainsKey(interfaceType))
                components.Add(interfaceType, new List<WeakReference<IComponent>>());

            weakRef = new WeakReference<IComponent>(item);
            components[interfaceType].Add(weakRef);
        }
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
    {
        components.Clear();
    }

    /// <summary>
    /// Checks to see if a <see cref="IComponent"/> is contained with the collection.
    /// </summary>
    /// <param name="item">The <see cref="IComponent"/> to check for.</param>
    /// <returns>True if the <see cref="IComponent"/> is in the collection, False if not.</returns>
    public bool Contains(IComponent item)
    {
        bool contains = false;
        foreach (var componentList in components.Values)
        {
            contains = componentList.Any(c =>
            {
                bool found = false;
                if (c.TryGetTarget(out IComponent target))
                    found = ReferenceEquals(target, item);
                return found;
            });

            if (contains)
                break;
        }

        return contains;
    }

    /// <summary>
    /// Copys the elements of the collection to an <see cref="Array"/>, starting a a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// he one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ICollection{T}"/>. 
    /// The Array must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">This exception is thrown if the array parameter is null.</exception>
    public void CopyTo(IComponent[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException("array");

        var list = this.ToList();
        list.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes an <see cref="IComponent"/> from the collection.
    /// </summary>
    /// <param name="item">The <see cref="IComponent"/> to remove.</param>
    /// <returns>True if the item was removed, False if not.</returns>
    public bool Remove(IComponent item)
    {
        return components.Remove(item.GetType());
    }
    #endregion
}
