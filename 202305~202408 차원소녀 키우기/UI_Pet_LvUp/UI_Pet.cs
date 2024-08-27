using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_ItemMasteryPopup;
using Vector3 = UnityEngine.Vector3;

public partial class UI_Pet : UIMenuBarBase, IMasteryPopup
{
    [SerializeField] UIOpenButtonStat _openMasteryOpenState;
    [SerializeField] RedDotComponent _openMasteryRedDot;
    [SerializeField] UI_ItemMasteryPopup _masteryPopup;

    [Header("펫리스트")]
    [SerializeField] ScrollRect _petScroll;
    [SerializeField] UI_PetElement _petElementPrefab;
    [SerializeField] Transform _petListParent;

    [Header("오른쪽 공통")]
    [SerializeField] PreviewSpace_ _preview;
    [SerializeField] Image _rightBg;
    [SerializeField] TMP_Text _petName;
    [SerializeField] Image _gradeIcon;
    [SerializeField] List<GameObject> _stars;

    [Header("강화 탭")]
    [SerializeField] GameObject _levelUp_AccompanyIcon;
    [SerializeField] TMP_Text _levelUp_LevelText;
    [SerializeField] Color _levelUp_LevelTextBlueColor, _levelUp_LevelTextRedColor;
    [SerializeField] GameObject _levelUp_Locked;
    [SerializeField] TMP_Text _levelUp_LockedDesc;
    [SerializeField] List<GameObject> _levelUp_Stats;
    [SerializeField] List<GameObject> _levelUp_AdvStats;
    [SerializeField] UI_Button _levelUp_EquipButton;
    [SerializeField] TMP_Text _levelUp_EquipButtonText;
    [SerializeField] UI_Button _levelUp_ComposeButton;
    [SerializeField] UI_Button _levelUp_ComposeAllButton;
    [SerializeField] UI_Button _levelUp_LevelUpButton;
    [SerializeField] GameObject _levelUp_PetIcon;
    [SerializeField] Image _levelUp_PetIconImage;
    [SerializeField] Image _levelUp_CostIcon;
    [SerializeField] TMP_Text _levelUp_LevelUpText, _levelUp_CostText;
    [SerializeField] Transform _levelUp_EffectPosition_LevelUp, _levelUp_EffectPosition_Advance;
    [SerializeField] UI_LevelUpResult _advanceResult;

    [Header("농장 효과")]
    [SerializeField] GameObject _farmInfoPopup;

    // 몬스터 모델크기 수동보정 리스트
    [Serializable]
    public class ModelSize
    {
        public int Id;
        public float Size;
        public float PositionY;
    }
    [Space]
    [SerializeField] List<ModelSize> _monsterSizeList;
    [SerializeField, ReadOnly] int _monsterId;

    List<UI_PetElement> _petUIs;
    PetInfoItem _selectedPet;
    bool _showShortageToast;
    bool _composePopupYes;
    List<PreviewModelInfo> _modelInfos;
    bool _isChange;

    bool SelectedHave => _selectedPet.Have;
    PetManager Pet => PetManager.Instance;
    List<PetInfoItem> AllPets => Pet.PetInfos;
    PlayerStatusInfo Player => PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF);

    public enum EButtonId
    {
        None,
        Pet,
        Equip,
        LevelUp,
        Compose,
        ComposeAll,
        OpenMastery,
        OpenFarmInfo,
        FarmInfo_Close,
    }

    public override void InitUI()
    {
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Character, gameObject, Refresh);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Exp_Pet, gameObject, Refresh);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, Refresh);
        _modelInfos = new();
        _advanceResult.gameObject.SetActive(false);
        _masteryPopup.Init(this);
        _farmInfoPopup.SetActive(false);

        // 모든 펫 리스트 생성
        _petUIs = new();
        for (int i = 0; i < AllPets.Count; i++)
        {
            UI_PetElement ui = Instantiate(_petElementPrefab, _petListParent);
            _petUIs.Add(ui);
        }
        _selectedPet = Pet.GetAccompanyPet();
        _selectedPet ??= AllPets[0];
        RefreshPetList(true);

        // 좌측탭 길이 조절
        foreach (BaseMenuBar_Menu baseMenu in MenuBar.MenuButtons)
        {
            MenuBar_Menu menu = (MenuBar_Menu)baseMenu;
            menu.Tweener.to = 322;
        }

        // 마스터리버튼 레드닷 초기화
        _openMasteryOpenState.Setting(new() { EContent.PetMastery });
        _openMasteryRedDot.SetAuto(new() { new RedDotComponent.RedDotInfo() { Content = EContent.PetMastery } });

        Refresh();
        RefreshPreview();
        base.InitUI();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            _showShortageToast = true;
            if (_isChange)
            {
                UserDataManager.Save_LocalData();
                _isChange = false;
            }
        }
        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);

        if (buttonInfo.State != UI_ButtonState.Click) return;

        switch (buttonId)
        {
            // 펫 선택
            case EButtonId.Pet:
                if (!int.TryParse(tokens[1], out int charId)) return;
                UI_PetElement newSelectedPet = _petUIs.Find(x => x.Info.Id == charId);
                if (newSelectedPet.Info == _selectedPet) return;
                _selectedPet = newSelectedPet.Info;
                _selectedPet.New = false;
                RefreshPreview();
                Refresh();
                break;

            // 펫 장착
            case EButtonId.Equip:
                if (IsAccompany(_selectedPet))
                {
                    Pet.UnaccompanyPet();
                }
                else
                {
                    Pet.AccompanyPet(_selectedPet.Id);
                }
                Refresh();
                break;
            case EButtonId.Compose:
                StartCoroutine(ComposeButton());
                break;
            case EButtonId.ComposeAll:
                StartCoroutine(ComposeAllButton());
                break;
            case EButtonId.LevelUp:
                LevelUpButton();
                break;
            case EButtonId.OpenMastery:
                _preview.ClearAll();
                _masteryPopup.Show(true);
                break;
            case EButtonId.OpenFarmInfo:
                ShowFarmInfoPopup(true);
                break;
            case EButtonId.FarmInfo_Close:
                ShowFarmInfoPopup(false);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
    }

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        foreach (PetInfoItem pet in AllPets)
        {
            pet.New = false;
        }
    }

    void Refresh()
    {
        RefreshPetList(false);
        RefreshRightMain();
    }

    public override bool OnBackKey()
    {
        if (_masteryPopup.gameObject.activeSelf)
        {
            _masteryPopup.Show(false);
            return false;
        }
        return base.OnBackKey();
    }

    void RefreshRightMain()
    {
        // 공통
        string spritePath = DataManager.Instance.GetSettingContents().ItemGrade_BgFrame1[(int)_selectedPet.PetData.Grade - 1];
        _rightBg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.UI_Parts, spritePath);
        _petName.text = $"+{_selectedPet.AdvanceStar} {LocalizeManager.GetText(_selectedPet.PetData.Nameid)}";
        _gradeIcon.sprite = InGameUtil.GetGradeTextSprite(_selectedPet.PetData.Grade);
        for (int i = 0; i < _stars.Count; i++)
        {
            _stars[i].SetActive(i < _selectedPet.itemData.Star);
        }
        RefreshLevelUps();
    }

    void RefreshLevelUps()
    {
        // 장착 표시
        _levelUp_AccompanyIcon.SetActive(IsAccompany(_selectedPet));

        // 강화 레벨
        bool canLevelUp = _selectedPet.CheckMaxLevelAndHave();
        _levelUp_LevelText.text = $"<#63CCB8>{_selectedPet.Level}</color> / {_selectedPet.PetAdvance.NextAdvanceData.Needfriendlevel}";

        // 승급시 표시
        List<StatValueInfo> stats = _selectedPet.GetStatValues();
        EPetLevelUpType levelUpType = _selectedPet.GetLevelUpType();
        bool isAdvance = levelUpType == EPetLevelUpType.Advance;
        _levelUp_Locked.SetActive(isAdvance);
        if (isAdvance)
        {
            _levelUp_LockedDesc.text = LocalizeManager.GetText("UI_Pet_Adv_Desc_01", _selectedPet.PetAdvance.Next2AdvanceData.Needfriendlevel);
        }

        // 보유 효과 1, 2
        for (int i = 0; i < _levelUp_Stats.Count; i++)
        {
            bool active = i < stats.Count;
            GameObject statUI = _levelUp_Stats[i];
            statUI.SetActive(active);
            if (active)
            {
                UpdateStatUI(_levelUp_Stats[i], stats[i]);
            }
        }

        // 추가 보유 효과
        for (int i = 0; i < _levelUp_AdvStats.Count; i++)
        {
            GameObject statUI = _levelUp_AdvStats[i];
            int statIndex = i + 2;
            bool active = statIndex < stats.Count;
            statUI.SetActive(active);
            if (active)
            {
                UpdateStatUI2(statUI.transform, stats[statIndex], statIndex);
            }
        }

        // 장착, 합성, 강화 버튼
        bool equipped = IsAccompany(_selectedPet);
        _levelUp_EquipButton.interactable = _selectedPet.Have;
        _levelUp_EquipButtonText.text = equipped ? LocalizeManager.GetText("UI_Btn_Pet_Release") : LocalizeManager.GetText("UI_Btn_Pet_Equip");
        SettingLevelUpButton(_levelUp_LevelUpButton, _selectedPet.PetLevel.GetLevelUpCost(), _selectedPet.CheckMaxLevelAndHave());
        _levelUp_ComposeButton.interactable = SelectedHave && Pet.CanCompose(_selectedPet);
        _levelUp_ComposeAllButton.interactable = _petUIs.Any(x => Pet.CanCompose(x.Info));
        _levelUp_CostIcon.gameObject.SetActive(false);
        _levelUp_PetIcon.gameObject.SetActive(false);
        switch (levelUpType)
        {
            case EPetLevelUpType.LevelUp:
                _levelUp_CostIcon.gameObject.SetActive(true);
                _levelUp_LevelUpText.text = LocalizeManager.GetText("UI_Btn_Pet_LvUp");
                break;
            case EPetLevelUpType.Advance:
                _levelUp_PetIcon.gameObject.SetActive(true);
                _levelUp_PetIconImage.sprite = AtlasManager.GetItemIcon(_selectedPet.PetData, EIconType.Icon);
                _levelUp_LevelUpText.text = LocalizeManager.GetText("UI_Btn_Pet_Adv");
                break;
        }
        if (canLevelUp)
        {
            ItemCostInfo cost = _selectedPet.GetLevelUpCost();
            _levelUp_CostIcon.sprite = AtlasManager.GetItemIcon(cost.Item, EIconType.MiniIcon);
            _levelUp_CostText.text = Formula.NumberToStringBy3(cost.Count);
            UIRoot.Instance.SetButtonItemShortage(_levelUp_LevelUpButton, !_selectedPet.CheckLevelUpCost());
            _levelUp_LevelUpButton.SetHoldable(levelUpType == EPetLevelUpType.LevelUp);
        }

        void UpdateStatUI(GameObject statUI, StatValueInfo stat)
        {
            statUI.transform.Find("StatName").GetComponent<TMP_Text>().text = stat.StatString;
            statUI.transform.Find("StatValue").GetComponent<TMP_Text>().text = $"+ {stat.ValueString}";
        }


        void UpdateStatUI2(Transform statObject, StatValueInfo stat, int statIndex)
        {
            FPetAdvance openStatData = _selectedPet.PetLevel.GetOpenStatAdvanceData(statIndex);
            if (openStatData != null)
            {
                bool unlock = _selectedPet.PetLevel.HasAdvanceStat(statIndex);
                statObject.Find("Lock").gameObject.SetActive(!unlock);
                statObject.Find("Unlock").gameObject.SetActive(unlock);
                if (unlock)
                {
                    TMP_Text statName = statObject.Find("Unlock/StatName").GetComponent<TMP_Text>();
                    TMP_Text statValue = statObject.Find("Unlock/StatValue").GetComponent<TMP_Text>();
                    statName.text = stat.StatString;
                    statValue.text = $"+{stat.ValueString}";
                }
                else
                {
                    TMP_Text txt = statObject.Find("Lock/Text").GetComponent<TMP_Text>();
                    txt.text = LocalizeManager.GetText("UI_Pet_AdvUnlock_Desc", $"+{openStatData.Advance}");
                }
            }
            else
            {
                statObject.Find("Lock").gameObject.SetActive(false);
                statObject.Find("Unlock").gameObject.SetActive(false);
            }
        }
    }

    void SettingLevelUpButton(UI_Button button, ItemCostInfo cost, bool interactable)
    {
        Image image = button.transform.Find("Image").GetComponent<Image>();
        TMP_Text tmp = button.transform.Find("CostText").GetComponent<TMP_Text>();
        button.interactable = interactable;
        image.gameObject.SetActive(cost.Item.Id != 0);
        image.sprite = AtlasManager.GetItemIcon(cost.Item.Id, EIconType.MiniIcon);
        if (!_selectedPet.PetLevel.IsMaxLevel)
        {
            tmp.text = Formula.NumberToStringBy3(cost.Count);
            UIRoot.Instance.SetButtonItemShortage(button, !_selectedPet.CheckLevelUpCost());
        }
        else
        {
            tmp.text = "MAX";
            UIRoot.Instance.SetButtonItemShortage(button, false);
        }
    }

    void RefreshPreview()
    {
        // 모델 출력
        _preview.ClearAll();
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return null;
            FPet data = _selectedPet.PetData;
            _monsterId = data.Id;
            ModelSize modifier = _monsterSizeList.Find(a => a.Id == _monsterId);
            PreviewModelInfo previewModel = _preview.GetModel(data.Resourceid);
            if (previewModel == null)
            {
                previewModel = _preview.AddCharacterModel(data, model =>
                {
                    var rt = _preview.ModelContainer.Find(data.Resourceid);
                    if (null != rt)
                    {
                        model.SetScale(rt.localScale.x);
                        model.GetOwnerModel().transform.SetLocalPositionAndRotation(rt.localPosition, rt.localRotation);
                    }

                    // 이펙트 크기 줄이기, 레이어 조정
                    LoopEffectInfo[] effects = model.GetAllLoopEffects();
                    foreach (LoopEffectInfo effect in effects) 
                    {
                        GameObject obj = effect.Element.EffectLink;
                        if (!obj) continue;
                        obj.transform.localScale = Vector3.one * 0.5f;
                        ParticleSystem[] particles = obj.GetComponentsInChildren<ParticleSystem>();
                        foreach (ParticleSystem particle in particles)
                        {
                            particle.GetComponent<Renderer>().sortingOrder = Canvas.sortingOrder + 1;
                        }
                    }
                });
                _modelInfos.Add(previewModel);
                previewModel.AnimationInfo = new()
                {
                    ActionAnimations = new()
                    {
                        EAniType.Ani_IdleW01,
                        EAniType.Ani_IdleW02,
                    }
                };
            }
        }
    }

    void RefreshPetList(bool scroll)
    {
        // 모든 펫
        for (int i = 0; i < _petUIs.Count; i++)
        {
            PetInfoItem petInfo = AllPets[i];
            bool selected = _selectedPet == petInfo;
            bool accompany = IsAccompany(petInfo);
            _petUIs[i].Setting(petInfo, selected, accompany);
            if (accompany && scroll)
            {
                _petScroll.FocusOnItem(_petUIs[i].GetComponent<RectTransform>());
            }
        }
    }

    void LevelUpButton()
    {
        EPetLevelUpType levelUpType = _selectedPet.GetLevelUpType();
        bool success = _selectedPet.LevelUp();
        if (!success)
        {
            if (_showShortageToast)
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                _showShortageToast = false;
            }
            return;
        }
        UIRoot.Instance.PlayLevelUpEffect(transform, null, null);
        string effectId = "";
        Transform effectPosition = null;
        switch (levelUpType)
        {
            case EPetLevelUpType.LevelUp:
                effectId = "Fx_UI_Upgrade_Equipment_Gauge_Line_01";
                effectPosition = _levelUp_EffectPosition_LevelUp;
                break;
            case EPetLevelUpType.Advance:
                effectId = "Fx_UI_Upgrade_Friend_Line_01";
                effectPosition = _levelUp_EffectPosition_Advance;
                break;
        }
        Play2DEffect(id: effectId,
            position: effectPosition.localPosition,
            parent: effectPosition.parent,
            reappearDelay: 0.2f);

        // 승급 결과
        if (levelUpType == EPetLevelUpType.Advance)
        {
            FPetAdvance currentData = _selectedPet.PetAdvance.CurrentAdvanceData;
            int expandLevel = _selectedPet.PetAdvance.NextAdvanceData.Needfriendlevel - currentData.Needfriendlevel;
            bool unlockOpenStat = currentData.Openhavestat > 0;
            _advanceResult.Show(LocalizeManager.GetText("UI_Pet_Adv_Success_Title"),
                LocalizeManager.GetText("UI_Equip_MaxLv"),
                $"+{expandLevel}",
                unlockOpenStat ? LocalizeManager.GetText("UI_Equip_Break_Unlock_Desc") : "",
                "");
        }

        _isChange = true;
        Refresh();
    }

    IEnumerator ComposeButton()
    {
        yield return StartCoroutine(ComposeConfirm());
        if (!_composePopupYes)
        {
            RefreshPreview();
            yield break;
        }

        PetComposeResultInfo result = Pet.Compose(_selectedPet);
        Player.RefreshValues(true);
        _isChange = true;
        yield return StartCoroutine(ShowComposePopup(LocalizeManager.GetText("UI_Compose_Result_Title_01"), result));
        Refresh();
        RefreshPreview();
    }

    IEnumerator ComposeAllButton()
    {
        yield return StartCoroutine(ComposeConfirm());
        if (!_composePopupYes)
        {
            RefreshPreview();
            yield break;
        }

        PetComposeResultInfo result = Pet.ComposeAll();
        Player.RefreshValues(true);
        _isChange = true;
        yield return StartCoroutine(ShowComposePopup(LocalizeManager.GetText("UI_Compose_Result_Title_02"), result));
        Refresh();
        RefreshPreview();
    }

    IEnumerator ComposeConfirm()
    {
        bool closed = false;
        _composePopupYes = false;
        _preview.ClearAll();
        UIManager.Instance.OpenUI(EUIName.Popup_NormalMessage, new()
        {
            openedCallback = (ui) =>
            {
                var popup = ui as Popup_NormalMessage;
                popup.SettingInfo(new ShowMessageBoxInfo()
                {
                    Title = LocalizeManager.GetText("UI_Pet_Popup_Title_Warning"), 
                    ConfirmButtonName = LocalizeManager.GetText("UI_Btn_OK"),
                    CancelButtonName = LocalizeManager.GetText("UI_Btn_Cancel"),
                    ConfirmCallback = () => _composePopupYes = true,
                    Content = LocalizeManager.GetText("UI_Pet_Popup_Desc_Warning")
                });
            },
            closedCallback = (_, _) =>
            {
                closed = true;
            }
        });
        yield return new WaitUntil(() => closed);
    }

    IEnumerator ShowComposePopup(string titleText, PetComposeResultInfo result)
    {
        BigInteger compoundedCount = result.ComposeCount * Pet.ComposeNeedCount;
        yield return StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
        {
            TitleText = titleText,
            DescText = LocalizeManager.GetText("UI_Compose_Result_Desc_02", compoundedCount.ToString("N0")),
            ItemIds = new() { result.ResultPet.Id },
            ItemCounts = new() { result.ComposeCount },
        }));
    }

    void ShowFarmInfoPopup(bool on)
    {
        _farmInfoPopup.SetActive(on);
        if (!on) return;
    }

    bool IsAccompany(PetInfoItem info) => Pet.GetAccompanyPet() == info;
}
