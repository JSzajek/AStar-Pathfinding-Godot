/// <summary>
/// Interface outlining an object with a loading status relevant
/// to scene changing.
/// </summary>
public interface IWaitable {
    
    /// <summary>
    /// Gets or sets whether the objecting is loading. 
    /// </summary>
    bool Loading {get; set;}
}