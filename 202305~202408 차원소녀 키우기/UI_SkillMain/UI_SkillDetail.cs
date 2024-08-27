using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[Obsolete]
public class UI_SkillDetail : UICell
{
    public enum ETabId
    {
        Grow = 1,
        TranscInfo = 2,
    }

    enum EButtonId
    {
        None,
        Close,
        Tab,
    }

    [SerializeField] SkillIconElement _skillIcon;
    [SerializeField] UIPlayTween _playTeen;
    [SerializeField] TMP_Text _nameText;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] UI_TabGroup _tapGroup;
    [SerializeField] UI_SkillDetail_Grow _uiSkillDetailGrow;
    [SerializeField] UI_SkillDetail_Info _uiSkillDetailInfo;
    UI_SkillMain _parentUi;
    SkillItem _skillitem;
    public SkillItem Skillitem => _skillitem;
    ETabId _showType;
    bool _isShow;
    public bool IsShow => _isShow;

    void Awake()
    {
        this.gameObject.SetActive(false);
        _isShow = false;
    }

    public void Show(UIBase parentUi, SkillItem item, ETabId type)
    {
        _parentUi = parentUi as UI_SkillMain;
        _skillitem = item;

        if (!_isShow)
        {
            this.gameObject.SetActive(true);
            _playTeen.Play(true);
            _isShow = true;
        }        
        else
            _skillitem.New = false;

        _tapGroup.Init();
        _tapGroup.SetSelectButton($"{EButtonId.Tab}_{(int)type}");
        ClickTab(type);
        //_parentUi.Refresh_GuideTutorialSelectValue();
        Refresh();
    }

    public void Refresh()
    {
        _skillIcon.Setting(_skillitem);

        _nameText.text = LocalizeManager.GetText(_skillitem.SkillData.Nameid);
        //_nameText.color = InGameUtil.GetGradeColor(_skillitem.SkillData.Grade);
        _levelText.text = LocalizeManager.GetText("LEVEL_MARK", _skillitem.Level);

        _uiSkillDetailGrow.Refresh();
    }

    public void OutUi()
    {
        if (!_isShow) return;        
        _isShow = false;
        _playTeen.Play(false);
        //_parentUi.Refresh_GuideTutorialSelectValue();
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click)
            return;
        var temp = buttonInfo.ID.Split('_');
        int idx = 0;
        if (temp.Length > 1)
            idx = int.Parse(temp[1]);
        switch (EnumHelper.Parse(temp[0], EButtonId.None))
        {
            case EButtonId.Close:
                OutUi();
                break;
            case EButtonId.Tab:
                ClickTab((ETabId)idx);
                break;
        }
    }

    public void ClickTab(ETabId type)
    {
        _showType = type;
        _uiSkillDetailGrow.gameObject.SetActive(_showType == ETabId.Grow);
        _uiSkillDetailInfo.gameObject.SetActive(_showType == ETabId.TranscInfo);
        switch (_showType)
        {
            case ETabId.Grow:
                _uiSkillDetailGrow.Show(_skillitem, this);
                break;
            case ETabId.TranscInfo:
                _uiSkillDetailInfo.Show(_skillitem, this);
                break;
        }
    }

    //public string GetTranscDescText(FSkillTranscendence tranData, FSkillLinkTranscendence linkSkillData)
    //{
    //    string[] skillTexts = { "", "", "", "" };
    //    for (int i = 0; i < linkSkillData.Addtargetskill.Count; i++)
    //    {
    //        int targetSkillId = linkSkillData.Addtargetskill[i];
    //        int addSkillId = linkSkillData.Addskill[i];
    //        if (targetSkillId == 0 || addSkillId == 0) break;
    //        FSkill targetSkillData = DataManager.Instance.GetSkillData(targetSkillId);
    //        skillTexts[i * 2] = LocalizeManager.GetText(targetSkillData.Nameid);
    //        FSkill addSkillData = DataManager.Instance.GetSkillData(addSkillId);
    //        skillTexts[i * 2 + 1] = Formula.NumberToStringBy3_Percent((int)addSkillData.Skillpowermax);
    //    }
    //    return LocalizeManager.GetText(linkSkillData.Adddescid, tranData.Needskilllevel, skillTexts[0], skillTexts[1], skillTexts[2], skillTexts[3]);
    //}

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.U))
        {
            _skillitem.Count += 10;
            Refresh();
            (UIManager.Instance.FindOpenUI(EUIName.UI_SkillMain) as UI_SkillMain)?.OnRefreshUI();
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            var item = _skillitem.GetSpendCost();
            var itemInfo = InventoryManager.Instance.GetItem(item.itemId);
            if (itemInfo != null)
                itemInfo.Count += 100;
            Refresh();
            (UIManager.Instance.FindOpenUI(EUIName.UI_SkillMain) as UI_SkillMain)?.OnRefreshUI();
        }
    }
#endif
}