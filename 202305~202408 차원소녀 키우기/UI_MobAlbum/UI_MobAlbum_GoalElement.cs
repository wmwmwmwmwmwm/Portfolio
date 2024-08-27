using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class UI_MobAlbum_GoalElement : MonoBehaviour
{
    [SerializeField] GameObject _itemParent, _statParent;
    [SerializeField] UI_Button _button;
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] Image _statIcon;
    //[SerializeField] Image _lock, _check, _check2;
    [SerializeField] Image _lock, _check;
    [SerializeField] TMP_Text _levelText;

    MonsterAlbumTotalInfo _info;
    FMobAlbumTotal _goal;

    public MonsterAlbumTotalInfo Info => _info;
    public FMobAlbumTotal Goal => _goal;
    public bool Locked => _info.NowExp < _goal.Step;
    public bool CanGetReward => !GotReward && _info.NowExp >= _goal.Step;
    public bool GotReward => _info.CurrentRewardStep >= _goal.Step;

    public void Setting(MonsterAlbumTotalInfo info, FMobAlbumTotal goal, int index, Transform leftPoint, Transform rightPoint)
    {
        _info = info;
        _goal = goal;
        _button.SetID($"{UI_MobAlbum.EButtonId.GoalReward}_{index}");

        bool isItem = _goal.Stat == EStat.NONE;
        _itemParent.SetActive(isItem);
        _statParent.SetActive(!isItem);
        if (isItem)
        {
            //_rewardIcon.sprite = AtlasManager.GetItemIcon(goal.Itemid, EIconType.Icon);
            //_rewardCount.text = Formula.NumberToStringBy3((BigInteger)(int)_goal.Itemcount);
            _itemIcon.Setting(_goal.Itemid, (BigInteger)(int)_goal.Itemcount);
        }
        else
        {
            _statIcon.sprite = InGameUtil.GetStatIcon(_goal.Stat);
        }

        _lock.gameObject.SetActive(Locked);
        _check.gameObject.SetActive(GotReward);
        //_check2.gameObject.SetActive(GotReward);
        _levelText.text = _goal.Step.ToString();
        float pos01 = (float)_goal.Step / _info.MonsterTotalGroup.Last().Step;
        transform.localPosition = Vector3.Lerp(leftPoint.localPosition, rightPoint.localPosition, pos01);
    }
}
