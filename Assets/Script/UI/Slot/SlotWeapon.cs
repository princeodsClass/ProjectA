using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotWeapon : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler,
						  IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField]
	Image _imgFrame, _imgGlow, _Icon, _imgType;

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
	TextMeshProUGUI _textUpgrade, _textCP;

	[SerializeField]
	GameObject _objUpgradeArrow, _objReinforceArrow, _objLimitbreakArrow,
			   _objLock, _objNewTag, _objDimed, _objSelected,
			   _goTargetFX;

	[SerializeField]
	RectTransform S;

	[SerializeField]
	GridLayoutGroup _gGrade;

	public ItemWeapon _Item;

	Canvas _canvas;
	Animator _animator;
	PageLobbyInventory _invenPage;

	uint _goodsKey;
	int u, _maxLevel, _cp, _slotNumber;
	float _fCellSize, _fStandardWidth;

	public bool _ableToLimitBreak, _ableToReInforce, _ableToUpgrade;
	bool _interactable = true;
	bool _isDragging = false;

	List<SlotWeapon> _exTarget = new List<SlotWeapon>();
	List<int> _exIndex = new List<int>();

	ScrollRect _scrollRect;
	UserAccount _user;
	SlotWeapon _targetSlot;
	Collider _collider;
	long[] _tar;
	int _tS;

	public enum SlotState
	{
		normal,
		unequiped,
		equiped,
		changeTarget,
		changeMain,
		recycleUnselected,
		recycleSelected,
		compareUnselected,
		compareSelected,
		compareNormal,
		reward,
		goods,
	}

	public enum EPresentType
	{
		current,
		origin,
		max,
	}

	SlotState _state = SlotState.normal;

	static long m_nMainID;

	public SlotWeapon() { }

	private void Awake()
	{
		_canvas = GameObject.Find(ComType.UI_ROOT_NAME).GetComponent<Canvas>();
		_animator = GetComponent<Animator>();
		_objNewTag.SetActive(false);
		_fCellSize = _gGrade.cellSize.x;
		_fStandardWidth = S.rect.width;

		_user = GameManager.Singleton.user;
		_tar = _user.m_nWeaponID;
		_targetSlot = null;

		u = GlobalTable.GetData<int>("countStandardUpgrade");

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			_invenPage = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();

		_collider = GetComponent<Collider>();
	}

	public void Initialize(ItemWeapon item, bool isButton = true, EPresentType type = EPresentType.current, uint goodsKey = 0, BattleInventory a_oPopupInventory = null)
	{
		_Item = item;
		_goodsKey = goodsKey;
		m_oBattleInventory = a_oPopupInventory;

		SetNewTag(_Item.bNew);

		RefreshInfo(type);
		_interactable = isButton;
	}

	public void SetNewTag(bool state)
	{
		if (null == _Item) return;

		_Item.bNew = state;
		_objNewTag.SetActive(_Item.bNew);

		if (!state && MenuManager.Singleton.CurScene == ESceneType.Lobby)
		{
			bool cur = false;

			foreach (ItemWeapon weapon in GameManager.Singleton.invenWeapon)
				if (weapon.bNew) cur = true;

			if (!cur) _invenPage.SetNewTag(0, false);
		}
	}

	public void RefreshInfo(EPresentType type = EPresentType.current) // bool isOrigin = false)
	{
		SetTargetFX(false);

		_maxLevel = u + _Item.nCurReinforce * u;
		_imgFrame.color = _colorFrame[_Item.nGrade];
		_imgGlow.color = _colorGlow[_Item.nGrade];
		_Icon.sprite = _Item.GetIcon();
		_imgType.sprite = ComUtil.GetWeaponSubtypeIcon(_Item.eSubType);

		for (int i = 0; i < _objGradeIndicator.Length; i++)
		{
			_objGradeIndicator[i].SetActive(i < _Item.nGrade);

			switch (type)
			{
				case EPresentType.current:
					_objLimitbreakIndicator[i].SetActive(i < _Item.nCurLimitbreak);
					break;
				case EPresentType.max:
					_objLimitbreakIndicator[i].SetActive(true);
					break;
				case EPresentType.origin:
					_objLimitbreakIndicator[i].SetActive(false);
					break;
			}
		}

		switch (type)
		{
			case EPresentType.current:
				PresentCurrent();
				break;
			case EPresentType.origin:
				PresentOrigin();
				break;
			case EPresentType.max:
				PresentMax();
				break;
		}

		// 그리드 레이아웃이 존재 할 경우
		if (transform.parent != null && transform.parent.TryGetComponent(out GridLayoutGroup oGridLayout))
		{
			ResizeCell(oGridLayout.cellSize.x);
		}
	}

	public void SetTargetFX(bool state)
	{
		if (null != _goTargetFX)
			_goTargetFX.SetActive(state);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//if ( _objDimed.activeSelf ) return;

		switch (_state)
		{
			case SlotState.recycleSelected:
			case SlotState.recycleUnselected:
			case SlotState.unequiped:
			case SlotState.normal:
			case SlotState.compareUnselected:
			case SlotState.reward:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					m_oBattleInventory.ScrollRect.OnBeginDrag(eventData);
					return;
				}

				_scrollRect.OnBeginDrag(eventData);
				break;
			case SlotState.equiped:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					return;
				}

				_isDragging = true;
				_invenPage._tempSlot.transform.position = transform.position;
				ComUtil.SetParent(_invenPage._tempSlot, transform);

				foreach (KeyValuePair<long, GameObject> t in _invenPage._dicWeapon)
					for (int i = 0; i < _tar.Length; i++)
						if (t.Key == _tar[i])
							t.Value.GetComponent<SlotWeapon>().SetEquipReady(true);
				break;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		switch (_state)
		{
			case SlotState.recycleSelected:
			case SlotState.recycleUnselected:
			case SlotState.unequiped:
			case SlotState.normal:
			case SlotState.compareUnselected:
			case SlotState.reward:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					m_oBattleInventory.ScrollRect.OnEndDrag(eventData);
					return;
				}

				_scrollRect.OnEndDrag(eventData);
				break;
			case SlotState.equiped:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					return;
				}
				
				if (false == _isDragging) return;
				_isDragging = false;
				_invenPage.InitializeWeapon();
				break;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		switch (_state)
		{
			case SlotState.recycleSelected:
			case SlotState.recycleUnselected:
			case SlotState.unequiped:
			case SlotState.normal:
			case SlotState.compareUnselected:
			case SlotState.reward:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					m_oBattleInventory.ScrollRect.OnDrag(eventData);
					return;
				}

				_scrollRect.OnDrag(eventData);
				break;
			case SlotState.equiped:
				// 전투 인벤토리 팝업이 존재 할 경우
				if (m_oBattleInventory != null)
				{
					return;
				}

				if (false == _isDragging) return;

				RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.transform as RectTransform,
																		eventData.position,
																		_canvas.worldCamera, out Vector3 worldPos);
				gameObject.transform.position = worldPos;
				break;
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (false == _isDragging) return;

		int slot = -1;

		if (null == _targetSlot) return;

		for (int i = 0; i < _tar.Length; i++)
			if (_tar[i] == _Item.id) slot = i;

		if (slot == -1)
			StartCoroutine(GameManager.Singleton.EquipWeapon(_tS, _Item.id));
		else
			StartCoroutine(GameManager.Singleton.EquipWeapon(slot, _Item.id, _tS, _targetSlot._Item.id));

		_exTarget.Clear();
		_exIndex.Clear();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		// if ( _objDimed.activeSelf ) return;
		_animator.SetTrigger("Pressed");
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		// if (_objDimed.activeSelf) return;
		_animator.SetTrigger("Unpressed");
	}

	void OnTriggerEnter(Collider other)
	{
		if (false == _isDragging || other.name != "SlotWeapon") return;

		SlotWeapon tar = other.GetComponent<SlotWeapon>();

		foreach (GameObject go in _invenPage._dicWeapon.Values)
			go.GetComponent<SlotWeapon>().SetTargetFX(false);

		for (int i = 0; i < _tar.Length; i++)
		{
			if (tar._Item.id == _tar[i])
			{
				if (false == _exTarget.Contains(tar))
				{
					_exTarget.Add(tar);
					_exIndex.Add(i);

					_targetSlot = tar;
					_tS = i;
					tar.SetTargetFX(true);
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (false == _isDragging || other.name != "SlotWeapon") return;

		SlotWeapon tar = other.GetComponent<SlotWeapon>();
		tar.SetTargetFX(false);

		if (_exTarget.Contains(tar))
		{
			int index = _exTarget.IndexOf(tar);

			_exTarget.RemoveAt(index);
			_exIndex.RemoveAt(index);

			_targetSlot = null;

			if (_exTarget.Count > 0)
			{
				_targetSlot = _exTarget[_exTarget.Count - 1];
				_tS = _exIndex[_exIndex.Count - 1];

				_targetSlot.SetTargetFX(true);
			}
			else
			{
				_targetSlot = null;
			}
		}
	}

	void PresentCurrent()
	{
		CheckUpgradeState();

		for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
		{
			_objReinforceIndicatorBG[i].SetActive(i < _Item.nCurLimitbreak);
			_objReinforceIndicator[i].SetActive(i < _Item.nCurReinforce);
		}

		_textUpgrade.gameObject.SetActive(true);
		_textUpgrade.text = (_ableToReInforce || _ableToLimitBreak) ? UIStringTable.GetValue("ui_max") : $"{_Item.nCurUpgrade}/{_maxLevel}";
		_textCP.text = ComUtil.ChangeNumberFormat(_Item.nCP);
		_objLock.SetActive(_Item.bIsLock);
	}

	void PresentOrigin()
	{
		for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
		{
			_objReinforceIndicatorBG[i].SetActive(false);
			_objReinforceIndicator[i].SetActive(false);
		}

		_textUpgrade.gameObject.SetActive(false);
		_textUpgrade.text = string.Empty;
		_textCP.text = ComUtil.ChangeNumberFormat(_Item.CalcCP(_Item.nAttackPowerStandard));
		_objLock.SetActive(false);
	}

	void PresentMax()
	{
		int ap = _Item.CalcAttackPower((_Item.nGrade + 1) * 10, _Item.nGrade + 1, _Item.nGrade + 1);

		for (int i = 0; i < _objReinforceIndicatorBG.Length; i++)
		{
			_objReinforceIndicatorBG[i].SetActive(i < _Item.nGrade);
			_objReinforceIndicator[i].SetActive(i < _Item.nGrade);
		}

		_textUpgrade.gameObject.SetActive(true);
		_textUpgrade.text = UIStringTable.GetValue("ui_max");
		_textCP.text = ComUtil.ChangeNumberFormat(_Item.CalcCP(ap));
		_objLock.SetActive(false);
	}

	void CheckUpgradeState()
	{
		_ableToLimitBreak = _ableToReInforce = _ableToUpgrade = false;

		if (_Item.nCurUpgrade == _maxLevel &&
			 _Item.nCurReinforce == _Item.nCurLimitbreak &&
			 _Item.nCurLimitbreak < _Item.nGrade)
			_ableToLimitBreak = ComUtil.CheckMaterial(_Item.CalcLimitbreakMaterial());
		else if (_Item.nCurUpgrade == _maxLevel &&
				  _Item.nCurReinforce < _Item.nCurLimitbreak)
			_ableToReInforce = ComUtil.CheckMaterial(_Item.CalcReinforceMaterial());
		else if (_Item.nCurUpgrade < _maxLevel)
			_ableToUpgrade = ComUtil.CheckMaterial(_Item.CalcUpgradeMaterial());

		_objLimitbreakArrow.SetActive(_ableToLimitBreak);
		_objReinforceArrow.SetActive(_ableToReInforce);
		_objUpgradeArrow.SetActive(_ableToUpgrade && (GameManager.Singleton.user.m_nWeaponID.Contains(_Item.id) || _Item.nCurUpgrade > 0));
	}

	public bool AbleToRecycle()
	{
		return (!(_Item.nCurUpgrade > 0 || _Item.nCurReinforce > 0 || _Item.nCurLimitbreak > 0));
	}

	public void SetSlotNumber(int slotNumber)
	{
		_slotNumber = slotNumber;
	}

	public void SetBaseScrollRect(ScrollRect sr)
	{
		_scrollRect = sr;
	}

	public void SetState(SlotState state)
	{
		_state = state;
		_collider.enabled = state == SlotState.equiped;

		CheckUpgradeState();
		if (state == SlotState.changeMain) m_nMainID = _Item.id;
	}

	//public void OnClick()
	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.dragging || !_interactable) return;

		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			switch (_state)
			{
				case SlotState.changeTarget:
					Equip();
					break;

				default:
					var oPopup = DetailPopup();
					oPopup.SetSlot(this);
					oPopup.SetupBattlePopupWeapon();

					break;
			}

			return;
		}

		switch (_state)
		{
			case SlotState.normal:
			case SlotState.equiped:
			case SlotState.unequiped:
				DetailPopup();
				break;
			case SlotState.reward:
				DetailPopup(true, false, true);
				break;
			case SlotState.changeTarget:
				Equip();
				break;
			case SlotState.goods:
				GoodsPopup();
				break;
			case SlotState.recycleUnselected:
				ExceptionRecycleUnselected();
				break;
			case SlotState.recycleSelected:
				GameObject.Find("PopupRecycle").GetComponent<PopupRecycle>().SelectWeapon(_Item, gameObject, PopupRecycle.ESelectType.Unselect);
				SetSelected(SlotState.recycleUnselected);
				break;
			case SlotState.compareNormal:
				DetailPopup(true, false);
				break;
			case SlotState.compareSelected:
				ExceptionCompareSelected();
				break;
			case SlotState.compareUnselected:
				GameObject.Find("PopupWeaponCompare").GetComponent<PopupWeaponCompare>().Select(_Item);
				SetSelected(SlotState.compareSelected);
				break;
		}
	}

	void ExceptionRecycleUnselected()
	{
		if (_Item.bIsLock)
		{
			PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			er.InitializeInfo("ui_error_title", "ui_error_recycle_locked", "ui_popup_button_confirm");
		}
		else if (GameManager.Singleton.user.IsEquipWeapon(_Item.id))
		{
			PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			er.InitializeInfo("ui_error_title", "ui_error_recycle_equiped", "ui_popup_button_confirm");
		}
		else if (_Item.nCurUpgrade + _Item.nCurReinforce + _Item.nCurLimitbreak > 0)
		{
			PopupSysMessage er = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
			er.InitializeInfo("ui_error_title", "ui_error_recycle_upgraded", "ui_popup_button_confirm");
		}
		else
		{
			GameObject.Find("PopupRecycle").GetComponent<PopupRecycle>().SelectWeapon(_Item, gameObject, PopupRecycle.ESelectType.Select);
			SetSelected(SlotState.recycleSelected);
		}
	}

	void ExceptionCompareSelected()
	{
		if (_objSelected.activeSelf)
		{
			GameObject.Find("PopupWeaponCompare").GetComponent<PopupWeaponCompare>().Unselect();
			SetSelected(SlotState.compareUnselected);
		}
		else
		{
			DetailPopup(true, false);
		}
	}

	public void SetDim(bool state)
	{
		_objDimed.SetActive(state);
		//_interactable = !state;

	}

	public void SetSelected(SlotState state)
	{
		_objSelected.SetActive(state == SlotState.compareSelected ||
							   state == SlotState.recycleSelected);
		_state = state;
	}

	public bool IsLock()
	{
		return _Item.bIsLock;
	}

	public void DetailPopupTutorial(bool isTutorial)
	{
		PopupWeapon pop = DetailPopup();

		GameObject button = pop.GetObjectUpgradeButton();
		RectTransform rt = button.GetComponent<RectTransform>();
		GameManager.Singleton.tutorial.SetMessage("ui_tip_weapon_upgrade_00", -250);
		GameManager.Singleton.tutorial.SetFinger(button,
												 pop.OnClickUpgrade,
												 rt.rect.width, rt.rect.height, 300);
	}

	public PopupWeapon DetailPopup(bool overlap = true, bool presentUpgrade = true, bool isRandom = false)
	{
		PopupWeapon detail = MenuManager.Singleton.OpenPopup<PopupWeapon>(EUIPopup.PopupWeapon, overlap);
		detail.InitializeInfo(_Item, presentUpgrade, isRandom, m_oBattleInventory);

		return detail;
	}

	public void GoodsPopup()
	{
		PopupItemBuy buy = MenuManager.Singleton.OpenPopup<PopupItemBuy>(EUIPopup.PopupItemBuy, true);
		buy.InitializeInfo(_goodsKey, _slotNumber);
	}

	public void SetEquipReady(bool state)
	{
		_animator.SetBool("IsReadyChange", state);
	}

	public void Equip()
	{
#if DISABLE_THIS
        // for ( int i = 0; i < 4; i++ )
        //     if ( _Item.id == GameManager.Singleton.user.m_nWeaponID[i] )
        //     {
        //         StartCoroutine(GameManager.Singleton.EquipWeapon(i, m_nMainID));
        //         _state = SlotState.equiped;
        //     }

        // GameObject.Find("PopupWeaponChange").GetComponent<PopupWeaponChange>().Close();
#endif // #if DISABLE_THIS

		for (int i = 0; i < ComType.G_MAX_NUM_EQUIP_WEAPONS; ++i)
		{
			// 선택 무기 일 경우
			if (_Item.id == GameManager.Singleton.user.m_nWeaponID[i])
			{
				PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
				StartCoroutine(GameManager.Singleton.EquipWeapon(i, m_nMainID, () => this.OnEquipWeapon(wait)));

				_state = SlotState.equiped;
			}
		}
	}

	void ResizeCell(float cur)
	{
		float t = cur / _fStandardWidth * _fCellSize;
		_gGrade.cellSize = new Vector2(t, t);
	}

	#region 추가
	private BattleInventory m_oBattleInventory = null;

	/** 무기를 장착했을 경우 */
	private void OnEquipWeapon(PopupWait4Response a_oWait)
	{
		a_oWait.Close();
		GameObject.Find("PopupWeaponChange")?.GetComponent<PopupWeaponChange>().Close();

		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			m_oBattleInventory.PopupWeaponChange.Close();
			m_oBattleInventory.Reset();
		}
	}
	#endregion // 추가
}
