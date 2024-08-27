using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoNumberSummaryCell : MonoBehaviour
	{
		public Image cellImage;
		public TMP_Text cellText;

		public Sprite cellSpriteBlue, cellSpriteRed, cellSpritePurple, cellSpriteGreen, cellSpriteYellow;

		int number;

		public void SetNumber(int newNumber)
		{
			number = newNumber;
			cellText.text = number.ToString();
		}

		public void MarkCell()
		{
			if (number <= 15)
			{
				cellImage.sprite = cellSpriteBlue;
			}
			else if (number <= 30)
			{
				cellImage.sprite = cellSpriteRed;
			}
			else if (number <= 45)
			{
				cellImage.sprite = cellSpritePurple;
			}
			else if (number <= 60)
			{
				cellImage.sprite = cellSpriteGreen;
			}
			else
			{
				cellImage.sprite = cellSpriteYellow;
			}
			cellImage.color = Color.white;
			cellText.color = Color.white;
		}
	}
}
