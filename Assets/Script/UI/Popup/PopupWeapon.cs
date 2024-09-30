using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWeapon : UIDialog
{
	[SerializeField]
	GameObject _objRootStats, _objRootWeapon, _objRootEffect, _objRootUpgradeMaterial, _objUpgradeArea,
			   _objStat, _objEffect, _objUpgrade;

	[SerializeField]
	Image _imgFrame, _imgGlow;

	[SerializeField]
	Color[] _colorFrame, _colorGlow;

	[Header("그레이드 별")]
	[SerializeField]
	GameObject[] _objGradeIndicator;

	[Header("리미트 브레이크 표기용 별")]
	[SerializeField]
	GameObject[] _objLimitbreakIndicator;

	[Header("강과 단계 표기 배경")]
	[SerializeField]
	GameObject[] _objReinforceIndicatorBG;

	[Header("강화 단계")]
	[SerializeField]
	GameObject[] _objReinforceIndicator;

	[SerializeField]
	TextMeshProUGUI _txtTitle, _textUpgrade, _txtCP,
					_txtDownButton, _txtEquipButton, _txtUnequipButton, _txtMaxUpgradeButton, _txtUpgradeButton,
					_txtCompareButton, _txtReinforceButton, _txtLimitbreakButton,
					_txtStatTitle, _txtEffectTitle, _txtUpgradeTitle, _txtUpgradeEnough, _txtNullEffect;

	[SerializeField]
	Slider _slUpgrader;

	[SerializeField]
	GameObject _goUpgradeButton, _goMaxUpgradeButton, _goReinforceButton, _goLimitbreakButton,
			   _goEquipButton, _goUnequipButton,
			   _goBase, _goNullEffect, _goButtonCompare, _goReinforceFX,
			   _goMaxUpgradeCover, _goUpgradeCover;

	[SerializeField]
	Toggle _toLock;

	[SerializeField]
	ParticleSystem _pUpgrade, _pReinforce, _pLimitbreak;

	GameObject _objWeapon;

	ItemWeapon _item;
	SlotWeapon _slot;
	PageLobbyTop _top;
	PageLobbyInventory _pageInven;

	Animator _animator;

	Coroutine _coRotateWeapon;

	int u, _maxLevel;
	bool _isEquip, _needReInforce, _needLimitBreak, _needUpgrade, _isRandom;

	string attackpower, aimspeed, attackrange, magazinesize, reloadspeed, attackdelay, collection, accuracy,
		   criticalchance, criticalratio, explosionrange;

	private void Awake()
	{
		Initialize();

		_animator = GetComponent<Animator>();

		u = GlobalTable.GetData<int>("countStandardUpgrade");

		if (m_MenuMgr.CurScene == ESceneType.Lobby)
		{
			_top = GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>();
			_pageInven = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();
		}
	}

	private void OnEnable()
	{
		_item = new ItemWeapon();
		_slot = new SlotWeapon();

		_needReInforce = false;
		_needLimitBreak = false;
	}

	public GameObject GetObjectUpgradeButton()
	{
		return _goUpgradeButton;
	}

	public void InitializeInfo(ItemWeapon item, bool isUpgrade = true, bool isRandom = false, BattleInventory a_oBattleInventory = null)
	{
		_item = item;
		_isRandom = isRandom;

		m_oBattleInventory = a_oBattleInventory;
		RefreshInfo();

		_item.bNew = false;
		if (null != _slot) _slot.SetNewTag(false);

		_objUpgrade.SetActive(isUpgrade);
		_goButtonCompare.SetActive(isUpgrade);
		_toLock.gameObject.SetActive(isUpgrade);
	}

	public void RefreshInfo()
	{
		_item.ReCalcPower();
		SetSlot();

		_maxLevel = u + _item.nCurReinforce * u;

		if (null != _slot) _slot.RefreshInfo();

		CheckUpgradeState(_item.nCurUpgrade, _item.nCurReinforce, _item.nCurLimitbreak);

		_txtTitle.text = _item.strName;

		_imgFrame.color = _colorFrame[_item.nGrade];
		_imgGlow.color = _colorGlow[_item.nGrade];

		InitializeWeapon();

		for (int i = 0; i < _objGradeIndicator.Length; i++)
		{
			_objGradeIndicator[i].SetActive(i < _item.nGrade);
			_objLimitbreakIndicator[i].SetActive(i < _item.nCurLimitbreak);
		}

		for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
		{
			_objReinforceIndicatorBG[i].SetActive(i < _item.nCurLimitbreak);
			_objReinforceIndicator[i].SetActive(i < _item.nCurReinforce);
		}

		_textUpgrade.text = (_needLimitBreak || _needReInforce) ? UIStringTable.GetValue("ui_max") : $"{_item.nCurUpgrade}/{_maxLevel}";
		_txtCP.text = $"CP {ComUtil.ChangeNumberFormat(_item.nCP)}";
		_slUpgrader.value = _item.nCurUpgrade / (float)_maxLevel;

		_isEquip = m_Account.m_nWeaponID.Contains<long>(_item.id);

		_goUnequipButton.SetActive(_isEquip);
		_goEquipButton.SetActive(!_isEquip);

		InitializeText();
		SetUpgradeArea();
		SetEffect();
		SetStats(_item);

		_toLock.isOn = _item.bIsLock;

		if (m_MenuMgr.CurScene == ESceneType.Lobby)
		{
			_top.InitializeCurrency();
			_pageInven.InitializeMaterial();
		}

		Resize();

		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			m_oBattleInventory.Reset();
		}
	}

	void SetSlot()
	{
		if (_item.id == 0) return;

		// 인벤토리가 없을 경우
		if (_pageInven == null)
		{
			return;
		}

		foreach (KeyValuePair<long, GameObject> sl in _pageInven._dicWeapon)
			if (_item.id == sl.Key)
				_slot = sl.Value.GetComponent<SlotWeapon>();

		if (null != _slot)
			_slot.RefreshInfo();
	}

	public void SetSlot(SlotWeapon a_oSlotWeapon)
	{
		_slot = a_oSlotWeapon;
		_slot?.RefreshInfo();
	}

	void SetEffect()
	{
		ComUtil.DestroyChildren(_objRootEffect.transform);

		if (_isRandom)
		{
			for (int i = 0; i < _item.nGrade; i++)
			{
				SlotEffect ef = m_MenuMgr.LoadComponent<SlotEffect>(_objRootEffect.transform, EUIComponent.SlotEffect);
				ef.Initialize(_item, null, 1, 1, true);
			}
		}
		else
		{
			_goNullEffect.SetActive(_item.fEffectValue.Sum() == 0);
			_txtNullEffect.text = UIStringTable.GetValue("ui_popup_weapon_effect_null");

			for (int i = 0; i < _item.nEffectKey.Length; i++)
			{
				if (_item.nEffectKey[i] > 0)
				{
					SlotEffect ef = m_MenuMgr.LoadComponent<SlotEffect>(_objRootEffect.transform, EUIComponent.SlotEffect);
					ef.Initialize(_item, EffectTable.GetData(_item.nEffectKey[i]), _item.fEffectValue[i], 1, false);
				}
			}
		}

	}

	void Resize()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_objRootEffect.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_objRootEffect.GetComponentInParent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_objRootStats.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_objRootStats.GetComponentInParent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_objEffect.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(_goBase.GetComponent<RectTransform>());
	}

	public void ToggleLock()
	{
		StartCoroutine(ChangeLockState());
	}

	IEnumerator ChangeLockState()
	{
		_item.bIsLock = _toLock.isOn;
		yield return StartCoroutine(m_DataMgr.ItemLock(_item));
		_slot.RefreshInfo();
	}

	enum EUpType
	{
		none,
		upgrade,
		reinforce,
		limitbreak,
	}

	EUpType CheckUpgradeState(int upgrade, int reinforce, int limitbreak)
	{
		int maxLevel = u + reinforce * u;

		if (upgrade == maxLevel && reinforce == limitbreak)
		{
			if (limitbreak < _item.nGrade)
			{
				_needLimitBreak = true;
				_needReInforce = false;
				_needUpgrade = false;
				return EUpType.limitbreak;
			}
			else
				return EUpType.none;
		}
		else if (upgrade == maxLevel)
		{
			_needLimitBreak = false;
			_needReInforce = true;
			_needUpgrade = false;
			return EUpType.reinforce;
		}
		else
		{
			_needLimitBreak = false;
			_needReInforce = false;
			_needUpgrade = true;
			return EUpType.upgrade;
		}
	}

	void SetUpgradeArea()
	{
		if (_item.nCurLimitbreak == _item.nGrade &&
				_item.nCurReinforce == _item.nGrade &&
				_item.nCurUpgrade == _maxLevel)
		{
			_objUpgradeArea.SetActive(false);
			_txtUpgradeEnough.gameObject.SetActive(true);
			_goMaxUpgradeCover.SetActive(true);
			_goUpgradeCover.SetActive(true);
			return;
		}
		else
		{
			_objUpgradeArea.SetActive(true);
			_txtUpgradeEnough.gameObject.SetActive(false);
			_goMaxUpgradeCover.SetActive(false);
			_goUpgradeCover.SetActive(false);
		}

		ComUtil.DestroyChildren(_objRootUpgradeMaterial.transform);

		Dictionary<uint, int> material = new Dictionary<uint, int>();

		_goUpgradeButton.SetActive(!_needReInforce && !_needLimitBreak);
		_goReinforceButton.SetActive(_needReInforce);
		_goLimitbreakButton.SetActive(_needLimitBreak);

		if (_needReInforce)
			material = _item.CalcReinforceMaterial();
		else if (_needLimitBreak)
			material = _item.CalcLimitbreakMaterial();
		else
			material = _item.CalcUpgradeMaterial();

		foreach (KeyValuePair<uint, int> m in material)
		{
			SlotMaterial slot = m_MenuMgr.LoadComponent<SlotMaterial>(_objRootUpgradeMaterial.transform, EUIComponent.SlotMaterial);
			slot.Initialize(m.Key, m.Value, true, false, true, SlotMaterial.EVolumeType.inven);
		}
	}

	public void OnClickMaxUpgrade()
	{
		EUpType type;

		int upgrade = _item.nCurUpgrade;
		int reinforce = _item.nCurReinforce;
		int limitbreak = _item.nCurLimitbreak;

		bool enable = true;

		Dictionary<uint, int> material = new Dictionary<uint, int>();
		Dictionary<uint, int> tm = new Dictionary<uint, int>();

		while (enable)
		{
			type = CheckUpgradeState(upgrade, reinforce, limitbreak);

			switch (type)
			{
				case EUpType.limitbreak:
					tm = _item.CalcLimitbreakMaterial(limitbreak);
					if (CheckMaterial(ComUtil.MergeDictionaries(material, tm)))
					{
						material = ComUtil.MergeDictionaries(material, tm);
						upgrade = 0;
						reinforce = 0;
						limitbreak++;
					}
					else
						enable = false;
					break;
				case EUpType.reinforce:
					tm = _item.CalcReinforceMaterial(limitbreak);
					if (CheckMaterial(ComUtil.MergeDictionaries(material, tm)))
					{
						material = ComUtil.MergeDictionaries(material, tm);
						reinforce++;
					}
					else
						enable = false;
					break;
				case EUpType.upgrade:
					tm = _item.CalcUpgradeMaterial(upgrade, limitbreak);
					if (CheckMaterial(ComUtil.MergeDictionaries(material, tm)))
					{
						material = ComUtil.MergeDictionaries(material, tm);
						upgrade++;
					}
					else
						enable = false;
					break;
				case EUpType.none:
					enable = false;
					break;
			}
		}

		if (material.Count > 0)
			StartCoroutine(MaxUpgrade(upgrade, reinforce, limitbreak, material));
		else
			ResouceException();
	}

	/// <summary>
	/// 필요한 재료를 가지고 있는지 확인하자.
	/// </summary>
	/// <param name="material"></param>
	/// <returns></returns>
	bool CheckMaterial(Dictionary<uint, int> material)
	{
		foreach (KeyValuePair<uint, int> m in material)
		{
			if (GameManager.Singleton.invenMaterial.GetItemCount(m.Key) < m.Value)
				return false;
		}

		return true;
	}

	IEnumerator MaxUpgrade(int upgrade, int reinforce, int limitbreak, Dictionary<uint, int> material)
	{
		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		_goMaxUpgradeButton.GetComponent<SoundButton>().interactable = false;

		_animator.SetTrigger("Upgrade");
		_pUpgrade.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		_pUpgrade.Play(true);
		StopCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));
		StartCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));

		Dictionary<long, int> m = new Dictionary<long, int>();

		m = ReCalcMaterialCount(material);

		yield return StartCoroutine(m_DataMgr.ItemMaxUpgrade(_item, upgrade, reinforce, limitbreak, m));

		m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Limitbreak, limitbreak);
		m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Reinforce, reinforce);
		m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Upgrade, upgrade);

		RefreshInfo();

		_goMaxUpgradeButton.GetComponent<SoundButton>().interactable = true;

		wait.Close();
	}

	public void OnClickReset()
	{
		if (_item.nCurUpgrade == 0 && _item.nCurReinforce == 0 && _item.nCurLimitbreak == 0)
		{
			PopupSysMessage popup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			popup.InitializeInfo("ui_error_title", "ui_error_reset_unable", "ui_popup_button_confirm");

			return;
		}

		PopupWeaponDownGrade down = m_MenuMgr.OpenPopup<PopupWeaponDownGrade>(EUIPopup.PopupWeaponDownGrade, true);
		down.InitializeInfo(_item, _slot, this);
	}

	public void OnClickCompare()
	{
		PopupWeaponCompare compare = m_MenuMgr.OpenPopup<PopupWeaponCompare>(EUIPopup.PopupWeaponCompare);
		compare.InitializeInfo(_item);
	}

	void ResouceException()
	{
		Dictionary<uint, int> material = new Dictionary<uint, int>();

		if (m_Account.m_nLevel >= GlobalTable.GetData<int>("valueShopOpenLevel"))
		{
			if (_needReInforce)
				material = _item.CalcReinforceMaterial();
			else if (_needLimitBreak)
				material = _item.CalcLimitbreakMaterial();
			else
				material = _item.CalcUpgradeMaterial();

			foreach (KeyValuePair<uint, int> temp in material)
			{
				if (temp.Value > m_InvenMaterial.GetItemCount(temp.Key))
				{
					LackControl(temp.Key);
					return;
				}
			}
		}
		else
		{
			PopupSysMessage popup = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			popup.InitializeInfo("ui_error_title", "ui_error_resource_lick", "ui_popup_button_confirm");
		}
	}

	void LackControl(uint lackKey)
	{
		ItemMaterial temp = new ItemMaterial(0, lackKey, 0);

		switch (temp.eType)
		{
			case EItemType.Currency:
				if (temp.nSubType == 3)
					PopupAddGameMoney();
				else
					OpenShopBox(1);
				break;
			case EItemType.Material:
			case EItemType.MaterialG:
				OpenShopBox(1);
				break;
			case EItemType.Part:
			case EItemType.GearPart:
				OpenShopBox((m_oBattleInventory != null) ? 2 : 0);
				break;
		}
	}

	void OpenShopBox(int index)
	{
		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			var oBoxDealGroup = BoxDealTable.GetSymbolDataPerGroup(1);
			var oBoxDeal = oBoxDealGroup[index];

			int grade = BoxTable.GetData(oBoxDeal.BoxKey).Grade;
			int subType = BoxTable.GetData(oBoxDeal.BoxKey).SubType;

			if (grade == 1 && subType == 2)
			{
				PopupBoxWeapon pbw = MenuManager.Singleton.OpenPopup<PopupBoxWeapon>(EUIPopup.PopupBoxWeapon, true);
				pbw.InitializeInfo(0);
			}
			else if (grade == 1 && subType == 7)
			{
				PopupBoxMaterial pbm = MenuManager.Singleton.OpenPopup<PopupBoxMaterial>(EUIPopup.PopupBoxMaterial, true);
				pbm.InitializeInfo();
			}
			else if (grade == 2 && subType == 7)
			{
				PopupBoxMaterialPremium pbmp =
					MenuManager.Singleton.OpenPopup<PopupBoxMaterialPremium>(EUIPopup.PopupBoxMaterialPremium, true);
				pbmp.InitializeInfo();
			}

			return;
		}
		
		SlotShopBox[] boxes = FindObjectsOfType<SlotShopBox>();
		boxes[index].OnClick(true);
	}

	public void PopupAddGameMoney()
	{
		PopupShopGameMoney ga = MenuManager.Singleton.OpenPopup<PopupShopGameMoney>(EUIPopup.PopupShopGameMoney, true);
	}

	public void OnClickLimitbreak()
	{
		if (!_slot._ableToLimitBreak)
		{
			ResouceException();
			return; ;
		}

		StartCoroutine(Limitbreak());
	}

	IEnumerator Limitbreak()
	{
		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<uint, int> tmaterial = _item.CalcLimitbreakMaterial();

		if (CheckMaterial(tmaterial))
		{
			_goLimitbreakButton.GetComponent<SoundButton>().interactable = false;

			_animator.SetTrigger("Upgrade");

			_pLimitbreak.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			_pLimitbreak.Play(true);

			GameAudioManager.PlaySFX("SFX/UI/sfx_ui_item_limitbreak_01", 0f, false, ComType.UI_MIX);

			StopCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));
			StartCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));

			Dictionary<long, int> material = new Dictionary<long, int>();

			material = ReCalcMaterialCount(_item.CalcLimitbreakMaterial());

			yield return StartCoroutine(m_DataMgr.ItemLimitbreak(_item, _item.nCurLimitbreak + 1, material));

			m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Limitbreak, _item.nCurLimitbreak + 1);
			m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Reinforce, 0);
			m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Upgrade, 0);

			RefreshInfo();

			_goLimitbreakButton.GetComponent<SoundButton>().interactable = true;
		}
		else
			ResouceException();

		wait.Close();
	}

	public void OnClickReinforce()
	{
		if (!_slot._ableToReInforce)
		{
			ResouceException();
			return;
		}

		StartCoroutine(Reinforce());
	}

	IEnumerator Reinforce()
	{
		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<uint, int> tmaterial = _item.CalcReinforceMaterial();

		if (CheckMaterial(tmaterial))
		{
			_goReinforceButton.GetComponent<SoundButton>().interactable = false;

			_animator.SetTrigger("Upgrade");
			_pReinforce.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			_pReinforce.Play(true);
			_goReinforceFX.SetActive(true);

			StopCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));
			StartCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));

			Dictionary<long, int> material = new Dictionary<long, int>();

			material = ReCalcMaterialCount(_item.CalcReinforceMaterial());

			yield return StartCoroutine(m_DataMgr.ItemReinforce(_item, _item.nCurReinforce + 1, material));
			m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Reinforce, _item.nCurReinforce + 1);

			RefreshInfo();

			_goReinforceButton.GetComponent<SoundButton>().interactable = true;
		}
		else
			ResouceException();

		wait.Close();
	}

	public void OnClickUpgrade(bool isTutorial)
	{
		if (!_slot._ableToUpgrade)
		{
			ResouceException();
			return;
		}

		StartCoroutine(Upgrade());
	}

	IEnumerator Upgrade()
	{
		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		Dictionary<uint, int> tmaterial = _item.CalcUpgradeMaterial();

		if (CheckMaterial(tmaterial))
		{
			_goUpgradeButton.GetComponent<SoundButton>().interactable = false;

			_animator.SetTrigger("Upgrade");
			_pUpgrade.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			_pUpgrade.Play(true);
			StopCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));
			StartCoroutine(FRotateWeapon(_objWeapon.transform.parent.transform));

			Dictionary<long, int> material = ReCalcMaterialCount(tmaterial);

			yield return StartCoroutine(m_DataMgr.ItemUpgrade(_item, _item.nCurUpgrade + 1, material));
			m_InvenWeapon.ModifyItem(_item.id, InventoryData<ItemWeapon>.EItemModifyType.Upgrade, _item.nCurUpgrade + 1);

			RefreshInfo();

			_goUpgradeButton.GetComponent<SoundButton>().interactable = true;
		}
		else
			ResouceException();

		wait.Close();
	}

	Dictionary<long, int> ReCalcMaterialCount(Dictionary<uint, int> material)
	{
		Dictionary<long, int> consume = new Dictionary<long, int>();

		long id;
		int count;

		foreach (KeyValuePair<uint, int> m in material)
		{
			id = m_InvenMaterial.GetItemID(m.Key);
			count = m_InvenMaterial.GetItemCount(m.Key) - m.Value;

			consume.Add(id, count);

			m_InvenMaterial.ModifyItem(id, InventoryData<ItemMaterial>.EItemModifyType.Volume, count);
		}

		return consume;
	}

	void InitializeText()
	{
		_txtDownButton.text = UIStringTable.GetValue("ui_popup_weapon_button_downgrade_caption");
		_txtEquipButton.text = UIStringTable.GetValue("ui_popup_weapon_button_equip_caption");
		_txtUnequipButton.text = UIStringTable.GetValue("ui_popup_weapon_button_unequip_caption");
		_txtCompareButton.text = UIStringTable.GetValue("ui_popup_weapon_button_compare_caption");

		_txtMaxUpgradeButton.text = UIStringTable.GetValue("ui_popup_weapon_button_upgrademax_caption");
		_txtUpgradeButton.text = UIStringTable.GetValue("ui_popup_weapon_button_upgrade_caption");
		_txtReinforceButton.text = UIStringTable.GetValue("ui_popup_weapon_button_reinforce_caption");
		_txtLimitbreakButton.text = UIStringTable.GetValue("ui_popup_weapon_button_limitbreak_caption");
		_txtUpgradeEnough.text = UIStringTable.GetValue("ui_popup_weapon_upgrade_enough");
		_txtStatTitle.text = UIStringTable.GetValue("ui_popup_weapon_stat_title");
		_txtEffectTitle.text = UIStringTable.GetValue("ui_popup_weapon_effect_title");
		_txtUpgradeTitle.text = UIStringTable.GetValue("ui_popup_weapon_upgrade_title");

		attackpower = UIStringTable.GetValue("ui_slot_stat_category_attackpower");
		aimspeed = UIStringTable.GetValue("ui_slot_stat_category_aimspeed");
		attackrange = UIStringTable.GetValue("ui_slot_stat_category_attackrange");
		magazinesize = UIStringTable.GetValue("ui_slot_stat_category_magazinesize");
		reloadspeed = UIStringTable.GetValue("ui_slot_stat_category_reloadspeed");
		attackdelay = UIStringTable.GetValue("ui_slot_stat_category_attackdelay");
		collection = UIStringTable.GetValue("ui_slot_stat_category_collection");
		accuracy = UIStringTable.GetValue("ui_slot_stat_category_accuracy");
		criticalchance = UIStringTable.GetValue("ui_slot_stat_category_criticalchance");
		criticalratio = UIStringTable.GetValue("ui_slot_stat_category_criticalratio");
		explosionrange = UIStringTable.GetValue("ui_slot_stat_category_explosionrange");
	}

	void InitializeWeapon()
	{
		if (null != _objWeapon) Destroy(_objWeapon);

		_objWeapon = m_ResourceMgr.CreateObject(EResourceType.Weapon, _item.strPrefab, _objRootWeapon.transform);

		GameObject obj = _objWeapon.GetComponent<WeaponModelInfo>().GetUIDummy();

		_objWeapon.transform.localPosition = new Vector3(0,
														-obj.transform.localPosition.z * obj.transform.localScale.z,
														obj.transform.localPosition.y * obj.transform.localScale.y);
		_objWeapon.transform.localRotation = obj.transform.localRotation;
		_objWeapon.transform.localScale = obj.transform.localScale;

		StopRotateWeapon();
		StartRotateWeapon();
	}

	IEnumerator RotateWeapon(Transform t)
	{
		float rotationAngle = 0f;

		while (gameObject.activeSelf)
		{
			rotationAngle = 30f * Time.deltaTime;

			t.transform.Rotate(Vector3.up, rotationAngle);

			yield return null;
		}

		yield break;
	}

	public void StartRotateWeapon()
	{
		_coRotateWeapon = StartCoroutine(RotateWeapon(_objWeapon.transform.parent.transform));
	}

	public void StopRotateWeapon()
	{
		if (null != _coRotateWeapon)
		{
			StopCoroutine(_coRotateWeapon);
			_coRotateWeapon = null;
		}
	}

	IEnumerator FRotateWeapon(Transform t)
	{
		float elapsedTime = 0f;
		float rotationAmount;

		while (elapsedTime < 0.5f)
		{
			rotationAmount = 720f * Time.deltaTime;

			t.Rotate(Vector3.up, rotationAmount);

			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}

	public void Equip()
	{
		StartCoroutine(EquipWeapon());
	}

	public void Equip(bool isTutorial)
	{
		StartCoroutine(EquipWeapon());
		m_Tutorial.Activate(false);
	}

	IEnumerator EquipWeapon()
	{
		int index = Array.IndexOf(m_Account.m_nWeaponID, _item.id);
		yield return new WaitForEndOfFrame();

		if (index > -1)
		{
			int cCount = 0;

			for (int i = 0; i < m_Account.m_nWeaponID.Length; i++)
				if (m_Account.m_nWeaponID[i] == 0) cCount++;

			if (cCount >= 3)
			{
				PopupSysMessage s = m_MenuMgr.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
				s.InitializeInfo("ui_error_title", "ui_error_minweapon", "ui_popup_button_confirm");

				yield break;
			}

			yield return StartCoroutine(this.CoEquipWeapon(index, 0));
		}
		else
		{
			index = Array.IndexOf(m_Account.m_nWeaponID, 0);

			if (index > -1)
			{
				yield return StartCoroutine(this.CoEquipWeapon(index, _item.id));
			}
			else
			{
				EquipReady();
			}
		}
	}

	public void EquipReady()
	{
		PopupWeaponChange change = m_MenuMgr.OpenPopup<PopupWeaponChange>(EUIPopup.PopupWeaponChange);
		change.Initialize();

		// 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			change.Init(m_oBattleInventory);
			change.SetWeapon(m_oBattleInventory.CreateSlotWeapon(_item, null).gameObject, true);

			for (int i = 0; i < m_Account.m_nWeaponID.Length; i++)
			{
				var oItemWeapon = m_oBattleInventory.FindEquipWeapon(i);

				// 무기가 없을 경우
				if (oItemWeapon == null)
				{
					continue;
				}

				change.SetWeapon(m_oBattleInventory.CreateSlotWeapon(oItemWeapon, null).gameObject, false);
			}

			return;
		}

		change.SetWeapon(_pageInven._dicWeapon[_item.id], true);

		for (int i = 0; i < m_Account.m_nWeaponID.Length; i++)
		{
			GameObject to = _pageInven.EquipWeapon(m_Account.m_nWeaponID[i]);
			change.SetWeapon(to);
		}
	}

	void SetStats(ItemWeapon item)
	{
		ComUtil.DestroyChildren(_objRootStats.transform);

		SlotStat AttackPower = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		AttackPower.Initialize(EWeaponStatType.ATTACKPOWER,
							   attackpower,
							   item.nAttackPower.ToString(),
							   item.nAttackPower > item.nAttackPowerStandard);

		SlotStat CriticalChance = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		CriticalChance.Initialize(EWeaponStatType.CRITICALCHANCE,
								  criticalchance,
								  $"{(MathF.Round(item.fCriticalChance * 10000f) / 100f)} %",
								  item.fCriticalChance > GlobalTable.GetData<float>("perCritical"));

		SlotStat AttackRange = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		AttackRange.Initialize(EWeaponStatType.ATTACKRANGE,
							   attackrange,
							   $"{(MathF.Round(item.nAttackRange / 100f) / 10f)} m",
							   item.nAttackRange > WeaponTable.GetData(item.nKey).Range);

		SlotStat CriticalRatio = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		CriticalRatio.Initialize(EWeaponStatType.CRITICALRATIO,
								 criticalratio,
								 $"x {(MathF.Round(item.fCriticalRatio * 100f) / 100f)}",
								 item.fCriticalRatio > GlobalTable.GetData<float>("ratioCritical"));

		SlotStat MagazineSize = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		MagazineSize.Initialize(EWeaponStatType.MAGAZINESIZE,
								magazinesize,
								item.nMagazineSize.ToString(),
								item.nMagazineSize > WeaponTable.GetData(item.nKey).MagazineSize);

		SlotStat ReloadTime = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
		ReloadTime.Initialize(EWeaponStatType.RELOADTIME,
							  reloadspeed,
							  $"{Mathf.Round(item.nReloadTime / 100f) / 10f} s",
							  item.nReloadTime < WeaponTable.GetData(item.nKey).ReloadTime);


		if (item.eSubType == EWeaponType.SR)
		{
			SlotStat AimTime = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
			AimTime.Initialize(EWeaponStatType.AIMTIME,
							   aimspeed,
							   $"{Mathf.Round(item.nAimTime / 100f) / 10f} s",
							   item.nAimTime < WeaponTable.GetData(item.nKey).AimTime);
		}
		else
		{
			if (item.eSubType != EWeaponType.Grenade)
			{
				SlotStat AttackDelay = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
				AttackDelay.Initialize(EWeaponStatType.ATTACKDELAY,
									   attackdelay,
									   MathF.Round(1000 / (float)item.nAttackDelay).ToString(),
									   item.nAttackDelay < WeaponTable.GetData(item.nKey).AttackDelay);
			}
		}

		if (item.eSubType == EWeaponType.Grenade)
		{
			SlotStat explosionRange = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
			explosionRange.Initialize(EWeaponStatType.EXPLOSIONRANGE,
									  explosionrange,
									  $"{Mathf.Round(EffectTable.GetGroup(WeaponTable.GetData(item.nKey).HitEffectGroup)[0].Value * (1 + item.fExplosionRangeRatio)) / 1000f} m",
									  item.fExplosionRangeRatio > 0);
		}
		else
		{
			SlotStat Collection = m_MenuMgr.LoadComponent<SlotStat>(_objRootStats.transform, EUIComponent.SlotStat);
			int t = GlobalTable.GetData<int>("valueStadardAccuracy");
			Collection.Initialize(item.eSubType == EWeaponType.SG ? EWeaponStatType.REDUCESPREAD : EWeaponStatType.ACCURACY,
									item.eSubType == EWeaponType.SG ? collection : accuracy,
									((int)(((float)t / item.nAccuracy)) / 100f).ToString(),
									item.nAccuracy > WeaponTable.GetData(item.nKey).Accuracy);
		}
	}

	private void OnDisable()
	{
		StopRotateWeapon();
		_pageInven?.InitializeWeapon();
		_pageInven?.InitializeGear();
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

	#region 추가
	private BattleInventory m_oBattleInventory = null;

	/** 전투 무기 팝업을 설정한다 */
	public void SetupBattlePopupWeapon()
	{
		// Do Something
	}

	/** 무기를 장착한다 */
	private IEnumerator CoEquipWeapon(int a_nIdx, long a_nWeaponID)
	{
		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		yield return StartCoroutine(m_GameMgr.EquipWeapon(a_nIdx, a_nWeaponID, () =>
		{
			// 전투 인벤토리 팝업이 존재 할 경우
			if (m_oBattleInventory != null)
			{
				m_oBattleInventory.Reset();
			}

			wait.Close();
			this.Close();
		}));
	}
	#endregion // 추가
}
