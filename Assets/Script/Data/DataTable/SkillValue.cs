using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class SkillTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int Weapon { get { return intValue((int)COLUMN.Weapon); } }
    public int UseType { get { return intValue((int)COLUMN.UseType); } }
    public int SkillType { get { return intValue((int)COLUMN.SkillType); } }
    public int SkillAniType { get { return intValue((int)COLUMN.SkillAniType); } }
    public int AttackType { get { return intValue((int)COLUMN.AttackType); } }
    public int DamageType { get { return intValue((int)COLUMN.DamageType); } }
    public float ratioAttackPower { get { return floatValue((int)COLUMN.ratioAttackPower); } }
    public float CastingTime { get { return floatValue((int)COLUMN.CastingTime); } }
    public float InvokeTime { get { return floatValue((int)COLUMN.InvokeTime); } }
    public int HitEffectGroup { get { return intValue((int)COLUMN.HitEffectGroup); } }
    public uint UpgradeMaterialKey00 { get { return GetUINT((int)COLUMN.UpgradeMaterialKey00); } }
    public int UpgradeMaterialCount00 { get { return intValue((int)COLUMN.UpgradeMaterialCount00); } }
    public uint UpgradeMaterialKey01 { get { return GetUINT((int)COLUMN.UpgradeMaterialKey01); } }
    public int UpgradeMaterialCount01 { get { return intValue((int)COLUMN.UpgradeMaterialCount01); } }
    public int RequirePoint { get { return intValue((int)COLUMN.RequirePoint); } }
}