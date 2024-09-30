using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PageLobbyShop : MonoBehaviour
{
    [SerializeField]
    RectTransform[] _rtScrollArea;

    [SerializeField]
    ComShopBox _comShopBox;

    [SerializeField]
    ComShopPremiumWeaponBox _comShopPremiumWeaponBox;

    [SerializeField]
    GameObject _goComShopADS, _goShopG5Box;

    [SerializeField]
    PageLobby _pageLobby;

    int _counterNew;
    int[] _counterBoxToken;

    private void Awake()
    {
        _counterNew = 0;
        _counterBoxToken = new int[4];

        SetComShopADS();
        SetG5Box();
    }

    public void SetComShopADS()
    {
        _goComShopADS.SetActive(GameManager.Singleton.user.IsVIP() ||
            GameManager.Singleton.user.CorrectEpisode + 1 >= GlobalTable.GetData<int>("valueAdsRewardOpenEpisode"));
    }

    public void SetG5Box()
    {
        _goShopG5Box.SetActive(GameManager.Singleton.user.CorrectEpisode + 1 >= GlobalTable.GetData<int>("valueG5ChestOpenEpisode"));
    }

    public void ReSize()
    {
        _rtScrollArea.ToList().ForEach(rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));

        InitializeSlotToken();
    }

    public void InitializeSlotToken()
    {
        if ( _comShopBox.gameObject.activeSelf ) _comShopBox.InitializeSlotToken();
        if ( _comShopPremiumWeaponBox.gameObject.activeSelf ) _comShopPremiumWeaponBox.InitializeSlotToken();
    }

    public void AddCounterNew(int count = 1)
    {
        _counterNew += count;

        SetNewTag();
    }

    public void AddBoxTokenCounter(int slot, int count)
    {
        _counterBoxToken[slot] = count;
        SetNewTag();
    }

    void SetNewTag()
    {
        _pageLobby.SetNewTag(0, ( _counterNew > 0 ||
                                  _counterBoxToken.Sum() > 0 ) && 
                                GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel") );
    }
}
