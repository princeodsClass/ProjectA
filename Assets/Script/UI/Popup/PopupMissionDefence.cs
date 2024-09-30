using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class PopupMissionDefence : UIDialog
{
    [SerializeField]
    Image _imgPossessionBG, _imgStartButtonBG;

    [SerializeField]
    Color[] _colorPossessionBG, _coiorPossessionFont;

    [SerializeField]
    Transform _tRootRewards;

    [SerializeField]
    TextMeshProUGUI _txtGroupTitle, _txtStandardLevel, _txtReserve,
                    _txtRewardsTitle, _txtRewardsDesc,
                    _txtButtonStartCaption, _txtButtonCloseCaption;

    [SerializeField]
    RectTransform[] _rt4Resize;

    [SerializeField]
    GameObject _goButtonAdd, _goButtonMinus,
               _goButtonAddLock, _goButtonMinusLock;

    uint _nTicketKey;
    int _nReserve, _maxDifficulty, _minDifficulty;
    int _curDifficulty = 0;

    List<MissionDefenceGroupTable> _liGroup;
    Dictionary<MissionDefenceGroupTable, List<MissionDefenceTable>> _dicWave = new Dictionary<MissionDefenceGroupTable, List<MissionDefenceTable>>();

    Coroutine _coRewards;
    WaitForSecondsRealtime _interval;

    private void Awake()
    {
        Initialize();

        _nTicketKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);
        _interval = new WaitForSecondsRealtime(GlobalTable.GetData<int>("timeDefenceRewardsAppear") /1000f);

        _curDifficulty = m_Account.m_nCurDefenceDifficulty;

        InitializeWave();
        InitializeText();
    }

    void InitializeText()
    {
        _txtButtonCloseCaption.text = UIStringTable.GetValue("ui_common_close");
        _txtButtonStartCaption.text = UIStringTable.GetValue("ui_start");
    }

    void InitializeWave()
    {
        _liGroup = MissionDefenceGroupTable.GetList();

        _liGroup.ForEach(group =>
        {
            List<MissionDefenceTable> liWave = MissionDefenceTable.GetDifficulty(group.Difficulty);
            _dicWave.Add(group, liWave);
        });

        _minDifficulty = 0;
        _maxDifficulty = _liGroup.Count - 1;
    }

    void SetReserve()
    {
        _nReserve = Math.Max(0, m_InvenMaterial.GetItemCount(_nTicketKey));
        _txtReserve.text = ComUtil.ChangeNumberFormat(_nReserve);
    }

    public void OnClickStart()
    {
        if (_curDifficulty > m_Account.m_nCurDefenceDifficulty)
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
            pop.InitializeInfo("ui_error_title", "ui_error_defence_first_lower", "ui_common_close");

            return;
        }

        if ( _nReserve < _liGroup[_curDifficulty].ItemCount )
        {
            PopupDefault pop = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
            pop.SetMessage("ui_error_lackofticket_defence");
            pop.SetButtonText("ui_common_open_shopbox", "ui_common_close", () =>
            {
                SlotShopBox[] boxes = GameObject.FindObjectsOfType<SlotShopBox>();
                boxes[1].GetComponent<SlotShopBox>().OnClick(true);
            });

            return;
        }

        GameManager.Singleton.StartCoroutine(m_DataMgr.StartDefence(_curDifficulty,
                                                                    m_InvenMaterial.GetItemID(_nTicketKey),
                                                                    _liGroup[_curDifficulty].ItemCount,
                                                                    ConsumeTicket));
    }

    void ConsumeTicket()
    {
        GameManager.Singleton.invenMaterial.ModifyItem(m_InvenMaterial.GetItemID(_nTicketKey),
                                                        InventoryData<ItemMaterial>.EItemModifyType.Volume,
                                                        m_InvenMaterial.GetItemCount(_nTicketKey) - _liGroup[_curDifficulty].ItemCount);
        StartDefence();
    }

    public void OnClickAdd()
    {
        _curDifficulty++;
        InitializeInfo();
    }

    public void OnClickMinus()
    {
        _curDifficulty--;
        InitializeInfo();
    }

    public void OnClickTicket()
    {
        PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
        pop.InitializeInfo(new ItemMaterial(0, _nTicketKey, 0));
    }

    public void InitializeInfo()
    {
        CorrectDifficulty();
        SetReserve();
        InitializePossession();
        Resize();

		_txtReserve.color = (m_InvenMaterial.GetItemCount(_nTicketKey) < _liGroup[_curDifficulty].ItemCount) ? Color.red : Color.white;
    }

    void InitializePossession()
    {
        _imgPossessionBG.color = _colorPossessionBG[_curDifficulty > m_Account.m_nCurDefenceDifficulty ? 0 : _curDifficulty + 1];

        _txtGroupTitle.text = NameTable.GetValue(_liGroup[_curDifficulty].NameKey);
        _txtStandardLevel.text = $"{UIStringTable.GetValue("ui_popup_hunt_npclevel_title")} : {_dicWave[_liGroup[_curDifficulty]][0].StandardNPCLevel} ~ {_dicWave[_liGroup[_curDifficulty]].Last().StandardNPCLevel}";

        _imgStartButtonBG.color = _coiorPossessionFont[_curDifficulty > m_Account.m_nCurDefenceDifficulty ? 0 : 1];
        _txtGroupTitle.color = _coiorPossessionFont[_curDifficulty > m_Account.m_nCurDefenceDifficulty ? 0 : 1];
        _txtStandardLevel.color = _coiorPossessionFont[_curDifficulty > m_Account.m_nCurDefenceDifficulty ? 0 : 1];
        _txtButtonStartCaption.color = _coiorPossessionFont[_curDifficulty > m_Account.m_nCurDefenceDifficulty ? 0 : 1];

        _txtRewardsTitle.text = $"{UIStringTable.GetValue("ui_component_hunt_reward_title")} : {_txtGroupTitle.text}";
        _txtRewardsDesc.text = UIStringTable.GetValue("ui_popup_rewards_desc");

        StopAllCoroutines();
        _coRewards = StartCoroutine(InitializeRewards());
    }

    IEnumerator InitializeRewards()
    {
        ComUtil.DestroyChildren(_tRootRewards);

        for ( int i = 0; i < _dicWave[_liGroup[_curDifficulty]].Count; i++ )
        {
            SlotDefenceRewardsGroup rGroup = m_MenuMgr.LoadComponent<SlotDefenceRewardsGroup>(_tRootRewards, EUIComponent.SlotDefenceRewardsGroup);
            rGroup.InitializeInfo(_dicWave[_liGroup[_curDifficulty]][i]);

            yield return _interval;
        }

    }

    void CorrectDifficulty()
    {
        _curDifficulty = Math.Max(_curDifficulty, _minDifficulty);
        _curDifficulty = Math.Min(_curDifficulty, _maxDifficulty);

        _goButtonMinus.GetComponent<SoundButton>().interactable = _curDifficulty > _minDifficulty;
        _goButtonAdd.GetComponent<SoundButton>().interactable = _curDifficulty < _maxDifficulty;

        _goButtonMinusLock.SetActive(!_goButtonMinus.GetComponent<SoundButton>().interactable);
        _goButtonAddLock.SetActive(!_goButtonAdd.GetComponent<SoundButton>().interactable);
    }

    IEnumerator Resize()
    {
        for ( int i = 0; i < _rt4Resize.Length; i++ )
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rt4Resize[i]);
            yield return null;
        }
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

	/** 방어전을 시작한다 */
	void StartDefence()
    {
        StopAllCoroutines();
        var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.DEFENCE, new STIDInfo(0, _curDifficulty, 0)));

		var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading, true);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReadyDefence(a_oSender, _curDifficulty)));
	}

	/** 방어전 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReadyDefence(PopupLoading a_oSender, int a_nDifficulty)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.DEFENCE,
			a_nDifficulty, 0);

		m_DataMgr.SetPlayStageID(0);
		m_DataMgr.SetContinueTimes(0);
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.DEFENCE, EPlayMode.DEFENCE, oMapInfoDict, true);

		this.ExLateCallFunc((a_oFuncSender) =>
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}
}
