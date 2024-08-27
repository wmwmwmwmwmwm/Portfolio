using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BoraBattle.Game.ShootingKing
{
	public partial class ShootingController
	{
		[Serializable]
		public class MapIdData
		{
			public int from, to;
			public GameMode gameMode;
			public int stageIndex;
			public int gameTime;
			public int zoomTime => gameTime switch
			{
				120 => 12,
				90 => 9,
				_ => 9
			};
			public float windMin, windMax;
		}

		[Serializable]
		public class GameModeData
		{
			public GameMode gameMode;
			public GameObject map;
			public GameObject rifle, scope;
			public List<ShootingStage> stages;
			public Material skybox;
			public AudioClip bgm;
		}

		[Serializable]
		public class FixedRandomData
		{
			public float windDirection;
			public float windSpeed;
			public int survivalTargetIndex;
		}

		[Serializable]
		public class Score
		{
			public ObscuredInt pointScore, perfectScore, niceScore, goodScore, timeBonus;
			public ObscuredInt perfectCount, niceCount, goodCount;
			public ObscuredInt TotalScore => pointScore + perfectScore + niceScore + goodScore + timeBonus;
		}
	}
}
