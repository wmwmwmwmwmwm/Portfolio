using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoCardButton : MonoBehaviour
	{
		public Image image;
		public Sprite activeSprite;

        public void SetBingoAvailable(bool available)
		{
			GetComponent<Image>().overrideSprite = available ? activeSprite : null;
		}
	}
}
