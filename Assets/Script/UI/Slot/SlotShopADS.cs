using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotShopADS : MonoBehaviour
{
    [SerializeField]
    GameObject[] _goCover;

    [SerializeField]
    GameObject _goMakerAD, _goMakerVIP, _goMakerFree, _goNewTag, _goScrapIcon;
    
    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtRewardCount, _txtClaimButtonCaption, _txtTimerCaption;

    [SerializeField]
    SoundButton _sb;

    ADSDealTable _deal;
    int _slotNumber;

    ComShopADS _comp;

    string D, h, m, s = string.Empty;

    public void InitializeInfo(ADSDealTable deal, int slotNumber, ComShopADS comp)
    {
        _deal = deal;
        _slotNumber = slotNumber;
        _comp = comp;

        _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, _deal.Icon);
        _txtRewardCount.text = _deal.Count.ToString();
        _txtClaimButtonCaption.text = UIStringTable.GetValue("ui_popup_battlereward_button_missclaim");

        D = $"<size=70%>{UIStringTable.GetValue("ui_day")}</size>";
        h = $"<size=70%>{UIStringTable.GetValue("ui_hour")}</size>";
        m = $"<size=70%>{UIStringTable.GetValue("ui_minute")}</size>";
        s = $"<size=70%>{UIStringTable.GetValue("ui_second")}</size>";

        _comp.AddCounterNew();
        SetScrapIcon();
        SetCover();
    }

    public void SetVIPMaker()
    {
        bool state = GameManager.Singleton.user.IsVIP();

        _goMakerVIP.SetActive(state);

        if ( !state )
        {
            uint ticket = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
            int count = GameManager.Singleton.invenMaterial.GetItemCount(ticket);

            _goMakerFree.SetActive(count > 0);
            _goMakerAD.SetActive(!_goMakerFree.activeSelf);
        }
        else
        {
            _goMakerAD.SetActive(false);
            _goMakerFree.SetActive(false);
        }
    }

    void SetCover()
    {
        SetVIPMaker();

        int buyCount = GameManager.Singleton.user.m_nBuyADSDeal[_slotNumber];
        TimeSpan time = GameManager.Singleton.eventController._eADSDeal.CheckRemainTime();

        if ( buyCount > 0 )
        {
            if (_deal.LimitCount <= buyCount)
            {
                ChangeCoverState(true);
                StartCoroutine(SetTimerText(time));
            }
            else
            {
                DateTime tar = GameManager.Singleton.user.m_dtADSDealSlot[_slotNumber].AddMilliseconds(_deal.TimeRepurchase);

                if (tar > DateTime.UtcNow)
                {
                    ChangeCoverState(true);
                    time = time < (tar - DateTime.UtcNow) ? time : tar - DateTime.UtcNow;
                    StartCoroutine(SetTimerText(time));
                }
                else
                {
                    ChangeCoverState(false);
                }
            }
        }
        else
        {
            ChangeCoverState(false);
        }
    }

    void SetScrapIcon()
    {
        if (ComUtil.GetItemType(_deal.ItemKey) == EItemType.Material)
        {
            string sd = _deal.ItemKey.ToString("X").Substring(3, 1);

            EItemType type = (EItemType)Convert.ToInt32(sd, 16);
            _goScrapIcon.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
        }
        else
            _goScrapIcon.SetActive(false);
    }

    public void OnClick()
    {
#if UNITY_EDITOR
        StartCoroutine(GetItem());
#else
        C3rdPartySDKManager.Singleton.ShowRewardAds(ResultADS);
#endif 
    }

    void ResultADS(CAdsManager cAdsManager, CAdsManager.STAdsRewardInfo sTAdsRewardInfo, bool result)
    {
        if (result)
        {
            StartCoroutine(GetItem());
            GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.ADSRewards));
        }
        else
        {
            GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.ADSRewards, false));
        }
    }

    IEnumerator GetItem()
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        yield return GameManager.Singleton.AddItemCS(_deal.ItemKey, _deal.Count);
        yield return StartCoroutine(GameDataManager.Singleton.SetADSDealInfo(_slotNumber, _deal.PrimaryKey));

        GameManager.Singleton.user.m_dtADSDealSlot[_slotNumber] = DateTime.UtcNow;
        GameManager.Singleton.user.m_nBuyADSDeal[_slotNumber]++;
        
        SetCover();

        FindObjectOfType<PageLobbyInventory>().InitializeInventory();
        FindObjectOfType<PageLobbyTop>().InitializeCurrency();

        wait.Close();
    }

    void ChangeCoverState(bool state)
    {
        _goCover.ToList().ForEach(cover => cover.SetActive(state));
        _goNewTag.SetActive(!state); // && ( _goMakerVIP.activeSelf || _goMakerFree.activeSelf ));
    }

    IEnumerator SetTimerText(TimeSpan time)
    {
        _sb.interactable = false;
        _comp.AddCounterNew(-1);

        while (time.TotalMilliseconds > 0f)
        {
            _txtTimerCaption.text = string.Empty;

            if (time.TotalDays > 1)
            {
                _txtTimerCaption.text = $"{time.Days}{D}";
                _txtTimerCaption.text = _txtTimerCaption.text + (time.Hours > 0 ? $"{time.Hours}{h}" : "");

                time = time.Subtract(TimeSpan.FromHours(1));

                yield return new WaitForSecondsRealtime(3600f);
            }
            else if (time.TotalHours > 1)
            {
                _txtTimerCaption.text = $"{time.Hours}{h}";
                _txtTimerCaption.text = _txtTimerCaption.text + (time.Minutes > 0 ? $"{time.Minutes}{m}" : "");

                time = time.Subtract(TimeSpan.FromMinutes(1));

                yield return new WaitForSecondsRealtime(60f);
            }
            else
            {
                _txtTimerCaption.text = _txtTimerCaption.text + (time.Minutes > 0 ? $"{time.Minutes}{m}" : "");
                _txtTimerCaption.text = _txtTimerCaption.text + (time.Seconds > 0 ? $"{time.Seconds}{s}" : "");

                time = time.Subtract(TimeSpan.FromSeconds(1));

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        _sb.interactable = true;
        SetCover();
    }

    private void OnEnable()
    {
        ChangeCoverState(false);
    }
}
