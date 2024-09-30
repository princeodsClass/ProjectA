using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotAbyssLeagueGrade : MonoBehaviour
{
    [SerializeField]
    GameObject _goBlockBase;

    [SerializeField]
    SlotLeagueBlock[] _sBlock;

    [SerializeField]
    Color[] _colorFrame;

    [SerializeField]
    Image _imgIcon, _imgBG;

    [SerializeField]
    TextMeshProUGUI _txtTitle;

    public void InitializeInfo(LeagueTable league, bool detail = false)
    {
        _imgIcon.sprite = LeagueTable.GetGradeIcon(league.MinRank);
        _txtTitle.text = NameTable.GetValue(league.NameKey);
        _imgBG.color = _colorFrame[league.Grade];

        _goBlockBase.SetActive(detail);

        if ( detail ) SetBlock(league);
    }

    void SetBlock(LeagueTable league)
    {
        int min = league.MinRank;
        int max = league.MaxRank;

        float gap = (max - min) / 4f;

        _goBlockBase.SetActive(true);

        _sBlock[0]._goCurrent.SetActive(GameManager.Singleton.user.m_nAbyssCurRank >= (min + gap));
        _sBlock[1]._goCurrent.SetActive(GameManager.Singleton.user.m_nAbyssCurRank >= (min + gap * 2));
        _sBlock[2]._goCurrent.SetActive(GameManager.Singleton.user.m_nAbyssCurRank >= (min + gap * 3));
    }
}
