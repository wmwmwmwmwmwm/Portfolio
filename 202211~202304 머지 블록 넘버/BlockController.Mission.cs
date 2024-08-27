using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using static GameInterface;
using System.Collections.Generic;
using static SoundManager;

public partial class BlockController
{
	void UpdateMissionUiIcons(List<MissionUI> uiList, List<Mission> _missions)
	{
		for (int i = 0; i < uiList.Count; i++)
		{
			MissionUI missionUI = uiList[i];
			if (i < _missions.Count)
			{
				missionUI.gameObject.SetActive(true);
				Mission mission = _missions[i];
				mission.UpdateUI(missionUI.image1, missionUI.image2, uiBlocks);
			}
			else
			{
				missionUI.gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// 게임 진행에 따른 미션 설명 정보를 갱신한다.
	/// </summary>
	void UpdateMissions()
	{
		for (int i = 0; i < missions.Count; i++)
		{
			Mission mission = missions[i];
			mission.UpdateInGameUI(missionUIs[i].textMain);
		}
	}

	void OnNumberChange(Block block)
	{
		// 점수 상승
		long number = TypeHelper.NumberTypeToLong(block.data.numberType);
		SetScore(score + number, false);
		MissionCountEffect(block).Forget();

		// 목표 숫자 미션 카운트
		foreach (Mission mission in missions)
		{
			switch (mission.missionType)
			{
				case MissionType.Number:
					Mission_Number missionNumber = (Mission_Number)mission;
					if (block.canMerge && block.data.numberType == missionNumber.missionNumberType)
					{
						missionNumber.clearCount++;
						MissionCountEffect(mission, block).Forget();
					}
					break;
			}
		}

		CheckTargetCellMissionEffect();

		// 퀘스트 카운팅
		long longNumber = TypeHelper.NumberTypeToLong(block.data.numberType);
		int questNumber = longNumber > int.MaxValue ? int.MaxValue : (int)longNumber;
		UserMgr.UpdateQuestCount(QuestType.TotalNumber, questNumber);
		if (block.data.numberType >= NumberType.N2K)
		{
			UserMgr.UpdateQuestCount(QuestType.Over2048, 1);
		}
	}

	/// <summary>
	/// 블록 점수가 1024T를 넘었을때 호출되는 특수 케이스 함수.
	/// </summary>
	/// <param name="block">병합 위치 블록</param>
	/// <param name="count">병합된 블록 개수</param>
	void OnNumberChangeOverMaxNumberType(Block block, int count)
	{
		// 점수 상승
		long number = TypeHelper.IntNumberTypeToLong((int)NumberType.N1024T + count);
		SetScore(score + number, false);
		MissionCountEffect(block).Forget();

		// 목표 숫자 미션 카운트
		foreach (Mission mission in missions)
		{
			switch (mission.missionType)
			{
				case MissionType.Number:
					Mission_Number missionNumber = (Mission_Number)mission;
					if (block.canMerge && block.data.numberType == missionNumber.missionNumberType)
					{
						missionNumber.clearCount++;
						MissionCountEffect(mission, block).Forget();
					}
					break;
			}
		}

		CheckTargetCellMissionEffect();

		// 퀘스트 카운팅
		long longNumber = TypeHelper.NumberTypeToLong(block.data.numberType);
		int questNumber = longNumber > int.MaxValue ? int.MaxValue : (int)longNumber;
		UserMgr.UpdateQuestCount(QuestType.TotalNumber, questNumber);
		if (block.data.numberType >= NumberType.N2K)
		{
			UserMgr.UpdateQuestCount(QuestType.Over2048, 1);
		}
	}

	void CheckTargetCellMissionEffect()
	{
		// 지정 위치 미션 이펙트
		foreach (Mission mission in missions)
		{
			switch (mission.missionType)
			{
				case MissionType.TargetCell:
					Mission_TargetCell missionTargetCell = (Mission_TargetCell)mission;
					List<Cell> missionCells = missionTargetCell.GetMissionCells();
					foreach (Cell cell in missionCells)
					{
						Block block = cell.accupiedBlock;
						if (block && (block.movedTurn > 0 || block.mergedTurn > 0) && block.canMerge && block.data.numberType == missionTargetCell.targetNumber)
						{
							MissionCountEffect(mission, block).Forget();
						}
					}
					break;
			}
		}
	}

	public async UniTask MissionCountEffect(Block block)
	{
		MissionType missionType = block.blockTypeInGame switch
		{
			BlockType.Normal => MissionType.Score,
			BlockType.Target => MissionType.MergeBlock,
			BlockType.Escape => MissionType.EscapeBlock,
			BlockType.Copy => MissionType.CopyBlock,
			_ => throw new System.NotImplementedException()
		};
		Mission mission = missions.Find(x => x.missionType == missionType);
		if (mission)
		{
			await MissionCountEffect(mission, block);
		}
	}

	async UniTask MissionCountEffect(Mission mission, Block block)
	{
		Vector3 startPosition = block.transform.position;
		Vector3 endPosition = missionUIs[missions.IndexOf(mission)].image1.transform.position;
		Vector3 v = endPosition - startPosition;
		v = new Vector3(Mathf.Sign(v.x) * Random.Range(-120f, -180f), v.y * Random.Range(-0.1f, -0.2f), 0f);
		Vector3 midPosition = startPosition + v;

		missionEffectCount++;
		GameObject effect = InitializeEffect(startPosition, missionCountEffectPrefab, false);
		Sequence sequence = DOTween.Sequence()
			.Append(effect.transform.DOMove(midPosition, 0.4f).SetEase(Ease.OutSine))
			.Append(effect.transform.DOMove(endPosition, 0.4f).SetEase(Ease.InSine));
		await sequence;
		SoundMgr.PlaySfx(SfxType.Mission);
		GameObject endEffect = InitializeEffect(endPosition, missionCountDestroyEffectPrefab, false);
		await UniTask.WaitUntil(() => !endEffect);
		missionEffectCount--;
	}
}
