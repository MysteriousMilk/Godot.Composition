namespace Godot.Composition
{
    /// <summary>
    /// Helper class that contains methods for working with <see cref="IComponent"/>s. 
    /// </summary>
    public static class ComponentHelper
    {
        /// <summary>
        /// Helper method used to find <see cref="IComponent"/>s associated with a node/entity.
        /// </summary>
        public static T FindComponent<T>(Node parentNode) where T : IComponent
        {
            T component = default;

            foreach (var child in parentNode.GetChildren())
            {
                if (child is IComponent c && c.GetType().InheritsOrImplements(typeof(T)))
                {
                    component = (T)c;
                    break;
                }
            }

            return component;
        }
    }
}
