using Godot;
using System.Collections.Generic;

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
        if (obj.HasNode(path)) {
            return obj.GetNode<T>(path);
        }
        else {
            GD.PrintErr(obj.Name + " couldn't locate object at " + path);
            return null;
        }
    }

    public static T GetFirstChild<T>(this Node obj) where T : class {
        var children = obj.GetChildren();
        foreach(var child in children) {
            if (child is T result) {
                return result;
            }
        }
        return null;
    }

    public static List<T> GetChildren<T>(this Node obj) where T : class {
        var results = new List<T>();
        var children = obj.GetChildren();
        foreach(var child in children) {
            if (child is Node node) {
                if (node is T res) {
                    results.Add(res);
                }
                if (node.GetChildCount() != 0) {
                    results.AddRange(node.GetChildren<T>());
                }
            }
        }
        return results;
    }
}