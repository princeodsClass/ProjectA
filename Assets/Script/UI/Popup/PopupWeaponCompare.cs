using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupWeaponCompare : UIDialog
{
    [SerializeField]
    TextMeshProUGUI _txtMainDesc, _txtTargetDesc, _txtSelectDesc;

    [SerializeField]
    Transform _tMain, _tTarget, _tListRoot;

    [SerializeField]
    GameObject _goButtonClose;

    [SerializeField]
    SoundToggle _soundToggle;

    [SerializeField]
    ScrollRect _scrollRect;

    List<SlotWeapon> _lList;

    PageLobbyInventory _page;

    ItemWeapon _item, _target;
    SlotWeapon _sMain, _sTarget;

    bool _isColor;
    int _nValue;
    float _fValue;
    string _sValue;
    
    string attackpower, aimspeed, attackrange, magazinesize, reloadspeed, attackdelay, collection, accuracy,
           criticalchance, criticalratio, explosionrange, _max;

    private void Awake()
    {
        Initialize();
        _page = GameObject.Find("InventoryPage")?.GetComponent<PageLobbyInventory>();
    }
    
    private void OnEnable()
    {
        ComUtil.DestroyChildren(_tMain);
        ComUtil.DestroyChildren(_tTarget);
        ComUtil.DestroyChildren(_tListRoot);

        _lList = new List<SlotWeapon>();
    }

    void InitializeText()
    {
        _txtSelectDesc.gameObject.SetActive(true);

        _txtTargetDesc.text = string.Empty;
        _txtSelectDesc.text = UIStringTable.GetValue("ui_popup_weaponcompare_desc");
        _goButtonClose.transform.GetComponentInChildren<TextMeshProUGUI>().text = UIStringTable.GetValue("ui_popup_button_exit");

        attackpower = UIStringTable.GetValue("ui_slot_stat_category_attackpower");
        aimspeed = UIStringTable.GetValue("ui_slot_stat_category_aimspeed");
        attackrange = UIStringTable.GetValue("ui_slot_stat_category_attackrange");
        magazinesize = UIStringTable.GetValue("ui_slot_stat_category_magazinesize");
        reloadspeed = UIStringTable.GetValue("ui_slot_stat_category_reloadspeed");
        attackdelay = UIStringTable.GetValue("ui_slot_stat_category_attackdelay");
        collection = UIStringTable.GetValue("ui_slot_stat_category_collection");
        accuracy = UIStringTable.GetValue("ui_slot_stat_category_accuracy");
        criticalchance = UIStringTable.GetValue("ui_slot_stat_category_criticalchance");
        criticalratio = UIStringTable.GetValue("ui_slot_stat_category_criticalratio");
        explosionrange = UIStringTable.GetValue("ui_slot_stat_category_explosionrange");

        _max = UIStringTable.GetValue("ui_max");
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public void InitializeInfo(ItemWeapon weapon)
    {
        _item = weapon;

        InitializeText();
        InitializeWeapon(weapon);
        InitializeWeaponAttributes(ref _txtMainDesc, weapon);
    }

    void InitializeWeaponAttributes(ref TextMeshProUGUI tarText, ItemWeapon weapon, int maxAP = 0)
    {
        tarText.text = string.Empty;

        tarText.text = $"<size=130%><color=yellow>{UIStringTable.GetValue("ui_popup_weapon_stat_title")}</color></size>\n";

        int ap = maxAP == 0 ? weapon.nAttackPower : maxAP;
        _isColor = ap > weapon.nAttackPowerStandard;
        _nValue = ap;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_nValue}</color>" : $"{_nValue}";
        tarText.text += $"\n{attackpower} : {_sValue}";

        _isColor = weapon.fCriticalChance > GlobalTable.GetData<float>("perCritical");
        _fValue = MathF.Round(weapon.fCriticalChance * 10000f) / 100f;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} %</color>" : $"{_fValue} %";
        tarText.text += $"\n{criticalchance} : {_sValue}";

        _isColor = weapon.nAttackRange > WeaponTable.GetData(weapon.nKey).Range;
        _fValue = weapon.nAttackRange / 1000f;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} m</color>" : $"{_fValue} m";
        tarText.text += $"\n{attackrange} : {_sValue}";

        _isColor = weapon.fCriticalRatio > GlobalTable.GetData<float>("ratioCritical");
        _fValue = MathF.Round(weapon.fCriticalRatio * 10000f) / 100f;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} %</color>" : $"{_fValue} %";
        tarText.text += $"\n{criticalratio} : {_sValue}";

        _isColor = weapon.nMagazineSize > WeaponTable.GetData(weapon.nKey).MagazineSize;
        _nValue = weapon.nMagazineSize;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_nValue}</color>" : $"{_nValue}";
        tarText.text += $"\n{magazinesize} : {_sValue}";

        _isColor = weapon.nReloadTime > WeaponTable.GetData(weapon.nKey).ReloadTime;
        _fValue = Mathf.Round(weapon.nReloadTime / 100f) / 10f;
        _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} s</color> s" : $"{_fValue} s";
        tarText.text += $"\n{reloadspeed} : {_sValue}";

        if (weapon.eSubType == EWeaponType.SR)
        {
            _isColor = weapon.nAimTime > WeaponTable.GetData(weapon.nKey).AimTime;
            _fValue = Mathf.Round(weapon.nAimTime / 100f) / 10f;
            _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} s</color>" : $"{_fValue} s"; ;
            tarText.text += $"\n{aimspeed} : {_sValue}";
        }
        else if (weapon.eSubType != EWeaponType.Grenade)
        {
            _isColor = weapon.nAttackDelay > WeaponTable.GetData(weapon.nKey).AttackDelay;
            _fValue = MathF.Round(1000 / (float)weapon.nAttackDelay);
            _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue}</color>" : $"{_fValue}"; ;
            tarText.text += $"\n{attackdelay} : {_sValue}";
        }

        if (weapon.eSubType == EWeaponType.Grenade)
        {
            _isColor = weapon.fExplosionRangeRatio > 0;
            _fValue = Mathf.Round(EffectTable.GetGroup(WeaponTable.GetData(weapon.nKey).HitEffectGroup)[0].Value * (1 + weapon.fExplosionRangeRatio)) / 1000f;
            _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_fValue} m</color>" : $"{_fValue} m";
            tarText.text += $"\n{explosionrange} : {_sValue}";
        }
        else
        {
            int t = GlobalTable.GetData<int>("valueStadardAccuracy");

            _isColor = weapon.nAccuracy > WeaponTable.GetData(weapon.nKey).Accuracy;
            _fValue = (int)((float)t / weapon.nAccuracy) / 100f;
            _sValue = _isColor ? $"<color=#{GlobalTable.GetData<string>("valueStatColor")}>{_nValue}</color>" : $"{_fValue}";
            tarText.text += $"\n{(weapon.eSubType == EWeaponType.SG ? collection : accuracy)} : {_sValue}";
        }

        InitializeWeaponEffects(ref tarText, weapon);
    }

    void InitializeWeaponEffects(ref TextMeshProUGUI tarText, ItemWeapon weapon)
    {
        tarText.text += $"\n\n<size=130%><color=yellow>{UIStringTable.GetValue("ui_popup_weapon_effect_title")}</color></size>\n";

        for (int i = 0; i < weapon.nEffectKey.Length; i++)
        {
            if (weapon.nEffectKey[i] > 0)
            {
                EffectTable effect = EffectTable.GetData(weapon.nEffectKey[i]);
                EEquipEffectType type = (EEquipEffectType)effect.Type;

                float cValue = weapon.fEffectValue[i];

                string name = NameTable.GetValue(effect.NameKey);
                string op = cValue > 0 ? "+" : "";

                switch (type)
                {
                    case EEquipEffectType.AttackPowerRatio:
                    case EEquipEffectType.AttackDelayRatio:
                    case EEquipEffectType.AttackPowerToB:
                    case EEquipEffectType.AttackPowerToE:
                    case EEquipEffectType.AttackPowerToBNE:
                    case EEquipEffectType.CriticalChance:
                    case EEquipEffectType.CriticalRatio:
                    case EEquipEffectType.DoubleShotChance:
                    case EEquipEffectType.AmmoSpeedRatio:
                    case EEquipEffectType.AccuracyRatio:
                    case EEquipEffectType.ReduceSpread:
                    case EEquipEffectType.ExplosionRangeRatio:
                    case EEquipEffectType.ReloadTime:
                    case EEquipEffectType.AimTime:
                        tarText.text += $"\n{name}  <color=#FFD02A>{op}{Mathf.Round(cValue * 10000f) / 100f} %</color>";
                        break;
                    case EEquipEffectType.PenetrateChance:
                    case EEquipEffectType.RicochetChance:
                        if ((EEffectOperationType)EffectTable.GetData(weapon.nEffectKey[i]).OperationType == EEffectOperationType.Replace)
                            tarText.text += $"\n{name}  <color=#FFD02A>{Mathf.Round(cValue * 10000f) / 100f} %</color>";
                        else
                            tarText.text += $"\n{name}  <color=#FFD02A>{op}{Mathf.Round(cValue * 10000f) / 100f} %</color>";
                        break;
                    case EEquipEffectType.AttackRange:
                        tarText.text += $"\n{name}  <color=#FFD02A>{op}{Mathf.Round(cValue / WeaponTable.GetData(weapon.nKey).Range * 10000f) / 100f} %</color>";
                        break;
                    case EEquipEffectType.KnockBackRange:
                    case EEquipEffectType.SHOOT_HIT_EXPLOSION:
                        tarText.text += $"\n{name}  <color=#FFD02A>{op}{Mathf.Round((cValue / 1000f) * 100f) / 100f} </color>";
                        break;
                    case EEquipEffectType.MagazineSize:
                        tarText.text += $"\n{name}  <color=#FFD02A>{op}{(int)cValue} </color>";
                        break;
                    case EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN:
                    case EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE:
                        tarText.text += $"\n{name}  <color=#FFD02A>{_max} {op}{Mathf.Round(cValue * 10000f) / 100f} %</color>";
                        break;
                    case EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION:
                        tarText.text += $"\n{name}  <color=#FFD02A>{Mathf.Round(cValue * 10000f) / 100f} %</color>";
                        break;
                    default:
                        tarText.text += $"\n{name}";
                        break;
                }
            }
        }
    }

    void InitializeWeapon(ItemWeapon weapon)
    {
        _item = weapon;

        if (null != m_InvenWeapon)
        {
            foreach (ItemWeapon w in m_InvenWeapon)
            {
                if (weapon.id == w.id)
                {
                    _sMain = AddWeapon(w, _tMain);
                    _sMain.SetState(SlotWeapon.SlotState.compareNormal);
                }
                else
                {
                    SlotWeapon t = AddWeapon(w, _tListRoot);
                    t.SetState(SlotWeapon.SlotState.compareUnselected);
                    t.SetBaseScrollRect(_scrollRect);
                    _lList.Add(t);
                }
            }
        }

        Unselect();
    }

    public void ChangeCompareType()
    {
        if (_soundToggle.isOn)
        {
            if (null != _sMain)
            {
                int apMainMax = _item.CalcAttackPower((_item.nGrade + 1) * 10, _item.nGrade + 1, _item.nGrade + 1);

                _sMain.RefreshInfo(SlotWeapon.EPresentType.max);
                InitializeWeaponAttributes(ref _txtMainDesc, _item, apMainMax);
            }

            if (null != _sTarget)
            {
                int apTargetMax = _target.CalcAttackPower((_target.nGrade + 1) * 10, _target.nGrade + 1, _target.nGrade + 1);

                _sTarget?.RefreshInfo(SlotWeapon.EPresentType.max);
                InitializeWeaponAttributes(ref _txtTargetDesc, _target, apTargetMax);
            }
        }
        else
        {
            if ( null != _sMain )
            {
                _sMain.RefreshInfo(SlotWeapon.EPresentType.current);
                InitializeWeaponAttributes(ref _txtMainDesc, _item);
            }
            
            if ( null != _sTarget )
            {
                _sTarget?.RefreshInfo(SlotWeapon.EPresentType.current);
                InitializeWeaponAttributes(ref _txtTargetDesc, _target);
            }
        }
    }

    public void Select(ItemWeapon weapon)
    {
        ComUtil.DestroyChildren(_tTarget, false);
        _sTarget = AddWeapon(weapon, _tTarget);
        _target = weapon;

        for ( int i = 0; i < _lList.Count; i++ )
            if ( _lList[i]._Item.id != weapon.id ) _lList[i].SetSelected(SlotWeapon.SlotState.compareUnselected);

        _soundToggle.interactable = true;
        _soundToggle.isOn = false;

        _txtSelectDesc.gameObject.SetActive(false);
        InitializeWeaponAttributes(ref _txtTargetDesc, weapon);
    }

    public void Unselect()
    {
        if ( _sTarget == null ) return;

        ComUtil.DestroyChildren(_tTarget);
        _sTarget = null;
        _target = null;

        for (int i = 0; i < _lList.Count; i++)
            _lList[i].SetSelected(SlotWeapon.SlotState.compareUnselected);

        _soundToggle.interactable = false;
        _soundToggle.isOn = false;

        InitializeText();
    }

    SlotWeapon AddWeapon(ItemWeapon weapon, Transform t)
    {
        SlotWeapon item = m_MenuMgr.LoadComponent<SlotWeapon>(t, EUIComponent.SlotWeapon);
        item.Initialize(weapon);
        item.SetState(SlotWeapon.SlotState.compareSelected);

        return item;
    }

    public void ResetWeapon()
    {
        if ( !gameObject.activeSelf )
            _page?.InitializeWeapon();
    }

    public override void Open() 
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void Escape()
    {
        base.Escape();
    }

    private void OnDisable()
    {
        ResetWeapon();
    }
}
