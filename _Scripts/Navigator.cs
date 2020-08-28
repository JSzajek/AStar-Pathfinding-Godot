/// <summary>
/// Singleton class representing a navigator holding connections
/// to all vital controller classes hosted within the application.
/// </summary>
public class Navigator : FreeingNode {

    #region Fields

    private static Navigator instance;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Navigator"/> class.
    /// </summary>
    public Navigator() {
        instance = this;
    }
    
    #endregion Constructors

    #region Public Properties

    /// <summary>
    /// Gets the SceneController instance of the application.
    /// </summary>
    public static SceneController SceneController => instance.Get<SceneController>("/root/SceneController");

    /// <summary>
    /// Gets the DllLoader instance of the application.
    /// </summary>
    public static DllLoader DllLoader => instance.Get<DllLoader>("/root/DllLoader");

    #endregion Public Properties
}