using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAttendance: MonoBehaviour
{
    [SerializeField]
    Color[] _colFrame, _colGlow;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtVolume, _txtReady, _txtComplete;

    [SerializeField]
    Image _imgIcon, _imgFXIcon, _imgFrame;

    [SerializeField]
    GameObject _goScrap, _goReady, _goReceivedMarker;

    [SerializeField]
    GameObject[] _goEdgeFX;

    AttendanceTable _attendance;
    Animator _animator;

    public void InitializeInfo(AttendanceTable attendance)
    {
        _animator = GetComponent<Animator>();
        _attendance = attendance;

        _txtTitle.text = string.Format($"{UIStringTable.GetValue("ui_popup_attendance_day")}", _attendance.Order);
        _txtReady.text = UIStringTable.GetValue("ui_popup_battlereward_button_missclaim");
        _txtComplete.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");
        _txtVolume.text = _attendance.RewardCount.ToString();
        _imgIcon.sprite = _imgFXIcon.sprite = ComUtil.GetIcon(_attendance.RewardKey);
        _imgFrame.sprite = AttendanceTable.GetAttendanceSlotFrameSprite(_attendance.RewardKey);

        _goReady.SetActive(GameManager.Singleton.user.m_nAttendanceContinuouslyCount + 1 == _attendance.Order &&
                           GameManager.Singleton.user.m_bIsAbleToAttendanceReward);
        _goReceivedMarker.SetActive(GameManager.Singleton.user.m_nAttendanceContinuouslyCount >= _attendance.Order &&
                                    !_goReady.activeSelf);

        for (int i = 0; i < _goEdgeFX.Length; i++)
            if (null != _goEdgeFX[i])
                _goEdgeFX[i].SetActive(i == ComUtil.GetItemGrade(_attendance.RewardKey));

        SetScrapIcon();
    }

    public void OnClick()
    {
        if ( _goReceivedMarker.activeSelf )
        {
            PopupSysMessage already = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            already.InitializeInfo("ui_error_title", "ui_error_abyss_rewards_already_get", "ui_common_close");
        }
        else if ( _goReady.activeSelf )
        {
            GetRewards();
        }
        else
        {
            PopupRewards();
        }
    }
   
    void GetRewards()
    {
        switch ( ComUtil.GetItemType(_attendance.RewardKey) )
        {
            case EItemType.Box:
                StartCoroutine(GameDataManager.Singleton.GetAttendanceRewards(_attendance));
                PopupBoxReward box = MenuManager.Singleton.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward, true);
                box.InitializeInfo(new ItemBox(0, _attendance.RewardKey, 0), false, false,
                                   1, PopupBoxReward.EBoxType.normal, null, GetItemCallBack);
                break;
            default:
                StartCoroutine(GetItem());
                break;
        }
    }

    IEnumerator GetItem()
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
        yield return StartCoroutine(GameDataManager.Singleton.GetAttendanceRewards(_attendance));
        wait.Close();

        ControlPopup();
    }

    void GetItemCallBack(PopupBoxReward sender)
    {
        ControlPopup();
    }

    void ControlPopup()
    {
        GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().SetAttendanceButtonTag();
        InitializeInfo(_attendance);
        _animator.SetTrigger("Obtain");
    }

    void PopupRewards()
    {
        switch ( ComUtil.GetItemType(_attendance.RewardKey) )
        {
            case EItemType.Weapon:
                PopupWeapon weaponpopup = MenuManager.Singleton.OpenPopup<PopupWeapon>(EUIPopup.PopupWeapon, true);
                weaponpopup.InitializeInfo(new ItemWeapon(0, _attendance.RewardKey, 1, 0, 0, 0, false), false, true);
                break;
            case EItemType.Gear:
                PopupGear gearpopup = MenuManager.Singleton.OpenPopup<PopupGear>(EUIPopup.PopupGear, true);
                gearpopup.InitializeInfo(new ItemGear(0, _attendance.RewardKey, 1, 0, 0, 0, false), false, true);
                break;
            case EItemType.Material:
                PopupMaterial matpopup = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
                matpopup.InitializeInfo(new ItemMaterial(0, _attendance.RewardKey, _attendance.RewardCount));
                break;
            case EItemType.Box:
                PopupBoxNormal boxpopup = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
                boxpopup.InitializeInfo(new ItemBox(0, _attendance.RewardKey, 0), false, false);
                break;
            default:
                break;
        }
    }

    void SetScrapIcon()
    {
        if (ComUtil.GetItemType(_attendance.RewardKey) == EItemType.Material)
        {
            string sd = _attendance.RewardKey.ToString("X").Substring(3, 1);

            EItemType type = (EItemType)Convert.ToInt32(sd, 16);
            _goScrap.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
        }
        else
            _goScrap.SetActive(false);
    }
}
