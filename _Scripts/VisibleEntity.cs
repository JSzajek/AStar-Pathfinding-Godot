using System;
using Godot;

/// <summary>
/// Abstract class representing all entities that switch
/// visibilty based on player view.
/// </summary>
public abstract class VisibleEntity : Spatial
{
	protected event EventHandler VisibilityChanged;
    
    /// <summary>
	/// Gets or sets the visible state of the entity.
	/// </summary>
    public abstract new bool Visible { get; set; }

    /// <summary>
    /// Gets or sets whether the entity is an always visible state.
    /// </summary>
    [Export]
    public bool AlwaysVisible {get; set;}

    /// <summary>
    /// Gets or sets the base spatial visible value.
    /// </summary>
    protected bool BaseVisible 
    {
        get => base.Visible;
        set => base.Visible = AlwaysVisible ? AlwaysVisible : value;
    }

    /// <summary>
    /// Triggers the visbility changed event.
    /// </summary>
    protected void OnVisibilityChanged() => VisibilityChanged?.Invoke(this, EventArgs.Empty);
}