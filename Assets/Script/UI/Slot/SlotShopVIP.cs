using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using TMPro;

public class SlotShopVIP : MonoBehaviour
{
    [Header("======== 컴포넌트 ========")]
    [SerializeField]
    Image _imgBG;

    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _textValue, _txtOnceDesc, _txtButtonBuyCaption, _txtButtonBuyCaptionOrigin,
                    _txtDiscountRatio;

    [SerializeField]
    GameObject _goWaitCom, _goOnceDesc, _goDiscount, _goOriginCost,
               _goButtonBGNormal, _goButtonBGDiscount;

    [SerializeField]
    Color _colCostFont, _colOriginCostFont;

    [Header("======== 구매 보상 ========")]
    [SerializeField]
    Image[] _imgPerkIcon;

    [SerializeField]
    TextMeshProUGUI[] _txtPerkDesc;

    VIPDealTable _ctbl;
    ComShopVIP _comp;

    Animator _animator;

    public void InitializeInfo(VIPDealTable ctbl, ComShopVIP comp)
    {
        _animator = GetComponent<Animator>();

        _ctbl = ctbl;
        _comp = comp;

        _imgBG.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Outgame, ctbl.ImageBG);
        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, ctbl.ImageIcon);

        _textValue.text = $"<size=60%>x</size> {ctbl.Period / 60 / 60 / 24}<size=60%> Days </size>";
        _txtOnceDesc.text = UIStringTable.GetValue("ui_component_ads_desc_onlyone");

        _imgPerkIcon[0].sprite = ComUtil.GetIcon(ctbl.RewardItemKey0);
        _imgPerkIcon[1].sprite = ComUtil.GetIcon(ctbl.RewardItemKey1);
        _imgPerkIcon[2].sprite = ComUtil.GetIcon(ctbl.RewardItemKey2);

        _txtPerkDesc[0].text = ctbl.RewardItemCount0.ToString();
        _txtPerkDesc[1].text = ctbl.RewardItemCount1.ToString();
        _txtPerkDesc[2].text = ctbl.RewardItemCount2.ToString();

        _goOnceDesc.SetActive(_ctbl.BuyType == 0);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _goButtonBGDiscount.SetActive(ctbl.IAPiOS_Discount != 1);
            _goDiscount.SetActive(ctbl.IAPiOS_Discount == 1);
            _goOriginCost.SetActive(ctbl.IAPiOS_Discount == 1);

            _txtButtonBuyCaption.color = ctbl.IAPiOS_Discount == 1 ? _colCostFont : _colOriginCostFont;
        }
        else
        {
            _goButtonBGDiscount.SetActive(ctbl.IAPAOS_Discount == 1);
            _goDiscount.SetActive(ctbl.IAPAOS_Discount == 1);
            _goOriginCost.SetActive(ctbl.IAPAOS_Discount == 1);

            _txtButtonBuyCaption.color = ctbl.IAPAOS_Discount == 1 ? _colCostFont : _colOriginCostFont;
        }

        _goButtonBGNormal.SetActive(!_goButtonBGDiscount.activeSelf);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtButtonBuyCaption.transform.parent.GetComponent<RectTransform>());

        StartCoroutine(SetPrice(ctbl));
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

            _txtButtonBuyCaptionOrigin.text = product_orgin?.metadata.localizedPriceString;

            _txtButtonBuyCaption.gameObject.SetActive(product != null);
            _txtButtonBuyCaption.text = product?.metadata.localizedPriceString;



            decimal orginPrice = product_orgin.metadata.localizedPrice;
            decimal currentPrice = product.metadata.localizedPrice;

            decimal discountRatio = 100 - (currentPrice / orginPrice * 100m);
            _txtDiscountRatio.text = $"{Math.Round(discountRatio, 2)} %";




            isVaildProduct = product != null;

            if (isVaildProduct) yield break;

            yield return new WaitForSeconds(1f);
        }
    }

    public void OnClick()
    {
        _comp.BuyVIP(_ctbl);
    }

    public IEnumerator SetSlotAniFocus()
    {
        _animator.SetBool("IsReadyChange", true);

        yield return new WaitForSeconds(1.0f);

        _animator.SetBool("IsReadyChange", false);
    }
}
