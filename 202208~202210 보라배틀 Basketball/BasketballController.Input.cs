using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoraBattle.Game.WorldBasketballKing
{
	public partial class BasketballController
	{
		public void OnBallPointerDown(PointerEventData eventData)
		{
			if (draggingBall) return;
			draggingBall = eventData.pointerCurrentRaycast.gameObject.GetComponent<BasketballBall>();
			Vector3 pointerPosition = ScreenToWorldPoint(eventData.position);
			for (int i = 0; i < lastPointerPositionsSize; i++)
			{
				lastPointerPositions[i] = pointerPosition;
			}
			draggingBall.thisRigidbody.useGravity = false;
			draggingBall.thisRigidbody.velocity = Vector3.zero;
			draggingBall.thisRigidbody.angularVelocity = Vector3.zero;
			draggingBall.thisCollider.enabled = false;
			draggingBall.isCleanShot = true;
			draggingBall.topGoalPass = false;
			draggingBall.bottomGoalPass = false;
			draggingBall.goal = false;
		}

		public void OnBallDrag(PointerEventData eventData)
		{
			if (!draggingBall) return;
			Vector3 pointerPosition = ScreenToWorldPoint(eventData.position);
			pointerPosition.y = Mathf.Max(pointerPosition.y, inputBottomPoint.position.y);
			draggingBall.transform.position = pointerPosition;
			if (draggingBall.transform.position.y > -1.8f)
			{
				OnBallPointerUp(eventData);
			}

			// 최근 0.5초의 드래그 포지션을 0.05초마다 저장
			if (Time.time - lastPointerUpdateTime > 0.5f / lastPointerPositionsSize)
			{
				lastPointerUpdateTime = Time.time;
				for (int i = 1; i < lastPointerPositionsSize; i++)
				{
					lastPointerPositions[i - 1] = lastPointerPositions[i];
				}
				lastPointerPositions[^1] = pointerPosition;
			}
		}

		public void OnBallPointerUp(PointerEventData eventData)
		{
			if (!draggingBall) return;
			draggingBall.middleColliderPass = false;
			PlaySFX(sfxShoot);
			Vector3 pointerUpPosition = ScreenToWorldPoint(eventData.position);
			float minY = lastPointerPositions.Min(x => x.y);
			float shootPower = pointerUpPosition.y - minY;
			Vector3 shootVelocity;
			if (shootPower > 1.5f)
			{
				shootPower = Mathf.Min(5f, shootPower);
				shootVelocity = Quaternion.Euler(-63f, 0f, 0f) * Vector3.forward * (25f + shootPower);
				draggingBall.thisRigidbody.angularVelocity += draggingBall.thisRigidbody.angularVelocity.WithX(-10f);
			}
			else
			{
				shootVelocity = Quaternion.Euler(-63f, 0f, 0f) * Vector3.forward * shootPower;
			}
			draggingBall.thisRigidbody.velocity = shootVelocity;
			draggingBall.thisRigidbody.useGravity = true;
			draggingBall.thisCollider.enabled = true;
			draggingBall = null;
		}

		Vector3 ScreenToWorldPoint(Vector2 screenPosition)
		{
			screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
			screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);
			return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 16f));
		}

		public void PauseButton()
		{
			PlaySFX(sfxButton);
			Time.timeScale = 0f;
			pausePanel.SetActive(true);
		}

		public void PausePanelTutorialButton()
		{
			PlaySFX(sfxButton);
			tutorialPanel.SetActive(true);
			SetTutorialPage(0);
		}

		public void PausePanelExitButton()
		{
			PlaySFX(sfxButton);
			exitConfirmPanel.SetActive(true);
		}

		public void PausePanelContinueButton()
		{
			Time.timeScale = 1f;
			pausePanel.SetActive(false);
		}

		public void ExitPanelExitButton()
		{
			userExitDemand = true;
		}

		public void ExitPanelContinueButton()
		{
			exitConfirmPanel.SetActive(false);
		}

		public void ResultPanelSubmitButton()
		{
			resultPanelNext = true;
		}

		public void TutorialPanelCloseButton()
		{
			PlaySFX(sfxButton);
			tutorialPanel.SetActive(false);
		}

		public void TutorialPanelPrevButton()
		{
			PlaySFX(sfxButton);
			SetTutorialPage(tutorialPanelIndex - 1);
		}

		public void TutorialPanelNextButton()
		{
			PlaySFX(sfxButton);
			if (tutorialPanelIndex == tutorialSprites.Count - 1)
			{
				TutorialPanelCloseButton();
			}
			else
			{
				SetTutorialPage(tutorialPanelIndex + 1);
			}
		}

		void SetTutorialPage(int index)
		{
			tutorialPanelIndex = index;
			for (int i = 0; i < tutorialPages.Count; i++)
			{
				tutorialPages[i].SetActive(tutorialPanelIndex == i);
			}
			tutorialImage.sprite = tutorialSprites[tutorialPanelIndex];
			tutorialPrevButton.SetActive(tutorialPanelIndex != 0);
		}
	}
}