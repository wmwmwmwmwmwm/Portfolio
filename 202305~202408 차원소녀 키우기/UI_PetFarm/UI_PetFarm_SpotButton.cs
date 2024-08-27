using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_PetFarm;

public class UI_PetFarm_SpotElement : MonoBehaviour
{
    public UI_Button _button;
    //public Image _bg;
    public Image _portrait;
    public Image _rewardIcon;
    public TMP_Text _rewardCount;
    public GameObject _empty;
    public GameObject _lock;
    public TMP_Text _lockText;
    public GameObject _full;
    public RedDotComponent _redDot;
    public GameObject _unlockNotice;
    public Animation _anim;

    PetFarmItemInfo PetFarm => InventoryManager.Instance.PetFarmItemInfo;
    FSettingContents Settings => DataManager.Instance.GetSettingContents();

    int _index;

    public void Setting(int spotIndex)
    {
        _unlockNotice.SetActive(false);
        _index = spotIndex;
        _button.SetID($"{EButtonId.Spot}__{_index}");
    }

    public void Refresh()
    {
        // 잠긴 상태
        int maxStage = (int)StageInfoManager.Instance.GetNormalStage().MaxValue;
        int lockStageLevel = Settings.Farm_Spot_OpenCondition[_index];
        bool isLocked = maxStage < lockStageLevel;
        _button.interactable = !isLocked;
        _lock.SetActive(isLocked);
        if (isLocked)
        {
            _lockText.text = $"{LocalizeManager.GetText("Stage_Name")}\n{lockStageLevel:N0}";
        }
        // 해금 애니메이션
        else if (!PetFarm.Spots[_index].UnlockShown)
        {
            PetFarm.SetUnlockShown(_index);
            //
        }

        // 세팅
        PetInfoItem leaderPet = PetFarm.GetPetInfo(_index, 0);
        bool isEmpty = leaderPet == null;
        _empty.SetActive(isEmpty);
        _rewardIcon.sprite = AtlasManager.GetItemIcon(Settings.Farm_Reward_ItemID, EIconType.Icon);
        if (!isEmpty)
        {
            _portrait.sprite = AtlasManager.GetItemIcon(leaderPet.PetData, EIconType.Icon);
            RewardItemInfo reward = PetFarm.GetSpotReward(_index, true);
            _rewardCount.text = Formula.NumberToStringBy3(reward.ItemCount);
            _full.SetActive(PetFarm.CheckFull(_index));
        }
        else
        {
            _rewardCount.text = "0";
            _full.SetActive(false);
        }
        //_redDot.SetActiveRedDot();
    }
}
