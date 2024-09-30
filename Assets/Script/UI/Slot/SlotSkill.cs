using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotSkill : MonoBehaviour
{
    [SerializeField]
    Image _imgSkillIcon;

    [SerializeField]
    TextMeshProUGUI _txtName, _txtDesc;

    [SerializeField]
    GameObject _goDesc;

    public void InitializeInfo(EffectTable item)
    {
        _txtName.text = NameTable.GetValue(item.NameKey);
        _txtDesc.text = DescTable.GetValue(item.DescKey);

        _imgSkillIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, item.Icon);
    }

    public void OnClick()
    {
        _goDesc.SetActive(!_goDesc.activeSelf);
    }
}
