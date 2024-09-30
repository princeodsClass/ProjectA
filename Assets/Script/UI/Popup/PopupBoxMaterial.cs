using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBoxMaterial : UIDialog
{
    [SerializeField]
    GameObject _goTip,
               _goButtonBuyToken, _goButtonBuyTokenTen, _goButtonBuyOne, _goButtonBuyTen;

    [SerializeField]
    Transform _tMaterialRoot, _tSymbolRoot;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtItemTitle, _txtTimerTitle, _txtTimer;

    [SerializeField]
    Image _imgCostOneIcon, _imgCostTenIcon, _imgCostTokenIcon, _imgCostTokenTenIcon;

    [SerializeField]
    TextMeshProUGUI _txtCostOne, _txtCostTen, _txtCostToken, _txtCostTokenTen,
                    _txtDescCostOne, _txtDescCostTen, _txtDescCostToken, _txtDescCostTokenTen,
                    _txtCrystal;

    [SerializeField]
    RectTransform[] _rtResizeTarget;

    List<RewardTable> g;

    BoxDealTable _dealKey;

    uint _boxKey, _costItemKey, _costTokenKey;
    int _costItemCount, _costTokenCount;
    int _dealIndex = 0;

	private bool m_bIsOverlap = false;
    DateTime _resetLastDatetime;

    public void InitializeInfo(bool a_bIsOverlap = false)
    {
        _dealKey = BoxDealTable.GetGroup(1)[_dealIndex];
		m_bIsOverlap = a_bIsOverlap;

        SetKeys();

        InitializeText();

        SetBody();
        SetButton();
        Resize();

        CheckToken();
        SetTop();
        ResetButton();

        StartCoroutine(ReSize());
    }

    void ResetButton()
    {
        _goButtonBuyToken.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyTokenTen.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyOne.GetComponent<SoundButton>().interactable = true;
        _goButtonBuyTen.GetComponent<SoundButton>().interactable = true;
    }

    public void SetTop()
    {
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    void SetBody()
    {
        ComUtil.DestroyChildren(_tMaterialRoot);

        BoxTable box = BoxTable.GetData(_boxKey);

        List<RewardTable> rt = new List<RewardTable>();
        rt = RewardTable.GetGroup(box.RewardGroup);

        List<RewardListTable> rlt = new List<RewardListTable>();

        for ( int i = 0; i < rt.Count; i++ )
            rlt.Add(RewardListTable.GetGroup(rt[i].RewardListGroup)[0]);

        for ( int i = 0; i < rlt.Count; i++)
        {
            SlotSimpleItem slot = m_MenuMgr.LoadComponent<SlotSimpleItem>(_tMaterialRoot, EUIComponent.SlotSimpleItem);

            bool isRandom = RewardListTable.GetGroup(rlt[i].Group).Count > 1;
            Sprite sprite = m_ResourceMgr.LoadSprite(EAtlasType.Icons, rt[i].Icon);
            slot.Initialize(rlt[i].RewardKey, sprite, rlt[i].RewardCountMin, rlt[i].RewardCountMax, true, isRandom);
        }
    }

    void SetKeys()
    {
        _boxKey = _dealKey.BoxKey;

        _costItemKey = _dealKey.CostItemKey;
        _costItemCount = _dealKey.CostItemCount;

        _costTokenKey = _dealKey.CostTokenKey;
        _costTokenCount = _dealKey.CostTokenCount;
    }

    void InitializeText()
    {
        _txtTitle.text = NameTable.GetValue(BoxTable.GetData(_boxKey).NameKey);

        _txtDesc.text = UIStringTable.GetValue("ui_popup_boxweapon_desc");
        _txtItemTitle.text = UIStringTable.GetValue("ui_popup_boxweapon_item_title");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");
    }

    void SetButton()
    {
        _imgCostOneIcon.sprite =
        _imgCostTenIcon.sprite =
        m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_costItemKey).Icon);

        _imgCostTokenIcon.sprite =
        _imgCostTokenTenIcon.sprite =
        m_ResourceMgr.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_costTokenKey).Icon);

        _txtCostOne.text = _dealKey.CostItemCount.ToString();
        _txtCostTen.text = ((_dealKey.CostItemCount
                                  * 10f
                                  * GlobalTable.GetData<float>("ratioShopBuyTenDiscount")
                                 )).ToString();
        _txtCostToken.text = _dealKey.CostTokenCount.ToString();
        _txtCostTokenTen.text = (_dealKey.CostTokenCount * 10).ToString();

        _txtDescCostOne.text =
        _txtDescCostToken.text = 
        $"<color=yellow>1</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
        _txtDescCostTen.text = _txtDescCostTokenTen.text =
        $"<color=yellow>10</color> {UIStringTable.GetValue("ui_popup_box_open_button_desc")}";
    }

    void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtDesc.rectTransform.parent.GetComponent<RectTransform>());

        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyOne.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyTen.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyToken.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_goButtonBuyTokenTen.GetComponent<RectTransform>());
    }

    void CheckToken()
    {
        _goButtonBuyToken.SetActive(_costTokenCount <= m_InvenMaterial.GetItemCount(_costTokenKey));
        _goButtonBuyTokenTen.SetActive((_costTokenCount * 10) <= m_InvenMaterial.GetItemCount(_costTokenKey));

        _goButtonBuyOne.SetActive(!_goButtonBuyToken.activeSelf);
        _goButtonBuyTen.SetActive(!_goButtonBuyTokenTen.activeSelf);
    }

    public void OnClickBuyOne()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        if (m_InvenMaterial.CalcTotalCrystal() < _costItemCount)
        {
            SetButtonState(true);

            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            return;
        }

        SetButtonState(false);
        StartCoroutine(BuyOne());
    }

    IEnumerator BuyOne()
    {
        yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(_costItemCount));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Material));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, m_bIsOverlap);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 1, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyToken()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        SetButtonState(false);
        StartCoroutine(BuyToken());
    }

    IEnumerator BuyToken()
    {
        yield return StartCoroutine(m_GameMgr.AddItemCS(_costTokenKey, -_costTokenCount));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Material));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 1, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyTen()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        int cost = (int)(_costItemCount * 10 * GlobalTable.GetData<float>("ratioShopBuyTenDiscount"));

        if (m_InvenMaterial.CalcTotalCrystal() < cost)
        {
            SetButtonState(true);

            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            return;
        }

        SetButtonState(false);
        StartCoroutine(BuyTen(cost));
    }

    IEnumerator BuyTen(int cost)
    {
        yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(cost));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Material, 10));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, m_bIsOverlap);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 10, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    public void OnClickBuyToken10()
    {
        if (!m_DataMgr.AbleToConnect()) return;

        SetButtonState(false);
        StartCoroutine(BuyToken10());
    }

    IEnumerator BuyToken10()
    {
        yield return StartCoroutine(m_GameMgr.AddItemCS(_costTokenKey, -_costTokenCount * 10));
        yield return StartCoroutine(m_DataMgr.AddBuyCount(EDealType.Material, 10));

        SetButtonState(true);

        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, m_bIsOverlap);
        ItemBox box = new ItemBox(0, _boxKey, 0);
        popupReward.InitializeInfo(box, false, false, 10, PopupBoxReward.EBoxType.shopbuy, _dealKey);
    }

    void SetButtonState(bool state)
    {
        _goButtonBuyOne.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyTen.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyToken.GetComponent<SoundButton>().interactable = state;
        _goButtonBuyTokenTen.GetComponent<SoundButton>().interactable = state;
    }

    IEnumerator ReSize()
    {
        for (int i = 0; i < _rtResizeTarget.Length; i++)
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rtResizeTarget[i]);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rtResizeTarget[i].GetComponentInParent<RectTransform>());
        }
    }

    public void OnClickDesc()
    {
        _goTip.SetActive(true);
    }

    public void OnClickTip()
    {
        _goTip.SetActive(false);
    }

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        m_MenuMgr.ShowPopupDimmed(true);
    }

    public override void Close()
    {
        base.Close();

        m_MenuMgr.ShowPopupDimmed(false);
    }

    public override void Escape()
    {
        base.Escape();
    }
}
