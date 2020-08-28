using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Singleton class representing a controller in charge of 
/// scene changing and loading.
/// </summary>
public class SceneController : FreeingNode
{
	#region Constants

	private const int minimumPreloadTime = 2;

	#endregion Constants

	#region Fields

	private float currentUpdateTime;
	private ResourceInteractiveLoader _loader;
	private PreloadableScene _preloadable;
	private Control canvas;
	private Label loadingLabel;
	private ProgressBar loadingProgress;
	
	private string loadingPath;
	private Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

	#endregion Fields

	#region Public Properties

	/// <summary>
	/// Eventhandler triggered when scene loading is completed.
	/// </summary>
	public event EventHandler LoadComplete;

	/// <summary>
	/// Gets or sets the current interactive resource loader.
	/// </summary>
	private ResourceInteractiveLoader Loader {
		get => _loader;
		set {
			_loader = value;
			if (_loader != null) {
				loadingProgress.MaxValue = _loader.GetStageCount() + 1;
				loadingLabel.Text = "Loading Scene...";
			}
		}
	}

	/// <summary>
	/// Gets or sets the current preloadable scene.
	/// </summary>
	private PreloadableScene Preloadable {
		get => _preloadable;
		set { 
			_preloadable = value;
			if (_preloadable != null) {
				loadingProgress.MaxValue = _preloadable.GetStageCount() + 1;
				loadingLabel.Text = "Initializing Scene...";
			}
		}
	}

	/// <summary>
	/// Gets or sets the current scene.
	/// </summary>
	public Node CurrentScene { get; set; }

	#endregion Public Properties

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="SceneController"/> class.
	/// </summary>
	public override void _Ready()
	{
		SetProcess(false);
		canvas = this.Get<Control>("CanvasLayer/Canvas");
		loadingLabel = canvas.Get<Label>("LoadingLabel");
		loadingProgress = canvas.Get<ProgressBar>("LoadingProgress");
		
		canvas.Visible = false;

		Viewport root = GetTree().Root;
		CurrentScene = root.GetChild(root.GetChildCount() - 1);
	}

	#endregion Constructors

	/// <summary>
	/// Physics process method called every frame
	/// </summary>
	/// <param name="delta">Time delta</param>
	public override async void _Process(float delta)
	{
		// Wait at least the minimum update time
		if (currentUpdateTime < minimumPreloadTime) {
			currentUpdateTime += delta;
			return;
		}

		if (Loader != null) {
			var status = Loader?.Poll();
			if((bool)status?.Equals(Error.FileEof)) {
				SetProcess(false);
				var resource = Loader.GetResource();
				resources.Add(loadingPath, resource);
				SetCurrentScene(resource);

				loadingProgress.Value = loadingProgress.MaxValue; 
				await Task.Delay(100); // Delay long enough to display value change

				Loader = null;

				if (CurrentScene is PreloadableScene scene) {
					currentUpdateTime = 0;
					Preloadable = scene;
					SetProcess(true);
				}
				else {
					canvas.Visible = false;
					SetProcess(false);
					LoadComplete?.Invoke(this, EventArgs.Empty);
				}
			}
			else if ((bool)status?.Equals(Error.Ok)) {
				// Update the progress of the loading
				loadingProgress.Value = Loader.GetStage();
			}
			else {
				// Error in loading resort to error screen
				Loader = null;
			}
		}
		if (Preloadable != null) {

			// Update canvas with progress
			loadingProgress.Value = Preloadable.GetStage();

			if (Preloadable.Poll()) {
				SetProcess(false);

				Preloadable = null;
				currentUpdateTime = 0;

				loadingProgress.Value = loadingProgress.MaxValue; 
				await Task.Delay(100); // Delay long enough to display value change

				canvas.Visible = false;
				LoadComplete?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	/// <summary>
	/// Switches the current scene to the main menu scene.
	/// </summary>
	public void GoToMainMenu() {
		GotoScene("res://_Scenes/MainMenu.tscn");
	}

	/// <summary>
	/// Switches the current scene to the scene at the passed file path.
	/// Along with apply the potential playerstate upon load.
	/// </summary>
	/// <param name="path">The filepath of the scene to load</param>
	/// <param name="state">The player state to apply - null when no player state should be applied</param>
	public void GotoScene(string path)
	{
		// Show loading screen and being wait for deferred go to call
		currentUpdateTime = 0;
		canvas.Visible = true;

		// Queue current Scene for deletion
		CallDeferred(nameof(DisposeCurrent), path);
	}

	/// <summary>
	/// Sets the current scene to the passed loaded resource.
	/// </summary>
	/// <param name="resource">The loaded resource - a PackedScene</param>
	private void SetCurrentScene(Resource resource) {
		// Instance the next scene
		var nextSceneInstance =  (resource as PackedScene).Instance();

		// Change the scene to the loaded next scene
		CurrentScene = nextSceneInstance;
		GetTree().Root.AddChild(nextSceneInstance);
		GetTree().CurrentScene = CurrentScene;
	}

	/// <summary>
	/// Clears the memory related to the current scene and begins loading
	/// the next scene passed as a file path.
	/// </summary>
	/// <param name="path">The file path of the next scene to load</param>
	private async void DisposeCurrent(string path) {
		CurrentScene.Free();
		loadingPath = path;
		
		if (ResourceLoader.HasCached(loadingPath)) 
		{
			SetCurrentScene(resources[loadingPath]);
			if (CurrentScene is PreloadableScene scene) 
			{
				currentUpdateTime = 0;
				Preloadable = scene;
				SetProcess(true);
			}
			else
			{
				await Task.Delay(minimumPreloadTime * 1000);
				canvas.Visible = false;
			}
		}
		else
		{
			// Begin loading of the resource packed scene
			Loader = ResourceLoader.LoadInteractive(loadingPath);
			
			// Begin processing of new Scene
			SetProcess(true);
		}
	}
}
