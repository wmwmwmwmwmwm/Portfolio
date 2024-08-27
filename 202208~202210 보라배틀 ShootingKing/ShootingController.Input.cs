using BoraBattle.Game.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoraBattle.Game.ShootingKing
{
	public partial class ShootingController
	{
		/// <summary>
		/// InputArea�� EventTrigger���� ȣ��
		/// </summary>
		public void InputAreaPointerDown(BaseEventData baseEventData)
		{
			if (isPaused) return;
			aimingStartTrigger = true;
			Vector2 pointerPosition = GetPointerScreenPosition(baseEventData);
			touchPosition = pressTouchPosition = pointerPosition;
		}

		/// <summary>
		/// InputArea�� EventTrigger���� ȣ��
		/// </summary>
		public void InputAreaDrag(BaseEventData baseEventData)
		{
			if (isPaused) return;
			touchPosition = GetPointerScreenPosition(baseEventData);
		}

		/// <summary>
		/// InputArea�� EventTrigger���� ȣ��
		/// </summary>
		public void InputAreaPointerUp(BaseEventData baseEventData)
		{
			StartCoroutine(PointerUpCoroutine());
			IEnumerator PointerUpCoroutine()
			{
				yield return null;
				if (isPaused) yield break;
				PlayerFireDemand();
			}
		}

		Vector2 GetPointerScreenPosition(BaseEventData baseEventData)
		{
			PointerEventData eventData = (PointerEventData)baseEventData;
			Vector2 pointerPosition = eventData.position;
			pointerPosition.x /= Screen.width;
			pointerPosition.y /= Screen.width;
			return pointerPosition;
		}

		/// <summary>
		/// ���� ��� �Ͻ����� ��ư
		/// </summary>
		public void PauseButton()
		{
			PlaySFX(sfxButton);
			isPaused = true;
			Time.timeScale = 0f;
			pauseBGMButton.overrideSprite = BGMMuted ? pauseBGMButtonOffSprite : null;
			pauseSFXButton.overrideSprite = SFXMuted ? pauseSFXButtonOffSprite : null;
			SetPanel(pausePanel);
		}

		/// <summary>
		/// �Ͻ����� �г� ����� ��ư
		/// </summary>
		public void PausePanelBGMButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.BgmVolume = BGMMuted ? 1f : 0f;
			pauseBGMButton.overrideSprite = BGMMuted ? pauseBGMButtonOffSprite : null;
		}

		/// <summary>
		/// �Ͻ����� �г� ȿ���� ��ư
		/// </summary>
		public void PausePanelSFXButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.SfxVolume = SFXMuted ? 1f : 0f;
			pauseSFXButton.overrideSprite = SFXMuted ? pauseSFXButtonOffSprite : null;
		}

		/// <summary>
		/// �Ͻ����� �г� ���� ��� ��ư
		/// </summary>
		public void PausePanelTutorialButton()
		{
			PlaySFX(sfxButton);
			SetPanel(tutorialPanel);
			SetTutorialPanelIndex(0);
		}

		/// <summary>
		/// �Ͻ����� �г� ���� ���� ��ư
		/// </summary>
		public void PausePanelExitButton()
		{
			PlaySFX(sfxGameFinishConfirm);
			SetPanel(exitConfirmPanel);
		}

		/// <summary>
		/// �Ͻ����� �г� �̾��ϱ� ��ư
		/// </summary>
		public void PausePanelContinueButton()
		{
			isPaused = false;
			Time.timeScale = 1f;
			SetPanel(null);
		}

		/// <summary>
		/// ���� Ȯ�� �г� ���� ���� ��ư
		/// </summary>
		public void ExitConfirmPanelExitButton()
		{
			userExitDemand = true;
		}

		/// <summary>
		/// ���� Ȯ�� �г� �̾��ϱ� ��ư
		/// </summary>
		public void ExitConfirmPanelContinueButton()
		{
			SetPanel(pausePanel);
		}

		public void TutorialPanelCloseButton()
		{
			PlaySFX(sfxButton);
			SetPanel(pausePanel);
		}

		/// <summary>
		/// Ʃ�丮�� �г� �ƹ����̳� ��ġ ��
		/// </summary>
		public void TutorialPanelNextButton()
		{
			PlaySFX(sfxButton);
			if (tutorialPanelIndex < tutorialPanelImages.Count - 1)
			{
				SetTutorialPanelIndex(tutorialPanelIndex + 1);
			}
			else
			{
				TutorialPanelCloseButton();
			}
		}

		void SetTutorialPanelIndex(int newIndex)
		{
			tutorialPanelIndex = newIndex;
			tutorialPanelImages.ForEach(x => x.gameObject.SetActive(false));
			tutorialPanelOverlays.ForEach(x => x.gameObject.SetActive(false));
			tutorialPanelImages[tutorialPanelIndex].gameObject.SetActive(true);
			tutorialPanelOverlays[tutorialPanelIndex].gameObject.SetActive(true);
		}

		/// <summary>
		/// ���â �������� ��ư
		/// </summary>
		public void ResultPanelSubmitButton()
		{
			resultPanelNext = true;
		}

		/// <summary>
		/// Ʃ�丮�� �ǳʶٱ�, ���� ���� ��ư
		/// </summary>
		public void TutorialSkipButton()
		{
			tutorialSkip = true;
		}

		void SetPanel(GameObject panel)
		{
			pausePanel.SetActive(false);
			exitConfirmPanel.SetActive(false);
			tutorialPanel.SetActive(false);
			resultPanel.gameObject.SetActive(false);
			panel?.SetActive(panel);
			topArea.GetComponent<CanvasGroup>().alpha = panel ? 0f : 1f;
		}
	}
}
