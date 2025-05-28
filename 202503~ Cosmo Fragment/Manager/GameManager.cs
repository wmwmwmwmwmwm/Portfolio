using GameCreator.Core;
using Naninovel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static SingletonManager;

public class GameManager : Singleton<GameManager>
{
	////////////////////
	// 세이브 데이터

	[Serializable]
	public class SharedData
	{
		[Serializable]
		public class LoadPreview
		{
			public int _Profile;
			public double _LastPlayedTime;
			public string _NextStageName;
		}
		public List<LoadPreview> _LoadPreviews = new();
		public int _Master, _Bgm, _Sfx, _Voice;
	}
	[HideInInspector] public SharedData _SharedData;

	// 세이브 데이터
	////////////////////
	
	[HideInInspector] public bool _IsSingleMode;
	[HideInInspector] public string _StartSceneName;
	Dictionary<GameObject, Action> _LoadSceneEvents;
	Dictionary<GameObject, Action> _UnloadSceneEvents;
	Dictionary<GameObject, Action> _ResolutionChangeEvents;
	IBackButton _BackButtonReceiver;
	Vector2Int _Resolution;

	public string CurrentSceneName => SceneManager.GetActiveScene().name;

	public float WH => (float)Screen.width / Screen.height;

	public float WHRatio => WH / (16f / 9f);

	protected override void Init()
	{
		_StartSceneName = SceneManager.GetActiveScene().name;
		_LoadSceneEvents = new();
		_UnloadSceneEvents = new();
		_ResolutionChangeEvents = new();
		SceneManager.sceneLoaded -= OnSceneLoad;
		SceneManager.sceneLoaded += OnSceneLoad;
		SceneManager.sceneUnloaded -= OnSceneUnload;
		SceneManager.sceneUnloaded += OnSceneUnload;
		OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		_Resolution = new(Screen.width, Screen.height);

		_IsSingleMode = true;
	}

	void Update()
	{
		// 뒤로가기
		if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1))
		{
			bool used = UI.OnBackButton();
			if (!used && _BackButtonReceiver != null)
			{
				_BackButtonReceiver.OnBackButton();
			}
		}

		// 해상도 변경 이벤트
		if (_Resolution.x != Screen.width || _Resolution.y != Screen.height)
		{
			_Resolution = new(Screen.width, Screen.height);
			InvokeEvent(_ResolutionChangeEvents);
		}
	}

	void OnApplicationQuit()
	{
		User.Save();
		Game.SaveSharedData();
	}

	public void SaveSharedData()
	{
		string json = JsonUtility.ToJson(_SharedData);
		File.WriteAllText(GetSharedDataPath(), json);
	}

	public void LoadSharedData()
	{
		string path = GetSharedDataPath();
		if (File.Exists(path))
		{
			string json = File.ReadAllText(GetSharedDataPath());
			_SharedData = JsonUtility.FromJson<SharedData>(json);
		}
		else
		{
			_SharedData = new();
		}
	}

	public void Quit()
	{
		Sound.PlaySfx(SoundManager.SfxType.Exit);
		if (Application.isEditor)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#endif
		}
		else
		{
			Application.Quit();
		}
	}

	void OnSceneLoad(Scene scene, LoadSceneMode mode)
	{
		// 뒤로가기 리시버 설정
		_BackButtonReceiver = scene.name switch
		{
			SceneName.Intro => Intro,
			SceneName.Title => Title,
			SceneName.Lobby => Lobby,
			SceneName.Room => Room,
			SceneName.Intro1 or SceneName.Intro2 => Cinematic,
			_ => null,
		};

		// 씬 로드 이벤트
		InvokeEvent(_LoadSceneEvents);
	}

	void OnSceneUnload(Scene scene)
	{
		InvokeEvent(_UnloadSceneEvents);
	}

	void InvokeEvent(Dictionary<GameObject, Action> eventDict)
	{
		List<GameObject> keyToRemove = new();
		foreach (KeyValuePair<GameObject, Action> e in eventDict)
		{
			if (e.Key)
			{
				e.Value.Invoke();
			}
			else
			{
				keyToRemove.Add(e.Key);
			}
		}
		keyToRemove.ForEach(x => eventDict.Remove(x));
	}

	public void RegisterEvent_LoadScene(GameObject receiver, Action action)
	{
		_LoadSceneEvents[receiver] = action;
	}

	public void RegisterEvent_UnloadScene(GameObject receiver, Action action)
	{
		_UnloadSceneEvents[receiver] = action;
	}

	public void RegisterEvent_ResolutionChange(GameObject receiver, Action action)
	{
		_ResolutionChangeEvents[receiver] = action;
	}

	public void LoadTitleScene()
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			UI.ShowLoading(true);
			yield return new WaitForSeconds(0.6f);
			yield return StartCoroutine(LoadSceneAsync(SceneName.Title));
			UI.ShowLoading(false);
		}
	}

	public void LoadLobbyScene()
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			UI.ShowLoading(true);
			yield return new WaitForSeconds(0.6f);
			yield return StartCoroutine(LoadSceneAsync(SceneName.Lobby));
			UI.ShowLoading(false);
		}
	}

	public void LoadRoomScene()
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			UI.ShowLoading(true);
			yield return new WaitForSeconds(0.6f);
			yield return StartCoroutine(LoadSceneAsync(SceneName.Room));
			UI.ShowLoading(false);
		}
	}

	public void LoadBattleScene(string scene, bool intro, bool multi = false)
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			//if (intro)
			//{
			//	// 콜로니 씬
			//	yield return StartCoroutine(WaitCinematic(SceneName.Intro1));

			//	// 시네마틱 씬
			//	yield return StartCoroutine(WaitCinematic(SceneName.Intro2));
			//}

			yield return null;
			NetworkConnectManager connectManager = FindAnyObjectByType<NetworkConnectManager>();
			connectManager.ConnectPlayer(multi);
			connectManager.CreateLocalGame();

			// 배틀 씬
			Sound.PlayBgm(SoundManager.BgmType.None);
			UI.ShowLoading(true, image2: true);
			yield return new WaitForSeconds(0.6f);
			if (!multi)
				NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);
			UI.ShowLoading(false);

			//IEnumerator WaitCinematic(string sceneName)
			//{
			//	UI.ShowLoading(true);
			//	yield return new WaitForSeconds(0.6f);
			//	yield return LoadSceneAsync(sceneName);
			//	UI.ShowLoading(false);

			//	UI._CinematicOverlay.SetActive(true);
			//	yield return null;
			//	PlayableDirector director = FindAnyObjectByType<PlayableDirector>();
			//	yield return new WaitUntil(() => director.state == PlayState.Paused || Cinematic._Skip);
			//	UI._CinematicOverlay.SetActive(false);
			//}
		}
	}

	public IEnumerator LoadStoryScene(CharacterName charName, int index, Script script)
	{
		// 진입 연출
		UI.FadeOut();
		yield return new WaitForSeconds(1.2f);
		UI.ShowLoading(true);
		float lobbyBgmPos = Sound._Bgm.time;

		// 이전 씬 끄기
		GameObject[] allObjs = SceneManager.GetActiveScene().GetRootGameObjects();
		List<GameObject> disabledObjs = new();
		foreach (GameObject obj in allObjs)
		{
			if (!obj.activeSelf) continue;
			if (obj == Room.gameObject) continue;

			obj.SetActive(false);
			disabledObjs.Add(obj);
		}

		// 이전 씬 BGM 끄기
		Sound.PlayBgm(SoundManager.BgmType.None);

		// 스토리 재생
		AsyncOperation operation = SceneManager.LoadSceneAsync(SceneName.Story, LoadSceneMode.Additive);
		yield return new WaitUntil(() => operation.isDone);
		UI.ShowLoading(false);
		UI.FadeIn();
		yield return StartCoroutine(Story.Play(script, "", false));

		// 스토리 감상 기록
		List<int> userData = UserData.GetCharacterInfo(charName)._HaveSeenStorys;
		if (!userData.Contains(index))
		{
			userData.Add(index);
			User.Save();
		}

		// 이전 씬 BGM 재생
		Sound.PlayBgm(SoundManager.BgmType.Lobby, 3f);
		Sound._Bgm.time = lobbyBgmPos;

		// 이전 씬으로 복귀
		yield return StartCoroutine(UnloadSceneAsync(SceneName.Story));
		UI.ShowLoading(false);
		UI.FadeIn(2f);
		//objsToDisable.ForEach(x => x.enabled = true);
		disabledObjs.ForEach(x => x.SetActive(true));
	}

	public IEnumerator LoadBattleStoryScene(Script script, string label)
	{
		// 배경 블러
		yield return StartCoroutine(Story.SetupBlurBackground());

		// 이전 씬 끄기
		List<Behaviour> objsToDisable = new();
		GatherObjects<Camera>();
		GatherObjects<Light>();
		GatherObjects<Volume>();
		objsToDisable.ForEach(x => x.enabled = false);

		void GatherObjects<T>() where T : Behaviour
		{
			objsToDisable.AddRange(FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(x => x.gameObject.scene.name == CurrentSceneName));
		}

		// 스토리 재생
		SceneManager.LoadSceneAsync(SceneName.BattleStory, LoadSceneMode.Additive);
		yield return StartCoroutine(Story.Play(script, label, true));

		// 이전 씬으로 복귀
		yield return StartCoroutine(UnloadSceneAsync(SceneName.BattleStory));
		objsToDisable.ForEach(x => x.enabled = true);
	}

	// 유사 LoadSceneAsync
	IEnumerator LoadSceneAsync(string targetSceneName)
	{
		Scene mainScene = SceneManager.GetActiveScene();

		// 임시 빈씬으로 전환 후
		SceneManager.LoadScene("EmptyScene", LoadSceneMode.Single);
		yield return new WaitWhile(() => mainScene.isLoaded);

		// 씬을 로드
		SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
		yield return new WaitUntil(() => SceneManager.GetSceneByName(targetSceneName).isLoaded);

		yield return null;
	}

	IEnumerator UnloadSceneAsync(string targetSceneName)
	{
		AsyncOperation operation = SceneManager.UnloadSceneAsync(targetSceneName);
		yield return new WaitUntil(() => operation.isDone);
	}

	string GetSharedDataPath() => Path.Combine(Application.persistentDataPath, "user.sav");
}
