using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WeaponTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public string Icon { get { return stringValue((int)COLUMN.Icon); } }
    public string IconSymbol { get { return stringValue((int)COLUMN.IconSymbol); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int AnimationType { get { return intValue((int)COLUMN.AnimationType); } }
    public string Prefab { get { return stringValue((int)COLUMN.Prefab); } }
    public float CorrectFOV { get { return floatValue((int)COLUMN.CorrectFOV); } }
    public int Range { get { return intValue((int)COLUMN.Range); } }
    public int MoveSpeed { get { return intValue((int)COLUMN.MoveSpeed); } }
    public int AimTime { get { return intValue((int)COLUMN.AimTime); } }
    public int Accuracy { get { return intValue((int)COLUMN.Accuracy); } }
    public int AttackType { get { return intValue((int)COLUMN.AttackType); } }
    public int DamageType { get { return intValue((int)COLUMN.DamageType); } }
    public int HitEffectGroup { get { return intValue((int)COLUMN.HitEffectGroup); } }
    public int ProjectileCount { get { return intValue((int)COLUMN.ProjectileCount); } }
    public int AttackPowerStandard { get { return intValue((int)COLUMN.AttackPowerStandard); } }
    public int ForceMin { get { return intValue((int)COLUMN.ForceMin); } }
    public int ForceMax { get { return intValue((int)COLUMN.ForceMax); } }
    public int WarheadSpeed { get { return intValue((int)COLUMN.WarheadSpeed); } }
    public int AttackDelay { get { return intValue((int)COLUMN.AttackDelay); } }
    public int BurstCount { get { return intValue((int)COLUMN.BurstCount); } }
    public int BurstDelay { get { return intValue((int)COLUMN.BurstDelay); } }
    public int ReloadTime { get { return intValue((int)COLUMN.ReloadTime); } }
    public int MagazineSize { get { return intValue((int)COLUMN.MagazineSize); } }
    public int EquipEffectGroup { get { return intValue((int)COLUMN.EquipEffectGroup); } }
    public int EquipEffectSelectionCount { get { return intValue((int)COLUMN.EquipEffectSelectionCount); } }
    public uint UpgradeMaterialKey { get { return GetUINT((int)COLUMN.UpgradeMaterialKey); } }
    public uint ReinforceMaterialKey { get { return GetUINT((int)COLUMN.ReinforceMaterialKey); } }
    public uint LimitbreakMaterialKey00 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey00); } }
    public uint LimitbreakMaterialKey01 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey01); } }
    public uint LimitbreakMaterialKey02 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey02); } }
    public uint LimitbreakMaterialKey03 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey03); } }
    public uint LimitbreakMaterialKey04 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey04); } }
    public uint LimitbreakMaterialKey05 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey05); } }
    public uint LimitbreakMaterialKey06 { get { return GetUINT((int)COLUMN.LimitbreakMaterialKey06); } }
    public uint RecycleMaterialKey00 { get { return GetUINT((int)COLUMN.RecycleMaterialKey00); } }
    public uint RecycleMaterialKey01 { get { return GetUINT((int)COLUMN.RecycleMaterialKey01); } }
    public uint RecycleMaterialKey02 { get { return GetUINT((int)COLUMN.RecycleMaterialKey02); } }
    public uint RecycleMaterialKey03 { get { return GetUINT((int)COLUMN.RecycleMaterialKey03); } }
    public uint RecycleMaterialKey04 { get { return GetUINT((int)COLUMN.RecycleMaterialKey04); } }
    public int RecycleMaterialCount00 { get { return intValue((int)COLUMN.RecycleMaterialCount00); } }
    public int RecycleMaterialCount01 { get { return intValue((int)COLUMN.RecycleMaterialCount01); } }
    public int RecycleMaterialCount02 { get { return intValue((int)COLUMN.RecycleMaterialCount02); } }
    public int RecycleMaterialCount03 { get { return intValue((int)COLUMN.RecycleMaterialCount03); } }
    public int RecycleMaterialCount04 { get { return intValue((int)COLUMN.RecycleMaterialCount04); } }

}
