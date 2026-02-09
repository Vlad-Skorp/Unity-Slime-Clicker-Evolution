using UnityEngine;

public static class ComponentExtensions
{
    public static bool TryGetComponentInParent<T>(this Component child, out T component, bool includeInactive = false) where T : Component
    {
        component = child.GetComponentInParent<T>(includeInactive);
        return component != null;
    }

    public static bool TryGetComponentInChildren<T>(this Component parent, out T component, bool includeInactive = false) where T : Component
    {
        component = parent.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }
}