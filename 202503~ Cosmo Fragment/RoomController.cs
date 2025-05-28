using Naninovel;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using static SingletonManager;
using static DataManager;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

public partial class RoomController : SingleInstance<RoomController>, IBackButton
{
	public class ModelLoadInfo
	{
		public CharacterName _CharName;
		public CharacterCloth _Data;

		[HideInInspector] public GameObject _Instance;
		public AsyncOperationHandle<GameObject> _LoadHandle;
	}

	public UI_Button _ExitButton;
	public RuntimeAnimatorController _AnimatorAsset;
	public Transform _ModelParent;
	public DraggableUI _CharacterDraggable;

	[Header("왼쪽")]
	public TMP_Text _NameText;
	public UI_Button _StoryButton;
	public UI_Button _Story_BackButton;
	public CanvasGroup _StoryPanel;
	public UI_StoryElement _StoryElementPrefab;
	public Transform _StoryElementParent;

	[Header("오른쪽")]
	public UI_Button _CharacterTabButton;
	public UI_Button _ClothTabButton;
	public Transform _CharacterTab, _ClothTab;
	public UI_Room_Element _CharacterElementPrefab;
	public UI_Room_Element _ClothElementPrefab;

	CharacterName _CharName;
	string _ClothName;
	Animator _CharAnimator;
	List<ModelLoadInfo> _ModelLoadInfos;
	bool _StoryPanelOpen;
	Coroutine _StoryPanelTween;
	List<UI_StoryElement> _StoryElements;
	List<UI_Room_Element> _CharElements;
	List<UI_Room_Element> _ClothElements;
	Coroutine _AnimationCoroutine;
	float _AnimIdleTime;
	bool _PlayClothAnim;

	const string IdleAnimName = "Idle";
	const string IdleLongAnimName = "IdleLong";
	const string ClothAnimName = "Cloth";

	void Start()
	{
		_ModelLoadInfos = new();
		_StoryElements = new();
		_CharElements = new();
		_ClothElements = new();
		_StoryPanel.gameObject.SetActive(false);
		_StoryElementPrefab.gameObject.SetActive(false);
		_CharacterElementPrefab.gameObject.SetActive(false);
		_ClothElementPrefab.gameObject.SetActive(false);

		// 버튼
		_ExitButton.onClick.AddListener(ExitButton);
		_CharacterDraggable.DragCallback = (_, eventData) => CharacterDrag(eventData);
		_StoryButton.onClick.AddListener(StoryButton);
		_Story_BackButton.onClick.AddListener(Story_BackButton);
		_CharacterTabButton.onClick.AddListener(() => RightTabButton(false));
		_ClothTabButton.onClick.AddListener(() => RightTabButton(true));

		// 시작
		_CharName = CharacterName.Hana;
		_ClothName = UserData.GetCharacterInfo(_CharName)._ClothName;
		RightTabButton(false);
	}

	void OnDestroy()
	{
		foreach (ModelLoadInfo info in _ModelLoadInfos)
		{
			if (info._Instance)
			{
				Addressables.Release(info._LoadHandle);
			}
		}
	}

	public bool OnBackButton()
	{
		return false;
	}

	void Refresh()
	{
		// 왼쪽
		Character info = Data.GetCharacter(_CharName);
		_NameText.text = info._DisplayName;
		if (_StoryPanelOpen)
		{
			_StoryElements.DestroyElements();
			for (int i = 0; i < info._StoryScripts.Count; i++)
			{
				UI_StoryElement element = Instantiate(_StoryElementPrefab, _StoryElementParent);
				element.gameObject.SetActive(true);
				element._Index = i;
				Script script = info._StoryScripts[i];
				element._Button.onClick.AddListener(() => StoryElementButton(element, script));
				int n = i + 1;
				element._Text.text = $"Episode {n} -  일상{n}";
				_StoryElements.Add(element);

				// 레드닷
				UserInfoManager.SaveData.CharacterInfo userData = UserData.GetCharacterInfo(info._Name);
				bool notSeenStory3 = !userData._HaveSeenStorys.Contains(i);
				element._Button.RedDot.SetActive(notSeenStory3);
			}
		}

		// 캐릭터 탭
		if (_CharacterTab.gameObject.activeSelf)
		{
			_CharElements.DestroyElements();
			foreach (Character charInfo in Data._Characters)
			{
				UI_Room_Element element = Instantiate(_CharacterElementPrefab, _CharacterTab);
				element.gameObject.SetActive(true);
				element._CharacterName = charInfo._Name;
				element._Button.onClick.AddListener(() => CharElementButton(element));
				element._Icon.sprite = charInfo._Thumbnail2;
				element._Name.text = charInfo._DisplayName;
				element._Selected.SetActive(charInfo._Name == _CharName);
				_CharElements.Add(element);

				// 레드닷
				bool anyNotSeenStory2 = false;
				UserInfoManager.SaveData.CharacterInfo userData = UserData.GetCharacterInfo(charInfo._Name);
				for (int storyIndex = 0; storyIndex < charInfo._StoryScripts.Count; storyIndex++)
				{
					if (!userData._HaveSeenStorys.Contains(storyIndex))
					{
						anyNotSeenStory2 = true;
						break;
					}
				}
				element._Button.RedDot.SetActive(anyNotSeenStory2);
			}
		}
		// 복장 탭
		else
		{
			_ClothElements.DestroyElements();
			Character charInfo = Data.GetCharacter(_CharName);
			foreach (CharacterCloth clothInfo in charInfo._Clothes)
			{
				UI_Room_Element element = Instantiate(_ClothElementPrefab, _ClothTab);
				element.gameObject.SetActive(true);
				element._ClothName = clothInfo._Name;
				element._Button.onClick.AddListener(() => ClothElementButton(element));
				element._Icon.sprite = clothInfo._Icon;
				element._Selected.SetActive(element._ClothName == _ClothName);
				_ClothElements.Add(element);
			}
		}

		// 모델 로드
		CharacterCloth clothData = Data.GetCharacter(_CharName).GetCloth(_ClothName);
		ModelLoadInfo info2 = _ModelLoadInfos.Find(x => x._Data == clothData);
		if (info2 == null)
		{
			info2 = new()
			{
				_CharName = _CharName,
				_Data = clothData
			};
			_ModelLoadInfos.Add(info2);
		}
		if (!info2._Instance)
		{
			info2._LoadHandle = Addressables.LoadAssetAsync<GameObject>(clothData._Model);
			info2._LoadHandle.WaitForCompletion();
			info2._Instance = Instantiate(info2._LoadHandle.Result, _ModelParent);
			info2._Instance.GetComponent<Animator>().runtimeAnimatorController = _AnimatorAsset;
			Util.SetActiveSchoolUniform(info2._Instance, clothData);
		}

		// 활성화
		foreach (ModelLoadInfo modelInfo in _ModelLoadInfos)
		{
			if (!modelInfo._Instance) continue;
			bool active = modelInfo._CharName == _CharName && modelInfo._Data._Name == _ClothName;
			modelInfo._Instance.SetActive(modelInfo._CharName == _CharName && modelInfo._Data._Name == _ClothName);
			if (active)
			{
				_CharAnimator = modelInfo._Instance.GetComponent<Animator>();
				_CharAnimator.keepAnimatorStateOnDisable = false;

				// 환복 애니메이션
				if (_PlayClothAnim)
				{
					_AnimIdleTime = Time.time;
					_CharAnimator.CrossFade(ClothAnimName, 0.3f, 0, 0f);
					_PlayClothAnim = false;
				}
			}
		}

		// 애니메이션
		if (_AnimationCoroutine != null)
		{
			StopCoroutine(_AnimationCoroutine);
			_AnimationCoroutine = null;
		}
		_AnimationCoroutine = StartCoroutine(AnimationCoroutine());
	}

	void ExitButton()
	{
		Sound.PlaySfx(SoundManager.SfxType.Exit);
		User.Save();
		Game.LoadLobbyScene();
	}

	void StoryElementButton(UI_StoryElement element, Script script)
	{
		StartCoroutine(Internal());
		IEnumerator Internal()
		{
			yield return StartCoroutine(Game.LoadStoryScene(_CharName, element._Index, script));
			_CharAnimator.CrossFade(IdleAnimName, 0f);
		}
	}

	void StoryButton()
	{
		if (_StoryPanelTween != null)
		{
			StopCoroutine(_StoryPanelTween);
			_StoryPanelTween = null;
		}
		_StoryPanel.gameObject.SetActive(true);
		Util.PanelTween(_StoryPanel, Vector2.left);
		_StoryPanelOpen = true;
		Refresh();
	}

	void Story_BackButton()
	{
		_StoryPanelOpen = false;
		_StoryPanelTween = StartCoroutine(Internal());
		IEnumerator Internal()
		{
			Util.PanelTween(_StoryPanel, Vector2.left, true);
			yield return new WaitForSeconds(0.6f);
			_StoryPanel.gameObject.SetActive(false);
			_StoryPanelTween = null;
		}
	}

	void RightTabButton(bool isClothTab)
	{
		_CharacterTabButton.transform.Find("Active").gameObject.SetActive(!isClothTab);
		_ClothTabButton.transform.Find("Active").gameObject.SetActive(isClothTab);
		_CharacterTab.gameObject.SetActive(!isClothTab);
		_ClothTab.gameObject.SetActive(isClothTab);
		Refresh();
	}

	void CharElementButton(UI_Room_Element element)
	{
		if (_CharName == element._CharacterName) return;

		_ModelParent.eulerAngles = Vector3.zero;
		_CharName = element._CharacterName;
		_ClothName = UserData.GetCharacterInfo(_CharName)._ClothName;
		Refresh();
	}

	void ClothElementButton(UI_Room_Element element)
	{
		string clothName = Data.GetCharacter(_CharName).GetCloth(element._ClothName)._Name;
		if (_ClothName == clothName) return;

		_ClothName = clothName;
		UserData.GetCharacterInfo(_CharName)._ClothName = clothName;
		_PlayClothAnim = true;
		Refresh();
	}

	void CharacterDrag(PointerEventData eventData)
	{
		float amount = eventData.delta.x / Screen.width * 1000f;
		_ModelParent.eulerAngles = _ModelParent.eulerAngles.WithY(_ModelParent.eulerAngles.y - amount);
	}

	IEnumerator AnimationCoroutine()
	{
		_AnimIdleTime = Time.time;
		float waitTime = Random.Range(12f, 30f);
		while (true)
		{
			if (Time.time - _AnimIdleTime > waitTime)
			{
				_AnimIdleTime = Time.time;
				waitTime = Random.Range(12f, 30f);
				_CharAnimator.CrossFade(IdleLongAnimName, 0.3f);
			}
			yield return null;
		}
	}
}
