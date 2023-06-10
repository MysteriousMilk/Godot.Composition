using System;
using System.Collections.Generic;
using System.Linq;

namespace Godot.Composition;

internal static class ExtensionMethods
{
    /// <summary>
    /// Finds all descendant <see cref="Node"/>s (of the given <see cref="Node"/>s) that meet a specific condition.
    /// </summary>
    /// <param name="node">The parent <see cref="Node"/>s to conduct the search from.</param>
    /// <param name="predicate">Delegate that evaluates each <see cref="Node"/>s to see if it belongs in the return list.</param>
    /// <returns>List of all <see cref="Node"/>s that meet the given condition.</returns>
    internal static IEnumerable<Node> FindDescendantNodesIf(this Node node, Predicate<Node> predicate)
    {
        List<Node> nodesFound = new List<Node>();
        FindDescendantNodesIf(node, predicate, ref nodesFound);
        return nodesFound;
    }

    private static void FindDescendantNodesIf(Node node, Predicate<Node> predicate, ref List<Node> nodesFound)
    {
        foreach (var child in node.GetChildren())
        {
            var childNode = child as Node;

            if (childNode != null)
            {
                if (predicate.Invoke(childNode))
                    nodesFound.Add(childNode);

                FindDescendantNodesIf(childNode, predicate, ref nodesFound);
            }
        }
    }

    /// <summary>
    /// Determines if the calling <see cref="Type"/> inherits from or implements the passed <see cref="Type"/>.
    /// </summary>
    /// <param name="child">The calling <see cref="Type"/> (possible child).</param>
    /// <param name="parent">The passed in <see cref="Type"/> (possible parent of the child).</param>
    /// <returns>True if the child <see cref="Type"/> inherits or implements the parent <see cref="Type"/>.</returns>
    internal static bool InheritsOrImplements(this Type child, Type parent)
    {
        var currentChild = parent.IsGenericTypeDefinition && child.IsGenericType ? child.GetGenericTypeDefinition() : child;

        while (currentChild != typeof(object))
        {
            if (parent == currentChild || currentChild.HasAnyInterfaces(parent))
                return true;

            currentChild = currentChild.BaseType != null && parent.IsGenericTypeDefinition && currentChild.BaseType.IsGenericType
                            ? currentChild.BaseType.GetGenericTypeDefinition()
                            : currentChild.BaseType;

            if (currentChild == null)
                return false;
        }

        return false;
    }

    /// <summary>
    /// Checks to see if the calling <see cref="Type"/> has any interfaces of the interface <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The calling <see cref="Type"/>.</param>
    /// <param name="interfaceType">The interface <see cref="Type"/>.</param>
    /// <returns>True if it has the interface, false if not.</returns>
    internal static bool HasAnyInterfaces(this Type type, Type interfaceType)
    {
        return type.GetInterfaces().Any(childInterface =>
        {
            var currentInterface = interfaceType.IsGenericTypeDefinition && childInterface.IsGenericType
                                    ? childInterface.GetGenericTypeDefinition()
                                    : childInterface;
            return currentInterface == interfaceType;
        });
    }
}
