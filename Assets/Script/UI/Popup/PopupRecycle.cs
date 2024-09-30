using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupRecycle : UIDialog
{
    [SerializeField]
    Transform _tResultRoot, _tWeaponRoot, _tGearRoot;

    [SerializeField]
    GameObject _goButtonClose, _goButtonRecycle, _goButtonRecycleDimmed;

    [SerializeField]
    GameObject[] _goPanels, _goTabButton, _goTabButtonBG;

    [SerializeField]
    SoundToggle[] _stFilterWeapon, _stFilterGear;

    [SerializeField]
    GameObject[] _stCounterWeapon, _stCounterGear;

    [SerializeField]
    TextMeshProUGUI _txtTapWeapon, _txtTapGear, _textDesc;

    [SerializeField]
    ScrollRect _scrollRect;

    [Space(20)]
    [Header("--- 여기부터 진행 창 ------------")]
    [SerializeField]
    GameObject _goProcessing;

    [SerializeField]
    GameObject _goProcessingImage;

    [SerializeField]
    TextMeshProUGUI _txtProcessingTitle;

    [SerializeField]
    Slider _slProcessing;

    PageLobbyInventory _page;

    Dictionary<long, GameObject> _shareWeapon, _shareGear;
    Dictionary<uint, int> _material;
    Dictionary<uint, GameObject> _result;
    Dictionary<long, GameObject> _selectWeapon, _selectGear;

    int _nCurTab = -1;

    bool _isProcessing = false;

    private void Awake()
    {
        Initialize();
        InitializeText();

        _page = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();
    }

    void InitializeText()
    {
        //_txtTitle.text = UIStringTable.GetValue("ui_popup_option_title");

        _goButtonClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_button_exit");
        _goButtonRecycle.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_page_lobby_inventory_button_recycle");

        _txtTapWeapon.text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_weapon");
        _txtTapGear.text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_gear");

        _textDesc.text = UIStringTable.GetValue("ui_popup_recycle_desc");
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public void InitializeInfo(Dictionary<long, GameObject> dicWeapon, Dictionary<long, GameObject> dicGear, int index)
    {
        InitializeWeapon(dicWeapon);
        InitializeGear(dicGear);

        OnClickTab(index);
    }

    void InitializeWeapon(Dictionary<long, GameObject> dicWeapon)
    {
        List<GameObject> remainder = new List<GameObject>();

        ComUtil.DestroyChildren(_tWeaponRoot);

        foreach ( KeyValuePair<long, GameObject> w in dicWeapon)
        {
            SlotWeapon slot = w.Value.GetComponent<SlotWeapon>();

            if ( slot.IsLock() ||
                 !slot.AbleToRecycle() ||
                 m_Account.m_nWeaponID.Contains(w.Key) )
            {
                remainder.Add(w.Value);
            }
            else
            {
                ComUtil.SetParent(_tWeaponRoot, w.Value.transform);
                _shareWeapon.Add(w.Key, w.Value);
                slot.SetSelected(SlotWeapon.SlotState.recycleUnselected);
                slot.SetBaseScrollRect(_scrollRect);
            }
        }

        for (int i = 0; i < remainder.Count; i++)
        {
            ComUtil.SetParent(_tWeaponRoot, remainder[i].transform);
            remainder[i].GetComponent<SlotWeapon>().SetDim(true);
            remainder[i].GetComponent<SlotWeapon>().SetSelected(SlotWeapon.SlotState.recycleUnselected);
            remainder[i].GetComponent<SlotWeapon>().SetBaseScrollRect(_scrollRect);
        }
    }

    void InitializeGear(Dictionary<long, GameObject> dicGear)
    {
        List<GameObject> remainder = new List<GameObject>();

        ComUtil.DestroyChildren(_tGearRoot);

        foreach (KeyValuePair<long, GameObject> w in dicGear)
        {
            SlotGear slot = w.Value.GetComponent<SlotGear>();

            if (slot.IsLock() ||
                 !slot.AbleToRecycle() ||
                 m_Account.m_nGearID.Contains(w.Key))
            {
                remainder.Add(w.Value);
            }
            else
            {
                ComUtil.SetParent(_tGearRoot, w.Value.transform);
                _shareGear.Add(w.Key, w.Value);
                slot.SetSelected(SlotGear.SlotState.recycleUnselected);
            }
        }

        for (int i = 0; i < remainder.Count; i++)
        {
            ComUtil.SetParent(_tGearRoot, remainder[i].transform);
            remainder[i].GetComponent<SlotGear>().SetDim(true);
            remainder[i].GetComponent<SlotGear>().SetSelected(SlotGear.SlotState.recycleUnselected);
        }
    }

    public void OnClickTab(int index)
    {
        if (index == 1)
        {
            if (m_Account.m_nLevel < GlobalTable.GetData<int>("valueGearOpenLevel"))
            {
                PopupSysMessage pop = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
                pop.InitializeInfo("ui_error_title", "ui_error_notenoughlevel_gear", "ui_common_close");

                return;
            }
        }

        _nCurTab = index;

        for (int i = 0; i < _goPanels.Length; i++)
        {
            _goTabButton[i].GetComponent<SoundButton>().enabled = index != i;
            _goTabButtonBG[i].gameObject.SetActive(index == i);

            _goPanels[i].gameObject.SetActive(index == i);
        }
    }

    public void OnClickFilterWeapon(int grade)
    {
        if (_shareWeapon == null || _shareWeapon.Count < 1) return;

        foreach (KeyValuePair<long, GameObject> m in _shareWeapon)
        {
            SlotWeapon s = m.Value.GetComponent<SlotWeapon>();

            if (s._Item.nGrade == grade + 1)
            {
                SelectWeapon(s._Item, m.Value, (ESelectType)(_stFilterWeapon[grade].isOn ? 0 : 1));
                s.SetSelected(_stFilterWeapon[grade].isOn ? SlotWeapon.SlotState.recycleSelected : SlotWeapon.SlotState.recycleUnselected);
            }
        }
    }

    public void OnClickFilterGear(int grade)
    {
        if (_shareGear == null || _shareGear.Count < 1) return;

        foreach (KeyValuePair<long, GameObject> m in _shareGear)
        {
            SlotGear s = m.Value.GetComponent<SlotGear>();

            if (s._Item.nGrade == grade + 1)
            {
                SelectGear(s._Item, m.Value, (ESelectType)(_stFilterGear[grade].isOn ? 0 : 1));
                s.SetSelected(_stFilterGear[grade].isOn ? SlotGear.SlotState.recycleSelected : SlotGear.SlotState.recycleUnselected);
            }
        }
    }

    void ResetInventory(bool force = false)
    {
        if (!gameObject.activeSelf || force)
        {
            _page.InitializeWeapon();
            _page.InitializeGear();
            _page.InitializeMaterial();
        }
    }

    public override void Open()
    {
        base.Open();

        Initialize();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }

    public enum ESelectType
    {
        Select,
        Unselect,
    }

    public void SelectWeapon(ItemWeapon item, GameObject go, ESelectType type)
    {
        if (type == ESelectType.Select)
        {
            if (!_selectWeapon.ContainsKey(item.id))
                _selectWeapon.Add(item.id, go);
            else
                return;
        }
        else
        {
            if (_selectWeapon.ContainsKey(item.id))
            {
                _selectWeapon.Remove(item.id);
                _stFilterWeapon[item.nGrade - 1].isOn = false;
            }
            else
                return;
        }

        SetCounterWeapon();
        AddMaterial(item, type);
        ChangeRecycleButtonState(_selectWeapon.Count > 0);
        SetPreview();        
    }

    public void SelectGear(ItemGear item, GameObject go, ESelectType type)
    {
        if (type == ESelectType.Select)
        {
            if (!_selectGear.ContainsKey(item.id))
                _selectGear.Add(item.id, go);
            else
                return;
        }
        else
        {
            if (_selectGear.ContainsKey(item.id))
            {
                _selectGear.Remove(item.id);
                _stFilterGear[item.nGrade - 1].isOn = false;
            }
            else
                return;
        }

        SetCounterGear();
        AddMaterial(item, type);
        ChangeRecycleButtonState(_selectGear.Count > 0);
        SetPreview();
    }

    void AddMaterial(ItemBase item, ESelectType type)
    {
        for (int i = 0; i < item.nRecycleMaterialKey.Length; i++)
            if (_material.ContainsKey(item.nRecycleMaterialKey[i]))
                _material[item.nRecycleMaterialKey[i]] += (item.RecycleMaterialCount[i]
                                                            * (type == ESelectType.Unselect ? -1 : 1));
            else
                if (item.nRecycleMaterialKey[i] > 0)
                _material.Add(item.nRecycleMaterialKey[i], item.RecycleMaterialCount[i]);
    }

    void SetCounter()
    {
        SetCounterWeapon();
        SetCounterGear();
    }

    void SetCounterWeapon()
    {
        for (int i = 0; i < _stCounterWeapon.Length; i++)
        {
            _stCounterWeapon[i].GetComponent<TextMeshProUGUI>().text =
                _selectWeapon.Count(item => item.Value.GetComponent<SlotWeapon>()._Item.nGrade == i + 1).ToString();
            _stCounterWeapon[i].SetActive(_stCounterWeapon[i].GetComponent<TextMeshProUGUI>().text != "0");
        }
    }

    void SetCounterGear()
    {
        for (int i = 0; i < _stCounterGear.Length; i++)
        {
            _stCounterGear[i].GetComponent<TextMeshProUGUI>().text =
                _selectGear.Count(item => item.Value.GetComponent<SlotGear>()._Item.nGrade == i + 1).ToString();
            _stCounterGear[i].SetActive(_stCounterGear[i].GetComponent<TextMeshProUGUI>().text != "0");
        }
    }

    void SetPreview()
    {
        ComUtil.DestroyChildren(_tResultRoot);
        _result.Clear();

        foreach ( KeyValuePair<uint, int> m in _material )
        {
            if (m.Value > 0)
            {
                SlotMaterial slot = m_MenuMgr.LoadComponent<SlotMaterial>(_tResultRoot, EUIComponent.SlotMaterial);
                slot.Initialize(m.Key, m.Value, true, false, true, SlotMaterial.EVolumeType.value);

                _result.Add(m.Key, slot.gameObject);
            }
        }
    }

    public void OnClickRecycle()
    {
        ChangeRecycleButtonState(false);
        StartCoroutine(Recycle());
    }

    IEnumerator Recycle()
    {
        m_Account.RefreshQuestCount(EQuestActionType.CompleteRecycle, _selectWeapon.Count + _selectGear.Count);

        ComUtil.DestroyChildren(_tResultRoot);

        DeleteWeapon();
        DeleteGear();

        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        _isProcessing = true;
        Coroutine w = StartCoroutine(SetSlider(wait));

        foreach (KeyValuePair<uint, int> m in _material)
            yield return StartCoroutine(m_GameMgr.AddItemCS(m.Key, m.Value));

        _isProcessing = false;
        StopCoroutine(w);
        _goProcessing.SetActive(false);
        wait.Close();

        OnEnable();
        ResetInventory(true);
        _page.OnClickRecycle(_nCurTab);
    }

    IEnumerator SetSlider(PopupWait4Response wait)
    {
        _slProcessing.value = 0f;
        _goProcessing.SetActive(true);

        WaitForSecondsRealtime wTime = new WaitForSecondsRealtime(0.01f);

        while (_isProcessing)
        {
            _slProcessing.value += 0.01f;

            if ( _slProcessing.value > 1 ) _slProcessing.value = 1;

            yield return wTime;
        }
    }

    void DeleteWeapon()
    {
        foreach (KeyValuePair<long, GameObject> s in _selectWeapon)
        {
            _shareWeapon.Remove(s.Key);

            m_DataMgr.DeleteItem(s.Key, EDatabaseType.inventory);
            Destroy(s.Value);

            m_InvenWeapon.RemoveItem(s.Key);
        }

        _selectWeapon.Clear();
    }

    void DeleteGear()
    {
        foreach (KeyValuePair<long, GameObject> s in _selectGear)
        {
            _shareGear.Remove(s.Key);

            m_DataMgr.DeleteItem(s.Key, EDatabaseType.inventory);
            Destroy(s.Value);

            m_InvenGear.RemoveItem(s.Key);
        }

        _selectGear.Clear();
    }

    private void OnEnable()
    {
        _selectWeapon = new Dictionary<long, GameObject>();
        _selectGear = new Dictionary<long, GameObject>();
        _shareWeapon = new Dictionary<long, GameObject>();
        _shareGear = new Dictionary<long, GameObject>();

        _material = new Dictionary<uint, int>();
        _result = new Dictionary<uint, GameObject>();

        ChangeRecycleButtonState(false);
        SetCounter();

        ComUtil.DestroyChildren(_tResultRoot);

        Array.ForEach(_stFilterWeapon, e => e.isOn = false);
        Array.ForEach(_stFilterGear, e => e.isOn = false);
    }

    void ChangeRecycleButtonState(bool state)
    {
        _goButtonRecycle.GetComponent<SoundButton>().interactable = state;
        _goButtonRecycleDimmed.SetActive(!state);

        _textDesc.transform.parent.gameObject.SetActive(state);
    }

    private void OnDisable()
    {
        ResetInventory();
    }
}
