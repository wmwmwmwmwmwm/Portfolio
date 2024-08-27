using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInterface;
using static SoundManager;

public partial class BlockController
{
	void UpdateSkillButton()
	{
        bool bIsOnAutoSkill = UserMgr.bIsOnAutoSkill;

        skillAuto_Text.color = (bIsOnAutoSkill) ? Color.white : new Color32(255, 255, 255, 128);
        skillButtonPoint.GetChild(0).gameObject.SetActive(bIsOnAutoSkill);
		skillAutoButton.transform.GetChild(0).gameObject.SetActive(bIsOnAutoSkill);
		skillAutoButton.transform.GetChild(1).gameObject.SetActive(!bIsOnAutoSkill);
    }

	void SetSkillGauge(float newSkillGauge, bool immediate = false)
	{
		bool getFulled = skillGauge < 1f && newSkillGauge >= 1f;
		skillGauge = newSkillGauge;
		skillGaugeImage.DOKill();
		skillGaugeImage.DOFillAmount(skillGauge, immediate ? 0f : 0.7f);
		skillGaugeGrayImage.DOKill();
		skillGaugeGrayImage.DOColor(Color.white.WithAlpha(1f - skillGauge), immediate ? 0f : 0.7f);
		if (!immediate)
		{
			if (getFulled)
            {
                ShowTutorial(TutorialPopup.tutorialSkill).Forget();
                skillGaugeFullEffect = InitializeEffect(skillButtonPoint.transform.position, skillGaugeFullEffectPrefab, false);
			}
			else
			{
				InitializeEffect(skillButtonPoint.transform.position, skillGaugeEffectPrefab, false);
			}
		}
	}

	async UniTask ActivateSkill()
	{
		SoundMgr.PlaySfx(SfxType.UseSkill);
		List<Block> skillTargetBlocks = new(boardBlocks);
		skillTargetBlocks = skillTargetBlocks.FindAll(x => x.data.blockType == BlockType.Normal && x != currentDropBlock);
		if (skillTargetBlocks.Count == 0) return;

		UserMgr.UpdateQuestCount(QuestType.UseSkill, 1);
		skillGauge = 0f;
		skillGaugeImage.fillAmount = 1f;
		skillGaugeImage.DOKill();
		skillGaugeImage.DOFillAmount(0f, 0.3f).ToUniTask().Forget();
		skillGaugeGrayImage.DOKill();
		skillGaugeGrayImage.DOColor(Color.white.WithAlpha(1f), 0.3f).ToUniTask().Forget();
		if (skillGaugeFullEffect) Destroy(skillGaugeFullEffect);

		userMovePause = true;
		NumberType minNumber = (NumberType)Mathf.Max((int)skillTargetBlocks.Min(x => x.data.numberType), 1);
		List<Block> minBlocks = skillTargetBlocks.FindAll(x => x.data.numberType == minNumber);
		List<UniTask> lineEffectTasks = new();
		foreach (Block block in minBlocks)
		{
			lineEffectTasks.Add(SkillEffect(block));
			await UniTask.Delay(100);
		}

		await lineEffectTasks;
		await BoardProcess();
		await CheckHighestNumber();
		combo = 0;
		fireBlockTrigger = false;
		userMovePause = false;

        // 쌓인 블록이 12개 이상 & 2줄 남았을때 경고 애니메이션 재생
        bool warning = (boardBlocks.Count > 12 && boardBlocks.Find(x => x.data.coord.y >= boardSize.y - 4));
        ShowWarningAlert(warning);

        async UniTask SkillEffect(Block blockAnimated)
		{
			Vector3 startPosition = skillButtonPoint.position;
			Vector3 endPosition = blockAnimated.transform.position;
			Vector3 v = (endPosition - startPosition).normalized;
			Vector3 cross = Vector3.Cross(v, Random.value < 0.5f ? Vector3.forward : Vector3.back);
			Vector3 midPosition = (startPosition + endPosition) / 2f + cross * 120f;
			GameObject skillEffect = InitializeEffect(startPosition, skillEffectPrefab, false);
			midPosition.z = skillEffect.transform.position.z;
			endPosition.z = skillEffect.transform.position.z;
			await skillEffect.transform.DOPath(new[] { midPosition, endPosition }, 0.4f, PathType.CatmullRom).SetEase(skillEffectEase);

			SoundMgr.PlaySfx(SfxType.NumberMerge);
			blockAnimated.SetNumberType(blockAnimated.data.numberType + 1);
			blockAnimated.mergePriority = true;
			BlockMergeEffect(blockAnimated);
			OnNumberChange(blockAnimated);

			GameObject endEffect = InitializeEffect(endPosition, skillEndEffectPrefab, true);
			await UniTask.WaitUntil(() => !endEffect);
		}
	}
}
