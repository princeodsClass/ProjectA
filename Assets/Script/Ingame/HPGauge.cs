using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPGauge : MonoBehaviour
{
    [SerializeField]
    GameObject[] _GaugeType;

    [SerializeField]
    GameObject _GaugeBG, _GaugeBase;

    RectTransform _rectGauge, _rectGaugeBase, _rectGaugeBG;

    [SerializeField]
    Transform _tParent;

    public enum GAUGETYPE
    {
        player = 0,
        enemy_normal,
        enemy_elite,
        enemy_boss
    }

    public GAUGETYPE _CharacterType;

    public float _MaxHP, _CurrentHP;

    private void Awake()
    {
        _MaxHP = 100;
        _CurrentHP = _MaxHP;

        SetCharacterType(0);
        SetGaugeType();
    }

    public void SetCharacterType(GAUGETYPE type)
    {
        _CharacterType = type;
    }

    public void SetCharacterType(int type)
    {
        _CharacterType = (GAUGETYPE)type;
    }

    void SetGaugeType()
    {
        for (int i = 0; i < _GaugeType.Length; i++)
            _GaugeType[i].SetActive(i == (int)_CharacterType);

        _rectGauge = _GaugeType[(int)_CharacterType].GetComponent<RectTransform>();
        _rectGaugeBase = _GaugeBase.GetComponent<RectTransform>();
        _rectGaugeBG = _GaugeBG.GetComponent<RectTransform>();
    }

    IEnumerator SetCurrentGauge(float damage)
    {
        float move;
        Vector2 vecMove;

        move = damage * _rectGauge.sizeDelta.x / 100;
        vecMove = new Vector2(move, 0f);

        _rectGauge.anchoredPosition = _rectGauge.anchoredPosition - vecMove;

        yield return new WaitForSeconds(0.2f);

        while (_rectGaugeBase.position.x > _rectGauge.position.x)
        {
            _rectGaugeBase.position = Vector3.MoveTowards(_rectGaugeBase.position,
                                                          _rectGauge.position,
                                                          0.05f);

            yield return null;
        }
    }

    public void test()
    {
        StopCoroutine("SetCurrentGauge");
        StartCoroutine(SetCurrentGauge(5f));
    }

    private void Update()
    {
        transform.position = Camera.main.WorldToScreenPoint(_tParent.position);
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }
}
