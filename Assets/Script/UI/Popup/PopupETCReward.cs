using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopupETCReward : UIDialog
{
	[SerializeField]
	Transform _tSlotRoot;

	[SerializeField]
	GameObject _goButtonClose, _goButtonEquip, _goGearIcon, _goMaterialIcon;

	[SerializeField]
	GameObject _goObtainDesc, _goDesc, _goEquipDesc;

	[SerializeField]
	TextMeshProUGUI _txtObtainDesc, _txtDesc, _txtEquipDesc;

	Action _callback;
	PageLobbyInventory _pageInven;
	PageLobby _pageLobby;

	EItemType _type;

	private void Awake()
	{
		Initialize();
		InitializeText();
	}

	public void InitializeInfo(uint key, int count, PageLobbyInventory pageInven, PageLobby pageLobby, Action callback = null)
	{
		_pageInven = pageInven;
		_pageLobby = pageLobby;
		_callback = callback;

		_type = ComUtil.GetItemType(key);

		PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		ComUtil.DestroyChildren(_tSlotRoot);

		SlotSimpleItemReward slot = m_MenuMgr.LoadComponent<SlotSimpleItemReward>(_tSlotRoot, EUIComponent.SlotSimpleItemReward);
		slot.Initialize(ComUtil.GetIcon(key), "1", ComUtil.GetItemName(key), ComUtil.GetItemGrade(key), true, false, key);
		slot.SetAppear();

		_txtDesc.text = _type == EItemType.Gear ? DescTable.GetValue(GearTable.GetData(key).DescKey) :
												  DescTable.GetValue(MaterialTable.GetData(key).DescKey);

		_goGearIcon.SetActive(_type == EItemType.Gear);
		_goMaterialIcon.SetActive(_type == EItemType.Material);

		GameManager.Singleton.StartCoroutine(m_GameMgr.AddItemCS(key, count, () =>
		{
			GameManager.Singleton.StartCoroutine(m_DataMgr.ObtainChapterWeapon( () =>
			{
				if ( _type == EItemType.Gear )
					_pageInven.InitializeGear();
				else
					_pageInven.InitializeMaterial();
				
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
		_txtObtainDesc.text = UIStringTable.GetValue("ui_hint_new_item_desc");
		_txtEquipDesc.text = UIStringTable.GetValue("ui_hint_new_weapon_auto_equip_desc");

		_goButtonClose.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_common_close");
		_goButtonEquip.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_weapon_button_equip_caption");
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