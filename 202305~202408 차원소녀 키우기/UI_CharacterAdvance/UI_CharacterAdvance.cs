using CodeStage.AntiCheat.ObscuredTypes;
using Coffee.UIEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class UI_CharacterAdvance : UIMenuBarBase
{
    public enum EButtonId
    {
        None,
        LevelUp,
        Return,
        Prev,
        Next,
        Advance,
        LevelUpAll,
    }

    [Header("배경")]
    [SerializeField] PreviewSpace_ _preview;
    [SerializeField] GameObject _bg_EditorOnly;
    [SerializeField] List<UI_CharacterAdvance_Slot> _slots;
    [SerializeField] List<Sprite> _slotIcons;
    [SerializeField] AnimationCurve _effectYCurve, _effectFadeCurve, _rimIntensityCurve;
    [SerializeField] Transform _effectDest;
    [SerializeField] UI_Button _advanceButton;
    [SerializeField] UIShiny _advanceButtonShiny;
    [SerializeField] TMP_Text _advanceButtonText;
    [SerializeField] TMP_Text _needLevelText;
    [SerializeField] Transform _orbEffect, _orbEffectReady;

    [Header("우측상단 스탯")]
    [SerializeField] TMP_Text _advanceName;
    [SerializeField] UI_Button _prevButton, _nextButton;
    [SerializeField] List<Transform> _infoRecords;

    [Header("우측하단 스탯")]
    [SerializeField] UI_Button _levelUpAllButton;
    [SerializeField] List<Transform> _statRecords;

    List<FCharacterAdvance> _allAdvance;
    int _previewLevel;
    string _defaultModelId, _defaultBodyId;
    List<PreviewModelInfo> _previewModels;
    bool _initPreview;
    Coroutine _rimAniCoroutine;
    int _pressStartLevel;
    bool _showShortageToast;
    float _effectDuration = 0.6f;

    UI_CharacterAdvance_Bg Bg => UIRoot.Instance.CharacterAdvanceBg;
    GameObject PreviewModel => _previewModels.First().Draw.GetOwnerModel();
    PlayerAdvanceManager Manager => PlayerAdvanceManager.Instance;
    AdvanceLevelInfoItem LevelInfo => Manager.AdvanceLevelInfo;

    public override void InitUI()
    {
        _previewModels = new();
        _bg_EditorOnly.SetActive(false);
        Bg.gameObject.SetActive(true);
        _allAdvance = DataManager.Instance.GetCharacterAdvanceMap().Values.ToList();
        FCharacter playerChar = DataManager.Instance.GetCharacterData(GameLoop.Instance.HeroID);
        _defaultModelId = playerChar.Resourceid;
        _defaultBodyId = playerChar.Bodyid;
        _previewLevel = -1;
        InitPreview();
        SetPreviewLevel(LevelInfo.Level);

        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, RefreshBgButtons);
        base.InitUI();
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    //public override void OnClosed()
    //{
    //    base.OnClosed();
    //    HideBg();
    //}

    public override void OutOnlyMyself(bool isDirect = false)
    {
        base.OutOnlyMyself(isDirect);
        HideBg();
    }

    //public override void OutUI(bool isDirect = false)
    //{
    //    base.OutUI(isDirect);
    //    HideBg();
    //}

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State == UI_ButtonState.HoldEnd)
        {
            _showShortageToast = true;
        }

        string[] tokens = buttonInfo.ID.Split('_');
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        switch (buttonId)
        {
            case EButtonId.LevelUp:
                // 배경버튼 레벨업
                int index = int.Parse(tokens[1]);
                UI_CharacterAdvance_Slot slot = _slots.Find(x => x.Index == index);
                switch (buttonInfo.State)
                {
                    case UI_ButtonState.Press:
                        _pressStartLevel = slot.Info.Level;
                        break;
                    case UI_ButtonState.Click:
                        OnLevelUpButtonClick(slot);
                        break;
                    case UI_ButtonState.HoldEnd:
                        if(_pressStartLevel != slot.Info.Level)
                        {
                            LevelUpHoldEndEffect();
                        }
                        RefreshPlayer();
                        UserDataManager.Save_LocalData();
                        break;
                }
                OnRefreshUI();
                break;
        }

        if (buttonInfo.State != UI_ButtonState.Click) return;
        switch (buttonId)
        {
            case EButtonId.Return:
                // 승급단계 돌아오기버튼
                SetPreviewLevel(LevelInfo.Level);
                break;
            case EButtonId.Prev:
                // 승급단계 이전버튼
                SetPreviewLevel(_previewLevel - 1);
                break;
            case EButtonId.Next:
                // 승급단계 다음버튼
                SetPreviewLevel(_previewLevel + 1);
                break;
            case EButtonId.Advance:
                // 승급버튼
                OnAdvanceButtonClick();
                break;
            case EButtonId.LevelUpAll:
                // 일괄강화 버튼
                OnLevelUpAllButtonClick();
                break;
        }
        OnRefreshUI();
    }

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshBgButtons();
        RefreshTopStats();
        RefreshBottomStats();
    }

    void InitPreview()
    {
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return null;
            PreviewModelInfo previewModel = _preview.AddModel(_defaultModelId, new string[1] { _defaultBodyId }, (draw) =>
            {
                _orbEffect.GetComponentInChildren<SeonTweener>().PlayForward();
                _orbEffect.SetParent(draw.GetOwnerModel().transform, false);
                _orbEffectReady.GetComponentInChildren<SeonTweener>().PlayForward();
                _orbEffectReady.SetParent(draw.GetOwnerModel().transform, false);

                draw.SetUseAnimations(new List<(int, EAniType)>() { (0, EAniType.Ani_Adv_Idle), (0, EAniType.Ani_Adv_Start) });
            });
            previewModel.AnimationInfo = new()
            {
                Idle = EAniType.Ani_Adv_Idle,
                Start = EAniType.Ani_Adv_Start,

                ActionAnimations = new()
                {
                }
            };
            _previewModels.Add(previewModel);
            _initPreview = true;
        }
    }

    void OnLevelUpAllButtonClick()
    {
        foreach (UI_CharacterAdvance_Slot slot in _slots) 
        {
            bool hasLevelUp = false;
            while (true)
            {
                if (!slot.Info.LevelUp(1)) break;
                hasLevelUp = true;
            }
            if (hasLevelUp)
            {
                LevelUpEffect(slot);
                LevelUpEffect(slot);
                LevelUpEffect(slot);
                LevelUpEffect(slot);
            }
        }
    }

    void OnLevelUpButtonClick(UI_CharacterAdvance_Slot slot)
    {
        if (!slot.Info.IsCheckLevelUp(1))
        {
            if (_showShortageToast)
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_ItemShortage"));
                _showShortageToast = false;
            }
            return;
        }
        slot.Info.LevelUp(1);
        LevelUpEffect(slot);        
    }

    void LevelUpEffect(UI_CharacterAdvance_Slot slot)
    {
        Play2DEffect(id: "Fx_UI_Upgrade_CharacterAdvance_Icon_01",
                        position: slot.Icon.transform.localPosition,
                        parent: slot.Icon.transform.parent,
                        reappearDelay: 0.2f);
        Vector3 startPos = slot.Icon.transform.position;
        Vector3 effectVector = _effectDest.position - startPos;
        effectVector.z = 0f;
        EffectElement effect = Play2DEffect(
            id: "Fx_CharacterAdvance_Projectile_01",
            position: Vector3.zero,
            parent: transform);
        StartCoroutine(ProjectileEffectMove(effect, startPos, effectVector, slot));
        Bg._tweenerActivateTime = 0f;
    }

    void LevelUpHoldEndEffect()
    {
        // 버튼 뗄때 이펙트
        Play2DEffect(id: "Fx_ModelUI_AbilityUp_Cha_01",
            position: Vector3.zero,
            parent: _preview.ModelContainer,
            reappearDelay: 0.2f);
        if (_rimAniCoroutine != null)
        {
            StopCoroutine(_rimAniCoroutine);
        }
        _rimAniCoroutine = StartCoroutine(PlayRimAnimation());
    }

    void OnAdvanceButtonClick()
    {
        bool canLevelUp = LevelInfo.CheckLevelUp(out bool notEnoughCharacterLevel);
        if (!canLevelUp)
        {
            if (notEnoughCharacterLevel) 
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_Shortage_ChaAdvance_02"));
            }
            else
            {
                UIManager.ShowToastMessage(LocalizeManager.GetText("Msg_Btn_Shortage_ChaAdvance_01"));
            }
            return;
        }

        bool resultClosed = false;
        BaseItem reward = InventoryManager.Instance.GetItem(LevelInfo.NextAdvanceData.Rewarditemid);
        int rewardCount = LevelInfo.NextAdvanceData.Rewarditemvalue;
        UIManager.Instance.OpenUI(EUIName.UI_CharacterAdvance_Result, new UICallbackInfo()
        {
            prvOpenUIWork = (p, callback) =>
            {
                PlayerCostumeManager.Instance.GiveCostume(reward.Id, rewardCount);
                LevelInfo.LevelUp();

                UIRoot.Instance.LoadingNetwork.Set("UI_CharacterAdvance.OnAdvanceButtonClick");
                UserDataManager.Save_ServerData(ServerRequestSync.CharacterAdvanceResult.ToString(), null, null, err =>
                {
                    callback(!err);
                    UIRoot.Instance.LoadingNetwork.Set("UI_CharacterAdvance.OnAdvanceButtonClick", false);
                }, 1.5f);
            },
            openedCallback = p =>
            {
                (p as UI_CharacterAdvance_Result).SetAdvanceLevel(LevelInfo.Level);
            },
            closedCallback = (c, v) =>
            {
                resultClosed = true;
                SetPreviewLevel(LevelInfo.Level);
                OnRefreshUI();
                RefreshPlayer();
            }
        });

        StartCoroutine(WaitClose());
        IEnumerator WaitClose()
        {
            yield return new WaitUntil(() => resultClosed);
            UIManager.Instance.ShowGetItemPopup(new()
            {
                TitleText = LocalizeManager.GetText("UI_AdvCostumeGain_Title"),
                ItemIds = new() { reward.Id },
                ItemCounts = new() { rewardCount },
            });
        }
    }

    void RefreshPreview()
    {
        // 레나 세팅
        StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return new WaitUntil(() => _initPreview);
            string bodyId = _defaultBodyId;
            GameModelDraw draw = _previewModels.First().Draw;
            draw.RemoveAllPartOfType(EModelPartType.Body);
            if (_previewLevel > 0)
            {
                CostumeItem item = InventoryManager.Instance.GetItem(LevelInfo.GetAdvanceData(_previewLevel).Rewarditemid) as CostumeItem;
                if (item == null) yield break;
                bodyId = item.CostumeData.Bodyid;
            }
            draw.LoadPartFromModelPart(bodyId);
        }
    }

    void RefreshBgButtons()
    {
        // 배경 오브젝트, 배경 레벨업버튼 세팅
        Bg.GetComponent<Canvas>().worldCamera = _preview.Camera.Camera;
        List<AdvanceStateInfoItem> stateInfos = Manager.AdanceStateInfos.Values.ToList();
        for (int i = 0; i < stateInfos.Count; i++)
        {
            bool buttonActive = LevelInfo.Level == _previewLevel;
            _slots[i].Setting(stateInfos[i], LevelInfo.GetAdvanceData(_previewLevel + 1), _slotIcons[i], i, buttonActive, Manager.AdvanceLevelInfo.CheckLevelUp(out _));
        }

        // 오브이펙트 세팅
        bool canLevelUp = LevelInfo.CheckLevelUp(out _);
        _orbEffect.Obj()?.gameObject.SetActive(!canLevelUp);
        _orbEffectReady.Obj()?.gameObject.SetActive(canLevelUp);
    }

    void RefreshTopStats()
    {
        // 우측상단 승급스탯 세팅
        string nameTextKey = "CharacterAdvance_Name_00";
        FCharacterAdvance advData = LevelInfo.GetAdvanceData(_previewLevel);
        if (_previewLevel > 0)
        {
            nameTextKey = advData.Advancename;
        }
        _advanceName.text = LocalizeManager.GetText(nameTextKey);
        _prevButton.gameObject.SetActive(_previewLevel > 0);
        _nextButton.gameObject.SetActive(_previewLevel < _allAdvance.Count - 1);
        for (int i = 0; i < _infoRecords.Count; i++)
        {
            Transform record = _infoRecords[i];
            StatValueInfo statValue = new();
            if (advData == null)
            {
                FCharacterAdvance advData1 = LevelInfo.GetAdvanceData(1);
                statValue.Stat = advData1.Rewardstat[i];
                statValue.Value = 0;
                statValue.CalcType = advData1.Calculation[i];
            }
            else
            {
                statValue.Stat = advData.Rewardstat[i];
                statValue.Value = (int)advData.Rewarstatvalue[i];
                statValue.CalcType = advData.Calculation[i];
            }
            record.Find("StatName").GetComponent<TMP_Text>().text = statValue.StatString;
            record.Find("StatValue").GetComponent<TMP_Text>().text = '+' + statValue.ValueString;
        }
    }

    void RefreshBottomStats()
    {
        // 레벨업버튼 세팅
        BigInteger charLevel = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).DefaultState[(int)EStat.LEVEL];
        FCharacterAdvance data = LevelInfo.GetAdvanceData(_previewLevel);
        int prevLevel = data != null ? (int)data.Conditionlevel[0] : 0;
        FCharacterAdvance nextData = LevelInfo.GetAdvanceData(_previewLevel + 1);
        BigInteger current = charLevel;
        if (_previewLevel > LevelInfo.Level)
        {
            current = 0;
        }
        else if (_previewLevel < LevelInfo.Level)
        {
            current = nextData.Conditionlevel[0] - prevLevel;
        }
        else
        {
            current -= prevLevel;
        }
        if (nextData != null)
        {
            _needLevelText.text = LocalizeManager.GetText("UI_CharacterAdvance_LevelInfo", charLevel, nextData.Conditionlevel[0]);
        }
        _advanceButton.gameObject.SetActive(!LevelInfo.IsMaxLevel && _previewLevel == LevelInfo.Level);
        _advanceButtonText.text = LocalizeManager.GetText("UI_Btn_Advance", nextData.Id);
        _levelUpAllButton.gameObject.SetActive(_previewLevel == LevelInfo.Level);

        // 누적 스탯 세팅
        List<StatValueInfo> statValues = new();
        foreach (KeyValuePair<ObscuredEnum<EStat>, FCharacterAdvanceStat> statInfo in DataManager.Instance.CharacterStatAdvanceStateMaps)
        {
            StatusValueInfos value = Manager.AdanceStateInfos[statInfo.Key].GetValue(EPLAYERTYPE.MYSELF, statInfo.Key);
            statValues.Add(new StatValueInfo()
            {
                Stat = statInfo.Key,
                Value = value.AddValue + value.MultiyValue,
                CalcType = ECalculation.Percent,
            });
        }
        for (int i = 0; i < _statRecords.Count; i++)
        {
            Transform record = _statRecords[i];
            record.Find("StatIcon").GetComponent<Image>().sprite = InGameUtil.GetStatIcon(statValues[i].Stat);
            record.Find("StatName").GetComponent<TMP_Text>().text = statValues[i].StatString;
            record.Find("StatValue").GetComponent<TMP_Text>().text = '+' + statValues[i].ValueString;
        }

        bool canLevelUp = LevelInfo.CheckLevelUp(out _);
        UIRoot.Instance.SetButtonItemShortage(_advanceButton, !canLevelUp);
        _advanceButtonShiny.enabled = canLevelUp;
        _levelUpAllButton.interactable = _slots.Any(x => x.Info.IsCheckLevelUp(1));
    }

    void RefreshPlayer()
    {
        PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).RefreshValues(true);
    }

    void SetPreviewLevel(int level)
    {
        if (level == _previewLevel) return;
        _previewLevel = Mathf.Clamp(level, 0, _allAdvance.Count);
        RefreshPreview();
    }

    void HideBg()
    {
        Bg.gameObject.SetActive(false);
        if (_orbEffect)
        {
            Destroy(_orbEffect.gameObject);
        }
        if (_orbEffectReady)
        {
            Destroy(_orbEffectReady.gameObject);
        }
    }

    IEnumerator ProjectileEffectMove(EffectElement effect, Vector3 startPos, Vector3 moveVector, UI_CharacterAdvance_Slot slot)
    {
        yield return new WaitUntil(() => effect.EffectLink);
        effect.EffectLink.transform.position = startPos;
        TrailRenderer trail = effect.EffectLink.GetComponentInChildren<TrailRenderer>();
        trail.Clear();
        Color trailColor = trail.material.GetColor("_Color_HDR");
        float normalizedTime = 0f;
        float yAmount2 = Random.Range(-1f, 1f);
        Vector3 rhs = Random.value < 0.5f ? Vector3.forward : Vector3.back;
        //Vector3 offset = Random.insideUnitSphere * 3f;
        //offset.z = 0f;
        Vector3 dest = startPos + moveVector;// + offset;
        while (normalizedTime < 1f)
        {
            if (!effect.EffectLink || !effect.EffectLink.activeSelf) break;

            // 이펙트 곡선이동
            Vector3 newPos = Vector3.Lerp(startPos, dest, normalizedTime);
            float yAmount = _effectYCurve.Evaluate(normalizedTime) * slot.ProjectileEffectYAmount * yAmount2;
            Vector3 yDirection = Vector3.Cross(moveVector.normalized, rhs);
            newPos += yDirection * yAmount;
            effect.EffectLink.transform.position = newPos;
            trailColor.a = _effectFadeCurve.Evaluate(normalizedTime);
            trail.material.SetColor("_Color_HDR", trailColor);
            normalizedTime += Time.unscaledDeltaTime / _effectDuration;

            yield return null;
        }
    }

    IEnumerator PlayRimAnimation()
    {
        SkinnedMeshRenderer[] renderers = PreviewModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Material> mats = renderers.Select(x => x.material).ToList();
        mats.ForEach(x => x.SetFloat("_UseRim", 1f));
        float normalizedTime = 0f;
        while (normalizedTime < 1f)
        {
            // 레나 림라이트 애니메이션
            foreach (Material mat in mats)
            {
                mat.SetFloat("_RimTC_ITS", _rimIntensityCurve.Evaluate(normalizedTime));
            }
            normalizedTime += Time.unscaledDeltaTime / _effectDuration;

            yield return null;
        }
        _rimAniCoroutine = null;
    }
}
