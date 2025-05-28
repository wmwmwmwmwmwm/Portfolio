using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SingletonManager;
using static SoundManager;

public class UI_Button : Button 
{
	UI_ButtonExpansion _ButtonExpansion;

	public GameObject RedDot => transform.Find("RedDot").gameObject;

	protected override void Start()
	{
		base.Start();

		_ButtonExpansion = GetComponent<UI_ButtonExpansion>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();

		if (!Application.isPlaying) return;
		if (transition == Transition.Animation)
		{
			// 시작 시 Normal 애니메이션 방지
			animator.ResetTrigger("Normal");

			ResetValues();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		if (!Application.isPlaying) return;
		if (transition == Transition.Animation)
		{
			ResetValues();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		if (!interactable) return;
		SfxType sfxType = SfxType.MouseOver;
		if (_ButtonExpansion)
		{
			sfxType = _ButtonExpansion._HoverSound;
		}
		Sound.PlaySfx(sfxType);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerClick(eventData);

		if (!interactable) return;
		SfxType sfxType = SfxType.Click;
		if (_ButtonExpansion)
		{
			sfxType = _ButtonExpansion._ClickSound;
		}
		Sound.PlaySfx(sfxType);

		// 누른 이후 Highlight 애니메이션 재생안되는 현상 방지
		EventSystem.current.SetSelectedGameObject(null);
	}

	void ResetValues()
	{
		// 초기화
		CanvasGroup[] canvasGroups = GetComponentsInChildren<CanvasGroup>(true);
		foreach (CanvasGroup canvasGroup in canvasGroups)
		{
			canvasGroup.alpha = canvasGroup.name == "Normal" || canvasGroup.name == "TextOff" ? 1f : 0f;
		}
		if (isActiveAndEnabled)
		{
			animator.Play("Normal", 0, 1f);
		}
	}
}
