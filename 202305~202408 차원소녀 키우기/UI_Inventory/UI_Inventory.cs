using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class UI_Inventory : UIBase
{
    public static Dictionary<EItemType, int> InventoryTypes = new Dictionary<EItemType, int>()
    {
        { EItemType.Use_Potion, 1 },
        { EItemType.Use_Buff, 1 },
        { EItemType.Use_Offline, 1 },
        { EItemType.Box, 1 },
    };

    public static ETabId StartTab;

    public enum ETabId
    {
        None, Potion, Buff, Etc
    }

    public enum EButtonId
    {
        None,
        BgClose,
        Item,
        Item_Close,
        Info_Close,
        Info_BoxInfo,
        Info_Use,
        Info_Min, Info_Minus, Info_Plus, Info_Max,
        Box_Close,
        Box_Entry,
    }

    //[SerializeField] UI_Button _bgCloseButton;
    [SerializeField] GameObject _itemsPanel, _itemInfoPanel, _boxInfoPanel;

    [Header("아이템 목록")]
    [SerializeField] UI_TabGroup _items_TabGroup;
    [SerializeField] UI_Inventory_ItemElement _items_ItemPrefab;
    [SerializeField] Transform _items_ListParent;
    [SerializeField] TMP_Text _items_NoneText;

    [Header("아이템 정보 패널")]
    [SerializeField] Image _itemInfo_TitleBg;
    [SerializeField] TMP_Text _itemInfo_ItemName;
    [SerializeField] TMP_Text _itemInfo_ItemDesc;
    [SerializeField] ItemIcon _itemInfo_ItemIcon;
    [SerializeField] TMP_Text _itemInfo_CountText;
    [SerializeField] UI_Button _itemInfo_BoxInfoButton;
    [SerializeField] GameObject _itemInfo_MinMax;
    [SerializeField] UI_Button _itemInfo_UseButton;

    [Header("박스 정보 패널")]
    [SerializeField] TMP_Text _boxInfo_ItemName;
    [SerializeField] ScrollRect _boxInfo_ScrollRect;
    [SerializeField] UI_Inventory_BoxInfoEntry _boxInfo_ItemEntryPrefab;
    [SerializeField] Transform _boxInfo_ListParent;
    [SerializeField] UI_Button _boxInfo_CloseButton;

    ETabId _currentTab;
    UI_Inventory_ItemElement __selectedItem;
    UI_Inventory_ItemElement _selectedItem
    {
        get => __selectedItem;
        set
        {
            SetSelectValue(value?.ItemId.ToString());
            __selectedItem = value;
        }
    }
    List<UI_Inventory_ItemElement> _items_Elements;
    List<UI_Inventory_BoxInfoEntry> _boxInfo_Entrys;
    UI_Inventory_BoxInfoEntry _boxInfo_Selected;
    BigInteger _itemInfo_Count;

    InventoryManager Inventory => InventoryManager.Instance;
    bool IsSelectMaxType
    {
        get
        {
            if (!_selectedItem) return false;
            if (_selectedItem.Item is BoxItem box && !box.IsSelect) return true;
            if (_selectedItem.Item.EItemType == EItemType.Use_Offline) return true;
            return false;
        }
    }
    bool IsSelectBox
    {
        get
        {
            if (!_selectedItem) return false;
            BoxItem box = _selectedItem.Item as BoxItem;
            return box != null && box.IsSelect;
        }
    }

    public override void InitUI()
    {
        _items_Elements = new();
        _boxInfo_Entrys = new();
        _itemInfoPanel.SetActive(false);
        _boxInfoPanel.SetActive(false);
        _boxInfo_ItemEntryPrefab.gameObject.SetActive(false);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Use_Potion, gameObject, OnRefreshUI);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Use_Buff, gameObject, OnRefreshUI);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Box, gameObject, OnRefreshUI);
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Use_Offline, gameObject, OnRefreshUI);
        //_bgCloseButton.gameObject.SetActive(false);

        _items_TabGroup.Init();
        if (StartTab != ETabId.None)
        {
            SelectTab(StartTab);
            _items_TabGroup.SetSelectButton((int)StartTab - 1);
            StartTab = ETabId.None;
        }
        else
        {
            SelectTab(ETabId.Potion);
            _items_TabGroup.SetSelectButton(0);
        }

        _items_TabGroup.Buttons.ForEach(
            p =>
            {
                var tabIndex = (ETabId)int.Parse(p.ID);
                p.GetComponentInChildren<RedDot_ItemNewTab>().Setting(
                    tabIndex switch
                    {
                        ETabId.Potion => Inventory.PotionItems.Values.ToList(),
                        ETabId.Buff => Inventory.BuffItems.Values.ToList(),
                        _ => Inventory.ItemsList.Where(p => p is BoxItem || p is UseOffLineItem).Select(x => (BaseItem)x).ToList()
                    }
                    );
            });

        base.InitUI();
    }

    public override void OutUI(bool isDirect = false)
    {
        _items_Elements.ForEach(p =>
        {
            if (p != null && p.Item != null)
                p.Item.New = false;
        });
        base.OutUI(isDirect);
    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        // 탭 선택
        if (buttonInfo.ButtonType == UI_ButtonType.TAB)
        {
            int tabIndex = int.Parse(buttonInfo.ID);
            SelectTab((ETabId)tabIndex);
            return;
        }

        // 버튼 선택
        string[] tokens = buttonInfo.ID.Split("__");
        EButtonId buttonId = EnumHelper.Parse(tokens[0], EButtonId.None);
        if (buttonId == EButtonId.BgClose)
        {
            if (_selectedItem)
            {
                buttonId = EButtonId.Info_Close;
            }
            else
            {
                buttonId = EButtonId.Item_Close;
            }
        }
        switch (buttonId)
        {
            case EButtonId.Item:
                int itemId = int.Parse(tokens[1]);
                if (itemId == 0) return;
                SelectItem(_items_Elements.Find(x => x.ItemId == itemId));
                break;
            case EButtonId.Item_Close:
                OutUI();
                break;
            case EButtonId.Info_Close:
                _itemInfoPanel.SetActive(false);
                _boxInfoPanel.SetActive(false);
                SelectItem(null);
                break;
            case EButtonId.Info_BoxInfo:
                _boxInfoPanel.SetActive(true);
                RefreshBoxInfo();
                break;
            case EButtonId.Info_Use:
                UseItem();
                break;
            case EButtonId.Info_Min:
                _itemInfo_Count = 1;
                RefreshItemInfo();
                break;
            case EButtonId.Info_Minus:
                if (_itemInfo_Count > 1) _itemInfo_Count--;
                RefreshItemInfo();
                break;
            case EButtonId.Info_Plus:
                if (_itemInfo_Count < _selectedItem.Item.Count) _itemInfo_Count++;
                RefreshItemInfo();
                break;
            case EButtonId.Info_Max:
                _itemInfo_Count = _selectedItem.Item.Count;
                RefreshItemInfo();
                break;
            case EButtonId.Box_Close:
                _boxInfoPanel.SetActive(false);
                break;
            case EButtonId.Box_Entry:
                if (!IsSelectBox) return;
                int index = int.Parse(tokens[1]);
                SelectBoxEntry(_boxInfo_Entrys[index]);
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        RefreshItemList();
        RefreshItemInfo();
    }

    public override bool OnBackKey()
    {
        if (_selectedItem)
        {
            SelectItem(null);
            return false;
        }
        return base.OnBackKey();
    }

    void SelectTab(ETabId tabId)
    {
        // 탭 선택
        if (_currentTab == tabId) return;
        _currentTab = tabId;
        for (int i = 0; i < _items_TabGroup.Buttons.Count; i++)
        {
            UIPlayTween tween = _items_TabGroup.Buttons[i].GetComponentInChildren<UIPlayTween>();
            if (i + 1 == (int)tabId)
            {
                tween.Play();
            }
            else
            {
                tween.Stop();
                tween.transform.rotation = Quaternion.identity;
                tween.transform.localScale = Vector3.one;
            }
        }
        _items_Elements.ForEach(p =>
        {
            if (p != null && p.Item != null)
                p.Item.New = false;
        });
        _items_Elements.ForEach(x => Destroy(x.gameObject));
        _items_Elements.Clear();
        RefreshItemList();
        SelectItem(null);
        RefreshBoxInfo();
    }

    void RefreshItemList()
    {
        // 아이템 목록 초기화
        List<BaseItem> items = _currentTab switch
        {
            ETabId.Potion => Inventory.PotionItems.Values.ToList(),
            ETabId.Buff => Inventory.BuffItems.Values.ToList(),
            _ => Inventory.ItemsList.Where(p => p is BoxItem || p is UseOffLineItem).Select(x => (BaseItem)x).ToList()
        };

        foreach (BaseItem item in items)
        {
            if (item.Count == 0) continue;
            if (_items_Elements.Find(x => x.Item == item)) continue;
            UI_Inventory_ItemElement element = Instantiate(_items_ItemPrefab, _items_ListParent);
            element.SetItemId(item.Id);
            _items_Elements.Add(element);
        }
        List<UI_Inventory_ItemElement> elementsCopy = new(_items_Elements);
        foreach (UI_Inventory_ItemElement itemElement in elementsCopy)
        {
            itemElement.Setting();
            if (itemElement.Item == null || itemElement.Item.Count == 0)
            {
                _items_Elements.Remove(itemElement);
                Destroy(itemElement.gameObject);
            }
        }
        _items_NoneText.gameObject.SetActive(_items_Elements.Count == 0);
        _items_NoneText.text = LocalizeManager.GetText(_currentTab switch
        {
            ETabId.Potion => "UI_Inventory_Empty_01",
            ETabId.Buff => "UI_Inventory_Empty_02",
            _ => "UI_Inventory_Empty_03",
        });

        //// 빈 아이콘으로 빈칸 채우기
        //int placeholderCount = 12 - _items_Elements.Count;
        //for (int i = 0; i < placeholderCount; i++)
        //{
        //    UI_Inventory_ItemElement element = Instantiate(_items_ItemPrefab, _items_ListParent);
        //    element.SetItemId(0);
        //    element.Setting();
        //    _items_Elements.Add(element);
        //}
    }

    void SelectItem(UI_Inventory_ItemElement newSelected)
    {
        // 아이템 선택
        _selectedItem = newSelected;
        _items_Elements.ForEach(x => x.Select(x == _selectedItem));
        _itemInfoPanel.SetActive(_selectedItem != null);
        //_bgCloseButton.gameObject.SetActive(_selectedItem != null);
        if (IsSelectMaxType)
        {
            _itemInfo_Count = _selectedItem.Item.Count;
        }
        else
        {
            _itemInfo_Count = 1;
        }
        RefreshItemInfo();
        if (IsSelectBox)
        {
            _boxInfoPanel.SetActive(true);
        }
        RefreshBoxInfo();
    }

    void RefreshItemInfo()
    {
        // 아이템 정보 패널 초기화
        if (!_selectedItem)
        {
            _itemInfoPanel.SetActive(false);
            //_bgCloseButton.gameObject.SetActive(false);
            _boxInfoPanel.SetActive(false);
            return;
        }
        BaseItem info = _selectedItem.Item;
        string spritePath = DataManager.Instance.GetSettingContents().ItemGrade_TitleFrame[(int)info.itemData.Grade - 1];
        _itemInfo_TitleBg.sprite = AtlasManager.GetSprite(EATLAS_TYPE.UI_Parts, spritePath);
        _itemInfo_ItemName.text = $"{LocalizeManager.GetText(info.itemData.Nameid)}";
        //_itemInfo_ItemName.color = InGameUtil.GetGradeColor(info.itemData.Grade);
        _itemInfo_ItemDesc.text = LocalizeManager.GetText(info.itemData.Descid);
        _itemInfo_ItemIcon.Setting(info.Id, info.Count);
        _itemInfo_CountText.text = _itemInfo_Count.ToString("N0");
        _itemInfo_MinMax.SetActive(info.EItemType != EItemType.Use_Potion);
        _itemInfo_BoxInfoButton.gameObject.SetActive(info.EItemType == EItemType.Box && !IsSelectBox);
        _itemInfo_UseButton.interactable = info.Count >= _itemInfo_Count;
    }

    void RefreshBoxInfo()
    {
        // 박스 정보 패널 초기화
        if (!_selectedItem || _selectedItem.Item.EItemType != EItemType.Box)
        {
            _boxInfoPanel.SetActive(false);
            return;
        }

        _boxInfo_ItemName.text = IsSelectBox ? LocalizeManager.GetText("UI_Box_Select_ListTitle")
            : LocalizeManager.GetText("UI_Inven_Box_Compo_Title");
        _boxInfo_Entrys.ForEach(x => Destroy(x.gameObject));
        _boxInfo_Entrys.Clear();
        BoxItem box = (BoxItem)_selectedItem.Item;
        List<BoxItem.ItemNode> list = box.GetRating();
        for (int i = 0; i < list.Count; i++)
        {
            BoxItem.ItemNode item = list[i];
            UI_Inventory_BoxInfoEntry entry = Instantiate(_boxInfo_ItemEntryPrefab, _boxInfo_ListParent);
            entry.gameObject.SetActive(true);
            entry.Setting(item, i);
            _boxInfo_Entrys.Add(entry);
        }
        _boxInfo_ScrollRect.verticalNormalizedPosition = 1f;

        // 선택형 상자 세팅
        _boxInfo_CloseButton.gameObject.SetActive(!IsSelectBox);
        if (IsSelectBox)
        {
            SelectBoxEntry(_boxInfo_Entrys[0]);
        }
        else
        {
            SelectBoxEntry(null);
        }
    }

    void UseItem()
    {
        switch (_selectedItem.Item.EItemType)
        {
            case EItemType.Use_Potion:
                // 포션 사용
                Inventory.UsePotion(_selectedItem.Item);
                break;
            case EItemType.Use_Buff:
                // 요리 사용
                if (Inventory.UseBuff(_selectedItem.Item, (int)_itemInfo_Count))
                {
                    _itemInfo_Count = 1;
                }
                break;
            case EItemType.Box:
                {
                    // 박스 사용
                    BoxItem box = (BoxItem)_selectedItem.Item;
                    if (box.Count < _itemInfo_Count) return;
                    List<BoxItem.ItemBoxResultItem> result;
                    if (IsSelectBox)
                    {
                        BoxItem.ItemBoxResultItem r = box.ReceiveContentsOfSelectBox((int)_itemInfo_Count, _boxInfo_Selected.Index);
                        result = new() { r };
                    }
                    else
                    {
                        result = box.ReceiveContentsOfBox((int)_itemInfo_Count);
                    }
                    box.Count -= _itemInfo_Count;
                    QuestManager.Instance.AddValue(EMissionType.Use_ETCItem, 0, _itemInfo_Count);
                    _itemInfo_Count = 1;
                    StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
                    {
                        TitleText = LocalizeManager.GetText("UI_Inven_OpenResult_Title"),
                        ItemIds = result.Select(x => (int)x.ItemID).ToList(),
                        ItemCounts = result.Select(x => (BigInteger)x.ItemCount).ToList()
                    }));
                }
                break;
            case EItemType.Use_Offline:
                {
                    var offlineItem = _selectedItem.Item as UseOffLineItem;
                    if (offlineItem.Count < _itemInfo_Count) return;
                    var result = offlineItem.ReceiveContentsOfItem((int)_itemInfo_Count);
                    StartCoroutine(UIManager.Instance.ShowGetItemPopupCoroutine(new()
                    {
                        TitleText = LocalizeManager.GetText("UI_Inven_OpenResult_Title"),
                        ItemIds = result.Select(x => (int)x.ItemID).ToList(),
                        ItemCounts = result.Select(x => (BigInteger)x.ItemCount).ToList()
                    }));
                }
                break;
        }
        UserDataManager.Save_LocalData();

        // 모두 소모시 다음 또는 이전 아이템 선택
        if (_selectedItem.Item.Count == 0)
        {
            int nextIndex = _items_Elements.IndexOf(_selectedItem) + 1;
            int prevIndex = _items_Elements.IndexOf(_selectedItem) - 1;
            if (nextIndex < _items_Elements.Count && _items_Elements[nextIndex].Item != null)
            {
                SelectItem(_items_Elements[nextIndex]);
            }
            else if (prevIndex >= 0 && prevIndex < _items_Elements.Count && _items_Elements[prevIndex].Item != null)
            {
                SelectItem(_items_Elements[prevIndex]);
            }
            else
            {
                SelectItem(null);
            }
        }
        OnRefreshUI();
    }

    void SelectBoxEntry(UI_Inventory_BoxInfoEntry newSelected)
    {
        _boxInfo_Selected = newSelected;
        foreach (UI_Inventory_BoxInfoEntry entry in _boxInfo_Entrys)
        {
            entry.Select(entry == _boxInfo_Selected);
        }
    }
}
