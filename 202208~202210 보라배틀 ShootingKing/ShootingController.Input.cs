using BoraBattle.Game.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoraBattle.Game.ShootingKing
{
	public partial class ShootingController
	{
		/// <summary>
		/// InputArea의 EventTrigger에서 호출
		/// </summary>
		public void InputAreaPointerDown(BaseEventData baseEventData)
		{
			if (isPaused) return;
			aimingStartTrigger = true;
			Vector2 pointerPosition = GetPointerScreenPosition(baseEventData);
			touchPosition = pressTouchPosition = pointerPosition;
		}

		/// <summary>
		/// InputArea의 EventTrigger에서 호출
		/// </summary>
		public void InputAreaDrag(BaseEventData baseEventData)
		{
			if (isPaused) return;
			touchPosition = GetPointerScreenPosition(baseEventData);
		}

		/// <summary>
		/// InputArea의 EventTrigger에서 호출
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
		/// 우측 상단 일시정지 버튼
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
		/// 일시정지 패널 배경음 버튼
		/// </summary>
		public void PausePanelBGMButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.BgmVolume = BGMMuted ? 1f : 0f;
			pauseBGMButton.overrideSprite = BGMMuted ? pauseBGMButtonOffSprite : null;
		}

		/// <summary>
		/// 일시정지 패널 효과음 버튼
		/// </summary>
		public void PausePanelSFXButton()
		{
			PlaySFX(sfxButton);
			GameInterface.Interface.Sound.SfxVolume = SFXMuted ? 1f : 0f;
			pauseSFXButton.overrideSprite = SFXMuted ? pauseSFXButtonOffSprite : null;
		}

		/// <summary>
		/// 일시정지 패널 게임 방법 버튼
		/// </summary>
		public void PausePanelTutorialButton()
		{
			PlaySFX(sfxButton);
			SetPanel(tutorialPanel);
			SetTutorialPanelIndex(0);
		}

		/// <summary>
		/// 일시정지 패널 게임 종료 버튼
		/// </summary>
		public void PausePanelExitButton()
		{
			PlaySFX(sfxGameFinishConfirm);
			SetPanel(exitConfirmPanel);
		}

		/// <summary>
		/// 일시정지 패널 이어하기 버튼
		/// </summary>
		public void PausePanelContinueButton()
		{
			isPaused = false;
			Time.timeScale = 1f;
			SetPanel(null);
		}

		/// <summary>
		/// 종료 확인 패널 게임 종료 버튼
		/// </summary>
		public void ExitConfirmPanelExitButton()
		{
			userExitDemand = true;
		}

		/// <summary>
		/// 종료 확인 패널 이어하기 버튼
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
		/// 튜토리얼 패널 아무곳이나 터치 시
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
		/// 결과창 점수제출 버튼
		/// </summary>
		public void ResultPanelSubmitButton()
		{
			resultPanelNext = true;
		}

		/// <summary>
		/// 튜토리얼 건너뛰기, 게임 시작 버튼
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
