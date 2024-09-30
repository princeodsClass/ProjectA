using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupHunt : UIDialog
{
	[SerializeField]
	TextMeshProUGUI _txtTitle, _txtBoostTitle,
					_txtLevelTitle, _txtNPCLevelTitle, _txtNPCLevel,
					_txtBoostBox, _txtButtonStartCaptionN, _txtButtonStartCaptionT, _txtCostTicket,
					_txtTicketCount, _txtHaveTicketCount;

	[SerializeField]
	GameObject _goButtonAdd, _goButtonMinus,
			   _goButtonAddLock, _goButtonMinusLock,
			   _goButtonStart, _goNormal, _goBoost, _goClearStamp;

	[SerializeField]
	Image _imgPossessionBG;

	[SerializeField]
	Toggle _tgBoostBox;

	[SerializeField]
	Color[] _color;

	[SerializeField]
	RectTransform _rtNPCInfo;

	[SerializeField]
	ComHuntReward _comHuntReward;

	HuntTable _hunt;

	int _curLevel;
	int _min, _max;
	int _costTicket;

	public void InitializeInfo(int huntlevel)
	{
		_curLevel = Math.Min(huntlevel, _max);
		_hunt = HuntTable.GetHuntData(_curLevel);

		CorrectLevel();

		InitializeBoost();
		InitializePossession();

		Resize();
	}

	void SetRange()
	{
		_min = m_Account.m_nHuntLevel - GlobalTable.GetData<int>("valueHuntMinRange");
		_min = Math.Max(_min, 0);

		_max = m_Account.m_nHuntLevel + GlobalTable.GetData<int>("valueHuntMaxRange");
		_max = Math.Min(_max, HuntTable.GetList().Count - 1);
	}

	void InitializePossession()
	{
		_imgPossessionBG.color = _color[_hunt.Episode];

		_txtLevelTitle.text = $"{NameTable.GetValue(_hunt.NameKey)} {_hunt.Order}";
		_txtNPCLevel.text = $" {_hunt.StandardLevel}";

		_goClearStamp.SetActive(_curLevel <= m_Account.m_nHuntLevel);
	}

	void InitializeText()
	{
		_txtTitle.text = UIStringTable.GetValue("ui_page_lobby_battle_button_hunt");
		_txtBoostTitle.text = UIStringTable.GetValue("ui_popup_boxreward_button_booster");
		_txtNPCLevelTitle.text = UIStringTable.GetValue("ui_popup_hunt_npclevel_title");

		_txtBoostBox.text = UIStringTable.GetValue("ui_popup_hunt_boost_box_caption");

		_txtButtonStartCaptionN.text =
		_txtButtonStartCaptionT.text =
			UIStringTable.GetValue("ui_start");
	}

	void InitializeReward()
	{
		_comHuntReward.SetAlert(m_InvenBox.GetItemCount() == 4);
	}

	void InitializeBoost()
	{
		_costTicket = 0;
		_tgBoostBox.isOn = false;

		int h = m_InvenMaterial.GetItemCount(_hunt.BoostItemKey);
		_txtTicketCount.text = h.ToString();

		string pre = h < _hunt.BoostItemCount ? "<color=red>" : "";
		string sur = h < _hunt.BoostItemCount ? "</color>" : "";

		_txtHaveTicketCount.text = $"( {pre}{_txtTicketCount.text} / {_hunt.BoostItemCount}{sur} )";
	}

	void CorrectLevel()
	{
		_curLevel = Math.Max(_curLevel, _min);
		_curLevel = Math.Min(_curLevel, _max);

		_goButtonMinus.GetComponent<SoundButton>().interactable = _curLevel > _min;
		_goButtonAdd.GetComponent<SoundButton>().interactable = _curLevel < _max;

		_goButtonMinusLock.SetActive(!_goButtonMinus.GetComponent<SoundButton>().interactable);
		_goButtonAddLock.SetActive(!_goButtonAdd.GetComponent<SoundButton>().interactable);
	}

	public void OnClickBoxBoostToggle()
	{
		if (m_InvenMaterial.GetItemCount(_hunt.BoostItemKey) < _hunt.BoostItemCount &&
			 _tgBoostBox.isOn)
		{
			/*
			PopupSysMessage mp = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			mp.InitializeInfo("ui_error_title", "ui_error_resource_lick", "ui_popup_button_confirm");
			*/

			OpenShopBox(1);
			_tgBoostBox.isOn = false;
		}

		_costTicket = _tgBoostBox.isOn ? _hunt.BoostItemCount : 0;
		_txtCostTicket.text = _costTicket.ToString();

		_comHuntReward.SetBoost(_tgBoostBox.isOn);

		_goNormal.SetActive(!_tgBoostBox.isOn);
		_goBoost.SetActive(_tgBoostBox.isOn);
	}

	void OpenShopBox(int index)
	{
		SlotShopBox[] boxes = FindObjectsOfType<SlotShopBox>();
		boxes[index].OnClick(true);
	}

	public void OnClickAdd()
	{
		_curLevel++;
		CorrectLevel();
		InitializeInfo(_curLevel);
	}

	public void OnClickMinus()
	{
		_curLevel--;
		CorrectLevel();
		InitializeInfo(_curLevel);
	}

	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, _hunt.BoostItemKey, 0));
	}

	void Resize()
	{
		_comHuntReward.Resize();

		LayoutRebuilder.ForceRebuildLayoutImmediate(_rtNPCInfo);
	}

	private void OnEnable()
	{
		Initialize();
		SetRange();
	}

	public override void Initialize()
	{
		base.Initialize();
		InitializeText();
		InitializeReward();
	}

	public override void Open()
	{
		base.Open();

		m_MenuMgr.ShowPopupDimmed(true);
	}

	public override void Close()
	{
		base.Close();

		m_MenuMgr.ShowPopupDimmed(false);
	}

	#region 추가
	/** 시작 버튼을 눌렀을 경우 */
	public void OnClickStartBtn()
	{
		if (m_InvenWeapon.GetItemCount() < m_Account.m_nMaxWeaponRepository)
		{
			// 클리어 된 에피소드가 없을 경우
			if (m_Account.CorrectEpisode < 0)
			{
				return;
			}

			int nEpisode = Mathf.Max(3, m_Account.CorrectEpisode + 1);
			nEpisode = UnityEngine.Random.Range(2, nEpisode);

			uint nEpisodeKey = ComUtil.GetEpisodeKey(nEpisode);
			var oEpisodeTable = EpisodeTable.GetData(nEpisodeKey);

			int nChapter = UnityEngine.Random.Range(1, oEpisodeTable.MaxChapter);

			do
			{
				nChapter = UnityEngine.Random.Range(1, oEpisodeTable.MaxChapter);
			} while (nChapter == 4);

			uint nChapterKey = ComUtil.GetChapterKey(nEpisode, nChapter);
			this.LoadMapInfos(nEpisode, nChapter);
		}
		else
		{
			PopupRepositorFull fu = m_MenuMgr.OpenPopup<PopupRepositorFull>(EUIPopup.PopupRepositorFull);
		}
	}

	/** 맵 정보를 로드한다 */
	private void LoadMapInfos(int a_nEpisode, int a_nChapter)
	{
		m_DataMgr.SetPlayHuntLV(_curLevel);

		if (_tgBoostBox.isOn) StartCoroutine(m_GameMgr.AddItemCS(_hunt.BoostItemKey, -_hunt.BoostItemCount));

		var oHandler = new PopupLoadingHandlerBattleReady();
		oHandler.Init(PopupLoadingHandlerBattleReady.MakeParams(EMapInfoType.CAMPAIGN, new STIDInfo(0, a_nChapter - 1, a_nEpisode - 1)));

		var oLoadingPopup = m_MenuMgr.OpenPopup<PopupLoading>(EUIPopup.PopupLoading);
		oLoadingPopup.Init(PopupLoading.MakeParams(PopupLoading.ELoadingType.BATTLE_READY, oHandler, (a_oSender) => this.OnCompleteLoadingBattleReady(a_oSender, a_nEpisode, a_nChapter)));
	}

	/** 전투 준비 로딩이 완료 되었을 경우 */
	private void OnCompleteLoadingBattleReady(PopupLoading a_oSender, int a_nEpisode, int a_nChapter)
	{
		var oMapInfoDict = CMapInfoTable.Singleton.LoadMapInfosDownloaded(EMapInfoType.CAMPAIGN, a_nChapter - 1, a_nEpisode - 1);

		m_DataMgr.SetPlayStageID(0);
		m_DataMgr.SetContinueTimes(0);
		m_DataMgr.SetIsHuntBoost(_tgBoostBox.isOn);
		m_DataMgr.SetupPlayMapInfo(EMapInfoType.CAMPAIGN, EPlayMode.HUNT, oMapInfoDict);

		m_DataMgr.ExLateCallFunc((a_oFuncSender) =>
		{
			m_MenuMgr.SceneEnd();
			m_MenuMgr.SceneNext(ESceneType.Battle);
		}, ComType.G_DURATION_LOADING_PROGRESS_ANI);
	}
	#endregion // 추가
}
