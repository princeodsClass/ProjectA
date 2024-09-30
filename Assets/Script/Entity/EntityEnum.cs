/*
 * 		entity type
 */ 
public enum ENTITY_TYPE
{
	BEGIN = 0,

	AbyssTable,
	AccountLevelTable,
	ADSDealTable,
	AttendanceTable,
	BattlePassTable,
	BoxTable,
	BoxDealTable,
	ChapterTable,
	CharacterTable,
	CharacterLevelTable,
	ContentsStringTable,
	CrystalDealTable,
	DailyDealTable,
	DescTable,
	DiceTable,
	EffectTable,
	EffectGroupTable,
	EpisodeTable,
	FieldObjectTable,
	GearTable,
	GlobalTable,
	HuntTable,
    InstanceLevelTable,
    LeagueTable,
	MaterialTable,
	MissionAdventureTable,
	MissionDefenceTable,
	MissionDefenceGroupTable,
    MissionZombieTable,
    MissionZombieGroupTable,
    MoneyDealTable,
	NameTable,
    NPCTable,
	NPCGroupTable,
    NPCHintGroupTable,
    NPCHintStringTable,
	NPCStatTable,
	PassDealTable,
	QuestTable,
	RecipeTable,
	RewardTable,
	RewardListTable,
	SkillTable,
	SkillGroupTable,
	StageTable,
	UIStringTable,
	VIPDealTable,
	WeaponTable,
	WorkshopLevelTable,

	END,
}

public static partial class Extensions
{
	
	public static string[] fileName = new string[]
	{
		"BEGIN",

		"Abyss",
		"AccountLevel",
		"ADSDeal",
		"Attendance",
		"BattlePass",
		"Box",
		"BoxDeal",
		"Chapter",
		"Character",
		"CharacterLevel",       
		"ContentsString",
		"CrystalDeal",
		"DailyDeal",
		"Desc",
		"Dice",
		"Effect",
		"EffectGroup",
		"Episode",
		"FieldObject",
		"Gear",
		"Global",
		"Hunt",
        "InstanceLevel",
        "League",
		"Material",
		"MissionAdventure",
		"MissionDefence",
		"MissionDefenceGroup",
        "MissionZombie",
        "MissionZombieGroup",
        "MoneyDeal",
		"Name",
		"NPC",
		"NPCGroup",
        "NPCHintGroup",
        "NPCHintString",
        "NPCStat",
		"PassDeal",
		"Quest",
		"Recipe",
		"Reward",
		"RewardList",
		"Skill",
		"SkillGroup",
		"Stage",
		"UIString",
		"VIPDeal",
		"Weapon",
		"WorkshopLevel",

		"END",
	};   

	     
	static string[] types = System.Enum.GetNames(typeof(ENTITY_TYPE));
	static Extensions()
	{
		for (int i = 0; i < types.Length; ++i)
		{
			types[i] = types[i].ToString();
		}
	}
	
	
	public static string FileName(this ENTITY_TYPE type)
	{
		return fileName[(int)type];
	}
	
	public static string ClassName(this ENTITY_TYPE type)
	{
		return types[(int)type];
	}

	public static string TypeName(this ENTITY_TYPE type)
	{
		return types[(int)type];
	}
}