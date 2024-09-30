using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBattleReward : UIDialog
{
	[SerializeField] private TMP_Text m_oGuideBtnText = null;
	[SerializeField] private TMP_Text m_oFailGuideText = null;

	[SerializeField] private GameObject m_oFailUIs = null;

	[SerializeField]
	TextMeshProUGUI _txtTitle, _txtResult, _txtRewardExp, _txtStageRewardExp,
					_txtResourceTitle, _txtWeaponTitle, _txtMissTitle,
					_txtGameMoney, _txtCrystal, _txtTicket,
					_txtBoxName, _txtBoxLevel;

	[SerializeField]
	GameObject _goTitle, _goRewardExp,
			   _goRewardResource, _goRewardWeapon, _goRewardMiss, _goBox,
			   _goBoxSlotFull, _goBoxSlotEmpty;

	[SerializeField]
	Transform _tResouoceRoot, _tWeaponRoot, _tMissRoot, _tBoxRoot;

	[SerializeField]
	GameObject _goWinBG, _goLooseBG, _goWinStageBG,
			   _goButtonMissSkip, _goButtonMissClaim,
			   _goButtonBoxReplace, _goButtonBoxSkip, _goButtonBoxOpenCrystal, _goButtonBoxOpenAd,
			   _goButtonBoxClose, _goButtonClose;

	[SerializeField]
	GameObject[] _goMarkerAD, _goMarkerVIP, _goMarkerFree, _goMarkerTicket;

	[SerializeField]
	ScrollRect _scrollRectWeapon, _scrollRectWeaponResource, _scrollRectWeaponMiss;

	List<uint> _Weapon;
	Dictionary<uint, int> _Resource;
	Dictionary<uint, int> _Missing;

	List<uint> _Acquire_Weapon = new List<uint>();
	Dictionary<uint, int> _Acquire_Resource = new Dictionary<uint, int>();

	ItemBox _box;

	uint _BoxKey;
	int _boxSlotNumber;
	bool m_bIsWin = false;
	public TMP_Text txtTitle => _txtTitle;

	private bool m_bIsCompleteCampaign = false;

	/** 가이드 버튼을 눌렀을 경우 */
	public void OnTouchGuideBtn()
	{
		var oPopupBattleUpgradeGuide = m_MenuMgr.OpenPopup<PopupBattleUpgradeGuide>(EUIPopup.PopupBattleUpgradeGuide, true);
		oPopupBattleUpgradeGuide.Init(PopupBattleUpgradeGuide.MakeParams(this.OnReceiveUpgradeGuidePopupCallback));
		oPopupBattleUpgradeGuide.Open();
	}

	/** 업그레이드 가이드 강화 콜백을 수신했을 경우 */
	private void OnReceiveUpgradeGuidePopupCallback(PopupBattleUpgradeGuide a_oSender)
	{
		this.OnClickCloseBtn();
	}

	private void OnEnable()
	{
		_Resource = new Dictionary<uint, int>();
		_Weapon = new List<uint>();
		_Missing = new Dictionary<uint, int>();

		Initialize();

		InitializeCurrency();
		RootReset();
	}

	void InitializeText()
	{
		_txtResourceTitle.text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_material");
		_txtWeaponTitle.text = UIStringTable.GetValue("ui_page_lobby_inventory_tab_weapon");
		_txtMissTitle.text = UIStringTable.GetValue("ui_popup_battlereward_missing_title");

		_goButtonMissSkip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_missskip");
		_goButtonMissClaim.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_missclaim");
		_goButtonBoxReplace.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_boxreplace");
		_goButtonBoxSkip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_boxskip");
		_goButtonBoxOpenCrystal.GetComponentInChildren<TextMeshProUGUI>().text = $"{UIStringTable.GetValue("ui_popup_battlereward_button_boxclaim_crystal")} \n {_box?.nOpenMaterialCount}";
		_goButtonBoxOpenAd.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_boxclaim_ad");

		_goButtonBoxClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_battlereward_button_boxconfiem");
		_goButtonClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_common_close");

		StartCoroutine(Resize());
	}

	IEnumerator Resize()
	{
		yield return null;

		LayoutRebuilder.ForceRebuildLayoutImmediate(_tResouoceRoot.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tWeaponRoot.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_tMissRoot.GetComponent<RectTransform>());
	}

	public void InitializeRewards(STIDInfo a_stIDInfo,
								  bool isWin,
								  List<uint> weapon = null,
								  Dictionary<uint, int> resource = null,
								  Dictionary<uint, int> missing = null,
								  uint box = 0,
								  bool a_bIsCompleteCampaign = false,
								  List<uint> acquire_weapon = null,
								  Dictionary<uint, int> acquire_resource = null)
	{
		m_bIsWin = isWin;
		m_bIsCompleteCampaign = a_bIsCompleteCampaign;

		SetTitle(a_stIDInfo, isWin);
		SetMarker();

		m_oFailUIs.SetActive(false);

		// 텍스트를 갱신한다
		m_oGuideBtnText.text = UIStringTable.GetValue("ui_hint_battle_desc_02");
		m_oFailGuideText.text = UIStringTable.GetValue("ui_hint_battle_desc_01");

		// 클리어에 실패했을 경우
		if (!isWin)
		{
			int nIsCompleteWeaponUpgradeGuideTutorial = PlayerPrefs.GetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.WEAPON_UPGRADE_GUIDE]);

			// NPC 조준 튜토리얼 진행이 가능 할 경우
			if (nIsCompleteWeaponUpgradeGuideTutorial <= 0 && GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueInventoryOpenLevel"))
			{
				this.ExLateCallFuncRealtime((a_oSender) => this.OnTouchGuideBtn(), 1.0f);
				PlayerPrefs.SetInt(ComType.HelpTutorialKindsKeyDict[EHelpTutorialKinds.WEAPON_UPGRADE_GUIDE], 1);
			}
		}

		_Weapon.AddRange(weapon);
		_Resource = ComUtil.MergeDictionaries(_Resource, resource); // 전투 신에서 딕셔너리 날라가니까

		_Acquire_Weapon.AddRange(acquire_weapon);
		_Acquire_Resource = ComUtil.MergeDictionaries(_Acquire_Resource, acquire_resource); // 전투 신에서 딕셔너리 날라가니까

		GameManager.Singleton.StartCoroutine(GetRewards());

		_Missing = missing.ToDictionary(e => e.Key, e => e.Value);  // 전투 신에서 딕셔너리 날라가니까

		if (BoxTable.IsContainsKey(box) && (m_bIsCompleteCampaign || (GameDataManager.Singleton.PlayMode != EPlayMode.CAMPAIGN && GameDataManager.Singleton.PlayMode != EPlayMode.TUTORIAL)))
		{
			_box = new ItemBox(0, box, 1, DateTime.MinValue);
			_BoxKey = box;
		}

		StartCoroutine(CoResource());
		InitializeText();

		m_GameMgr.ApplyQuestCounter();
	}

	IEnumerator GetRewards()
	{
		yield return new WaitForEndOfFrame();

		if (_Acquire_Weapon != null)
			foreach (uint key in _Acquire_Weapon)
				yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(key, 1));

		if (_Acquire_Resource != null)
			foreach (KeyValuePair<uint, int> r in _Acquire_Resource)
				yield return GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(r.Key, r.Value));
	}

	void SetMarker()
	{
		if (m_Account.IsVIP())
		{
			Array.ForEach(_goMarkerVIP, e => e.SetActive(true));
			Array.ForEach(_goMarkerAD, e => e.SetActive(false));
			Array.ForEach(_goMarkerFree, e => e.SetActive(false));
			Array.ForEach(_goMarkerTicket, e => e.SetActive(false));
		}
		else
		{
			Array.ForEach(_goMarkerVIP, e => e.SetActive(false));
			Array.ForEach(_goMarkerFree, e => e.SetActive(false));

			if (int.Parse(_txtTicket.text) > 0)
			{
				Array.ForEach(_goMarkerAD, e => e.SetActive(false));
				Array.ForEach(_goMarkerTicket, e => e.SetActive(true));
			}
			else
			{
				Array.ForEach(_goMarkerAD, e => e.SetActive(true));
				Array.ForEach(_goMarkerTicket, e => e.SetActive(false));
			}
		}
	}

	void SetTitle(STIDInfo a_stIDInfo, bool isWin)
	{
		bool bIsCampaign = GameDataManager.Singleton.PlayMode == EPlayMode.CAMPAIGN;
		bIsCampaign = bIsCampaign || GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL;

		uint chapterKey = ComUtil.GetChapterKey(a_stIDInfo.m_nEpisodeID + 1, a_stIDInfo.m_nChapterID + 1);
		ChapterTable chapter = ChapterTable.GetData(chapterKey);

		// 사냥 모드 일 경우
		if (GameDataManager.Singleton.PlayMode == EPlayMode.HUNT)
		{
			var oHuntTable = HuntTable.GetHuntData(GameDataManager.Singleton.PlayHuntLV);
			_txtTitle.text = $"{NameTable.GetValue(oHuntTable.NameKey)} {oHuntTable.Order}";
		}
		// 보너스 모드 일 경우
		else if (GameDataManager.Singleton.PlayMode == EPlayMode.BONUS)
		{
			_txtTitle.text = UIStringTable.GetValue("ui_bonus_chapter_title");
		}
		// 캠페인 모드 일 경우
		else if (GameDataManager.Singleton.PlayMode == EPlayMode.CAMPAIGN || GameDataManager.Singleton.PlayMode == EPlayMode.TUTORIAL)
		{
			int nClearStage = GameDataManager.Singleton.PlayStageID;
			nClearStage += m_bIsCompleteCampaign ? 1 : 0;

			_txtTitle.text = $"Stage {Mathf.Max(1, nClearStage)}";
		}
		else
		{
			_txtTitle.text = NameTable.GetValue(chapter.NameKey);
		}

		if (isWin)
		{
			_goWinBG.SetActive(!bIsCampaign || m_bIsCompleteCampaign);
			_goLooseBG.SetActive(false);

			if (_goWinStageBG != null)
			{
				_goWinStageBG.SetActive(bIsCampaign && !m_bIsCompleteCampaign);
			}

			_txtResult.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");

			if (GameDataManager.Singleton.AcquireStageEXP > 0)
			{
				_goRewardExp.SetActive(true);
				_txtRewardExp.text = $"+{GameDataManager.Singleton.AcquireStageEXP}";

				if (_txtStageRewardExp != null)
				{
					_txtStageRewardExp.text = $"+{GameDataManager.Singleton.AcquireStageEXP}";
				}
			}
		}
		else
		{
			_goWinBG.SetActive(false);
			_goLooseBG.SetActive(true);

			if (_goWinStageBG != null)
			{
				_goWinStageBG.SetActive(false);
			}

			_txtResult.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_fail");
		}
	}

	IEnumerator CoResource()
	{
		foreach (KeyValuePair<uint, int> r in _Resource)
		{
			SlotMaterial resource = m_MenuMgr.LoadComponent<SlotMaterial>(_tResouoceRoot, EUIComponent.SlotMaterial);
			resource.Initialize(r.Key, r.Value, true);

			yield return new WaitForSecondsRealtime(0.2f);
		}

		StartCoroutine(CoWeapon());
	}

	IEnumerator CoWeapon()
	{
		foreach (uint key in _Weapon)
		{
			SlotWeapon weapon = m_MenuMgr.LoadComponent<SlotWeapon>(_tWeaponRoot, EUIComponent.SlotWeapon);
			ItemWeapon item = new ItemWeapon(0, key, 1, 0, 0, 0, false);
			weapon.Initialize(item, true, SlotWeapon.EPresentType.origin);
			weapon.SetState(SlotWeapon.SlotState.reward);
			weapon.SetBaseScrollRect(_scrollRectWeapon);

			yield return new WaitForSecondsRealtime(0.2f);
		}

		if (_Missing.Count > 0)
		{
			_goRewardMiss.SetActive(true);
			_goButtonClose.SetActive(false);
			m_oFailUIs.SetActive(false);
			StartCoroutine(CoMissing());
		}
		else
		{
			_goRewardMiss.SetActive(false);
			SetBox();
		}
	}

	IEnumerator CoMissing()
	{
		_goRewardMiss.SetActive(true);

		foreach (KeyValuePair<uint, int> m in _Missing)
		{
			switch (m.Key.ToString("X").Substring(0, 2))
			{
				case "20":
					SlotWeapon weapon = m_MenuMgr.LoadComponent<SlotWeapon>(_tMissRoot, EUIComponent.SlotWeapon);
					ItemWeapon item = new ItemWeapon(0, m.Key, 1, 0, 0, 0, false);
					weapon.Initialize(item, true, SlotWeapon.EPresentType.origin);
					weapon.SetState(SlotWeapon.SlotState.reward);
					weapon.SetBaseScrollRect(_scrollRectWeaponMiss);
					break;
				case "22":
					SlotMaterial resource = m_MenuMgr.LoadComponent<SlotMaterial>(_tMissRoot, EUIComponent.SlotMaterial);
					resource.Initialize(m.Key, m.Value, true);
					break;
			}

			yield return new WaitForSecondsRealtime(0.2f);
		}

		_goButtonMissSkip.SetActive(true);
	}

	public void GetMissRewards()
	{
#if UNITY_EDITOR
		StartCoroutine(GetMiss());
#else
		C3rdPartySDKManager.Singleton.ShowRewardAds(ResultADSMiss);
#endif
	}

	void ResultADSMiss(CAdsManager cAdsManager, CAdsManager.STAdsRewardInfo sTAdsRewardInfo, bool result)
	{
		if (result)
		{
			InitializeCurrency();
			StartCoroutine(GetMiss());
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.MissingItemCollection));
		}
		else
		{
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.MissingItemCollection, false));
		}
	}

	IEnumerator GetMiss()
	{
		var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		foreach (KeyValuePair<uint, int> miss in _Missing)
			yield return StartCoroutine(m_GameMgr.AddItemCS(miss.Key, miss.Value));

		InitializeCurrency();
		SetMarker();

		wait.Close();
		OnClickMissingSkip();
	}

	void SetBox()
	{
		if (null == _box)
		{
			_goButtonClose.SetActive(true);
			m_oFailUIs.SetActive(!m_bIsWin);
			return;
		}

		_goBox.SetActive(true);

		GameObject _Box = m_ResourceMgr.CreateObject(EResourceType.Box, _box.strPrefeb, _tBoxRoot);

		if (_Box.GetComponent<BoxRewards>() == null)
			_Box.AddComponent<BoxRewards>();

		_txtBoxName.text = _box.strName;
		_txtBoxLevel.text = $"{UIStringTable.GetValue("ui_level")} {_box.nLevel}";

		_goButtonBoxOpenAd.SetActive(_box.nGrade == 1);

		if (m_InvenBox.GetItemCount() == 4)
		{
			_goBoxSlotFull.SetActive(true);
			_goBoxSlotEmpty.SetActive(false);
		}
		else if (m_InvenBox.GetItemCount() < 4)
		{
			_goBoxSlotFull.SetActive(false);
			_goBoxSlotEmpty.SetActive(true);

			for (int i = 0; i < 4; i++)
			{
				_boxSlotNumber = CheckSlot(i);
				if (_boxSlotNumber > -1) break;
			}

			StartCoroutine(m_DataMgr.AddBox(_boxSlotNumber, _BoxKey));
		}
	}

	public void OnClickTicket()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey"), 0));
	}

	public void OnClickGamemoney()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, ComType.KEY_ITEM_GOLD, 0));
	}

	public void OnClickCrystal()
	{
		PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
		pop.InitializeInfo(new ItemMaterial(0, ComType.KEY_ITEM_CRYSTAL_FREE, 0));
	}

	public void OpenBoxAD()
	{
#if UNITY_EDITOR
		OpenBoxInstance();
#else
		C3rdPartySDKManager.Singleton.ShowRewardAds(ResultADSBox);
#endif
	}

	void ResultADSBox(CAdsManager cAdsManager, CAdsManager.STAdsRewardInfo sTAdsRewardInfo, bool result)
	{
		if (result)
		{
			InitializeCurrency();
			OpenBoxInstance();
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.BoxOpen));
		}
		else
		{
			GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.BoxOpen, false));
		}
	}

	public void OpenBoxCrystal()
	{
		StartCoroutine(OpenImmediately());
	}

	IEnumerator OpenImmediately()
	{
		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		_goButtonBoxOpenCrystal.GetComponent<SoundButton>().interactable = false;
		ComUtil.DestroyChildren(_tBoxRoot);

		if (m_GameMgr.invenMaterial.CalcTotalCrystal() >= _box.nOpenMaterialCount)
		{
			int t = m_GameMgr.invenMaterial.GetItemCount(ComType.KEY_ITEM_CRYSTAL_FREE) - _box.nOpenMaterialCount;

			if (t < 0)
			{
				yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_PAY, t));

				if (_box.nOpenMaterialCount != -t)
					yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -(_box.nOpenMaterialCount + t)));
			}
			else
			{
				yield return StartCoroutine(m_GameMgr.AddItemCS(ComType.KEY_ITEM_CRYSTAL_FREE, -_box.nOpenMaterialCount));
			}

			OpenBoxInstance();
		}
		else
		{
			PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

			_goButtonBoxOpenCrystal.GetComponent<SoundButton>().interactable = true;
			wait.Close();
			yield break;
		}

		wait.Close();
	}

	void OpenBoxInstance()
	{
		ComUtil.DestroyChildren(_tBoxRoot);

		PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
		popupReward.InitializeInfo(_box, false, true);
	}

	int CheckSlot(int i)
	{
		foreach (ItemBox b in m_InvenBox)
			if (b.nSlotNumber == i) return -1;

		return i;
	}

	public override void Initialize()
	{
		_goRewardMiss.SetActive(false);
		_goBox.SetActive(false);
		_goRewardExp.SetActive(false);

		base.Initialize();
	}

	public void OnClickBoxReplace()
	{
		PopupBoxExchange replace = m_MenuMgr.OpenPopup<PopupBoxExchange>(EUIPopup.PopupBoxExchange, true);
		ItemBox box = new ItemBox(0, _BoxKey, 1, DateTime.MinValue);
		replace.InitializeInfo(box);
	}

	public void OnClickMissingSkip()
	{
		_goRewardMiss.SetActive(false);

		SetBox();
	}

	public void OnClickCloseBtn()
	{
		m_MenuMgr.SceneEnd();
		m_MenuMgr.SceneNext(ESceneType.Lobby);
	}

	public void InitializeCurrency()
	{
		_txtGameMoney.text = m_GameMgr.invenMaterial.CalcTotalMoney().ToString();
		_txtCrystal.text = m_GameMgr.invenMaterial.CalcTotalCrystal().ToString();
		_txtTicket.text = m_InvenMaterial.GetItemCount(GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey")).ToString();
	}

	void RootReset()
	{
		_boxSlotNumber = -1;

		_goRewardMiss.SetActive(false);
		_goButtonMissSkip.SetActive(false);

		ComUtil.DestroyChildren(_tResouoceRoot);
		ComUtil.DestroyChildren(_tWeaponRoot);
		ComUtil.DestroyChildren(_tMissRoot);
		ComUtil.DestroyChildren(_tBoxRoot);
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
		//base.Close();
	}

	public override void Escape()
	{
		//base.Escape();
	}
}
