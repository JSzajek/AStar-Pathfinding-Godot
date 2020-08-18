using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension class for Node class.
/// </summary>
public static class NodeExtension {

	/// <summary>
	/// Extended Get method that gets the root node of the current scene.
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <returns>The Root Node</returns>
	public static Node GetRoot(this Node obj) {
		return obj.Get<Node>("/root");
	}

	/// <summary>
	/// Extended GetParent to attempt to recursively dig
	/// upwards to grab the first parent of the node of the given type.
	/// </summary>
	/// 
	/// <typeparam name="T">The return object type</typeparam>
	public static T GetFirstParent<T>(this Node obj) where T : class {
		var parent = obj.GetParent();
		if (parent.Equals(obj.GetRoot())){
			return null;
		}
		else if (parent is T result) {
			return result;
		}
		else {
			return parent.GetFirstParent<T>();
		}
	}

	/// <summary>
	/// Extended GetNode method to quietly attempt to grab
	/// node or indicate otherwise.
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <param name="path">The path of the node</param>
	/// <typeparam name="T">The return object type</typeparam>
	public static T Get<T>(this Node obj, string path) where T : class {
		if (obj.HasNode(path)) {
			return obj.GetNode<T>(path);
		}
		else {
			GD.PrintErr(obj.Name + " couldn't locate object at " + path);
			return null;
		}
	}

	/// <summary>
	/// Extended GetNode method to quietly attempt to grab
	/// node or indicate otherwise.
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <param name="path">The path of the node</param>
	/// <typeparam name="T">The return object type</typeparam>
	public static T Get<T>(this Node obj, NodePath path) where T : class 
	{
		if (path != null && obj.HasNode(path))
		{
			return obj.GetNode<T>(path);
		}
		else {
			GD.PrintErr(obj.Name + " couldn't locate object at " + (path ?? "null path"));
			return null;
		}
	}

	/// <summary>
	/// Extended Get method to get the first child of the node
	/// of the passed type
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <param name="deep">Determines whether to do a deep recursive search</param>
	/// <typeparam name="T">The return object type</typeparam>
	public static T GetFirstChild<T>(this Node obj, bool deep = false) where T : class {
		var children = obj.GetChildren();
		if (deep) {
			return obj.GetChildren<T>(deep).FirstOrDefault();
		}
		foreach(var child in children) {
			if (child is T result) {
				return result;
			}
		}
		return null;
	}

	/// <summary>
	/// Extended GetChildren method getting all of the children
	/// of the passed starting from the node.
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <param name="deep">Determines whether to do a deep recursive search</param>
	/// <typeparam name="T">The return object type</typeparam>
	public static List<T> GetChildren<T>(this Node obj, bool deep = false) where T : class {
		var results = new List<T>();
		var children = obj.GetChildren();
		foreach(var child in children) {
			if (child is Node node) {
				if (node is T res) {
					results.Add(res);
				}
				if (deep && node.GetChildCount() != 0) {
					results.AddRange(node.GetChildren<T>());
				}
			}
		}
		return results;
	}

	/// <summary>
	/// Extended Remove child method to remove all of the given
	/// type of children from the node. 
	/// </summary>
	/// <param name="obj">The Node</param>
	/// <param name="deep">Determines whether to do a deep recursive search</param>
	/// <typeparam name="T">The type of the object to remove</typeparam>
	public static void RemoveAllChildren<T>(this Node obj, bool deep = false) where T : class {
		var children = obj.GetChildren<T>(deep);
		foreach(var child in children) {
			if (child is Node node) {
				obj.RemoveChild(node);
			}
		}
	}
}