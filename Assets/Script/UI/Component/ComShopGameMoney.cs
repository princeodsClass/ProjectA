using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopGameMoney : MonoBehaviour
{
    [SerializeField]
    GameObject _goButtonClose;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtTimerTitle, _txtTimer;

    [SerializeField]
    Transform _tGoodsRoot;

    [SerializeField]
    RectTransform[] _tForResize;

    List<MoneyDealTable> _goods = new List<MoneyDealTable>();
    PopupShopGameMoney _popup = null;

    string D, h, m, s = string.Empty;

    private void OnEnable()
    {
        InitializeInfo();
        _goButtonClose.SetActive(_popup != null);
    }

    public void InitializeInfo()
    {
        InitializeText();

        StartCoroutine(InitializeDealInfo());
        StartCoroutine(SetTimerText());
    }

    public void SetFrame(PopupShopGameMoney popup)
    {
        _popup = popup;
    }

    public void OnClickClose()
    {
        _popup?.Close();
    }

    IEnumerator SetTimerText()
    {
        TimeSpan time = GameManager.Singleton.eventController._eGameMoneyDeal.CheckRemainTime();

        while (time.TotalMilliseconds > 0f)
        {
            _txtTimer.text = string.Empty;

            _txtTimer.text = time.Days > 0 ? $"{time.Days}{D} " : "";
            _txtTimer.text = _txtTimer.text + (time.Hours > 0 ? $"{time.Hours}{h} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Minutes > 0 ? $"{time.Minutes}{m} " : " ");
            _txtTimer.text = _txtTimer.text + (time.Seconds > 0 ? $"{time.Seconds}{s} " : " ");

            time = time.Subtract(TimeSpan.FromSeconds(1));

            yield return new WaitForSecondsRealtime(1f);
        }

        yield return StartCoroutine(InitializeDealInfo());
    }

    IEnumerator InitializeDealInfo()
    {
        yield return StartCoroutine(GameDataManager.Singleton.GetGameMoneyDealInfo());
    }

    public void SetGoods()
    {
        ComUtil.DestroyChildren(_tGoodsRoot);

        _goods = MoneyDealTable.GetList();

        for (int i = 0; i < _goods.Count; i++)
        {
            SlotShopGameMoney slot = MenuManager.Singleton.LoadComponent<SlotShopGameMoney>(_tGoodsRoot, EUIComponent.SlotShopGameMoney);
            slot.InitializeInfo(_goods[i], !GameManager.Singleton.user.m_nGameMoneyDealKey.Contains(_goods[i].PrimaryKey));
        }

        Resize();
    }

    void Resize()
    {
        for ( int i = 0; i < _tForResize.Length; i++ )
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tForResize[i]);
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_shop_gamemoneydeal_title");
        _txtDesc.text = UIStringTable.GetValue("ui_shop_gamemoneydeal_desc");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }
}
