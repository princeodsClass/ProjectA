using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotShopBox : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtSlotName, _txtPrice, _txtToken, txtButtonCaption;

    [SerializeField]
    Image _iconBox, _iconPrice, _iconToken;

    [SerializeField]
    Slider _tokenSlider;

    [SerializeField]
    GameObject _goTokenOn;

    Animator _animator;
    BoxDealTable _boxDeal;
    ComShopBox _comp;

    int _slotIndex;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void InitializeInfo(BoxDealTable boxDeal, ComShopBox comp, int slotIndex)
    {
        _boxDeal = boxDeal;
        _comp = comp;
        _slotIndex = slotIndex;

        BoxTable box = BoxTable.GetData(_boxDeal.BoxKey);
        MaterialTable cost = MaterialTable.GetData(_boxDeal.CostItemKey);

        _txtSlotName.text = NameTable.GetValue(box.NameKey);
        _txtPrice.text = _boxDeal.CostItemCount.ToString();
        txtButtonCaption.text = UIStringTable.GetValue("ui_slot_box_complete_caption");

        _iconBox.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, box.Icon);
        _iconPrice.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, cost.Icon);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtPrice.gameObject.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_iconPrice.transform.parent.gameObject.GetComponent<RectTransform>());

        InitializeToken();
    }

    public void OnClick(bool overlap = false)
    {
        int grade = BoxTable.GetData(_boxDeal.BoxKey).Grade;
        int subType = BoxTable.GetData(_boxDeal.BoxKey).SubType;

        if (grade == 1 && subType == 2)
        {
            PopupBoxWeapon pbw = MenuManager.Singleton.OpenPopup<PopupBoxWeapon>(EUIPopup.PopupBoxWeapon, overlap);
            pbw.InitializeInfo(0);
        }
        else if (grade == 1 && subType == 7)
        {
            PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, overlap);
            pbm.InitializeInfo();

        }
        else if (grade == 2 && subType == 7)
        {
            PopupBoxMaterialPremium pbmp =
                MenuManager.Singleton.OpenPopup<PopupBoxMaterialPremium>(EUIPopup.PopupBoxMaterialPremium, overlap);
            pbmp.InitializeInfo();
        }
    }

    public void InitializeToken()
    {
        MaterialTable token = MaterialTable.GetData(_boxDeal.CostTokenKey);

        int hc = GameManager.Singleton.invenMaterial.GetItemCount(token.PrimaryKey);
        int rc = _boxDeal.CostTokenCount;

        _txtToken.text = $"{hc} / {rc}";
        _tokenSlider.value = hc / (float)rc;

        _iconToken.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, token.Icon);
        _goTokenOn.SetActive(hc >= rc);
        _comp.AddCounterNew(_slotIndex, hc / rc);
    }

    public void OnClickTutorial(bool isTutorial)
    {
        OnClick();
        GameObject openToken = GameObject.Find("BuyToken");
        RectTransform rt = openToken.GetComponent<RectTransform>();
        GameManager.Singleton.tutorial.SetFinger(openToken,
                                                 GameObject.Find("PopupBoxWeapon").GetComponent<PopupBoxWeapon>().OnClickBuyToken,
                                                 rt.rect.width, rt.rect.height, 300);
    }
}
