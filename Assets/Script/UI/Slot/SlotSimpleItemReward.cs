using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AssetKits.ParticleImage;

public class SlotSimpleItemReward : MonoBehaviour
{
    [SerializeField]
    Image _imgFrame, _imgGlow, _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtVolume, _txtName;

    [SerializeField]
    ParticleImage _piEdge;

    [SerializeField]
    Color[] _colorFrame, _colorGlow;

    [SerializeField]
    GameObject[] _goEdgeFX;

    [Header("그레이드 별")]
    [SerializeField]
    GameObject[] _objGradeIndicator;

    [SerializeField]
    RectTransform S;

    [SerializeField]
    GridLayoutGroup _gGrade;

    [SerializeField]
    GameObject _goVolumn, _goFX, _goScrapIcon;

    Animator animator = null;
    float _fCellSize, _fCellSpacing, _fStandardWidth;
    
    public uint _itemKey;
    int _grade;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
        _fCellSize = _gGrade.cellSize.x;
        _fCellSpacing = _gGrade.spacing.x;
        _fStandardWidth = S.rect.width;
    }

    public void Initialize(Sprite icon, string volume, string name, int grade = 0, bool isGrade = false, bool isCounting = true, uint key = 0)
    {
        _itemKey = key;

        _imgIcon.sprite = icon;
        _txtName.text = name;
        _txtVolume.text = volume;

        _grade = grade;
        _imgFrame.color = _colorFrame[grade];
        _imgGlow.color = _colorGlow[grade];
        _piEdge.gameObject.SetActive(grade >= 3);

        for ( int i = 0; i <  _objGradeIndicator.Length; i++ )
            _objGradeIndicator[i].SetActive( isGrade && i < grade );

        _goVolumn.SetActive(isCounting);

        SetScrapIcon(key);

        for ( int i = 0; i < _goEdgeFX.Length; i++ )
            if ( null != _goEdgeFX[i] )
                _goEdgeFX[i].SetActive(i == grade);
    }

    public void Boosting()
    {
        int d = 0;

        if ( int.TryParse(_txtVolume.text, out d) )
        {
            _txtVolume.text = $"{_txtVolume.text} + <color=green>{_txtVolume.text}</color>";
        }
        else
        {
            _imgFrame.color = _colorFrame[_grade + 1];
            _imgGlow.color = _colorGlow[_grade + 1];
            _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons,
                              WeaponTable.GetData(_itemKey + (uint)Math.Pow(16, 5)).Icon);

            for (int i = 0; i < _objGradeIndicator.Length; i++)
                _objGradeIndicator[i].SetActive(i < _grade + 1);
        }
        
        animator.SetTrigger("BoostIdle");
        _goFX.SetActive(true);
    }

    void ResizeCell(float cur)
    {
        float t = cur / _fStandardWidth * _fCellSize;
        _gGrade.cellSize = new Vector2(t, t);

        float s = cur / _fStandardWidth * _fCellSpacing;
        _gGrade.spacing = new Vector2(s, 0);
    }

    public void SetAppear()
    {
        _txtName.gameObject.SetActive(true);
        gameObject.SetActive(true);
        animator.SetTrigger("Pop");
        
        ResizeCell(transform.parent.GetComponent<GridLayoutGroup>().cellSize.x);
    }

    public void SetRestart()
    {
        // animator.Play("SlotSimpleItemRewardIdle");
        _txtName.gameObject.SetActive(false);
        // gameObject.SetActive(true);
        animator.SetTrigger("Restart");
        ResizeCell(transform.parent.GetComponent<GridLayoutGroup>().cellSize.x);
    }

    public void SetBoost()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Boost");
    }

    void SetScrapIcon(uint key)
    {
        if ( key > 0 )
        {
            if ("22" == key.ToString("X").Substring(0, 2))
            {
                string sd = key.ToString("X").Substring(3, 1);

                EItemType type = (EItemType)Convert.ToInt32(sd, 16);
                _goScrapIcon.SetActive(type == EItemType.Material || type == EItemType.MaterialG);

                return;
            }
        }

        _goScrapIcon.SetActive(false);
    }

    public virtual void ESoundAppear()
    {
        GameAudioManager.PlaySFX("SFX/UI/sfx_ui_whoosh_normal_01", 0f, false, ComType.UI_MIX);
    }

    public virtual void ESoundRestart()
    {
        GameAudioManager.PlaySFX("SFX/UI/sfx_popup_appear_00", 0f, false, ComType.UI_MIX);
    }
}
