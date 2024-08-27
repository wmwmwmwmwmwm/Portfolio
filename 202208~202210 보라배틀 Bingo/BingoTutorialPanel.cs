using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoTutorialPanel : MonoBehaviour
	{
		public Sprite activeDotSprite;
		[Serializable]
		public class TutorialPage
		{
			public Sprite image;
			public string textKey;
		}
		public List<TutorialPage> tutorialPages;
		public Image tutorialImage;
		public TMP_Text tutorialText;
		public Button prevButton;
		public List<Image> dots;
		[NonSerialized] public int pageIndex;

		public void SetTutorialPage(int index)
		{
			pageIndex = index;
			tutorialImage.sprite = tutorialPages[index].image;
			tutorialText.text = BingoController.instance.GetLocalizedText(tutorialPages[index].textKey);
			for (int i = 0; i < dots.Count; i++)
			{
				Image dot = dots[i];
				dot.overrideSprite = i == pageIndex ? activeDotSprite : null;
			}
			prevButton.gameObject.SetActive(index > 0);
		}
	}
}