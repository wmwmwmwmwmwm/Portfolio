using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoraBattle.Game.WorldBasketballKing
{
	public partial class BasketballController
	{
		class Score
		{
			public ObscuredInt scoreCount;
			public ObscuredInt comboCount;
			public ObscuredInt scoreCountScore => scoreCount * scorePerScoreCount;
			public ObscuredInt comboCountScore => comboCount * scorePerComboCount;
			public ObscuredInt totalScore => scoreCountScore + comboCountScore;
		}

		const int scorePerScoreCount = 200;
		const int scorePerComboCount = 20;

		[Serializable]
		public class SeedData
		{
			public int from, to;
			public BasketballStage stage;
			public List<int> targetScores;
			public AudioClip bgm;
		}
	}
}