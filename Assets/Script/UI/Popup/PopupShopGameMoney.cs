using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupShopGameMoney : UIDialog
{
    [SerializeField] TextMeshProUGUI _txtGameMoney, _txtCrystal;

    private void Refresh()
    {
        ComShopGameMoney popup = GetComponentInChildren<ComShopGameMoney>();
        ComShopGameMoney compo = GameObject.Find("ShopPage")?.GetComponentInChildren<ComShopGameMoney>();

        if (true == popup?.gameObject.activeInHierarchy) popup.InitializeInfo();
        if (null != compo) compo.InitializeInfo();
    }

    public void SetTop()
    {
        _txtGameMoney.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalMoney());
        _txtCrystal.text = ComUtil.ChangeNumberFormat(m_GameMgr.invenMaterial.CalcTotalCrystal());
    }

    private void OnEnable()
    {
        GetComponentInChildren<ComShopGameMoney>().SetFrame(this);
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
