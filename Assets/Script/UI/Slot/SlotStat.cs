using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotStat : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _textName, _textValue;

    public void Initialize(EWeaponStatType type, string name, string value, bool isColor = false)
    {
        _imgIcon.sprite = GetIcon(type);
        _textName.text = name;
        _textValue.text = isColor ?  $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{value}</color>" : value;
    }

    public void Initialize(EGearStatType type, string name, string value, bool isColor = false)
    {
        _imgIcon.sprite = GetIcon(type);
        _textName.text = name;
        _textValue.text = isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{value}</color>" : value;
    }

    public Sprite GetIcon(EWeaponStatType type)
	{
		string temp = type.ToString();

        return GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, $"ICON_EFFECT_{temp}");
    }

    public Sprite GetIcon(EGearStatType type)
    {
        string temp = type.ToString();

        return GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, $"ICON_EFFECT_{temp}");
    }
}
