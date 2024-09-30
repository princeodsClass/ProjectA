using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopupWeaponReward : UIDialog
{
	[SerializeField]
	Transform _tSlotRoot;

	[SerializeField]
	GameObject _goButtonClose, _goButtonEquip;

	[SerializeField]
	GameObject _goObtainDesc, _goDesc, _goEquipDesc;

	[SerializeField]
	TextMeshProUGUI _txtObtainDesc, _txtDesc, _txtEquipDesc;

	Action _callback;
	PageLobbyInventory _pageInven;
	PageLobby _pageLobby;


	private void Awake()
	{
		Initialize();
		InitializeText();

		_goButtonEquip.SetActive(Array.IndexOf(m_Account.m_nWeaponID, 0) == -1);
		_goEquipDesc.SetActive(!_goButtonEquip.activeSelf);
	}

	public void InitializeInfo(uint key, int count, PageLobbyInventory pageInven, PageLobby pageLobby, Action callback = null)
	{
		_pageInven = pageInven;
		_pageLobby = pageLobby;
		_callback = callback;

		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		ComUtil.DestroyChildren(_tSlotRoot);

		SlotSimpleItemReward slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tSlotRoot, EUIComponent.SlotSimpleItemReward);
		slot.Initialize(ComUtil.GetIcon(key), "1", ComUtil.GetItemName(key), ComUtil.GetItemGrade(key), true, false, key);
		slot.SetAppear();
		_txtDesc.text = DescTable.GetValue(WeaponTable.GetData(key).DescKey);

		GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(key, count, () =>
		{
			GameManager.Singleton.StartCoroutine(m_DataMgr.ObtainChapterWeapon( () =>
			{
				_pageInven.InitializeWeapon();
				wait.Close();
			}));
		}));
	}

	public void SetCallback(Action action)
	{
		_callback = action;
	}

	void InitializeText()
    {
		_txtObtainDesc.text = UIStringTable.GetValue("ui_hint_new_weapon_desc");
		_txtEquipDesc.text = UIStringTable.GetValue("ui_hint_new_weapon_auto_equip_desc");

		_goButtonClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_common_close");
		_goButtonEquip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_weapon_button_equip_caption");
	}

	public void EquipReady()
	{
		_pageLobby.OnButtonMenuClick(1);

		PageLobby pageLobby = _pageLobby;
		Action callback = _callback;
		_callback = null;

		PopupWeaponChange change = m_MenuMgr.OpenPopup<PopupWeaponChange>(EUIPopup.PopupWeaponChange);

		change.Initialize();
		change.SetWeapon(_pageInven._dicWeapon[m_GameMgr._tempWeaponID], true);
		change.SetCallback(() =>
		{
			pageLobby.OnButtonMenuClick(2);
			callback?.Invoke();
		});
		
		for (int i = 0; i < m_Account.m_nWeaponID.Length; i++)
		{
			GameObject to = _pageInven.EquipWeapon(m_Account.m_nWeaponID[i]);
			change.SetWeapon(to);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
	}

    public override void Escape()
    {
		return;
    }

    public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
		m_GameMgr._tempWeaponID = default;
		_callback?.Invoke();
		base.Close();
	}
}