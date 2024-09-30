using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotEffect : MonoBehaviour
{
    [SerializeField]
    Image _imgIcon;

    [SerializeField]
    TextMeshProUGUI _txtTitle;

    [SerializeField]
    Slider _slProgress;

    [SerializeField]
    GameObject _goRefresh;

    public void Initialize(ItemBase itembase, EffectTable effect, float curValue, int fixxed, bool isRandom = false)
    {
        ItemWeapon weapon = itembase as ItemWeapon;
        ItemGear gear = itembase as ItemGear;

        if (isRandom)
        {
            _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, "Icon_Pictoicon_Question");
            _slProgress.gameObject.SetActive(false);

            if (weapon != null)
            {
                SetTextValue(weapon, null, curValue);
            }
            else if (gear != null)
            {
                SetTextValue(gear, null, curValue);
            }
        }
        else
        {
            _imgIcon.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, effect.Icon);
            _slProgress.gameObject.SetActive(true);
            _slProgress.value = effect.ValueMin == effect.ValueMax ?
                                1 :
                                (curValue - effect.ValueMin) / (effect.ValueMax - effect.ValueMin);

            if (weapon != null)
            {
                SetTextValue(weapon, effect, curValue);
            }
            else if (gear != null)
            {
                SetTextValue(gear, effect, curValue);
            }
        }

        _goRefresh.SetActive(fixxed == 0);
    }

    void SetTextValue(ItemWeapon item, EffectTable effect, float curValue)
    {
        string name = effect == null ? UIStringTable.GetValue("ui_random") : NameTable.GetValue(effect.NameKey);
        WeaponTable w = WeaponTable.GetData(item.nKey);

        EEquipEffectType type = effect == null ? EEquipEffectType.None : (EEquipEffectType)effect.Type;
        EEffectOperationType opType = effect == null ? EEffectOperationType.ETC : (EEffectOperationType)effect.OperationType;

        string op = curValue > 0 ? "+" : "";
        string max = UIStringTable.GetValue("ui_max");

        switch ( type )
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
                _txtTitle.text = $"{name}  <color=#FFD02A>{op}{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                break;
            case EEquipEffectType.PenetrateChance:
            case EEquipEffectType.RicochetChance:
                if ( opType == EEffectOperationType.Replace )
                    _txtTitle.text = $"{name}  <color=#FFD02A>{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                else
                    _txtTitle.text = $"{name}  <color=#FFD02A>{op}{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                break;
            case EEquipEffectType.AttackRange:
                _txtTitle.text = $"{name}  <color=#FFD02A>{op}{Mathf.Round(curValue / w.Range * 10000f) / 100f} %</color>";
                break;
            case EEquipEffectType.KnockBackRange:
            case EEquipEffectType.SHOOT_HIT_EXPLOSION:
                _txtTitle.text = $"{name}  <color=#FFD02A>{op}{Mathf.Round((curValue / 1000f) * 100f) / 100f} </color>";
                break;
            case EEquipEffectType.MagazineSize:
                _txtTitle.text = $"{name}  <color=#FFD02A>{op}{(int)curValue} </color>";
                break;
            case EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN:
            case EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE:
                _txtTitle.text = $"{name}  <color=#FFD02A>{max} {op}{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                break;
            case EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION:
                _txtTitle.text = $"{name}  <color=#FFD02A>{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                break;
            default:
                _txtTitle.text = $"{name}";
                break;
        }
    }

    void SetTextValue(ItemGear item, EffectTable effect, float curValue)
    {
        string name = effect == null ? UIStringTable.GetValue("ui_random") : NameTable.GetValue(effect.NameKey);
        GearTable g = GearTable.GetData(item.nKey);

        EEquipEffectType type = effect == null ? EEquipEffectType.None : (EEquipEffectType)effect.Type;

        string op = curValue > 0 ? "+" : "";

        switch (type)
        {
            case EEquipEffectType.DefencePowerRatio:
            case EEquipEffectType.MaxHPRatio:
            case EEquipEffectType.AttackPowerPistol:
            case EEquipEffectType.AttackPowerSMG:
            case EEquipEffectType.AttackPowerAR:
            case EEquipEffectType.AttackPowerMG:
            case EEquipEffectType.AttackPowerSR:
            case EEquipEffectType.AttackPowerSG:
            case EEquipEffectType.AttackPowerGE:
            case EEquipEffectType.DefencePowerMelee:
            case EEquipEffectType.DefencePowerRange:
            case EEquipEffectType.DefencePowerExplosion:
            case EEquipEffectType.AttackPowerAfterKill:
            case EEquipEffectType.MoveSpeedRatioAfterKill:
            case EEquipEffectType.CureAfterKill:
            case EEquipEffectType.BleedingAfterAttack:
            case EEquipEffectType.MoveSpeedRatioAfterAttack:
                _txtTitle.text = $"{name}  <color=#FFD02A>{op}{Mathf.Round(curValue * 10000f) / 100f} %</color>";
                break;
            default:
                _txtTitle.text = $"{name}";
                break;
        }
    }
}
