using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using TMPro;

public class SlotBuyVIP : MonoBehaviour
{
    [SerializeField]
    GameObject _goDiscount, _goCancelLine, _goOriginCost, _goDesc,
               _goBGNormal, _goBGDiscount,
               _goWaitCom;

    [SerializeField]
    TextMeshProUGUI _txtOrizinCost, _txtCurrentCost,
                    _txtPeriod, _txtDiscountRatio,
                    _txtDesc;

    [SerializeField]
    Color _colCostNormal, _colCostDiscount;

    VIPDealTable _deal;
    PopupShopVIP _popup;

    public void InitializeInfo(VIPDealTable deal, PopupShopVIP popup)
    {
        _deal = deal;
        _popup = popup;

        _txtPeriod.text = $"<size=80%>x</size> {deal.Period / 60 / 60 / 24}<size=80%> Days </size>";
        _txtDesc.text = UIStringTable.GetValue("ui_component_ads_desc_onlyone");

        _goDesc.SetActive(deal.BuyType == 0);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _goBGDiscount.SetActive(deal.IAPiOS_Discount != 1);
            _goCancelLine.SetActive(deal.IAPiOS_Discount == 1);
            _goDiscount.SetActive(deal.IAPiOS_Discount == 1);
            _goOriginCost.SetActive(deal.IAPiOS_Discount == 1);

            _txtCurrentCost.color = deal.IAPiOS_Discount == 1 ? _colCostDiscount : _colCostNormal;
        }
        else
        {
            _goBGDiscount.SetActive(deal.IAPAOS_Discount == 1);
            _goCancelLine.SetActive(deal.IAPAOS_Discount == 1);
            _goDiscount.SetActive(deal.IAPAOS_Discount == 1);
            _goOriginCost.SetActive(deal.IAPAOS_Discount == 1);

            _txtCurrentCost.color = deal.IAPAOS_Discount == 1 ? _colCostDiscount : _colCostNormal;
        }

        _goBGNormal.SetActive(!_goBGDiscount.activeSelf);

        StartCoroutine(SetPrice(deal));
    }

    IEnumerator SetPrice(VIPDealTable deal)
    {
        bool isVaildProduct = false;
        Product product, product_orgin;

        while (true)
        {
            product = C3rdPartySDKManager.Singleton.GetProduct(Application.platform == RuntimePlatform.IPhonePlayer ?
                      deal.IAPiOS : deal.IAPAOS);
            product_orgin = C3rdPartySDKManager.Singleton.GetProduct(Application.platform == RuntimePlatform.IPhonePlayer ?
                            deal.IAPiOS_Original : deal.IAPAOS_Original);

            _goWaitCom.SetActive(product == null);

            _txtOrizinCost.text = product_orgin?.metadata.localizedPriceString;
            _txtCurrentCost.text = product?.metadata.localizedPriceString;

            decimal orginPrice = product_orgin.metadata.localizedPrice;
            decimal currentPrice = product.metadata.localizedPrice;

            decimal discountRatio = 100 - ( currentPrice  / orginPrice * 100m );
            _txtDiscountRatio.text = $"{Math.Round(discountRatio, 2)} %";

            isVaildProduct = product != null;

            if (isVaildProduct)
                yield break;

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnClick()
    {
        _popup.BuyVIP(_deal);
    }
}
