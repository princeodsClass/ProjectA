using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupShopCrystal : UIDialog
{
	[SerializeField] TextMeshProUGUI _txtGameMoney, _txtCrystal;

    private void Refresh()
	{
		ComShopCrystal popup = GetComponentInChildren<ComShopCrystal>();
		ComShopCrystal compo = GameObject.Find("ShopPage")?.GetComponentInChildren<ComShopCrystal>();

		if (null != compo) compo.InitializeInfo();
		if (true == popup.gameObject.activeInHierarchy) popup.InitializeInfo();

		#region 추가
		var oPopupBattleContinue = GameObject.Find("PopupBattleContinue")?.GetComponentInChildren<PopupBattleContinue>();
		oPopupBattleContinue?.Reopen();
		#endregion // 추가
	}

    public void SetTop()
    {
        _txtGameMoney.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalMoney());
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    private void OnEnable()
    {
		GetComponentInChildren<ComShopCrystal>().SetFrame(this);
		SetTop();
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
		Refresh();
		base.Close();
	}

	public override void Escape()
	{
		Refresh();
		base.Escape();
	}

	private void Awake()
	{
		Initialize();
	}
}
