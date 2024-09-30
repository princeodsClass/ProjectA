using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBoxExchange : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc;

    [SerializeField]
    Transform _tSlotMain;

    [SerializeField]
    Transform[] _tSlotBox = new Transform[4];

    [SerializeField]
    GameObject _goButton;

    List<SlotBox> _slotBox = new List<SlotBox>();
    ItemBox _box;

    long _nSelectSlotID = -1;

    private void Awake()
    {
        InitializeText();
    }

    void OnEnable()
    {
        Initialize();
        SetButtonState();
    }

    void SetButtonState(bool state = false)
    {
        _goButton.GetComponent<SoundButton>().interactable = state;
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_boxexchange_title");
        _txtDesc.text = UIStringTable.GetValue("ui_popup_boxexchange_desc");

        _goButton.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_boxexchange_button");
    }

    public void InitializeInfo(ItemBox box)
    {
        _box = box;
        SetMainBox();
        SetCurrentBox();
    }

    public void ChangeSelectSlot(long id, int slotNumber)
    {
        SetButtonState(true);

        _nSelectSlotID = id;
        _slotBox.ForEach(slot =>  slot.SetHeader(slot.GetID() == id));

        if ( _slotBox[slotNumber]._rewardBoxState == ERewardBoxState.Complete )
            _txtDesc.text = UIStringTable.GetValue("ui_popup_boxexchange_desc_complete");
        else if (_slotBox[slotNumber]._rewardBoxState == ERewardBoxState.Opening)
            _txtDesc.text = UIStringTable.GetValue("ui_popup_boxexchange_desc_openning");
        else
            _txtDesc.text = UIStringTable.GetValue("ui_popup_boxexchange_desc");
    }    

    void SetMainBox()
    {
        SlotBox slotBox = m_MenuMgr.LoadComponent<SlotBox>(_tSlotMain, EUIComponent.SlotBox);
        slotBox.Initialize(_box, true, false, true, true,SlotBox.placeState.exchange_main);
    }

    void SetCurrentBox()
    {
        foreach (ItemBox box in m_InvenBox)
        {
            SlotBox slotBox = m_MenuMgr.LoadComponent<SlotBox>(_tSlotBox[box.nSlotNumber], EUIComponent.SlotBox);
            slotBox.Initialize(box, true, false, true, true, SlotBox.placeState.exchange_target);
            slotBox.SetNewTag(false);

            _slotBox.Add(slotBox);
        }
    }

    public override void Initialize()
    {
        ComUtil.DestroyChildren(_tSlotMain);
        _slotBox.Clear();
        _tSlotBox.ToList().ForEach(go => ComUtil.DestroyChildren(go));

        base.Initialize();
    }

    public void OnClickExchange()
    {
        SetButtonState();

        ERewardBoxState t = ERewardBoxState.END;

        for ( int i = 0; i < 4; i++ )
            if ( _slotBox[i].GetID() == _nSelectSlotID )
                t = _slotBox[i]._rewardBoxState;

        if ( t == ERewardBoxState.Complete )
        {
            //to do : 오픈 완료된 것 보상을 받게 해야 하는데..
        }
        else
        {

        }
        
        StartCoroutine(m_DataMgr.ExchangeBox(_nSelectSlotID, _box.nKey));
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
