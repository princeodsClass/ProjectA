using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupBoxNormal : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtDesc, _txtOpenTime, _txtOpenMaterialCount,
                    _txtRewardType, _txtButtonOpenAd, _txtTicketCount;

    [SerializeField]
    Dictionary<Image, TextMeshProUGUI> _dicRewards;

    [SerializeField]
    GameObject _goButtonOpenStart, _goButtonOpenImmediately, _goButtonOpenAd,
               _goMarkerAD, _goMarkerVIP, _goMarkerFree, _goMarkerTicket;

    [SerializeField]
    Transform _tRewardListRoot;

    [SerializeField]
    RectTransform[] _rtResizeTarget;

    [SerializeField]
    Image _imgBoxIcon, _imgTicketIcon;

    List<GameObject> slot = new List<GameObject>();
    ItemBox _box;
    BoxTable _boxData;

    uint _ticket;
    int _nOpenMaterialCount;
    string h, m, s;

    private void Awake()
    {
        Initialize();

        _ticket = GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey");
        _imgTicketIcon.sprite = ComUtil.GetIcon(_ticket);

        _txtDesc.text = UIStringTable.GetValue("ui_popup_boxnormal_desc");
        _txtRewardType.text = UIStringTable.GetValue("ui_popup_boxnormal_reward_type");

        h = UIStringTable.GetValue("ui_hour");
        m = UIStringTable.GetValue("ui_minute");
        s = UIStringTable.GetValue("ui_second");
    }

    void DisplayRewardsOutline()
    {
        List<RewardTable> rewardsOutline = new List<RewardTable>();
        rewardsOutline = RewardTable.GetGroup(_box.nRewardGroup);

        EAtlasType eAtlas = EAtlasType.Icons;

        foreach (GameObject obj in slot)
            Destroy(obj);

        slot.Clear();

        for (int i = 0; i < rewardsOutline.Count; i++)
        {
            SlotSimpleItem slotSimpleItem = m_MenuMgr.LoadComponent<SlotSimpleItem>(_tRewardListRoot, EUIComponent.SlotSimpleItem);
            slot.Add(slotSimpleItem.gameObject);

            // bool isRandom = ( rewardsOutline.Count > 1 && rewardsOutline[i].SelectionType == 0 ) ||
            //                RewardListTable.GetGroup(rewardsOutline[i].RewardListGroup).Count > 1;

           bool isRandom = RewardListTable.GetGroup(rewardsOutline[i].RewardListGroup).Count > 1;

           Sprite sprite = m_ResourceMgr.LoadSprite(eAtlas, rewardsOutline[i].Icon);
           slotSimpleItem.Initialize(rewardsOutline[i].ItemKey, sprite, rewardsOutline[i].MinCount, rewardsOutline[i].MaxCount, true, isRandom);
        }

        StartCoroutine(ReSize());
    }

    IEnumerator ReSize()
    {
        for ( int i = 0; i < _rtResizeTarget.Length; i++ )
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rtResizeTarget[i]);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rtResizeTarget[i].GetComponentInParent<RectTransform>());
        }

        SetMaker();
    }

    void SetMaker()
    {
        if (m_Account.IsVIP())
        {
            _goMarkerVIP.SetActive(true);
            _goMarkerAD.SetActive(false);
            _goMarkerFree.SetActive(false);
            _goMarkerTicket.SetActive(false);
        }
        else
        {
            _goMarkerVIP.SetActive(false);
            _goMarkerFree.SetActive(false);
            _goMarkerTicket.SetActive(int.Parse(_txtTicketCount.text) > 0);
            _goMarkerAD.SetActive(!_goMarkerTicket.activeSelf);
        }
    }

    public void OnClickTicket()
    {
        PopupMaterial pop = m_MenuMgr.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
        pop.InitializeInfo(new ItemMaterial(0, _ticket, 0));
    }

    public void InitializeInfo(ItemBox box, bool state, bool isButton = true, bool isTutorial = false)
    {
        _box = box;
        _boxData = BoxTable.GetData(_box.nKey);
        _txtTitle.text = $"{box.strName}"; // [ {UIStringTable.GetValue("ui_level")} : {box.nLevel} ]";
        _txtTicketCount.text = m_InvenMaterial.GetItemCount(_ticket).ToString();

        TimeSpan tempRemainTime;

        tempRemainTime = CalculateRemaintime(box);

        _txtOpenTime.text = (tempRemainTime.Hours > 0 ? $"{tempRemainTime.Hours}{h}" : "");
        _txtOpenTime.text = _txtOpenTime.text + (tempRemainTime.Minutes > 0 ? $" {tempRemainTime.Minutes}{m}" : "");
        _txtOpenTime.text = _txtOpenTime.text + (tempRemainTime.Seconds > 0 ? $" {tempRemainTime.Seconds}{s}" : "");

        if ( isButton )
        {
            if ( m_GameMgr.IsExistOpeningBox() > -1 )
            {
                _goButtonOpenStart.SetActive(false);
                _goButtonOpenImmediately.SetActive(true);
            }
            else
            {
                _goButtonOpenStart.SetActive(_boxData.OpenTime > 1);
                _goButtonOpenImmediately.SetActive(_boxData.AbleToADOpen != 1);
            }

            _goButtonOpenAd.SetActive(_boxData.AbleToADOpen == 1);

            _nOpenMaterialCount = Mathf.CeilToInt((float)(tempRemainTime / TimeSpan.FromMilliseconds(box.nOpenDatetime) * box.nOpenMaterialCount));
            _txtOpenMaterialCount.text = _nOpenMaterialCount == 0 ? UIStringTable.GetValue("ui_paytype_free") :
                                                                    _nOpenMaterialCount.ToString();
            _txtButtonOpenAd.text = UIStringTable.GetValue("ui_popup_battlereward_button_boxclaim_ad");
        }
        else
        {
            _goButtonOpenStart.SetActive(false);
            _goButtonOpenAd.SetActive(false);
            _goButtonOpenImmediately.SetActive(false);
        }

        if ( isTutorial )
        {
            Rect rt = _goButtonOpenImmediately.GetComponent<RectTransform>().rect;
            m_Tutorial.SetFinger(_goButtonOpenImmediately, OpenImmediately, rt.width, rt.height, 500);
        }

        _imgBoxIcon.sprite = _box.GetIcon();

        DisplayRewardsOutline();
    }

    TimeSpan CalculateRemaintime(ItemBox box)
    {
        if (box._openStartDatetime == DateTime.MinValue)
        {
            return TimeSpan.FromMilliseconds(box.nOpenDatetime);
        }
        else
        {
            return box._openStartDatetime.AddMilliseconds(box.nOpenDatetime) - DateTime.UtcNow;
        }
    }

    public void OpenStart()
    {
        StartCoroutine(SetStart());
    }

    IEnumerator SetStart()
    {
        PopupWait4Response wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        Dictionary<string, string> fields = new Dictionary<string, string>
        {
            { "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
            { "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
            { "table", EDatabaseType.box.ToString() },
            { "id", _box.id.ToString() },
            { "openDatetime", ComUtil.EnUTC() },
        };

        yield return StartCoroutine(m_DataMgr.ModifyItem(fields));

        if (m_GameMgr.GetOption().m_Notice.bPush)
        {
            DateTime tar = _box._openStartDatetime.AddMilliseconds(_box.nOpenDatetime).ToLocalTime();
            bool t = m_GameMgr.GetOption().m_Notice.bNightPush ||
                     (tar.TimeOfDay < new TimeSpan(21, 0, 0) && tar.TimeOfDay > new TimeSpan(6, 0, 0));

            if (t)
                C3rdPartySDKManager.Singleton.AddNoti(UIStringTable.GetValue("noti_box_open_title"),
                                    UIStringTable.GetValue("noti_box_open_desc"),
                                    tar,
                                    (int)_box.id);
        }

        wait.Close();

        if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
            GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().CheckBoxState();

        Close();
    }

    public void OpenAD()
    {
#if UNITY_EDITOR
        OpenImmediately();
#else
        C3rdPartySDKManager.Singleton.ShowRewardAds(ResultADS);
#endif 
    }

    void ResultADS(CAdsManager cAdsManager, CAdsManager.STAdsRewardInfo sTAdsRewardInfo, bool result)
    {
        if (result)
        {
            OpenImmediately();
            GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.BoxOpen));
        }
        else
        {
            GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ADSView(EADSViewType.BoxOpen, false));
        }
    }

    public void OpenCrystal()
    {
        StartCoroutine(OpenBox());
    }

    IEnumerator OpenBox()
    {
        CalculateRemaintime(_box);

        if (m_GameMgr.invenMaterial.CalcTotalCrystal() >= _nOpenMaterialCount )
        {
            yield return StartCoroutine(m_InvenMaterial.ConsumeCrystal(_nOpenMaterialCount));

            OpenImmediately();
        }
        else
        {
            PopupShopCrystal ct = MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);

            yield break;
        }

        Close();
    }

    public void OpenImmediately(bool isTutorial = false)
    {
        PopupBoxReward popupReward = m_MenuMgr.OpenPopup<PopupBoxReward>(EUIPopup.PopupBoxReward);
        
        if ( isTutorial )
        {
            popupReward.InitializeInfo(_box, true, false, 1, PopupBoxReward.EBoxType.normal, null, TutorialInventory);
        }
        else
            popupReward.InitializeInfo(_box);

		C3rdPartySDKManager.Singleton.RemoveNoti((int)_box.id);
    }

    void TutorialInventory(PopupBoxReward sender)
    {
        PopupDefault pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault);
        pop.SetMessage("ui_hint_reinforce_desc");
        pop.SetButtonText("ui_popup_button_experience", "ui_popup_boxreward_button_skip", () =>
        {
            GameObject go = GameObject.Find("InvenDisableBtn");
            RectTransform rt = go.GetComponent<RectTransform>();
            m_GameMgr.tutorial.SetFinger(go, MoveToInventory, rt.rect.width, rt.rect.height);
        }, null, "TutorialReinforce");
    }

    void MoveToInventory(bool isTutorial)
    {
        GameObject.Find("PageLobby").GetComponent<PageLobby>().OnButtonMenuClickEquipTutorial();
        //m_GameMgr.tutorial.Activate(false);
    }

    private void OnEnable()
    {
        m_Tutorial.Activate(false);
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
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }
}
