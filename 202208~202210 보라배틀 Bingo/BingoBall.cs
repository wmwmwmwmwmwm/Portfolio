using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoBall : MonoBehaviour
	{
		public List<Sprite> ballSpriteAssets;
		public Image ballSprite, timerBackgroundImage, timerFillImage;
		public BingoImageNumber imageNumber;

		public bool Expired => remainedTime < 0f;
		[NonSerialized] public ObscuredFloat remainedTime;
		[NonSerialized] public ObscuredFloat appearingTime;
		public float RemainedTimePercent => remainedTime / BingoController.instance.BingoBallDuration;

		public void Initialize(int number, float duration)
		{
			imageNumber.SetNumber(number);
			int spriteIndex = (number - 1) / 15;
			ballSprite.sprite = ballSpriteAssets[spriteIndex];
			remainedTime = duration;
			appearingTime = BingoController.instance.appearingDuration;
		}

		public void SetRemainedTime(float newRemainedTime)
		{
			remainedTime = newRemainedTime;
			timerFillImage.fillAmount = RemainedTimePercent;
			timerBackgroundImage.gameObject.SetActive(remainedTime > 0f);
			timerFillImage.gameObject.SetActive(remainedTime > 0f);
		}
	}
}
