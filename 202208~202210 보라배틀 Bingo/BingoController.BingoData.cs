using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	// 게임 데이터 관리용 구조체, 클래스
	public partial class BingoController
	{
		BingoDataComparer bingoDataComparer;
		BingoNumberBoxComparer bingoNumberBoxComparer;

		[Serializable]
		public class SeedToGameModeData
		{
			public int from, to;
			public GameMode gameMode;
			public List<BingoItem.ItemType> usingItemTypes;
		}

		[Serializable]
		public class BingoData
		{
			public enum BingoType { Vertical, Horizontal, Cross, Edge };
			public BingoType bingoType;
			/// <summary>
			/// Horizontal일 때 Y좌표,
			/// Vertical일 때 X좌표,
			/// Cross일 때 0 또는 1,
			/// Edge일 때 0
			/// </summary>
			public int intData;
			public List<BingoNumberBox> associatedNumberBoxes;
		}

		class BingoDataComparer : IEqualityComparer<BingoData>, IComparer<BingoData>
		{
			public int Compare(BingoData x, BingoData y)
			{
				if (x.bingoType != y.bingoType) return x.bingoType - y.bingoType;
				else return x.intData - y.intData;
			}
			public bool Equals(BingoData x, BingoData y) => x.bingoType == y.bingoType && x.intData == y.intData;
			public int GetHashCode(BingoData obj) => obj.bingoType.GetHashCode() ^ obj.intData.GetHashCode();
		}

		[Serializable]
		public class BingoBoard
		{
			public GameObject root;
			public Image bg;
			public Color bgColor;
			public List<BingoNumberBox> boardLinear;
			[NonSerialized] public BingoNumberBox[,] board;
			[NonSerialized] public bool blackout;
			[NonSerialized] public List<BingoData> availableBingos;
			[NonSerialized] public List<BingoData> achievedBingos;
            [NonSerialized] public int lastAvailableBingoCount;
		}

		[Serializable]
		public class Score
		{
			public ObscuredInt markCount;
			public ObscuredInt timeBonus;
			public ObscuredInt bingoCount;
			public ObscuredInt boosterBonus;
			public ObscuredInt blackoutCount;
			public ObscuredInt remainedTime;
			public ObscuredInt blackoutBonus;
			public ObscuredInt penalty;

			public int markScore => markCount * scorePerMark;
			public int bingoScore => bingoCount * scorePerBingo;
			public int blackoutScore => blackoutCount * scorePerBlackout;
			public int TotalScore
			{
				get
				{
					return markScore + timeBonus + bingoScore + boosterBonus + blackoutScore + blackoutBonus + penalty;
				}
			}
		}

		const int scorePerMark = 300;
		const int scorePerBingo = 2000;
		const int scorePerBlackout = 10000;
		const int scorePerItem = 1000;
		const int remainedTimeScore = 44;
		const int penaltyWrongNumber = -50;
		const int penaltyWrongBingo = -200;
	}
}
