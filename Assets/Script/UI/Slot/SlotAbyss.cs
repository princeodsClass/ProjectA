using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotAbyss : MonoBehaviour
{
    [SerializeField]
    GameObject _goRecord, _goLock, _goSelect;

    [SerializeField]
    TextMeshProUGUI _txtFloorTitle, _txtEnemyLevel,
                    _txtRecordTitle, _txtRecord;

    [SerializeField]
    Animator _aRecord;

    PopupAbyss _popup;
    SlotAbyssGroup _group = null;
    AbyssTable _ctbl = null;
    bool _state = false;
    int _nFloor = 0;

    public void InitializeInfo(int floor, SlotAbyssGroup group, PopupAbyss pop)
    {
        _popup = pop;
        _group = group;
        _nFloor = floor;
        _ctbl = AbyssTable.GetData((uint)(0x37800000 + GameManager.Singleton.user.m_nAbyssGroup * 256 + _nFloor));

        _txtFloorTitle.text = string.Format($"{UIStringTable.GetValue("ui_slot_abyss_floortitle")}", _nFloor);
        _txtEnemyLevel.text = $"{UIStringTable.GetValue("ui_slot_abyss_enemytitle")} : {_ctbl.StandardNPCLevel}";

        _goRecord.SetActive(false);
        _goLock.SetActive(false);
        _popup.SetFloor(_nFloor, this);

        SetSelect(false);
        SetRecord();
    }

    public int GetAbyssFloor()
    {
        return _nFloor;
    }

    public void SetLock(bool state)
    {
        _goLock.SetActive(state);
    }

    public void OnClick()
    {
        if (_goLock.activeSelf)
        {
            PopupSysMessage p = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            p.InitializeInfo("ui_error_title", "ui_error_lack_abyssfloor", "ui_popup_button_confirm");

            return;
        }

        if ( _state )
        {
			var oHandler = new PopupLoadingHandlerBattleReady();
			oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.ABYSS, new STIDInfo(0, _nFloor - 1, _ctbl.Group)));

			var oLoadingPopup = MenuManager.Singleton.OpenPopup<PopupLoading>(EUIPopup.PopupLoading, true);
			oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, this.OnCompleteLoadingBattleReady));
        }
        else
        {
            SetSelect(true);
            _popup.SelectFloor(_nFloor);
        }

    }

    public void SetRecord()
    {
        int ms = GameManager.Singleton.user.m_nAbyssBestLap[_nFloor - 1];

        if ( ms == 0 )
        {
            _goRecord.SetActive(false);
        }
        else
        {
            TimeSpan time = TimeSpan.FromMilliseconds(ms);

            _txtRecordTitle.text = UIStringTable.GetValue("ui_slot_abyss_recordtitle");
            _txtRecord.text = $"{time.Minutes}:{time.Seconds:D2}.{time.Milliseconds:D3}";
            _goRecord.SetActive(true);
        }
    }

    public void SetSelect(bool state)
    {
        _state = state;
        _goSelect.SetActive(state);
    }

    public void SetNewRecord()
    {
        _aRecord.SetTrigger("New");
    }

    private void OnEnable()
    {

    }

	/** 전투 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReady(PopupLoading a_oSender)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.ABYSS, _nFloor - 1, _ctbl.Group);

		GameDataManager.Singleton.SetPlayStageID(0);
		GameDataManager.Singleton.SetContinueTimes(0);
		GameDataManager.Singleton.SetupPlayMapInfo(EMapInfoType.ABYSS, EPlayMode.ABYSS, oMapInfoDict);

		this.ExLateCallFunc((a_oFuncSender) =>
		{
			MenuManager.Singleton.SceneEnd();
			MenuManager.Singleton.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}
}