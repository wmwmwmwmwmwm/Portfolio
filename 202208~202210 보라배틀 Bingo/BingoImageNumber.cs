using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoImageNumber : MonoBehaviour
	{
		public List<Sprite> numberSpriteAssets;
		public List<Image> numberImages;

		public int number;

		public void SetNumber(int newNumber)
		{
			number = newNumber;
			int ten = number / 10;
			int one = number % 10;
			numberImages[0].gameObject.SetActive(ten > 0);
			numberImages[0].sprite = numberSpriteAssets[ten];
			numberImages[1].sprite = numberSpriteAssets[one];
			numberImages[0].SetNativeSize();
			numberImages[1].SetNativeSize();
		}
	}
}
