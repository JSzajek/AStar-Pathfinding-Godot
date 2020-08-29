using Godot;

/// <summary>
/// Wrapper Node class to ensure that there are no
/// remaining orphaned nodes at the closure of the application.
/// </summary>
public class FreeingNode : Node
{
    /// <summary>
    /// Performs memory managment freeing the node.
    /// </summary>
    public override void _ExitTree()
    {
        QueueFree();
    }
}