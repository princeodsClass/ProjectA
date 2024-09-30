using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComMissionDefence : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtReserve, _txtRewardsDesc, _txtLimitLevel;

    [SerializeField]
    GameObject _goCover, _goNewTag;

    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    Transform _tRewardsRoot;

    uint _unTicketKey;
    int _nReserve;

    private void Awake()
    {
        _unTicketKey = GlobalTable.GetData<uint>(ComType.G_VALUE_GLOBAL_TICKET_DEC_KEY);

        InitializeInfo();
    }

    void InitializeInfo()
    {
        _imgIcon.sprite = ComUtil.GetIcon(_unTicketKey);

        InitializeText();
        SetCover();

        CheckItemCount();
    }

    void InitializeText()
    {
        _txtTitle.text = UIStringTable.GetValue("ui_component_mission_defence_title");
        _txtRewardsDesc.text = UIStringTable.GetValue("ui_popup_adventure_toprewards");
        _txtLimitLevel.text = $"Lv.{GlobalTable.GetData<int>("valueMissionDefenceOpenLevel")}";
    }

    public void CheckItemCount()
    {
        _nReserve = Math.Max(0, GameManager.Singleton.invenMaterial.GetItemCount(_unTicketKey));
        _txtReserve.text = ComUtil.ChangeNumberFormat(_nReserve);
        _goNewTag.SetActive(IsAvailable() && _nReserve > 0);

        SetBestRewards();
    }

    void SetBestRewards()
    {
        ComUtil.DestroyChildren(_tRewardsRoot);

        MissionDefenceTable tar = MissionDefenceTable.GetDifficulty(GameManager.Singleton.user.m_nCurDefenceDifficulty).Last();
                
        List<RewardTable> rGroup = RewardTable.GetGroup(tar.RewardGroup);
        List<RewardListTable> rewards;

        Sprite sp;

        for (int i = 0; i < rGroup.Count; i++)
        {
            rewards = RewardListTable.GetGroup(rGroup[i].RewardListGroup);
            sp = ComUtil.GetIcon(rewards[0].RewardKey);

            SlotAbyssRewards rSlot = MenuManager.Singleton.LoadComponent<SlotAbyssRewards>(_tRewardsRoot, EUIComponent.SlotAbyssRewards);
            rSlot.InitializeInfo(rewards[0].RewardKey, sp, rewards[0].RewardCountMin, rewards.Count > 1);
        }
    }

    public void OnClick()
    {
        if (!IsAvailable())
        {
            PopupSysMessage pop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
            pop.InitializeInfo("ui_error_title", "ui_hint_mission_defence_desc_2", "ui_common_close", null, "TutorialDefence");
            pop.AddMessage("ui_error_notenoughlevel_defence", 2);

            return;
        }

        PopupMissionDefence popup = MenuManager.Singleton.OpenPopup<PopupMissionDefence>(EUIPopup.PopupMissionDefence);
        popup.InitializeInfo();
    }

    public void OnClickTutorial(bool isTutorial)
    {
        OnClick();
    }

    public void OnClickTicket()
    {
        PopupMaterial pop = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial);
        pop.InitializeInfo(new ItemMaterial(0, _unTicketKey, 0));
    }

    void SetCover()
    {
        _goCover.SetActive(!IsAvailable());
    }

    bool IsAvailable()
    {
        return GameManager.Singleton.user.m_nLevel >= GlobalTable.GetData<int>("valueMissionDefenceOpenLevel");
    }
}
