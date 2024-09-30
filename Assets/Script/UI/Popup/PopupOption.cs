using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupOption : UIDialog
{
    [SerializeField]
    GameObject _goButtonLanguageApply, _goButtonAccount, _goButtonDiscode,
               _goAccount, _goAuid;
    
    [SerializeField]
    TextMeshProUGUI _txtTitle,
                    _txtBGCaption, _txtSFXCaption,
                    _txtAntiAliasCaption, _txtVibration, _txtVPad, _txtAlarm, _txtNightAlarm,
                    _txtLanguageLabel, _txtLanguageApply,
                    _txtButtonCaptionAccount, _txtButtonCaptionDiscode,
                    _txtAccount,
                    _txtVersionValue, _txtAuid;

    [SerializeField]
    Slider _sBGMSlider, _sSFXSlider;

    [SerializeField]
    SoundToggle _tAntiAliasing, _tVibration, _tVPad, _tNoti, _tNightNoti;

    [SerializeField]
    TMP_Dropdown _ddLanguage;

    [SerializeField]
    RectTransform[] _rt4Resize;

    OptionData _optionData;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        InitializeText();
        InitializeInfo();
        InitializeAccount();
    }

    void InitializeInfo()
    {
        _optionData = GameManager.Singleton.GetOption();

        InitializeSlider();
        InitializeToggle();
        InitializeLanguage();
        InitializeAccount();

        StartCoroutine(Resize());
    }

    public void InitializeAccount()
    {
        _goAccount.SetActive(m_Account.m_strNickname != ComType.DEFAULT_NICKNAME);
        _txtAccount.text = m_Account.m_strNickname;

        InitializeText();
    }

    void InitializeSlider()
    {
        _sBGMSlider.value = _optionData.m_Audio.fBGM_Vol;
        _sSFXSlider.value = _optionData.m_Audio.fSFX_Vol;
    }

    void InitializeToggle()
    {
        _tAntiAliasing.isOn = _optionData.m_Game.bAntiAliasing;
        _tVibration.isOn = _optionData.m_Game.bVibration;
        _tVPad.isOn = _optionData.m_Game.bViewJoystic;
        _tNoti.isOn = _optionData.m_Notice.bPush;
        _tNightNoti.isOn = _optionData.m_Notice.bNightPush;
    }

    void InitializeLanguage()
    {
        _goButtonLanguageApply.SetActive(false);
        _ddLanguage.ClearOptions();

        List<string> language = new List<string>();

        for (int i = 1; i < (int)ELanguage.END; i++)
        {
            //if ( Enum.IsDefined(typeof(ELanguage), i) )
            {
                string item = UIStringTable.GetValue($"ui_popup_option_language_{((ELanguage)i).ToString().ToLower()}");
                language.Add(item);
            }
        }

        _ddLanguage.AddOptions(language);
        _ddLanguage.onValueChanged.AddListener(SetLanguage);
        _ddLanguage.value = PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT) - 1;
    }

    void SetLanguage(int index)
    {
        _goButtonLanguageApply.SetActive(PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT) != index + 1);
    }

    public void ReStart(bool isLanguage = false)
    {
        if ( isLanguage )
        {
            PopupDefault pop = m_MenuMgr.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
            pop.SetTitle("ui_attention");
            pop.SetMessage("ui_hint_change_language");
            pop.SetButtonText("ui_popup_button_next", "ui_popup_button_later", () =>
            {
                ComUtil.SetLanguage(_ddLanguage.value + 1);
                m_GameMgr.Restart();
            }, InitializeLanguage);
        }
        else
        {
            m_GameMgr.Restart();
        }
    }

    public void ResetAccount()
    {
        PlayerPrefs.DeleteAll();

        m_GameMgr.Restart();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_popup_option_title");
        _txtBGCaption.text = UIStringTable.GetValue("ui_popup_option_bgm_caption");
        _txtSFXCaption.text = UIStringTable.GetValue("ui_popup_option_sfx_caption");
        _txtAntiAliasCaption.text = UIStringTable.GetValue("ui_popup_option_antialias_caption");
        _txtVibration.text = UIStringTable.GetValue("ui_popup_option_vibration_caption");
        _txtVPad.text = UIStringTable.GetValue("ui_popup_option_vpad_caption");
        _txtAlarm.text = UIStringTable.GetValue("ui_popup_option_alarm_caption");
        _txtNightAlarm.text = UIStringTable.GetValue("ui_popup_option_nightalarm_caption");

        _txtLanguageLabel.text = UIStringTable.GetValue("ui_popup_option_language_label");
        _txtLanguageApply.text = UIStringTable.GetValue("ui_apply");

        _txtButtonCaptionAccount.text = _goAccount.activeSelf ?
                                        UIStringTable.GetValue("ui_popup_option_unlink_caption") :
                                        UIStringTable.GetValue("ui_popup_option_account_caption");
        _txtVersionValue.text = $"{UIStringTable.GetValue("ui_version")} : <color=yellow>{Application.version}</color>";

#if DEBUG || UNITY_EDITOR
        _goAuid.SetActive(true);
        _txtAuid.text = $"server : <color=yellow>{GameDataTableManager.m_URL}</color> \n AUID : <color=yellow>{PlayerPrefs.GetInt(ComType.STORAGE_UID)}</color>";
#else
        _goAuid.SetActive(false);
#endif
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        Initialize();
        //m_MenuMgr.ShowPopupDimmed(true);
    }

    public override void Close()
    {
        base.Close();

        if ( m_MenuMgr.CurScene == ESceneType.Battle )
            GameObject.Find("Joystick").GetComponent<FloatingJoystick>().SetView(_tVPad.isOn);
    }

    public override void Escape()
    {
        base.Escape();
    }

    private void OnDisable()
    {
        SaveOption();
    }

    public void ChangeAntialiasing()
    {
        _optionData.m_Game.bAntiAliasing = _tAntiAliasing.isOn;
        // SaveOption();
    }

    public void ChangeVibration()
    {
        _optionData.m_Game.bVibration = _tVibration.isOn;
        // SaveOption();
    }

    public void ChangeVPad()
    {
        _optionData.m_Game.bViewJoystic = _tVPad.isOn;
        // SaveOption();
    }

    public void ChangeNoti()
    {
        _optionData.m_Notice.bPush = _tNoti.isOn;
        // SaveOption();
    }

    public void ChangeNightNoti()
    {
        _optionData.m_Notice.bNightPush = _tNightNoti.isOn;
        // SaveOption();
    }

    public void ChangeBGMValue()
    {
        _optionData.m_Audio.fBGM_Vol = _sBGMSlider.value;
        m_AudioMgr.SetOption(_optionData.m_Audio);
        // SaveOption();
    }

    public void ChangeSFXValue()
    {
        _optionData.m_Audio.fSFX_Vol = _sSFXSlider.value;
        m_AudioMgr.SetOption(_optionData.m_Audio);
        // SaveOption();
    }

    public void OnClickAccount()
    {
        PopupInputMail m = m_MenuMgr.OpenPopup<PopupInputMail>(EUIPopup.PopupInputMail, true);
        m.InitializeInfo(this);
    }

    IEnumerator Resize()
    {
        yield return new WaitForEndOfFrame();

        Array.ForEach(_rt4Resize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

    void SaveOption()
    {
        m_GameMgr.SaveOption();
    }

    public void OnClickClose()
    {
        Close();
    }

    public void OnClickDiscord()
    {
        Application.OpenURL(GlobalTable.GetData<string>("urlDiscord"));
    }

    public void OnClickTestAds()
    {

    }

}
