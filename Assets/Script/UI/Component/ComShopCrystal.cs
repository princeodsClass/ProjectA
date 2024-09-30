using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopCrystal : MonoBehaviour
{
    [SerializeField]
    GameObject _goButtonClose;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtBonus;

    [SerializeField]
    Transform _tGoodsRoot;

    [SerializeField]
    RectTransform[] _tForResize;

    List<CrystalDealTable> _goods = new List<CrystalDealTable>();
    List<SlotShopCrystal> _liSlot = new List<SlotShopCrystal>();
    PopupShopCrystal _popup = null;

    uint _ticket;

    private void OnEnable()
    {
        InitializeInfo();
        _goButtonClose.SetActive(_popup != null);
    }

    public void InitializeInfo()
    {
        _ticket = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");

        StartCoroutine(InitializeDealInfo());
        InitializeText();
    }

    public void OnClickTicket()
    {
        PopupMaterial pop = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial);
        pop.InitializeInfo(new ItemMaterial(0, _ticket, 0));
    }

    public void SetFrame(PopupShopCrystal popup)
    {
        _popup = popup;
    }

    public void OnClickClose()
    {
        _popup?.Close();
    }

    IEnumerator InitializeDealInfo()
    {
        yield return StartCoroutine(GameDataManager.Singleton.GetCrystalDealInfo());
    }

    public void SetGoods(List<uint> deallist)
    {
        if ( _goods.Count == 0 )
        {
            _goods = CrystalDealTable.GetList();

            for (int i = 0; i < _goods.Count; i++)
            {
                SlotShopCrystal slot = MenuManager.Singleton.LoadComponent<SlotShopCrystal>(_tGoodsRoot, EUIComponent.SlotShopCrystal);
                slot.InitializeInfo(_goods[i], !deallist.Contains(_goods[i].PrimaryKey));

                _liSlot.Add(slot);
            }

            Resize();
        }
        else
        {
            for (int i = 0; i < _liSlot.Count; i++)
                _liSlot[i].InitializeInfo(_goods[i], !deallist.Contains(_goods[i].PrimaryKey));
        }

    }

    void Resize()
    {
        for ( int i = 0; i < _tForResize.Length; i++ )
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tForResize[i]);
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_shop_crystaldeal_title");
        _txtDesc.text = UIStringTable.GetValue("ui_shop_crystaldeal_desc");
        _txtBonus.text = UIStringTable.GetValue("ui_hint_purchase_desc");
    }
}
