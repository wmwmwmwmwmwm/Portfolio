using UnityEngine;

public class SingletonManager : SingleInstance<SingletonManager>
{
	[SerializeField] GameManager _GameManager;
	[SerializeField] DataManager _DataManager;
	[SerializeField] UIManager _UIManager;
	[SerializeField] SoundManager _SoundManager;
	[SerializeField] UserInfoManager _UserInfoManager;
	[SerializeField] StoryManager _StoryManager;
	[SerializeField] MultiplayerManager _MultiplayerManager;

	public static GameManager Game => GameManager.Instance;
	public static DataManager Data => DataManager.Instance;
	public static UIManager UI => UIManager.Instance;
	public static SoundManager Sound => SoundManager.Instance;
	public static UserInfoManager User => UserInfoManager.Instance;
	public static UserInfoManager.SaveData UserData => UserInfoManager.Instance._SaveData;
	public static StoryManager Story => StoryManager.Instance;
	public static MultiplayerManager Multiplayer => MultiplayerManager.Instance;

	// Intro 씬
	public static IntroController Intro => IntroController.Instance;

	// Bg_Title 씬
	public static TitleController Title => TitleController.Instance;

	// FPS Title 씬
	public static LobbyController Lobby => LobbyController.Instance;

	// FPS Room 씬
	public static RoomController Room => RoomController.Instance;

	// Colony, Lev_Elevator_Prologue 씬
	public static CinematicController Cinematic => CinematicController.Instance;

	void Start()
	{
		GameManager.CreateInstance(_GameManager.gameObject);
		DataManager.CreateInstance(_DataManager.gameObject);
		UIManager.CreateInstance(_UIManager.gameObject);
		SoundManager.CreateInstance(_SoundManager.gameObject);
		UserInfoManager.CreateInstance(_UserInfoManager.gameObject);
		StoryManager.CreateInstance(_StoryManager.gameObject);
		MultiplayerManager.CreateInstance(_MultiplayerManager.gameObject);
	}
}
