using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public uint NameKey { get { return GetUINT((int)COLUMN.NameKey); } }
    public uint DescKey { get { return GetUINT((int)COLUMN.DescKey); } }
    public string Prefab { get { return stringValue((int)COLUMN.Prefab); } }
    public string WeaponPrefab { get { return stringValue((int)COLUMN.WeaponPrefab); } }
    public int Grade { get { return intValue((int)COLUMN.Grade); } }
    public int Type { get { return intValue((int)COLUMN.Type); } }
    public int Theme { get { return intValue((int)COLUMN.Theme); } }
    public int Camp { get { return intValue((int)COLUMN.Camp); } }
    public int isHunt { get { return intValue((int)COLUMN.isHunt); } }
    public int isSummon { get { return intValue((int)COLUMN.isSummon); } }
    public int isNecessary { get { return intValue((int)COLUMN.isNecessary); } }
    public int FirstEpisode { get { return intValue((int)COLUMN.FirstEpisode); } }
    public int DefenceGroup { get { return intValue((int)COLUMN.DefenceGroup); } }
    public int ProjectileCount { get { return intValue((int)COLUMN.ProjectileCount); } }
    public int AnimationType { get { return intValue((int)COLUMN.AnimationType); } }
    public int DamageType { get { return intValue((int)COLUMN.DamageType); } }
    public int ActionType { get { return intValue((int)COLUMN.ActionType); } }
    public int DetectionRange { get { return intValue((int)COLUMN.DetectionRange); } }
    public int TrackingRange { get { return intValue((int)COLUMN.TrackingRange); } }
    public int HelpRange { get { return intValue((int)COLUMN.HelpRange); } }
    public int SightAngle { get { return intValue((int)COLUMN.SightAngle); } }
    public int SightRange { get { return intValue((int)COLUMN.SightRange); } }
    public int HearingRange { get { return intValue((int)COLUMN.HearingRange); } }
    public int AttackRange { get { return intValue((int)COLUMN.AttackRange); } }
    public int HitEffectGroup { get { return intValue((int)COLUMN.HitEffectGroup); } }
    public int WarheadSpeed { get { return intValue((int)COLUMN.WarheadSpeed); } }
    public int ForceMin { get { return intValue((int)COLUMN.ForceMin); } }
    public int ForceMax { get { return intValue((int)COLUMN.ForceMax); } }
    public float SpeedRun { get { return floatValue((int)COLUMN.SpeedRun); } }
    public float SpeedWalk { get { return floatValue((int)COLUMN.SpeedWalk); } }
    public int SpeedRotation { get { return intValue((int)COLUMN.SpeedRotation); } }
    public string StatsValue { get { return stringValue((int)COLUMN.StatsValue); } }
    public int AbyssPoint { get { return intValue((int)COLUMN.AbyssPoint); } }
    public int GoldenPoint { get { return intValue((int)COLUMN.GoldenPoint); } }
    public int SkillPoint { get { return intValue((int)COLUMN.SkillPoint); } }
    public int EndPoint { get { return intValue((int)COLUMN.EndPoint); } }
    public int InstancePoint { get { return intValue((int)COLUMN.InstancePoint); } }
    public uint HintGroup { get { return GetUINT((int)COLUMN.HintGroup); } }
}
