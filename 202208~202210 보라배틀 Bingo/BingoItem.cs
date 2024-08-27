using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoItem : MonoBehaviour
	{
		public enum ItemType { Select1, Select2, Horizontal, Vertical, RandomBall, AddTime, DoubleScore };
		public ItemType itemType;

		[NonSerialized] public ObscuredFloat remainedTime;
		[NonSerialized] public List<BingoNumberBox> selectedNumberBoxes;
		[NonSerialized] public BingoBall selectedBingoBall;

		void Start()
		{
			selectedNumberBoxes = new List<BingoNumberBox>();
		}

		public bool IsBlockingMain()
		{
			switch (itemType)
			{
				case ItemType.Select1:
				case ItemType.Select2:
				case ItemType.Horizontal:
				case ItemType.Vertical:
				case ItemType.RandomBall:
					if (remainedTime > 0f) return true;
					break;
			}
			return false;
		}

		public void SetRemainedTime(float newRemainedTime, TMP_Text textComponent)
		{
			remainedTime = newRemainedTime;
			textComponent.text = Mathf.Floor(remainedTime + 0.9f).ToString("0");
		}
	}
}