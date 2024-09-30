using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StageTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public int RewardExp { get { return intValue((int)COLUMN.RewardExp); } }
    public int RewardPassExp { get { return intValue((int)COLUMN.RewardPassExp); } }
    public int Episode { get { return intValue((int)COLUMN.Episode); } }
    public int Chapter { get { return intValue((int)COLUMN.Chapter); } }
    public int Order { get { return intValue((int)COLUMN.Order); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public float IntanceExpRatio { get { return floatValue((int)COLUMN.IntanceExpRatio); } }
    public int TargetEndPoint { get { return intValue((int)COLUMN.TargetEndPoint); } }
    public int CorrectedLevel { get { return intValue((int)COLUMN.CorrectedLevel); } }
    public uint EffectGroupInstanceLvUp { get { return GetUINT((int)COLUMN.EffectGroupInstanceLvUp); } }
    public uint EffectGroupRoulette { get { return GetUINT((int)COLUMN.EffectGroupRoulette); } }
    public uint AdditionalEffectGroupKey { get { return GetUINT((int)COLUMN.AdditionalEffectGroupKey); } }
    public int AdditionalEffectGroupCount { get { return intValue((int)COLUMN.AdditionalEffectGroupCount); } }
    public float ChectSpawnRatio { get { return floatValue((int)COLUMN.ChectSpawnRatio); } }
    public int WeaponChestWeight { get { return intValue((int)COLUMN.WeaponChestWeight); } }
    public int MaterialChestWeight { get { return intValue((int)COLUMN.MaterialChestWeight); } }
    public int CoinChestWeight { get { return intValue((int)COLUMN.CoinChestWeight); } }
    public int CrystalChestWeight { get { return intValue((int)COLUMN.CrystalChestWeight); } }
    public uint WeaponChestGroup { get { return GetUINT((int)COLUMN.WeaponChestGroup); } }
    public uint MaterialChestGroup { get { return GetUINT((int)COLUMN.MaterialChestGroup); } }
    public uint CoinChestGroup { get { return GetUINT((int)COLUMN.CoinChestGroup); } }
    public uint CrystalChestGroup { get { return GetUINT((int)COLUMN.CrystalChestGroup); } }
    public string BGM { get { return stringValue((int)COLUMN.BGM); } }
}
