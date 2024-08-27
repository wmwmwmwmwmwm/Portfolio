using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_Dungeon;

public class UI_Dungeon_RewardElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] TMP_Text _titleText;
    [SerializeField] ItemIcon _itemIcon;
    [SerializeField] GameObject _already, _canGetReward;
    [SerializeField] Transform _movePivot;

    StageInfo _stage;

    public void Setting(StageInfo stage, bool already, bool canGetReward, float yPos)
    {
        _stage = stage;
        int level = stage.Level;
        _button.SetID($"{EButtonId.Clear_Reward}__{level}");
        _titleText.text = level.ToString();
        (int itemId, BigInteger count) = _stage.GetFirstRewardList().First();
        _itemIcon.Setting(itemId, count);
        _itemIcon.SetShiny(canGetReward);
        _already.SetActive(already);
        _canGetReward.SetActive(canGetReward);
        //_button.interactable = canGetReward;
        _itemIcon.DontShowTooltip = canGetReward;
        _movePivot.localPosition = new(0f, yPos, 0f);
    }
}
