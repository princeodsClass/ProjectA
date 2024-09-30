using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopBox : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    Transform _tBoxRoot;

    List<BoxDealTable> _boxDeal = new List<BoxDealTable>();
    SlotShopBox[] b;

    PageLobbyShop _pageLobbyShop;
    int[] _counterNew;

    private void Awake()
    {
        _counterNew = new int[3];
        _pageLobbyShop = GameObject.Find("ShopPage").GetComponent<PageLobbyShop>();

        InitializeText();
        SetBox();
    }

    void SetBox()
    {
        ComUtil.DestroyChildren(_tBoxRoot);
        b = new SlotShopBox[3];

        _boxDeal = BoxDealTable.GetSymbolDataPerGroup(1);

        for (int i = 0; i < _boxDeal.Count; i++)
        {
            b[i] = MenuManager.Singleton.LoadComponent<SlotShopBox>(_tBoxRoot, EUIComponent.SlotShopBox);
            b[i].InitializeInfo(_boxDeal[i], this, i);
        }
    }

    public void InitializeSlotToken()
    {
        for ( int i = 0; i < b.Length;i++ )
            b[i].InitializeToken();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_shop_boxdeal_title");
    }

    public void AddCounterNew(int slotIndex, int count)
    {
        _counterNew[slotIndex] = count;
        _pageLobbyShop.AddBoxTokenCounter(slotIndex, count);
    }
}
