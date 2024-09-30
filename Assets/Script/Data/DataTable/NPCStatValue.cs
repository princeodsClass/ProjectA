using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class NPCStatTable
{
    public uint PrimaryKey { get { return GetUINT((int)COLUMN.PrimaryKey); } }
    public int Level { get { return intValue((int)COLUMN.Level); } }
    public int HitRate { get { return intValue((int)COLUMN.HitRate); } }
    public int PowerAttack { get { return intValue((int)COLUMN.PowerAttack); } }
    public int PowerDefence { get { return intValue((int)COLUMN.PowerDefence); } }
    public int HP { get { return intValue((int)COLUMN.HP); } }
    public int AimTime { get { return intValue((int)COLUMN.AimTime); } }
    public int SelectionFactorAttack { get { return intValue((int)COLUMN.SelectionFactorAttack); } }
    public int SelectionFactorSkillGroup { get { return intValue((int)COLUMN.SelectionFactorSkillGroup); } }
    public int AttackSkillGroup { get { return intValue((int)COLUMN.AttackSkillGroup); } }
    public int HPSkillGroup25 { get { return intValue((int)COLUMN.HPSkillGroup25); } }
    public int HPSkillGroup50 { get { return intValue((int)COLUMN.HPSkillGroup50); } }
    public int HPSkillGroup75 { get { return intValue((int)COLUMN.HPSkillGroup75); } }
    public int ContinuouslySkillGroup { get { return intValue((int)COLUMN.ContinuouslySkillGroup); } }
    public int RewardGroup { get { return intValue((int)COLUMN.RewardGroup); } }
    public int MidiumRewardGroup10 { get { return intValue((int)COLUMN.MidiumRewardGroup10); } }
    public int MidiumRewardGroup20 { get { return intValue((int)COLUMN.MidiumRewardGroup20); } }
    public int MidiumRewardGroup30 { get { return intValue((int)COLUMN.MidiumRewardGroup30); } }
    public int MidiumRewardGroup40 { get { return intValue((int)COLUMN.MidiumRewardGroup40); } }
    public int MidiumRewardGroup50 { get { return intValue((int)COLUMN.MidiumRewardGroup50); } }
    public int MidiumRewardGroup60 { get { return intValue((int)COLUMN.MidiumRewardGroup60); } }
    public int MidiumRewardGroup70 { get { return intValue((int)COLUMN.MidiumRewardGroup70); } }
    public int MidiumRewardGroup80 { get { return intValue((int)COLUMN.MidiumRewardGroup80); } }
    public int MidiumRewardGroup90 { get { return intValue((int)COLUMN.MidiumRewardGroup90); } }
}
