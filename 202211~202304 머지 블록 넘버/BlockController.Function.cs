using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

#if !MS && !AWS
using Firebase.Analytics;
#endif

using static Column;
using static GameInterface;
using static ServerApi;
using static SoundManager;
using static BigIntegerUtil;
using static CommonDefines;
using static UnityEngine.ParticleSystem;

using BInteger = System.Numerics.BigInteger;

public partial class BlockController
{
	public float DirectionToRotation(Vector2Int v)
	{
		if (v == Vector2Int.right) return 0f;
		if (v == Vector2Int.up) return 90f;
		if (v == Vector2Int.left) return 180f;
		if (v == Vector2Int.down) return 270f;
		return 360f;
	}

	void ColumnAnimation(int activeColumnIndex, ColumnState columnState)
	{
		for (int i = 0; i < columns.Count; i++)
		{
			columns[i].SetColumnState(i == activeColumnIndex ? columnState : ColumnState.None);
		}
	}

	void SetScore(BInteger newScore, bool immediate)
	{
		float scoreToAnimate = BigIntToFloat(score);
		score = BInteger.Min(newScore, 9200000000000000000);	// ���� ���� �ִ� �ѵ��� 922���� �Ѿ�� ���ϰ� ���´�.
		float endScore = BigIntToFloat(score);
		if (immediate)
		{
			endlessModeScoreText.text = score.ToString("N0");
		}
		else
		{
			float animationTime = GetScoreAnimationTime(scoreToAnimate);
			DOTween.To(() => scoreToAnimate, x => scoreToAnimate = x, endScore, animationTime).SetEase(Ease.Linear).OnUpdate(() =>
			{
				endlessModeScoreText.text = scoreToAnimate.ToString("N0");
			});
		}
	}

	void BlockDropEffect(Block block)
	{
		GameObject effect = InitializeEffect(block.transform.position, dropEffectPrefab, true);
		if (block.canMerge)
		{
			ApplyEffectColor(effect, block);
		}
	}

	void BlockMergeEffect(Block block)
	{
		GameObject effect = InitializeEffect(block.transform.position, mergeEffectPrefab, true);
		if (block.canMerge)
		{
			ApplyEffectColor(effect, block);
		}
	}

	public void BlockDestroyEffect(Block block)
	{
		switch (block.blockTypeInGame)
		{
			case BlockType.Normal:
			case BlockType.Target:
				GameObject effect = InitializeEffect(block.transform.position, destroyBlockEffectPrefab, true);
				ApplyEffectColor(effect, block);
				break;
			case BlockType.Dummy:
				InitializeEffect(block.transform.position, destroyDummyEffectPrefab, true);
				break;
			case BlockType.Ice:
				GameObject effect2 = InitializeEffect(block.transform.position, destroyIceEffectPrefab, true);
				ApplyEffectColor(effect2, block);
				break;
		}
	}

	public void DummyBlockSetHPEffect(Block block)
	{
		SoundMgr.PlaySfx(SfxType.BlockDummy);
		InitializeEffect(block.transform.position, dummySetHpEffectPrefab, true);
	}

	void ConnectorDestroyEffect(Connector connector)
	{
		InitializeEffect(connector.transform.position, destroyConnectorEffectPrefab, true);
	}

	public void CopyBlockStartEffect(Block block)
	{
		InitializeEffect(block.transform.position, copyStartEffectPrefab, true);
	}

	public void CopyBlockEndEffect(Block block)
	{
		InitializeEffect(block.transform.position, copyEndEffectPrefab, true);
	}

	public void AllMergeBlockEffect(Vector3 spawnPosition)
	{
		InitializeEffect(spawnPosition, allMergeEffectPrefab, true);
	}

	public void TargetBlockEffect(Block block)
	{
		SoundMgr.PlaySfx(SfxType.BlockTarget);
		GameObject effect = InitializeEffect(block.transform.position, targetBlockEffectPrefab, true);
		ApplyEffectColor(effect, block);
	}

	GameObject InitializeEffect(Vector3 effectPosition, GameObject effectPrefab, bool adjustScale)
	{
		effectPosition.z = mainCanvas.transform.position.z - 300f;
		GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.identity);
		effect.transform.SetParent(boardParticleFrontLayer, true);
		if (adjustScale)
		{
			effect.GetComponent<UIParticle>().scale = boardScaleMultiplier;
		}
		return effect;
	}

	void ApplyEffectColor(GameObject effect, Block block)
	{
		foreach (Transform particle in effect.transform)
		{
			if (!particle.TryGetComponent(out ParticleSystem particleComponent)) continue;
			if (!particle.name.EndsWith("_Color")) continue;
			MainModule mainModule = particleComponent.main;
			float startColorAlpha = mainModule.startColor.color.a;
			mainModule.startColor = BlockFactoryInst.GetBlockColor(block.data.numberType).WithAlpha(startColorAlpha);
		}
	}

	async void Retry()
	{
		Time.timeScale = 1f;
		StopGame();
		switch (gameMode)
		{
			case GameMode.Endless:
				DeleteSavedStage();
				break;
		}
		await CommonCanvas.screenTransition.FadeOut();
		SceneManager.LoadScene(SceneName.InGame);

#if !MS && !AWS
        FirebaseAnalytics.LogEvent("STAGE_Retry");
#endif
    }

	async UniTask Exit()
	{
        Time.timeScale = 1f;
		StopGame();
		await CommonCanvas.screenTransition.FadeOut();
		AdMgr.HideBannerAd();
        SceneManager.LoadScene(SceneName.MainMenu);
    }

	async void ComboEffect(Vector3 spawnPosition)
	{
        Material comboMat;
        int iAnim = 1;
        SfxType sfx = SfxType.NumberMerge;
        if (combo <= 3)
        {
            comboMat = comboMat1;
            iAnim = 1;
        }
        else if (combo <= 5)
        {
            comboMat = comboMat2;
            iAnim = 2;
            sfx = SfxType.ComboMerge;
        }
        else
        {
            comboMat = comboMat3;
            iAnim = 3;
            sfx = SfxType.MultiComboMerge;
        }
        SoundMgr.PlaySfx(sfx);

        GameObject comboEffect = Instantiate(comboEffectPrefab, boardParticleFrontLayer);
		comboEffect.transform.position = spawnPosition + comboEffectPrefab.transform.position;
		TMP_Text comboText = comboEffect.transform.GetChild(0).GetComponent<TMP_Text>();
		comboText.text = string.Format("COMBO {0}", combo);
		comboText.fontMaterial = comboMat;
		comboText.GetComponent<Animator>().SetInteger("iAnim", iAnim);
		Debug.Log("combo: " + combo);
		await UniTask.Delay(1000);
		Destroy(comboEffect);
	}

	void AutoSaveStage()
	{
		autoSavedStage = StageMgr.DeepCopyStage(stageData);
		autoSavedStage.stageBlocks.Clear();
		for (int i = 0; i < boardBlocks.Count; i++)
		{
			if (boardBlocks[i] == currentDropBlock) continue;
			autoSavedStage.stageBlocks.Add(boardBlocks[i].data.GetCopy());
		}
	}

	void DeleteSavedStage()
	{
		autoSavedStage = null;
		StageMgr.DeleteStageJsonFile(boardSize.x);
	}

	void AutoSaveStageToDisk()
	{
		if (!autoSavedStage) return;
		StageMgr.SaveStageToJson(new IngameSaveData
		{
			stage = autoSavedStage,
			bIsDecreaseDropSpeedEnabled = slowDropSpeedEnabled,
			bIsIncreaseSkillGaugeEnabled = increaseSkillGaugeEnabled,
			score = this.score.ToString(),
			skillGauge = this.skillGauge,
			spawnBlockCount = this.spawnBlockCount,
			highestNumberType = this.highestNumberType,
			endlessCoinAccumulated = this.endlessCoinAccumulated,
			nContinue = this.nContinue,
			bIsInRanking = this.isInRanking
		}, boardSize.x);
	}

	Block GetHighestBlock()
	{
		NumberType highest = NumberType.N2;
		if (boardBlocks.Count > 0)
		{
			highest = boardBlocks.Max(x => x.data.numberType);
		}
		Block highestBlock = boardBlocks.Find(x => x.data.numberType == highest);
		return highestBlock;
	}

	float GetScoreAnimationTime(float beforeValue) => Math.Clamp((BigIntToFloat(score) - beforeValue) / 100f, 0.2f, 0.4f);

	async UniTask OnBeforeCreateBlock()
	{
		await CheckHighestNumber();

        // ���� ����� 12�� �̻� & 2�� �������� ��� �ִϸ��̼�
        bool warning = (boardBlocks.Count > 12 && boardBlocks.Find(x => x.data.coord.y >= boardSize.y - 4));
		ShowWarningAlert(warning);
	}

	async UniTask CheckHighestNumber()
	{
		switch (gameMode)
		{
			case GameMode.Endless:
				// ��� ���̽��ھ� ����, 1024�̻� ����̸� ���� ����
				Block highestBlock = GetHighestBlock();
				if (highestBlock)
				{
					// ���� �������� �ְ� ���� ����
					switch (boardSize.x)
					{
						case 4:
							UserMgr.EndlessModeHighestBlock47 = (NumberType)Mathf.Max((int)UserMgr.EndlessModeHighestBlock47, (int)highestBlock.data.numberType);
							break;
						case 6:
							UserMgr.EndlessModeHighestBlock67 = (NumberType)Mathf.Max((int)UserMgr.EndlessModeHighestBlock67, (int)highestBlock.data.numberType);
							break;
					}

					// ���� ������ �ְ� ���� ����
					NumberType oldValue = highestNumberType;
					highestNumberType = (NumberType)Mathf.Max((int)highestNumberType, (int)highestBlock.data.numberType);

					// ���� ������ �ְ� ���ڸ� �����ߴ��� �˻��Ѵ�.
					if(highestNumberType > oldValue)
					{
						// �ְ� ���� �޼� �˾��� ���� ��� ��ŷ ������ �ε�Ǿ� ���� �ʰų�, ������ 300�� �̳���� ��ŷ �����͸� �ε��Ѵ�.
						var type = (boardSize.x == 4) ? LeaderboardType.INFINITE47 : LeaderboardType.INFINITE67;
						bool canRegist = GameInfoMgr.CanRegistToRankingList(type, score);
                        if (!GameInfoMgr.IsDetailedRankingListAvailable(type) || (!isInRanking && canRegist))
						{
							if (canRegist)
							{
                                isInRanking = true;
								Debug.Log($"Load new ranking data");
                            }
                            GameInfoMgr.GetDetailedUserRankingList(type).Forget();
                        }
						else
						{
                            GameInfoMgr.GetRankPredictCurve(type).Forget();
						}

                        // 1024 �̻��� �ְ� ���� ���鶧���� ���� ����
                        if (highestNumberType >= NumberType.N1024)
                        {
                            int highestFrom1024 = Mathf.Max(0, highestNumberType - NumberType.N512);
                            int oldFrom1024 = Mathf.Max(0, oldValue - NumberType.N512);
                            int coinAmount = newHighestNumberCoinAmount * Mathf.Max(1, highestFrom1024 - oldFrom1024);
                            await ShowHighestNumberPopup(highestBlock, coinAmount);

                            // ���Ѹ�� 4x7�� ��� �ְ� ���ڸ� �����Ҷ����� ��� ���� ������ �����ؾ� �ϴ��� üũ�Ѵ�.
                            CheckBlockSpawnRangeOnInf47();
                        }
                    }
				}
				break;
		}
	}

	void ShowWarningAlert(bool active)
	{
		if (!warningImage.gameObject.activeSelf && active)
		{
			for(int x = 0; x < boardSize.x; x++)
			{
                boardCells[x, 5].warning.DOKill();
                boardCells[x, 5].warning.color = boardCells[x, 5].warning.color.WithAlpha(0f);
                boardCells[x, 5].warning.DOFade(1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
                boardCells[x, 5].warning.gameObject.SetActive(true);
            }

			warningImage.DOKill();
			warningImage.color = warningImage.color.WithAlpha(0f);
			warningImage.DOFade(0.3f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
		}
		else if (warningImage.gameObject.activeSelf && !active)
		{
            for (int x = 0; x < boardSize.x; x++)
            {
				boardCells[x, 5].warning.gameObject.SetActive(false);
            }
        }
		warningImage.gameObject.SetActive(active);
	}

    /// <summary>
    /// ���Ѹ�� 4x7���� ���� �ְ� ��� ������ ���� ��� ���� ������ �������ִ� �Լ�.
    /// </summary>
    void CheckBlockSpawnRangeOnInf47()
	{
		int MIN_HEIGHT = 6;

		// ���Ѹ�� 4x7�� �ƴ϶�� �ƹ��͵� ���� �ʴ´�.
		if (gameMode != GameMode.Endless || boardSize.x != 4) return;

		// height�� �ּҰ��̶�� ������ �������� �ʿ䰡 �����Ƿ� �����Ѵ�.
		if (height <= MIN_HEIGHT) return;

		// ���� ��� ���� ������ �����ؾ��ϴ� �ñⰡ �ƴ϶�� �ƹ��͵� ���� �ʴ´�.
		if (highestNumberType < NumberType.N2K) return;

		// ���� ��ô�� �� width���� ���� ��� ���� ������ �����Ѵ�.
		int progress = (highestNumberType - NumberType.N1024) % (int)NumberType.N1024;		// Ư�� ���� ����(K, M, B, T) ������ ���� ��Ȳ
		int offset = (highestNumberType - NumberType.N2K) / (int)NumberType.N1024;
        bool bIsIncreasing = progress % (width - offset) == 0;
		int range = (bIsIncreasing) ? 7 : 6;
        var spawnData = endlessSpawnDatas4x7[0];
		endlessSpawnDatas4x7[0].minDifference = spawnData.maxDifference + range - 1;

		Debug.LogFormat("Block spawn range is adjusted => range: {0}, maxDifference: {1}, minDifference: {2}", 
			range, endlessSpawnDatas4x7[0].maxDifference, endlessSpawnDatas4x7[0].minDifference);
	}

	async UniTask StageFailAnimation()
	{
		warningImage.DOKill();
		for (int x = 0; x < boardSize.x; x++) boardCells[x, 5].warning.DOKill();
		SoundMgr.PlaySfx(SfxType.GameOver);

		for (int y = boardSize.y - 1; y >= 0; y--)
		{
			List<Block> blocks = boardBlocks.FindAll(x => x.data.coord.y == y);
			List<UniTask> tasks = new();
			foreach (Block block in blocks)
			{
				Color.RGBToHSV(block.baseImage.color, out float h, out _, out float v);
				Color destColor = Color.HSVToRGB(h, 0f, v * 0.5f);
				tasks.Add(block.baseImage.DOColor(destColor, 0.2f).SetEase(Ease.Linear).ToUniTask());
				tasks.Add(block.overlayImageBeforeText.DOColor(Color.gray, 0.2f).SetEase(Ease.Linear).ToUniTask());
				block.transform.DOShakePosition(0.2f, strength: 2f, vibrato: 100, snapping: true, fadeOut: false).ToUniTask().Forget();
			}
			await tasks;
		}
		await UniTask.Delay(1200);
	}

	void StageFailAnimationRevert()
	{
		foreach (Block block in boardBlocks)
		{
			block.baseImage.color = Color.white;
			block.overlayImageBeforeText.color = Color.white;
			block.SetNumberType(block.data.numberType);
		}
	}

	void DestroyObjects<T>(List<T> list) where T : MonoBehaviour
	{
		foreach (T obj in list)
		{
			if (obj) Destroy(obj.gameObject);
		}
		list.Clear();
	}

	void PanelPopEffect(GameObject panel)
	{
		panel.transform.DOPunchScale(Vector3.one * 0.03f, 0.2f, 0, 1f);
	}

	BInteger GetLastHighScore()
	{
		return boardSize.x switch
		{
			4 => UserMgr.EndlessModeHighScore47,
			_ => UserMgr.EndlessModeHighScore67,
		};
	}

	void Pause_UpdateButtons()
	{
		pause_SkillAutoCheckmark.gameObject.SetActive(UserMgr.bIsOnAutoSkill);
		CommonResource.ChangeToggleColor(pause_SkillAutoToggle, UserMgr.bIsOnAutoSkill);
		pause_BgmImage.gameObject.SetActive(!SoundMgr.bgmMute);
		CommonResource.ChangeToggleColor(pause_BgmToggle, !SoundMgr.bgmMute);
        pause_SfxImage.gameObject.SetActive(!SoundMgr.sfxMute);
        CommonResource.ChangeToggleColor(pause_SfxToggle, !SoundMgr.sfxMute);
    }

	async UniTask<bool> ShowTutorial(int tutorialNumber)
    {
		if (UserMgr.GetDidTutorial(tutorialNumber)) return false;
		Time.timeScale = 0f;
        await CommonCanvas.tutorialPopup.Show(tutorialNumber);
		UserMgr.SetDidTutorial(tutorialNumber);
		Time.timeScale = 1f;
		return true;
    }

	async UniTask SaveToLeaderboard(LeaderboardType type, BInteger score, NumberType highestNumber)
	{
		bIsCompleteSaveLeaderboard = false;
        await SaveLeaderboard(type, score, highestNumber);
		Debug.Log("SaveLeaderboard completed");
		bIsCompleteSaveLeaderboard = true;
    }
}
