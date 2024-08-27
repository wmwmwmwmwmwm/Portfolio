using CodeStage.AntiCheat.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UI_SkillMain;

//[Obsolete]
//public class UI_OrderSkillUI : UICell//, IMainSkillUI
//{
//    enum EOrderMode
//    {
//        Select,
//        Move,
//    }

//    enum EButtonId
//    {
//        None,

//        OrderSkillIconElement,
//        OrderOK,
//        OrderCancel,
//    }

//    [SerializeField] OrderSkillIconElement[] orderSkillIconElements;
//    [SerializeField] UI_SkillDetail _ui_skillDetail;

//    [SerializeField, ReadOnly] List<SkillOrderInfo.OrderInfo> _copySkillOrderInfoList;
//    [SerializeField, ReadOnly] OrderSkillIconElement _selectOrderSkillIcon;
//    [SerializeField, ReadOnly] int _selectSlotIdx = 0;
//    [SerializeField, ReadOnly] int _setIndex = 0;
//    [SerializeField, ReadOnly] EOrderMode _orderMode = EOrderMode.Select;

//    UI_SkillMain _parentUI;
//    bool isChangePosition = false;

//    public void Init(UI_SkillMain parentUI)
//    {
//        _parentUI = parentUI;
//    }

//    public void Show(int setIndex)
//    {
//        if (gameObject.activeSelf) return;
//        isChangePosition = false;
//        _setIndex = setIndex;
//        _copySkillOrderInfoList = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots.PlayerSkillOrders[_setIndex].ToList();
//        gameObject.SetActive(true);
//        ResetSelected();
//        _parentUI.OnRefreshUI();
//    }

//    void ResetSelected()
//    {
//        _orderMode = EOrderMode.Select;
//        _selectSlotIdx = -1;
//        _selectOrderSkillIcon = null;
//    }

//    public void Refresh()
//    {
//        for (int i = 0; i < orderSkillIconElements.Length; i++)
//        {
//            OrderSkillIconElement element = orderSkillIconElements[i];
//            if (i < _copySkillOrderInfoList.Count)
//            {
//                SkillItem skillItemInfo = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots.PlayerSkillSlots[_setIndex][(int)_copySkillOrderInfoList[i].WeaponType][_copySkillOrderInfoList[i].SlotIndex].ItemInfo;
                
//                bool isSel = _selectSlotIdx == i;
//                element.Setting(i, skillItemInfo, isSel);
//                if (_orderMode == EOrderMode.Move)
//                    element.ArrowActive(!isSel);
//                else
//                    element.ArrowActive(false);
//            }
//            else
//            {
//                element.Setting(i, null, false);
//                element.ArrowActive(false);
//            }
//            element.GetButton().interactable = i < _copySkillOrderInfoList.Count;
//        }
//    }

//    public override void EventButton(UIButtonInfo buttonInfo)
//    {
//        if (buttonInfo.State != UI_ButtonState.Click) return;

//        string[] temp = buttonInfo.ID.Split('_');
//        int idx = 0;
//        if (temp.Length > 1)
//            idx = int.Parse(temp[1]);
//        switch (EnumHelper.Parse(temp[0], EButtonId.None))
//        {
//            case EButtonId.OrderSkillIconElement:
//                {
//                    OrderSkillIconElement selOrderSkillIcon = buttonInfo.Trans.GetComponent<OrderSkillIconElement>();
//                    if (selOrderSkillIcon.GetSkillItemInfo() != null)
//                    {
//                        _ui_skillDetail.Show(_parentUI, selOrderSkillIcon.GetSkillItemInfo(), UI_SkillDetail.ETabId.Grow);
//                    }

//                    int selIdx = idx - 1;
//                    if (_selectOrderSkillIcon == null)
//                    {
//                        _selectSlotIdx = selIdx;
//                        _selectOrderSkillIcon = buttonInfo.Trans.GetComponent<OrderSkillIconElement>();
//                        _orderMode = EOrderMode.Move;
//                    }
//                    else if (_selectOrderSkillIcon.GetIndex() == selIdx)
//                    {
//                        _orderMode = EOrderMode.Select;
//                    }
//                    else
//                    {
//                        isChangePosition = true;
//                        (_copySkillOrderInfoList[selIdx], _copySkillOrderInfoList[_selectSlotIdx]) = (_copySkillOrderInfoList[_selectSlotIdx], _copySkillOrderInfoList[selIdx]);
//                        _orderMode = EOrderMode.Select;
//                        ResetSelected();
//                    }
//                    Refresh();
//                }
//                break;
//            case EButtonId.OrderOK:
//                SkillAllSlotInfos skillSlots = PlayerInfosManager.Instance.GetInfo(EPLAYERTYPE.MYSELF).SkillSlots;
//                skillSlots.PlayerSkillOrders[_setIndex].Copy(_copySkillOrderInfoList);
//                _parentUI.Show(/*EUISkillMode.Main,*/ _setIndex);
//                if (isChangePosition)
//                    UserDataManager.Save_LocalData();
//                isChangePosition = false;
//                break;
//            case EButtonId.OrderCancel:
//                _parentUI.Show(/*EUISkillMode.Main,*/ _setIndex);
//                break;
//        }
//        _parentUI.EventButton(buttonInfo);
//    }

//    public bool OnBackKey()
//    {
//        if (_orderMode == EOrderMode.Move)
//        {
//            _orderMode = EOrderMode.Select;
//            Refresh();
//        }
//        else
//        {
//            _parentUI.Show(/*EUISkillMode.Main, */_setIndex);
//        }

//        return false;
//    }
//}