using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static SingletonManager;

public partial class LobbyController
{
	[Header("편성")]
	public GameObject _Troop;
	public List<Transform> _Troop_CharacterPositions;
	public List<GameObject> _Troop_CharNames;
	public List<DraggableUI> _Troop_CharDraggables;
	public Canvas _Troop_DragHandle;
	public UI_Button _Troop_EnterButton;

	int _Troop_DraggingIndex;

	void Troop_Show(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			_Troop_DragHandle.gameObject.SetActive(false);
			if (Game._IsSingleMode)
			{
				ViewSetting(ViewType.Stage);
				Stage_Refresh();
			}
			else
			{
				ViewSetting(ViewType.Multi);
			}
			_Troop.SetActive(false);

			// 애니메이션 플래그 재설정
			_CharInfos.ForEach(x => x._Troop_GunAnimFlag = false);

			return;
		}

		ViewSetting(ViewType.Troop);
		_Troop_DragHandle.gameObject.SetActive(false);
		_Troop.SetActive(true);
		UI.FadeIn(0.3f);
		Troop_Refresh();
	}

	void Troop_Refresh()
	{
		// 모델
		HideModels();
		for (int i = 0; i < UserData.Troop.Count; i++)
		{
			CharacterName charName = UserData.Troop[i];
			if (charName < 0) continue;

			ShowModel(ViewType.Troop, charName, i);
			CharInfo info = GetCharInfo(charName);
			if (info._HasGunPose && !info._Troop_GunAnimFlag)
			{
				info.GetAnimator().Play("GunPose", 0, 0f);
				info._Troop_GunAnimFlag = true;
			}
		}

		// 캐릭터 이름
		for (int i = 0; i < UserData.Troop.Count; i++)
		{
			CharacterName charName = UserData.Troop[i];
			bool valid = charName >= 0;
			_Troop_CharNames[i].SetActive(valid);
			if (!valid) continue;

			_Troop_CharNames[i].GetComponentInChildren<TMP_Text>().text = charName.ToString();
		}
	}

	void Troop_EnterButton()
	{
		Cards_Show(true, true);
	}

	void Troop_CharBeginDrag(GameObject dragObject, PointerEventData eventData)
	{
		DraggableUI draggable = dragObject.GetComponent<DraggableUI>();
		_Troop_DraggingIndex = _Troop_CharDraggables.IndexOf(draggable);
		_Troop_DragHandle.gameObject.SetActive(true);
	}

	void Troop_CharDrag(GameObject dragObject, PointerEventData eventData)
	{
		Vector2 canvasPos = eventData.position / _Troop_DragHandle.scaleFactor;
		RectTransform handle = _Troop_DragHandle.transform.GetChild(0).GetComponent<RectTransform>();
		handle.anchoredPosition = canvasPos;
	}

	void Troop_CharEndDrag(GameObject dragObject, GameObject dropPlaceObject, PointerEventData eventData)
	{
		if (dropPlaceObject && dropPlaceObject.TryGetComponent(out DraggableUI dropPlace))
		{
			int dropIndex = _Troop_CharDraggables.IndexOf(dropPlace);
			(UserData.Troop[_Troop_DraggingIndex], UserData.Troop[dropIndex]) = (UserData.Troop[dropIndex], UserData.Troop[_Troop_DraggingIndex]);
			Sound.PlaySfx(SoundManager.SfxType.Click);
		}

		_Troop_DragHandle.gameObject.SetActive(false);
		Troop_Refresh();
	}

	void Troop_CharClick(int index)
	{
		StartCoroutine(Internal());

		IEnumerator Internal()
		{
			CharacterName lastChar = UserData.Troop[index];
			Sound.PlaySfx(SoundManager.SfxType.Click);
			_Char_ShowFromMain = false;
			Char_Show(true, UserData.Troop[index]);
			_Char_TroopSelected = false;
			yield return new WaitUntil(() => !_Characters.activeSelf);
			Troop_Show(true);
			if (!_Char_TroopSelected) yield break;

			int already = UserData.Troop.IndexOf(_Char_CharacterName);
			if (already >= 0)
			{
				// 이미 선택됐으면 자리 바꾸기
				(UserData.Troop[already], UserData.Troop[index]) = (UserData.Troop[index], UserData.Troop[already]);
			}
			else
			{
				UserData.Troop[index] = _Char_CharacterName;
			}

			// 애니메이션 플래그 재설정
			GetCharInfo(_Char_CharacterName)._Troop_GunAnimFlag = false;
			GetCharInfo(lastChar)._Troop_GunAnimFlag = false;

			Troop_Refresh();
		}
	}
}
