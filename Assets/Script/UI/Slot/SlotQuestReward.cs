using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotQuestReward : MonoBehaviour
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

    PopupQuest _pop;
    PopupWait4Response _wait;

    Animator _animator;

    public enum ERewardType
    {
        Half,
        Full,
    }

    uint _key;
    ERewardType _type;
    int _count;
    bool _isLock;


    public void InitializeInfo(PopupQuest pop, uint key, ERewardType type, int count, bool isObtain = false, bool isLock = false)
    {
        _animator = GetComponent<Animator>();

        _pop = pop;
        _key = key;
        _type = type;
        _count = count;
        _isLock = isLock;

        _imgIcon.sprite = _imgFXIcon.sprite = ComUtil.GetIcon(key);
        _txtVolume.text = $"{count}";

        _goLock.SetActive(isLock);
        Array.ForEach(_goFXObtainable, go => go.SetActive(!isLock && !isObtain));
        
        _goCheckMark.SetActive(isObtain);

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
            PopupRewards();
        else
        {
            _wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
            StartCoroutine(GameDataManager.Singleton.SetQuestAccountRewardsInfo(_type, GetRewardsCallback));
        }
            
    }
    
    void GetRewardsCallback()
    {
        GameManager.Singleton.StartCoroutine(GameManager.Singleton.AddItemCS(_key, _count, AddItemCallback));

        InitializeInfo(_pop, _key, _type, _count, true, false);
        _wait.Close();

        _animator.SetTrigger("Obtain");
        FindObjectOfType<PageLobbyBattle>().CheckQuestState();
    }

    void AddItemCallback()
    {
        GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>().InitializeCurrency();
        GameManager.Singleton.RefreshInventory(GameManager.EInvenType.All);
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
