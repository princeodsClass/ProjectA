using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PopupAttendance : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtTimer, _txtMissionKey, _txtGameMoney, _txtCrystal;

    [SerializeField]
    Transform _tRootSlot;

    SlotAttendance[] _slotattendance;
    List<AttendanceTable> _attendance;

    string h, m, s = string.Empty;

    public void InitializeInfo(List<AttendanceTable> attendance)
    {
        _attendance = attendance;
        _slotattendance = new SlotAttendance[8];
        ComUtil.DestroyChildren(_tRootSlot);

        for ( int i = 0; i < attendance.Count; i++ )
        {
            _slotattendance[i] = m_MenuMgr.LoadComponent<SlotAttendance>(_tRootSlot, EUIComponent.SlotAttendance);
            _slotattendance[i].InitializeInfo(_attendance[i]);
        }

        InitializeText();
        StartCoroutine(SetTimer());
        SetTop();
    }

    public void SetTop()
    {
        _txtMissionKey.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcMissionTicket());
        _txtGameMoney.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalMoney());
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue(_attendance[0].Title);
        _txtDesc.text = UIStringTable.GetValue("ui_popup_attendance_desc");

        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    IEnumerator SetTimer()
    {
        TimeSpan remainTime;
        DateTime now, midnight;

        while (true)
        {
            now = DateTime.UtcNow;
            midnight = now.Date.AddDays(1);

            remainTime = midnight - now;

            if (remainTime.Hours > 0)
                _txtTimer.text = $"{remainTime.Hours}{h} {remainTime.Minutes}{m} {remainTime.Seconds}{s}";
            else if (remainTime.Minutes > 0)
                _txtTimer.text = $"{remainTime.Minutes}{m} {remainTime.Seconds}{s}";
            else
                _txtTimer.text = $"{remainTime.Seconds}{s}";

            yield return YieldInstructionCache.WaitForSecondsRealtime(1f);
        }
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
    }

    public override void Close()
    {
        PageInitialize();
        base.Close();
    }

    void PageInitialize()
    {
        PageLobbyInventory inven = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();

        inven.InitializeWeapon();
        inven.InitializeGear();
        inven.InitializeMaterial();
        inven.InitializeCharacter();
    }

    public override void Escape()
    {
        base.Escape();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        FindObjectOfType<ButtonAttendance>().Initialize();
    }
}
