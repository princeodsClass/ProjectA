using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAbyssLeague : MonoBehaviour
{
    [SerializeField]
    Transform _tRootGrade, _tRootRewards;

    [SerializeField]
    GameObject[] _goCurrent;

    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    Color[] _colorFrame;

    [SerializeField]
    Image _imgBG;

    SlotAbyssLeagueGrade _grade;
    bool _isCurrent = false;

    LeagueTable _league;

    public void InitializeInfo(LeagueTable league, bool current = false)
    {
        ClearGo();

        Array.ForEach(_goCurrent, go => go.SetActive(current));
        _isCurrent = current;
        _league = league;
        GameManager.Log($"{league.PrimaryKey}.isCurrent : {current}", "red");

        SlotAbyssLeagueGrade grade = MenuManager.Singleton.LoadComponent<SlotAbyssLeagueGrade>(_tRootGrade, EUIComponent.SlotAbyssLeagueGrade);
        grade.InitializeInfo(league);

        SetGrade(league);
        SetRewards(league);
    }

    public bool IsCurrent()
    {
        GameManager.Log($"{_league.PrimaryKey}.isCurrent : {_isCurrent}", "yellow");
        return _isCurrent;
    }

    void SetRewards(LeagueTable league)
    {
        List<RewardTable> rGroup = RewardTable.GetGroup(league.RewardsGroup);
        List<RewardListTable> rewards;

        Sprite sp;

        for ( int i = 0; i < rGroup.Count; i++ )
        {
            rewards = RewardListTable.GetGroup(rGroup[i].RewardListGroup);
            sp = ComUtil.GetIcon(rewards[0].RewardKey);

            SlotAbyssRewards rSlot = MenuManager.Singleton.LoadComponent<SlotAbyssRewards>(_tRootRewards, EUIComponent.SlotAbyssRewards);
            rSlot.InitializeInfo(rewards[0].RewardKey, sp, rewards[0].RewardCountMin);
        }
    }

    void SetGrade(LeagueTable league)
    {
        _imgBG.color = _colorFrame[league.Grade];
    }

    void ClearGo()
    {
        ComUtil.DestroyChildren(_tRootGrade);
        ComUtil.DestroyChildren(_tRootRewards);
    }

    void ResetBlock(bool force = false)
    {
        // ComUtil.DestroyChildren(_tRootBlock, force);
    }

    private void Awake()
    {
        ResetBlock(true);
    }

    private void OnEnable()
    {
        ResetBlock();
    }
}
