using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWeaponChange : UIDialog
{
	[SerializeField] TextMeshProUGUI _txtTitle;
	[SerializeField] Transform _tEquipment, _tMain;

	PageLobbyInventory _page;
	UIMultiScrollRect _scrollRect;

	Action _callback;

	private void Awake()
	{
		Initialize();
		_txtTitle.text = UIStringTable.GetValue("ui_popup_weaponchange_desc");
	}

	private void OnEnable()
	{
		_page = GameObject.Find("InventoryPage")?.GetComponent<PageLobbyInventory>();
		_scrollRect = _page?.GetComponent<UIMultiScrollRect>();

		// 스크롤 영역이 존재 할 경우
		if (_scrollRect != null)
		{
			_scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_oBattleInventory = null;
	}

	public void SetCallback(Action action)
	{
		_callback = action;
	}

	public void SetWeapon(GameObject obj, bool m = false)
	{
		SlotWeapon slotWeapon = obj.GetComponent<SlotWeapon>();

		if (m)
		{
			ComUtil.SetParent(_tMain, obj.transform);
			slotWeapon.SetState(SlotWeapon.SlotState.changeMain);
		}
		else
		{
			_page?.SetWeaponAnimation(obj);

			ComUtil.SetParent(_tEquipment, obj.transform);
			slotWeapon.SetState(SlotWeapon.SlotState.changeTarget);
		}

		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			slotWeapon.SetEquipReady(true);
			m_oSlotWeaponList.Add(obj);
		}
	}

	public void ResetWeapon()
	{
		if (!gameObject.activeSelf)
			_page?.InitializeWeapon();

		// 전투 인벤토리 팝업이 존재 할 경우
		if (m_oBattleInventory != null)
		{
			for(int i = 0; i < m_oSlotWeaponList.Count; ++i)
			{
				GameResourceManager.Singleton.ReleaseObject(m_oSlotWeaponList[i], false);
			}

			m_oBattleInventory.Reset();
		}
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

	private void OnDisable()
	{
		ResetWeapon();
		_callback?.Invoke();
		_callback = null;
	}

	#region 추가
	private BattleInventory m_oBattleInventory = null;
	private List<GameObject> m_oSlotWeaponList = new List<GameObject>();

	/** 초기화 */
	public virtual void Init(BattleInventory a_oBattleInventory)
	{
		m_oBattleInventory = a_oBattleInventory;
		a_oBattleInventory.SetPopupWeaponChange(this);
	}
	#endregion // 추가
}
