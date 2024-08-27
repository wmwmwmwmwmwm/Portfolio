using Coffee.UIExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Pet_Gacha : UIMenuBarBase
{
    enum EButtonId
    {
        None,
        DetailInfoButton,
        LevelReward,
    }

    [SerializeField] Canvas _bgCanvas;
    [SerializeField] GachaLevelUp _levelInfo;
    [SerializeField] GachaButton _gachaButton1, _gachaButton2;
    [SerializeField] UIParticle _openEffect;

    UI_GachaCommon _gachaCommon;
    GachaInfoItem _gachaInfo
    {
        get => _gachaCommon._gachaInfo;
        set => _gachaCommon._gachaInfo = value;
    }

    public override void InitUI()
    {
        _gachaCommon = new();
        UIManager.Instance.RegisterEvent_GetItem(EItemType.Currency, gameObject, OnRefreshUI);
        _openEffect.gameObject.SetActive(false);

        _gachaInfo = GachaInfoManager.Instance.GetGachaInfo(EGachaType.Pet);
        _levelInfo.Setting(_gachaInfo);
        base.InitUI();

    }

    public override void EventButton(UIButtonInfo buttonInfo)
    {
        if (buttonInfo.State != UI_ButtonState.Click) return;

        switch (EnumHelper.Parse(buttonInfo.ID, EButtonId.None))
        {
            case EButtonId.LevelReward:
                _gachaCommon.OnClickLevelReward();
                break;
            case EButtonId.DetailInfoButton:
                _gachaCommon.ShowDetailInfo();
                break;
        }
    }

    protected override void OnPrvOpenWork(Action<bool> resultCallback) => resultCallback?.Invoke(true);

    protected override void RefreshUI(LanguageType langCode)
    {
        _bgCanvas.sortingOrder = MenuBar.Canvas.sortingOrder - 1;
        _levelInfo.Refresh();
        FGacha data = _gachaInfo.GachaCosts.First();
        _gachaButton1.Setting(this, null, _gachaInfo, false, data);
        BaseItem costItem = InventoryManager.Instance.GetItem(data.Itemid_01);
        BigInteger count = costItem.Count > int.MaxValue ? int.MaxValue : costItem.Count;
        int secondCount = (int)count / data.Price_01;
        int maxCount = DataManager.Instance.GetSettingContents().PetGachaMaxCount;
        secondCount = Mathf.Clamp(secondCount, 2, maxCount);
        FGacha secondData = new()
        {
            Id = data.Id,
            Gachatype = data.Gachatype,
            Count = secondCount,
            Itemid_01 = data.Itemid_01,
            Price_01 = data.Price_01 * secondCount,
            Gachaitemid = data.Gachaitemid,
        };
        _gachaButton2.Setting(this, null, _gachaInfo, true, secondData);
        _gachaButton2.GetButton().name = "GachaButton_All";     //Tutorial 때문이 이름 고정
        _gachaButton1.RefreshButtonState();
        _gachaButton2.RefreshButtonState();
    }

    public IEnumerator GachaOpenCoroutine()
    {
        _openEffect.gameObject.SetActive(true);
        _openEffect.Play();
        yield return new WaitForSecondsRealtime(_openEffect.particles.First().main.duration);
        _openEffect.gameObject.SetActive(false);
    }
}
