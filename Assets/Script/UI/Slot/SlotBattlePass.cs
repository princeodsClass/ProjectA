using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotBattlePass : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon, _imgFXIcon;

    [SerializeField]
    TextMeshProUGUI _txtVolume;

    [SerializeField]
    GameObject _goScrapIcon, _goLock, _goCheckMark;

    [SerializeField]
    GameObject[] _goFXObtainable;

    [SerializeField]
    GameObject[] _goFrame;

    PopupBattlePass _pop;
    BattlePassTable _pass;

    Animator _animator;

    public enum PassType
    {
        plus,
        elite,
        normal
    }

    uint _key;
    int _count;
    bool _isLock;

    PassType _type;

    public void InitializeInfo(PopupBattlePass pop, BattlePassTable pass, uint key,
                               Sprite icon, int count, int type, bool isObtain = false, bool isLock = false)
    {
        _animator = GetComponent<Animator>();

        _pop = pop;
        _pass = pass;
        _key = key;
        _count = count;
        _type = (PassType)type;
        _isLock = isLock;

        _imgIcon.sprite = _imgFXIcon.sprite = icon;
        _txtVolume.text = $"{count}";

        for (int i = 0; i < _goFrame.Length; i++)
            _goFrame[i].SetActive(i == type);

        _goLock.SetActive(isLock);
        _goCheckMark.SetActive(isObtain);

        Array.ForEach(_goFXObtainable, go => go.SetActive(!isLock && !isObtain &&
                                                          _pass.Lv <= GameManager.Singleton.user.m_nPassLevel));

        SetScrapIcon();
    }

    void SetScrapIcon()
    {
        if ( ComUtil.GetItemType(_key) == EItemType.Material )
        {
            string sd = _key.ToString("X").Substring(3, 1);

            EItemType type = (EItemType)Convert.ToInt32(sd, 16);
            _goScrapIcon.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
        }
        else
            _goScrapIcon.SetActive(false);
    }

    public void OnClick()
    {
        if ( _isLock )
        {
            PopupPurchase();
        }
        else
        {
            if ( _pass.Lv <= GameManager.Singleton.user.m_nPassLevel )
                GetRewards();
            else
                PopupRewards();
        }
    }

    void PopupPurchase()
    {
        if ( _type == PassType.elite )
            _pop.OnClickBuyElite();
        else if ( _type == PassType.plus )
            _pop.OnClickBuyPlus();
    }

    void GetRewards()
    {
        InitializeInfo(_pop, _pass, _key, _imgIcon.sprite, _count, (int)_type, true);
        StartCoroutine(RewardGet());
    }

    IEnumerator RewardGet()
    {
        yield return StartCoroutine(GameDataManager.Singleton.SetBattlePass(_pop, _pass, (int)_type));

        if (_key.ToString("X").Substring(0, 2) != "24") SetObtainAni();
    }

    public void SetObtainAni()
    {
        _animator.SetTrigger("Obtain");
    }

    void PopupRewards()
    {
        switch (_key.ToString("X").Substring(0, 2))
        {
            case "22":
                PopupMaterial ma = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
                ma.InitializeInfo(new ItemMaterial(0, _key, 0));
                break;
            case "24":
                PopupBoxNormal bo = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
                bo.InitializeInfo(new ItemBox(0, _key, 0), false, false);
                break;
        }
    }
}
