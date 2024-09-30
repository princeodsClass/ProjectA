using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopADS : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtTicketCount;

    [SerializeField]
    GameObject _goDesc;

    [SerializeField]
    Transform _tSlotRoot;

    [SerializeField]
    RectTransform[] _tForResize;

    List<ADSDealTable> _item;
    List<SlotShopADS> _liSlot;

    PageLobbyShop _pageLobbyShop;
    uint _ticket;
    int _counterNew;

    Coroutine _coTicket;

    private void OnEnable()
    {
        _counterNew = 0;
        _pageLobbyShop = GameObject.Find("ShopPage").GetComponent<PageLobbyShop>();
        _ticket = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");

        InitializeText();
        StartCoroutine(InitializeDealInfo());

        _coTicket = StartCoroutine(CheckTicket());

        Resize();
    }

    IEnumerator CheckTicket()
    {
        WaitForSecondsRealtime w = new WaitForSecondsRealtime(1f);

        while ( true )
        {
            Resize();

            _txtTicketCount.text = $"x {GameManager.Singleton.invenMaterial.GetItemCount(_ticket)}";

            _goDesc.SetActive(!GameManager.Singleton.user.IsVIP());

            /*
            _imgIcon.gameObject.SetActive(!GameManager.Singleton.user.IsVIP());
            _txtTicketCount.gameObject.SetActive(!GameManager.Singleton.user.IsVIP());
            _txtDesc.transform.parent.gameObject.SetActive(!GameManager.Singleton.user.IsVIP());
            */

            yield return w;
        }
    }

    void Initialize()
    {
        _item = new List<ADSDealTable>();
        _liSlot = new List<SlotShopADS>();

        _imgIcon.sprite = ComUtil.GetIcon(_ticket);

        SetGoods();
    }

    public void SetSlotMaker()
    {
        _liSlot?.ForEach(e => e.SetVIPMaker());
    }

    void SetGoods()
    {
        SetGoodsList();

        ComUtil.DestroyChildren(_tSlotRoot);

        for (int i = 0; i < _item.Count; i++)
        {
            if (null != _item[i])
            {
                SlotShopADS ads = MenuManager.Singleton.LoadComponent<SlotShopADS>(_tSlotRoot, EUIComponent.SlotShopADS);
                ads.InitializeInfo(_item[i], i, this);
                _liSlot.Add(ads);
            }
        }

        StartCoroutine(SetTimer());
    }

    IEnumerator SetTimer()
    {
        TimeSpan time = GameManager.Singleton.eventController._eDailyDeal.CheckRemainTime();

        yield return new WaitForSecondsRealtime((float)time.TotalSeconds);
        yield return StartCoroutine(GameDataManager.Singleton.GetADSDealInfo());

        Initialize();
    }

    public void AddCounterNew(int count = 1)
    {
        _counterNew += count;
        _pageLobbyShop.AddCounterNew(count);
    }

    void SetGoodsList()
    {
        DateTime now = DateTime.UtcNow;
        DateTime preTime = GameManager.Singleton.user.m_dtADSDealReset;

        DateTime targetTime = now.AddDays(1);
        targetTime = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, 0, 0, 0);

        if (targetTime.AddDays(-1) > preTime)
            CreateGoodsList(4);
        else
            GetGoodsList();
    }

    void CreateGoodsList(int count)
    {
        List<ADSDealTable> all = new List<ADSDealTable>();
        List<ADSDealTable> add = new List<ADSDealTable>();
        List<ADSDealTable> del = new List<ADSDealTable>();

        all = ADSDealTable.GetList(GameManager.Singleton.user.m_nLevel);
        all.ForEach(t => { if (t.SelectionType == 1) { _item.Add(t); del.Add(t); } });

        del.ForEach(t => all.Remove(t));

        add = ADSDealTable.GetDistinctRandomElements(all, count - _item.Count);
        add.ForEach(t => { _item.Add(t); });

        for (int i = 0; i < _item.Count; i++)
            GameManager.Singleton.user.m_nADSDealKey[i] = _item[i].PrimaryKey;

        StartCoroutine(GameManager.Singleton.eventController._eADSDeal.RecodeEvent());
    }

    void GetGoodsList()
    {
        ADSDealTable ad;
        uint t;

        for (int i = 0; i < 4; i++)
        {
            t = GameManager.Singleton.user.m_nADSDealKey[i];
            ad = t == 0 ? null : ADSDealTable.GetData(GameManager.Singleton.user.m_nADSDealKey[i]);
            _item.Add(ad);
        }
    }

    IEnumerator InitializeDealInfo()
    {
        yield return StartCoroutine(GameDataManager.Singleton.GetADSDealInfo());

        Initialize();
    }

    void Resize()
    {
        for ( int i = 0; i < _tForResize.Length; i++ )
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tForResize[i]);
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_component_ads_title");
        _txtDesc.text = UIStringTable.GetValue("ui_hint_shop_ads");
    }

    public void OnClickTicket()
    {
        PopupMaterial pop = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial);
        pop.InitializeInfo(new ItemMaterial(0, _ticket, 0));
    }
}
