using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotShopCrystal : MonoBehaviour
{
    [SerializeField]
    GameObject _goCounter, _goCounterBonus, _goWaitCom;

    [SerializeField]
    Image _imgIcon, _imgCostIcon;

    [SerializeField]
    TextMeshProUGUI _txtName, _txtCost, _txtRewardCount,
                    _txtRewardBonusCountOriginal, _txtRewardBonusCountBonus,
                    _txtBonusRatio;

    Animator _animator;
    CrystalDealTable _deal;
    bool _isBonus;
    float _bonusRatio;

    public void InitializeInfo(CrystalDealTable deal, bool isBonus = false)
    {
        _deal = deal;
        _isBonus = isBonus;
        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, deal.Icon);
        _imgCostIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(deal.CostItemKey).Icon);
        _txtName.text = NameTable.GetValue(deal.NameKey);

        GameManager.Singleton.StartCoroutine(SetCost());

        _animator = GetComponent<Animator>();
        _bonusRatio = GlobalTable.GetData<float>("ratioCrystalDealBonus");

        SetBonus();
    }

    IEnumerator SetCost()
    {
        _imgCostIcon.gameObject.SetActive(false);

        while (true)
        {
            Product product = C3rdPartySDKManager.Singleton.GetProduct(GetProdectID());

            _goWaitCom.SetActive(product == null);
            _txtCost.gameObject.SetActive(product != null);
            _txtCost.text = product.metadata.localizedPriceString;

            if ( null != product ) yield break;

            yield return new WaitForSeconds(1f);
        }
    }

    string GetProdectID()
    {
        return Application.platform == RuntimePlatform.IPhonePlayer ? _deal.IAPiOS : _deal.IAPAOS;
    }

    public void OnClick()
    {
        string productID = GetProdectID();

        C3rdPartySDKManager.Singleton.PurchaseProduct(productID, CheckPurchase);
    }

    void CheckPurchase(CPurchaseManager purchaseManager, string pID, bool result, string a_oReceipt)
    {
        if ( result )
        {
            StartCoroutine(VerifiedReceipt(pID, a_oReceipt));
        }
        else
        {
            PopupSysMessage fail = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            fail.InitializeInfo("ui_error_title", "ui_error_purchase", "ui_popup_button_confirm");
        }
    }

    IEnumerator VerifiedReceipt(string pID, string a_oReceipt)
    {
        yield return StartCoroutine(GameDataManager.Singleton.VerifyReceipt(a_oReceipt, pID, () =>
        {
            StartCoroutine(Buy(pID));
        }));
    }

    IEnumerator Buy(string pID)
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        C3rdPartySDKManager.Singleton.SendPurchaseLog(C3rdPartySDKManager.Singleton.GetProduct(pID));

        yield return StartCoroutine(GameManager.Singleton.AddItemCS(_deal.RewardItemKey,
                                   (int)(_deal.RewardItemCount * ( _isBonus ? _bonusRatio : 1 ))));
        yield return StartCoroutine(GameDataManager.Singleton.SetCrystalDealInfo(_deal.PrimaryKey));

        PopupSysMessage s = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
        s.InitializeInfo("ui_popup_option_alarm_caption", "ui_shop_deal_complete", "ui_popup_button_confirm");

        InitializeInfo(_deal, false);
    }


    void SetBonus()
    {
        if ( _isBonus )
        {
            _goCounter.SetActive(false);
            _goCounterBonus.SetActive(true);

            _txtRewardBonusCountOriginal.text = _deal.RewardItemCount.ToString();
            _txtRewardBonusCountBonus.text = (_deal.RewardItemCount * _bonusRatio).ToString();
            _txtBonusRatio.text = $"X {_bonusRatio}";
        }
        else
        {
            _goCounter.SetActive(true);
            _goCounterBonus.SetActive(false);

            _txtRewardCount.text = _deal.RewardItemCount.ToString();
        }
    }
}
