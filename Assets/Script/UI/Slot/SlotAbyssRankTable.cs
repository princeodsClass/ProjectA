using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

public class SlotAbyssRankTable : MonoBehaviour
{
    [SerializeField]
    Color[] _colorBG, _colorFont;

    [SerializeField]
    Color _colorBGMine;

    [SerializeField]
    Image _BG;

    [SerializeField]
    TextMeshProUGUI _txtRank, _txtName, _txtFloor, _txtBestLap;

    [SerializeField]
    Image _imgSymbol;

    long _auid = default;
    bool _own = false;

    public void InitializeInfo(int order, PopupAbyss pop, JObject item)
    {
        _own = PlayerPrefs.GetInt(ComType.STORAGE_UID) == (long)item.GetValue("auid");
        _BG.color = _own ? _colorBGMine :_colorBG[order % 2];

        if ( _own )
        {
            GameManager.Singleton.user.m_nAbyssCurRank = (int)item.GetValue("ranking");
            pop.SetGrade();
        }

        _auid = (long)item.GetValue("auid");
        _txtRank.text = (string)item.GetValue("ranking");
        _txtName.text = (string)item.GetValue("nickname");
        _txtFloor.text = (string)item.GetValue("chapter");

        TimeSpan duration = TimeSpan.FromMilliseconds((int)item.GetValue("duration"));
        string laptime = $"{duration.Minutes}:{duration.Seconds:D2}.{duration.Milliseconds:D3}";

        _txtBestLap.text = laptime;

        _txtRank.color = _colorFont[_own ? 1 : 0];
        _txtName.color = _colorFont[_own ? 1 : 0];
        _txtFloor.color = _colorFont[_own ? 1 : 0];
        _txtBestLap.color = _colorFont[_own ? 1 : 0];

        SetSymbol((int)item.GetValue("ranking"));
    }

    void SetSymbol(int ranking)
    {
        Sprite ti = LeagueTable.GetGradeIcon(ranking);

        if ( null != ti )
        {
            _imgSymbol.sprite = ti;
            _imgSymbol.gameObject.SetActive(true);
            _txtRank.gameObject.SetActive(false);
        }
        else
        {
            _imgSymbol.gameObject.SetActive(false);
            _txtRank.gameObject.SetActive(true);
        }
    }

    public bool IsOwn()
    {
        return _own;
    }

    public void OnClick()
    {
        StartCoroutine(GameDataManager.Singleton.GetOtherUserInfo(_auid));
    }
}
