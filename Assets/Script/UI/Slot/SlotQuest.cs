using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotQuest : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtGauge, _txtButtonCaption, _txtCoverCaption, _txtButtonCoverCaption;

    [SerializeField]
    Transform _tRewardNormalRoot, _tRewardVIPRoot;

    [SerializeField]
    GameObject _goVIPBG, _goNormalBG, _goButtonDisableBG, _goCompleteCover;

    [SerializeField]
    Slider _slGauge;

    PopupQuest _pop;
    QuestTable _quest;
    int _order, _count;
    bool _isVIPQuest;

    public void InitializeInfo(PopupQuest pop, QuestTable quest, int order, int count = 0, bool isComplete = false)
    {
        _pop = pop;
        _quest = quest;
        _order = order;
        _count = count;

        _isVIPQuest = order == 4 || order == 5;

        _txtCoverCaption.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");
        _txtButtonCoverCaption.text = UIStringTable.GetValue("ui_popup_battlereward_titleresult_success");
        _txtTitle.text = $"{NameTable.GetValue(quest.TitleKey)} ({(int)((float)quest.Point * (_isVIPQuest ? GlobalTable.GetData<float>("ratioVIPQuestPoint") : 1f))} p )";

        _txtGauge.text = count == quest.RequireCount ?
                         UIStringTable.GetValue("ui_popup_battlereward_titleresult_success") :
                         $"{count} / {quest.RequireCount}";
        _slGauge.value = (float)count / (float)quest.RequireCount;
        _goCompleteCover.SetActive(isComplete);

        _goVIPBG.SetActive(_isVIPQuest);
        _goNormalBG.SetActive(!_isVIPQuest);

        SetButtonState();
        SetRewards(quest);
    }

    void SetButtonState()
    {
        if ( GameManager.Singleton.user.IsVIP() )
        {
            _goButtonDisableBG.SetActive(_count < _quest.RequireCount);
            _txtButtonCaption.text = _count < _quest.RequireCount ?
                                     UIStringTable.GetValue("ui_move") :
                                     UIStringTable.GetValue("ui_popup_battlereward_button_boxconfiem");
        }
        else
        {
            _goButtonDisableBG.SetActive(_isVIPQuest ? false : _count < _quest.RequireCount);
            _txtButtonCaption.text = _isVIPQuest ?
                                     UIStringTable.GetValue("ui_component_vip_title") :
                                        _count < _quest.RequireCount ?
                                        UIStringTable.GetValue("ui_move") :
                                        UIStringTable.GetValue("ui_popup_battlereward_button_boxconfiem");
        }
    }

    void SetRewards(QuestTable quest)
    {
        ComUtil.DestroyChildren(_tRewardNormalRoot);
        ComUtil.DestroyChildren(_tRewardVIPRoot);

        SlotAbyssRewards normalSlot = MenuManager.Singleton.LoadComponent<SlotAbyssRewards>(_tRewardNormalRoot, EUIComponent.SlotAbyssRewards);
        normalSlot.InitializeInfo(quest.RewardsKey00, ComUtil.GetIcon(quest.RewardsKey00), quest.RewardsCount00);

        SlotAbyssRewards vipSlot = MenuManager.Singleton.LoadComponent<SlotAbyssRewards>(_tRewardVIPRoot, EUIComponent.SlotAbyssRewards);
        vipSlot.InitializeInfo(quest.RewardsKey01, ComUtil.GetIcon(quest.RewardsKey01), quest.RewardsCount01);
    }

    public void OnClickClaim()
    {
        if ( _count < _quest.RequireCount )
            if ( GameManager.Singleton.user.IsVIP() )
                QuestIsNotComplete();
            else
                if (_isVIPQuest)
                    PopupVIP();
                else
                    QuestIsNotComplete();
        else
            if ( GameManager.Singleton.user.IsVIP() )
                ClaimRewards();
            else
                if (_isVIPQuest)
                    PopupVIP();
                else
                    ClaimRewards();
    }

    void PopupVIP()
    {
        MenuManager.Singleton.OpenPopup<PopupShopVIP>(EUIPopup.PopupShopVIP, true);
    }

    void QuestIsNotComplete()
    {
        PopupDefault pop = MenuManager.Singleton.OpenPopup<PopupDefault>(EUIPopup.PopupDefault, true);
        pop.SetTitle("ui_error_title");
        pop.SetMessage("ui_error_quest_notcomplete");
        pop.SetButtonText("ui_move", "ui_common_close", MoveControl);
    }

    void ClaimRewards()
    {
        PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

        StartCoroutine(GameDataManager.Singleton.CompleteQuest(GameManager.Singleton.user.m_nQuestID[_order], _order, () =>
        {
            GameManager.Singleton.user.m_bQuestIsComplete[_order] = true;
            InitializeInfo(_pop, _quest, _order, _quest.RequireCount, true);
            _pop.InitializeInfo();

            FindObjectOfType<PageLobbyBattle>().CheckQuestState();

            wait.Close();
        }));
    }

    void MoveControl()
    {
        PageLobby lobby = FindObjectOfType<PageLobby>();

        switch ( (EQuestActionType)_quest.Action )
        {
            case EQuestActionType.ClearAbyss:
                lobby.OnButtonMenuClick(4);
                FindObjectOfType<ComCommunityAbyss>().OnClick();
                break;
            case EQuestActionType.ClearAdventure:
                lobby.OnButtonMenuClick(3);
                FindObjectOfType<PageLobbyMission>().OnTouchAdventureBtn();
                break;
            case EQuestActionType.ClearDefence:
                lobby.OnButtonMenuClick(3);
                FindObjectOfType<ComMissionDefence>().OnClick();
                break;
            case EQuestActionType.CompleteCraft:
                lobby.OnButtonMenuClick(2);
                FindObjectOfType<PageLobbyInventory>().OnClickWorkshop();
                break;
            case EQuestActionType.CompleteRecycle:
                lobby.OnButtonMenuClick(2);
                FindObjectOfType<PageLobbyInventory>().OnClickRecycle(0);
                break;
            case EQuestActionType.CompleteDailyDeal:
                lobby.OnButtonMenuClick(0);
                break;
            default:
                GameManager.Singleton.StartCoroutine(FindObjectOfType<PageLobbyBattle>().BattleButtonShake());
                break;
        }

        _pop.Close();
    }
}
