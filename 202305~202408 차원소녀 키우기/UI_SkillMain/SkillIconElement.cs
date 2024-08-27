using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[Obsolete]
public class SkillIconElement : MonoBehaviour
{
    const int STAR_MAXCOUNT = 6;

    [Serializable]
    public class Star
    {
        [SerializeField] RectTransform _root;
        [SerializeField] Image _write_Image;
        [SerializeField] Image _yellow_Image;

        public Star(RectTransform root)
        {
            _root = root;
            _write_Image = _root.Find("white_Image").GetComponent<Image>();
            _yellow_Image = _root.Find("yellow_Image").GetComponent<Image>();
        }
        public Star Copy()
        {
            var copyObj = GameObject.Instantiate(_root);
            copyObj.transform.SetParent(_root.parent);
            copyObj.transform.localPosition = Vector3.zero;
            copyObj.transform.localScale = Vector3.one;
            copyObj.gameObject.SetActive(true);
            return new Star(copyObj);
        }
        public void Del()
        {
            GameObject.Destroy(_root.gameObject);
        }
        public void Rot(float rot)
        {
            _root.localEulerAngles = new Vector3(0, 0, rot);
        }

        public void Set(bool is_Transce)
        {
            _write_Image.gameObject.SetActive(!is_Transce);
            _yellow_Image.gameObject.SetActive(is_Transce);
        }
    }

    [SerializeField] Image _skillIcon;
    [SerializeField] Sprite _emptyIcon;
    [SerializeField] Star _origin_Star;
    //[SerializeField, ReadOnly] List<Star> _stars;
    [SerializeField] Image _costume;
    [SerializeField] Image _minusImage;

    SkillItem _skillItem;

    public SkillItem Skill => _skillItem;

    public void Setting(SkillItem skillItem)
    {
        _skillItem = skillItem;
        Refresh();
    }

    public void SetActiveMinusButton(bool isActive)
    {
        bool active = isActive && _skillItem != null;
        _minusImage.gameObject.SetActive(active);
    }

    public void Refresh()
    {
        //for (int i = 0; i < _stars.Count; i++)
        //{
        //    _stars[i].Del();
        //}
        //_stars.Clear();

        if (_skillItem == null)
        {
            _skillIcon.sprite = _emptyIcon;
            _minusImage.gameObject.SetActive(false);
            _costume.gameObject.SetActive(false);
            return;
        }

        _skillIcon.sprite = AtlasManager.GetItemIcon(_skillItem, EIconType.Icon);
        _costume.gameObject.SetActive(_skillItem.IsCostumeSkill);

        //if (_skillItem.AdvanceStar > 0)
        //{
        //    float startRot = (STAR_MAXCOUNT - 1) * 0.5f * 18.0f;
        //    for (int i = 0; i < _skillItem.AdvanceStar; i++)
        //    {
        //        var temp = _origin_Star.Copy();
        //        temp.Rot(startRot - i * 18.0f);
        //        temp.Set(i < _skillItem.TranscendenceStar);
        //        _stars.Add(temp);
        //    }
        //}
    }
}
