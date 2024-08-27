using System;
using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;

public class UI_CharacterAdvance_Result : UIBase
{   
    enum BUTTON_ID
    {
        None,
        Close
    }

    [SerializeField] PreviewSpace_ _preview;
    [SerializeField] TMP_Text _titleText, _advanceNameText;
    [SerializeField] UI_CharacterAdvance_Result_Element _characterAdvanceResultElement;
    [SerializeField] Transform _elementParent;
    //[SerializeField] Transform _effectPosition;
    [SerializeField] Transform _modelParent;

    FCharacterAdvance _fCharacterAdvance;
    GameModelDraw _draw;

    protected override void OnPrvOpenWork(Action<bool> resultCallback)
    {   
        resultCallback(true);
    }

    protected override void RefreshUI(LanguageType langCode)
    {
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click)
            return;

        switch (EnumHelper.Parse(buttonInfo.ID, BUTTON_ID.None))
        {
            case BUTTON_ID.Close:
                OutUI();
                _draw.Release();
                break;
        }
    }

    public void SetAdvanceLevel(int level)
    {
        _titleText.text = LocalizeManager.GetText("UI_CharacterAdvance_Result");
        _fCharacterAdvance = DataManager.Instance.GetCharacterAdvanceData(level);
        _advanceNameText.text = LocalizeManager.GetText(_fCharacterAdvance.Advancename);

        // 스탯 출력
        for (int i = 0; i < _fCharacterAdvance.Rewardstat.Count; i++)
        {
            if (_fCharacterAdvance.Rewardstat[i] == EStat.NONE) continue;
            UI_CharacterAdvance_Result_Element element = Instantiate(_characterAdvanceResultElement, _elementParent);
            StatValueInfo statValue = new()
            {
                Stat = _fCharacterAdvance.Rewardstat[i],
                Value = (int)_fCharacterAdvance.Rewarstatvalue[i],
                CalcType = _fCharacterAdvance.Calculation[i],
            };
            element.Setting(statValue.StatIcon, statValue.StatString, $"+{statValue.ValueString}");
        }

        // 모델 출력
        CostumeItem costume = InventoryManager.Instance.GetItem(_fCharacterAdvance.Rewarditemid) as CostumeItem;
        if (costume == null) return;
        FCharacter playerChar = DataManager.Instance.GetCharacterData(GameLoop.Instance.HeroID);
        PreviewModelInfo previewModel = _preview.AddModel(playerChar.Resourceid, new string[1] { costume.CostumeData.Bodyid }, OnModelLoaded);

        void OnModelLoaded(GameModelDraw draw)
        {
            _draw = draw;
            Renderer[] renderers = _draw.GetOwnerModel().GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("UI");
            }
            _draw.GetOwnerModel().transform.SetParent(_modelParent, false);
            StartCoroutine(PlayAnims());

            IEnumerator PlayAnims()
            {
                _draw.PlayAnimAction(EAniType.Ani_IdleW01, false, false);
                yield return new WaitForSecondsRealtime(_draw.GetAnimTime(EAniType.Ani_IdleL));
                _draw.PlayAnimAction(EAniType.Ani_Idle, true, true);
            }
        }
    }
}
