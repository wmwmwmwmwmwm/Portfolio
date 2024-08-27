using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PetFarmItemInfo;
using Vector3 = UnityEngine.Vector3;

public partial class UI_PetFarm : UIMenuBarBase
{
    [SerializeField] UI_PetFarm_PetElement _elementPrefab;

    [Header("메인화면")]
    [SerializeField] List<UI_PetFarm_SpotElement> _spots;
    [SerializeField] UI_Button _getRewardAllButton;

    [Header("상세정보")]
    [SerializeField] GameObject _infoPopup;
    [SerializeField] GameObject _infoPopupMain;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] Transform _infoPetsParent;
    [SerializeField] TMP_Text _rewardText;
    [SerializeField] ItemIcon _rewardIcon;
    [SerializeField] TMP_Text _timeText;
    [SerializeField] Transform _infoPopupLeftPosition;
    [SerializeField] UI_Button _getRewardButton;

    [Header("보유 펫")]
    [SerializeField] GameObject _equipPanel;
    [SerializeField] Transform _allPetsParent;

    int _selectedSpot;
    List<UI_PetFarm_PetElement> _infoPets;
    List<UI_PetFarm_PetElement> _allPets;

    PetFarmItemInfo PetFarm => InventoryManager.Instance.PetFarmItemInfo;
    FSettingContents Settings => DataManager.Instance.GetSettingContents();
    PetFarmSpot SelectedSpot => PetFarm.Spots[_selectedSpot];

    public enum EButtonId
    {
        None,
        Spot,
        GetRewardAll,
        Info_Close,
        Info_Pet,
        Info_OpenEquipPanel,
        Info_AutoEquip,
        Info_GetReward,
        Equip_Pet,
        Equip_Cancel,
        Equip_OK,
    }

    public override void InitUI()
    {
        _infoPets = new();
        _allPets = new();
        for (int i = 1; i < (int)EPetFarmSpot.MAXCOUNT; i++)
        {
            int index = i - 1;
            UI_PetFarm_SpotElement spot = _spots[index];
            spot.Setting(index);
        }
        Timer.ADD_MinChangeCallbacks(RefreshAll);

        ShowInfoPopup(false);
        RefreshSpots();

        base.InitUI();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;
        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);

        switch (buttonId)
        {
            case EButtonId.Spot:
                _selectedSpot = int.Parse(tokens[1]);
                ShowInfoPopup(true);
                break;
            case EButtonId.GetRewardAll:
                GetRewardAllButton();
                break;
            case EButtonId.Info_Close:
                ShowInfoPopup(false);
                break;
            case EButtonId.Info_Pet:
                int slot = int.Parse(tokens[1]);
                SelectInfoPet(slot);
                break;
            case EButtonId.Info_OpenEquipPanel:
                ShowEquipPanel(true);
                break;
            case EButtonId.Info_AutoEquip:
                AutoEquipButton();
                break;
            case EButtonId.Info_GetReward:
                GetRewardButton();
                break;
            case EButtonId.Equip_Pet:
                int petId = int.Parse(tokens[1]);
                SelectEquipPanelPet(_allPets.Find(x => x.Pet.Id == petId));
                break;
            case EButtonId.Equip_Cancel:
                ShowEquipPanel(false);
                break;
            case EButtonId.Equip_OK:
                EquipPanel_OKButton();
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode) { }

    public override bool OnBackKey()
    {
        if (_infoPopup.activeSelf)
        {
            ShowInfoPopup(false);
            return false;
        }
        return base.OnBackKey();
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        Timer.Remove_MinChangeCallbacks(RefreshAll);
    }

    void RefreshAll()
    {
        RefreshSpots();
        RefreshInfoPopup();
    }

    void RefreshSpots()
    {
        foreach (UI_PetFarm_SpotElement spot in _spots)
        {
            spot.Refresh();
        }
        bool anyReward = false;
        for (int i = 0; i < PetFarm.Spots.Length; i++)
        {
            RewardItemInfo reward = PetFarm.GetSpotReward(i, true);
            if (reward.ItemCount > 0)
            {
                anyReward = true;
                break;
            }
        }
        _getRewardAllButton.interactable = anyReward;
    }

    void ShowInfoPopup(bool on)
    {
        _infoPopup.SetActive(on);
        ShowEquipPanel(false);
        if (!on) return;

        RefreshInfoPopup();
    }

    void RefreshInfoPopup()
    {
        // 장착 펫 리스트
        _infoPets.ClearGameObjectList();
        for (int index = 0; index < SelectedSpot.Slots.Length; index++)
        {
            int slot = (int)SelectedSpot.Slots[index];
            UI_PetFarm_PetElement element = Instantiate(_elementPrefab, _infoPetsParent);
            PetManager.Instance.PetInfoDic.TryGetValue(slot, out PetInfoItem petInfo);
            bool special = petInfo != null && petInfo.PetData.Farm_special == SelectedSpot.SpotType;
            element.Setting(petInfo, _selectedSpot, special, true);
            element.Button.SetID($"{EButtonId.Info_Pet}__{index}");
            _infoPets.Add(element);
        }

        // 텍스트, 보상
        _titleText.text = LocalizeManager.GetText(Settings.Farm_Spot_Name[_selectedSpot]);
        BigInteger rewardPerMinute = PetFarm.GetSpotReward(_selectedSpot, false).ItemCount;
        _rewardText.text = LocalizeManager.GetText("UI_Farm_Spot_RewardPerMin", Formula.NumberToStringBy3(rewardPerMinute));
        _rewardIcon.Setting(PetFarm.GetSpotReward(_selectedSpot, true));
        TimeSpan span = TimeSpan.FromMinutes(PetFarm.GetRewardTime(_selectedSpot, false));
        _timeText.text = $"{span.ToLocalizedString()} {LocalizeManager.GetText("UI_Farm_Spot_MaxTime")}";
        _getRewardButton.interactable = PetFarm.GetSpotReward(_selectedSpot, true).ItemCount > 0;
    }

    void ShowEquipPanel(bool on)
    {
        _equipPanel.SetActive(on);
        _infoPopupMain.transform.localPosition = on ? _infoPopupLeftPosition.localPosition : Vector3.zero;
        if (!on) return;

        // 모든 펫 리스트
        _allPets.ClearGameObjectList();
        foreach (PetInfoItem petInfo in PetManager.Instance.PetInfos)
        {
            if (!petInfo.Have) continue;
            UI_PetFarm_PetElement element = Instantiate(_elementPrefab, _allPetsParent);
            bool special = petInfo.PetData.Farm_special == SelectedSpot.SpotType;
            element.Setting(petInfo, _selectedSpot, special, false);
            element.Button.SetID($"{EButtonId.Equip_Pet}__{petInfo.Id}");
            _allPets.Add(element);
        }
    }

    void SelectInfoPet(int slot)
    {
        // 비어있으면 보유펫 열기
        if (PetFarm.GetPetInfo(_selectedSpot, slot) == null)
        {
            ShowEquipPanel(true);
        }
        // 장착 해제
        else
        {
            PetFarm.SetPet(_selectedSpot, slot, 0);
            RefreshAll();
        }
    }

    void SelectEquipPanelPet(UI_PetFarm_PetElement selected)
    {
        // 이미 장착중인 펫은 장착 해제
        if (PetFarm.CheckEquipped(_selectedSpot, selected.Pet.Id))
        {
            int slot = Array.IndexOf(SelectedSpot.Slots, selected.Pet.Id);
            PetFarm.SetPet(_selectedSpot, slot, 0);
        }
        // 다른 스팟에 장착중인 펫은 장착 불가
        else if (PetFarm.CheckEquipOther(_selectedSpot, selected.Pet.Id))
        {
            int spot = Array.FindIndex(PetFarm.Spots, x => x.Slots.Contains(selected.Pet.Id));
            string spotName = LocalizeManager.GetText(Settings.Farm_Spot_Name[spot]);
            UIManager.ShowToastMessage(LocalizeManager.GetText("Toast_Farm_SetUpSomewhere", spotName));
            return;
        }
        // 남은 슬롯이 없으면 장착 불가
        else if (SelectedSpot.Slots.All(x => x > 0))
        {
            UIManager.ShowToastMessage(LocalizeManager.GetText("Toast_Farm_AlreadyPetSetUp"));
            return;
        }
        // 장착
        else
        {
            int emptySlot = Array.IndexOf(SelectedSpot.Slots, 0);
            PetFarm.SetPet(_selectedSpot, emptySlot, selected.Pet.Id);
        }

        RefreshAll();
    }

    void GetRewardAllButton()
    {
        // 보상 지급
        List<RewardItemInfo> items = PetFarm.ReceiveRewardAll();
        UIManager.Instance.ShowGetItemPopup(new()
        {
            rewardInfos = items,
        });
    }

    void AutoEquipButton()
    {
    }

    void GetRewardButton()
    {
        // 보상 지급
        List<RewardItemInfo> items = PetFarm.ReceiveReward(_selectedSpot);
        UIManager.Instance.ShowGetItemPopup(new()
        {
            rewardInfos = items,
        });
    }

    void EquipPanel_OKButton()
    {
    }
}
