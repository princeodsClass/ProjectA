using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComShopDailyDeal : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtTimerTitle, _txtTimer;

    [SerializeField]
    GameObject[] _goSlotRoot, _goSlotCover;

    [SerializeField]
    Image[] _iconPrice;

    [SerializeField]
    TextMeshProUGUI[] _txtPrice;

    List<DailyDealTable> _item;

    bool isNewList = false;
    string D, h, m, s = string.Empty;

    private void Awake()
    {
        InitializeText();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_shop_dailydeal_title");
        _txtTimerTitle.text = UIStringTable.GetValue("ui_shop_resettime");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";
    }

    public void Initialize()
    {
        _item = new List<DailyDealTable>();

        StartCoroutine(SetGoods());
    }

    public void SetTimer()
    {
        StartCoroutine(SetTimerText());
    }

    IEnumerator SetTimerText()
    {
        TimeSpan time = GameManager.Singleton.eventController._eDailyDeal.CheckRemainTime();

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

        Initialize();

        yield break;
    }

    IEnumerator SetGoods()
    {
        string type = string.Empty;

        SetGoodsList();

        yield return StartCoroutine(GameDataManager.Singleton.GetDailyDealInfo());

        for (int i = 0; i < _item.Count; i++)
        {
            ComUtil.DestroyChildren(_goSlotRoot[i].transform);

            if (null != _item[i])
            {
                type = _item[i].ItemKey.ToString("X").Substring(0, 2);

                switch (type)
                {
                    case "20": SetWeapon(i); break;
                    case "23":
                    case "11":
                    case "22": SetMaterial(i); break;
                }
            }

            SetCover(i, GameManager.Singleton.user.m_bBuyDailyDeal[i]);
        }
       
        SetTimer();

        isNewList = false;
    }

    void SetGoodsList()
    {
        DateTime now = DateTime.UtcNow;
        DateTime preTime = GameManager.Singleton.user.m_dtDailyDealReset;

        DateTime targetTime = now.AddDays(1);
        targetTime = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, 0, 0, 0);

        if (targetTime.AddDays(-1) > preTime)
            CreateGoodsList(4);
        else
            GetGoodsList();
    }

    public void SetCover(int slotNumber, bool state)
    {
        _goSlotCover[slotNumber].SetActive(state);
    }

    void SetWeapon(int slotNumber)
    {
        ItemWeapon weapon = new ItemWeapon();
        weapon.Initialize(0, _item[slotNumber].ItemKey, 1, 0, 0, 0, false);

        SlotWeapon w = MenuManager.Singleton.LoadComponent<SlotWeapon>(_goSlotRoot[slotNumber].transform, EUIComponent.SlotWeapon);
        w.Initialize(weapon, true, SlotWeapon.EPresentType.origin, _item[slotNumber].PrimaryKey);
        w.SetState(SlotWeapon.SlotState.goods);
        w.SetSlotNumber(slotNumber);

        SetPrice(slotNumber);
    }

    void SetMaterial(int slotNumber)
    {
        ItemMaterial material = new ItemMaterial();
        material.Initialize(0, _item[slotNumber].ItemKey, _item[slotNumber].Count);

        SlotMaterial m = MenuManager.Singleton.LoadComponent<SlotMaterial>(_goSlotRoot[slotNumber].transform, EUIComponent.SlotMaterial);
        m.Initialize(material, true, true, true, SlotMaterial.EVolumeType.value, SlotMaterial.SlotState.goods, _item[slotNumber].PrimaryKey);
        m.SetSlotNumber(slotNumber);

        SetPrice(slotNumber);
    }

    void SetPrice(int slotNumber)
    {
        _iconPrice[slotNumber].sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(_item[slotNumber].CostItemKey).Icon);
        _txtPrice[slotNumber].text = _item[slotNumber].CostItemCount.ToString();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(_txtPrice[slotNumber].gameObject.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_iconPrice[slotNumber].transform.parent.gameObject.GetComponent<RectTransform>());

        
    }

    void GetGoodsList()
    {
        DailyDealTable d;
        uint t;

        for (int i = 0; i < 4; i++)
        {
            t = GameManager.Singleton.user.m_nDailyDealKey[i];
            d = t == 0 ? null : DailyDealTable.GetData(GameManager.Singleton.user.m_nDailyDealKey[i]);
            _item.Add(d);
        }
    }

    void CreateGoodsList(int count)
    {
        List<DailyDealTable> all = new List<DailyDealTable>();
        List<DailyDealTable> add = new List<DailyDealTable>();
        List<DailyDealTable> del = new List<DailyDealTable>();

        all = DailyDealTable.GetList(GameManager.Singleton.user.m_nLevel);
        all.ForEach( t => { if ( t.SelectionType == 1 ) { _item.Add(t); del.Add(t); } } );

        del.ForEach( t => all.Remove(t) );

        add = DailyDealTable.GetDistinctRandomElements(all, count - _item.Count);
        add.ForEach(t => { _item.Add(t); });

        for ( int i = 0; i < _item.Count; i++ )
            PlayerPrefs.SetString($"{ComType.SHOP_DAILY_GOODS_LIST}[{i}]", _item[i].PrimaryKey.ToString());

        StartCoroutine(GameManager.Singleton.eventController._eDailyDeal.RecodeEvent());

        isNewList = true;
    }

    void DeleteGoods()
    {
        for (int i = 0; i < _goSlotRoot.Length; i++)
        {
            ComUtil.DestroyChildren(_goSlotRoot[i].transform);
            PlayerPrefs.SetString($"{ComType.SHOP_DAILY_GOODS_LIST}[{i}]", "0");
        }
    }

    private void OnEnable()
    {
        DeleteGoods();
        Initialize();
    }
}
