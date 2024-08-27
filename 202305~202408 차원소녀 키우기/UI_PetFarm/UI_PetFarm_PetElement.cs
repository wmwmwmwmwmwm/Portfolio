using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class UI_PetFarm_PetElement : MonoBehaviour
{
    [SerializeField] UI_Button _button;
    [SerializeField] Image _bg;
    [SerializeField] Image _portrait;
    [SerializeField] TMP_Text _text;
    [SerializeField] GameObject _equipMark;
    [SerializeField] GameObject _equipMarkOther;
    [SerializeField] GameObject _special;

    PetInfoItem _pet;

    public UI_Button Button => _button;
    public PetInfoItem Pet => _pet;
    PetFarmItemInfo PetFarm => InventoryManager.Instance.PetFarmItemInfo;

    public void Setting(PetInfoItem pet, int spotIndex, bool special, bool isInfoSlot)
    {
        _pet = pet;
        bool active = pet != null;
        _portrait.gameObject.SetActive(active);
        _equipMark.SetActive(active);
        _equipMarkOther.SetActive(active);
        _special.SetActive(active);
        if (pet != null)
        {
            _bg.sprite = InGameUtil.GetGradeItemBg(_pet.itemData.Grade);
            _portrait.sprite = AtlasManager.GetItemIcon(_pet.PetData, EIconType.Icon);
            _text.text = $"{pet.PetData.Farm_output}/m";
            _equipMark.SetActive(!isInfoSlot && PetFarm.CheckEquipped(spotIndex, pet.Id));
            _equipMarkOther.SetActive(!isInfoSlot && PetFarm.CheckEquipOther(spotIndex, pet.Id));
            _special.SetActive(special);
        }
        else
        {
            _bg.sprite = InGameUtil.GetGradeItemBg(EItemGrade.E);
            _text.text = "";
        }
    }
}
