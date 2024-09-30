using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupRepositoryExtent : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtCurrentSize, _txtCost, _txtCrystal, _txtButtonConfirm;

    [SerializeField]
    GameObject _goButtonConfirm, _goButtonAdd, _goButtonSubstract;

    PageLobbyTop _top;
    PageLobbyInventory _inven;

    int _curCount, _maxCount;
    int _tabIndex;
    int _add = 0;
    int _cost = 0;

    private void Awake()
    {
        Initialize();

        if (m_MenuMgr.CurScene == ESceneType.Lobby)
        {
            _top = GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>();
            _inven = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();
        }
    }

    void InitializeText()
    { 
        _goButtonConfirm.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_repository_extent_button_caption");
    }

    private void OnEnable()
    {
        _add = 0;
        _cost = 0;
        _curCount = 0;
        _maxCount = 0;
    }

    public void InitializeInfo(int tabIndex, int add = 0)
    {
        string cPre, cSuf, mPre, mSuf;

        _txtCrystal.text = m_InvenMaterial.CalcTotalCrystal().ToString();

        _tabIndex = tabIndex;
        _add += add;

        _goButtonSubstract.GetComponent<SoundButton>().interactable = _add >= 2;

        switch (tabIndex)
        {
            case 0:
                _txtTitle.text = UIStringTable.GetValue("ui_popup_repository_extent_weapon_title");
                _curCount = m_InvenWeapon.GetItemCount();
                _maxCount = m_Account.m_nMaxWeaponRepository;
                break;
            case 2:
                _txtTitle.text = UIStringTable.GetValue("ui_popup_repository_extent_gear_title");
                _curCount = m_InvenGear.GetItemCount();
                _maxCount = m_Account.m_nMaxGearRepository;
                break;
        }

        cPre = _maxCount < _curCount ? "<color=red>" : "";
        cSuf = _maxCount < _curCount ? "</color>" : "";
        mPre = _add > 0 ? "<color=yellow>" : "";
        mSuf = _add > 0 ? "</color>" : "";

        _txtCurrentSize.text = $"{cPre}{_curCount}{cSuf}  / {mPre}{_maxCount + _add}{mSuf}";

        SetConfirmButton();
    }

    void SetConfirmButton()
    {
        string t = UIStringTable.GetValue("ui_popup_repository_extent_button_caption");

        int a = 0;
        int tc = 0;
        int cost = 0;

        switch(_tabIndex)
        {
            case 0:
                tc = GlobalTable.GetData<int>("countStandardWeaponCapacity");
                _cost = GlobalTable.GetData<int>("costWeaponRepositoryExtent");
                break;
            case 2:
                tc = GlobalTable.GetData<int>("countStandardGearCapacity");
                _cost = GlobalTable.GetData<int>("costGearRepositoryExtent");
                break;
        }

        _txtButtonConfirm.text = $"{t}<color=yellow> ({_add})</color>";

        for (int i = _maxCount; i < _maxCount + _add; i++)
            cost = cost + ( ( i + 1 ) - tc ) * _cost;

        _cost = cost;

        _txtCost.text = cost.ToString();
    }

    public void OnClickAdd()
    {
        _goButtonAdd.GetComponent<SoundButton>().interactable = false;
        InitializeInfo(_tabIndex, 1);
        _goButtonAdd.GetComponent<SoundButton>().interactable = true;
    }

    public void OnClickSubstract()
    {
        if ( _add < 2 ) return;

        _goButtonSubstract.GetComponent<SoundButton>().interactable = false;
        InitializeInfo(_tabIndex, -1);
    }

    public void OnClickConfirm()
    {
        _goButtonConfirm.GetComponent<SoundButton>().interactable = false;

        StartCoroutine(CoConfirm());
    }

    IEnumerator CoConfirm()
    {
        if (m_GameMgr.invenMaterial.CalcTotalCrystal() >= _cost)
        {
            int t = m_GameMgr.invenMaterial.GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) - _cost;

            if (t < 0)
            {
                yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_PAY, t));

                if (_cost != -t)
                    yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -(_cost + t)));
            }
            else
            {
                yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -_cost));
            }

        }
        else
        {
            MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            _goButtonConfirm.GetComponent<SoundButton>().interactable = true;
            yield break;
        }

        int count = ( _tabIndex == 0 ? m_Account.m_nMaxWeaponRepository : m_Account.m_nMaxGearRepository ) + _add;

        yield return StartCoroutine(m_DataMgr.IncreaseRepository(_tabIndex, count));

        _goButtonConfirm.GetComponent<SoundButton>().interactable = true;

        if (m_MenuMgr.CurScene == ESceneType.Lobby)
        {
            _top.InitializeCurrency();
            _inven.OnPageButtonClick(_tabIndex);
        }

        Close();
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }
}
