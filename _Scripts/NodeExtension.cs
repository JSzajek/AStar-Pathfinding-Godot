using Godot;

/// <summary>
/// Extension class for basic Node class.
/// </summary>
public static class NodeExtension {

    /// <summary>
    /// Extended GetNodeOrNull method to quietly attempt to grab
    /// node or indicate otherwise.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static T Get<T>(this Node obj, string path) where T : class {
        T result = obj.GetNodeOrNull<T>(path);
        if (result == null) {
            GD.PrintErr(obj.Name + " couldn't locate object at " + path);
        }
        return result;
    }
}