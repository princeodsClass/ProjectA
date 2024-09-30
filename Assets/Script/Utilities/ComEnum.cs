using System;
using System.Collections;
using System.Collections.Generic;

public enum EOSType
{
	Editor,
	Android,
	iOS,
}

public enum EButtonSoundType
{
	none_sound,
	normal_sound,
	critical_sound,
}

public enum EPermission
{
	StorageRead,
	StorageWrite,
}

public enum ESceneType
{
	NONE = -1,
	Title,
	Lobby,
	Battle,
	BattleTest,
	GameScene,

	#region 추가
	MapEditor,
	#endregion // 추가
	MAX
}

public enum EAtlasType
{
	Icons = 0,
	Common,
	InGame,
	Outgame,

	#region 추가
	MapEditor,
	#endregion // 추가

	END
}

public enum ETableType
{
	GLOBAL = 0,
	ACCOUNTLEVEL,
	WORKSHOPLEVEL,
	BATTLEPASS,

	CHARACTER = 17,

	NPC = 19,
	NPCSTAT,

	SKILL = 22,
	SKILLGROUP,

	WEAPON = 32,

	MATERIAL = 34,
	GEAR,
	BOX,
	DICE,

	EFFECTGROUP = 41,
	EFFECT,

	FIELDOBJECT = 47,
	EPISODE,
	CHAPTER,

	HUNT = 52,

	MISSIONADVENTURE = 54,
	ABYSS,

	LEAGUE = 63,
	RECIPE,

	REWARD = 96,
	REWARDLIST,
	DAILYDEAL,
	BOXDEAL,
	PASSDEAL,

	CRYSTALDEAL = 105,
	MONEYDEAL,
	VIPDEAL,
	ADSDEAL,


	ATTENDANCE = 111,
	NAME,
	DESC,
	UISTRING,
	CONTENTSSTRING,

	END = 127,
}

public enum EResourceType
{
	#region 추가
	None = -1,
	#endregion // 추가

	Effect_BG = 0,
	Effect_CH,
	Effect_UI,
	Character,
	Weapon,
	Gear,
	Box,
	Object,
	ObjectLevel,
	UI,
	UIPage,
	UIPopup,
	UIComponent,
	UIButton,
	UIETC,
	Custom,

	#region 추가
	BG_Walkable = 100,
	BG_NotWalkable,
	BG_Decoration,
	BG_Etc,
	BG_Temp,

	Character_NPC,
	UI_Battle,
	BG,
	Effect,
	Etc,
	#endregion // 추가

	End
}

public enum EUIType
{
	Page = 0,
	Popup,
	Component,
	End
}

public enum EUIPage
{

	PageTitle = 1,
	PageLobby,
	PageBattle,

	AboveTutorial,
	AboveQuestCard,

	#region 추가
	PageMapEditor,
	#endregion // 추가

	End
}

public enum EUIRepositoryPage
{
	Weapon,
	Material,
	Gear,
}

/// <summary>
/// ( 임시 ) 정상 처리될 때까지 위에 오버랩 되는 팝업 인덱스가 위로 오도록 처리 필..!!
/// escape 리스너 문제 처리하자.
/// </summary>
public enum EUIPopup
{
	PopupLoading = 0,

	PopupDefault,
	PopupSysMessage,

	PopupWait4Response,

	PopupNotice,

	PopupShopVIP,
	PopupShopGameMoney,
	PopupShopCrystal,

	PopupMaterial,

	PopupWeaponDownGrade,
	PopupGearDownGrade,

    PopupBoxMaterial,
    PopupBoxMaterialPremium,

    PopupWeapon,
	PopupGear,

	PopupPurchase,

	PopupWeaponChange,
	PopupWeaponCompare,

	PopupItemBuy,

	PopupWeaponReward,
	PopupETCReward,
	PopupLevelUp,

	PopupRecycle,

	PopupBoxWeapon,

	PopupBoxNormal,
	PopupBoxReward,
	PopupBoxExchange,
	PopupRepositorFull,

	PopupBattlePass,
	PopupEPRoadMap,
	PopupInputMail,
	PopupOption,
	PopupBattlePause,
	PopupBattleUpgradeGuide,
	PopupBattleReward,
	PopupBattleInfiniteReward,
	PopupMultyReward,
	PopupHunt,

	PopupAttendance,

	PopupWorkshopResult,

	PopupWorkshopSelect,
	PopupWorkshopRegist,
	PopupWorkshopItem,
	PopupWorkshop,

	PopupRepositoryExtent,
	PopupBattleBuffRoulette,
	PopupBattleContinue,
	BattleInventory,
	PopupBattleInstEffect,
	PopupDiceReward,
	PopupMissionAdventureKeyBuy,
	PopupMissionAdventure,
	PopupMissionDefence,
	PopupOtherUser,
	PopupAbyss,

	PopupAgentEnhance,
	PopupAgentBuy,
	PopupAgent,
		
	PopupAgree,
	PopupTrackingDesc,

	PopupQuest,

	End
}

public enum EUIComponent
{
	SlotItem = 1,
	SlotWeapon,
	SlotMaterial,
	SlotGear,
	SlotBox,

	SlotMessage,

	SlotSimpleItem,
	SlotSimpleItemReward,
	SlotEffect,
	SlotStat,
	SlotBattlePass,

	SlotShopBox,
	SlotShopCrystal,
	SlotShopGameMoney,
	SlotShopADS,
	SlotShopVIP,
	SlotBuyVIP,

	SlotSkill,
	SlotAbyss,
	SlotAbyssGroup,
	SlotAbyssRankTable,
	SlotAbyssLeague,
	SlotAbyssLeagueGrade,
	SlotAbyssRewards,

	SlotDefenceRewardsGroup,

	SlotRecipeItem,
	SlotRecipeItemGroup,

	SlotBattlePassSet,
	SlotEpisode,
	SlotWorkshopItem,
	SlotWorkshopSymbol,
	SlotAlarm,

	SlotChapterBlock,

	SlotAttendance,

	SlotQuest,
	SlotQuestReward,

	ToastMessage,

	TooltipItem,
	TooltipItemList,
	TooltipMessage,
	TooltipToast,

	BattleSlotWeapon,
	End
}

public enum EItemType
{
	FieldObject = -1,
	Common = 0,
	Weapon,
	Gear,
	Character,
	CCard,

	GearPart = 7,
	Dice,
	MaterialG,
	Part,
	Token,
	Currency,
	Material,
	Box,
	ETC,
	END
}

public enum EWeaponType
{
	Common,
	Pistol,
	SMG,
	AR,
	MG,
	SR,
	SG,
	Grenade,

	Unknown = 0x1000000
}

public enum EGearType
{
	Common,
	head,
	upper,
	hands,
	lower,
}

public enum EWeaponAnimationType
{
	Common,
	Pistol,
	SMG,
	AR_Normal,
	AR_PistolGrip,
	MG,
	DMG,
	SR,
	Grenade,
	GrenadeLauncher,
	Mortar,
	RocketLauncher,
	Turret,
	END
}

public enum EWeaponStatType
{
	ATTACKPOWER,
	AIMTIME,
	ATTACKRANGE,
	MAGAZINESIZE,
	RELOADTIME,
	ATTACKDELAY,
	ACCURACY,
	REDUCESPREAD,
	CRITICALCHANCE,
	CRITICALRATIO,
	EXPLOSIONRANGE,
}

public enum EGearStatType
{
	DEFENCEPOWER,
	MAXHP,
}

public enum EOptionCategory
{
	BGM,
	SFX,

}

public enum ECharacterType
{
	Player = 1,
	Enemy_Normal,
	Enemy_Elite,
	Enemy_Boss,
}

public enum ERewardBoxState
{
	Ready = 0,
	ReadyCrystal,
	ReadyAd,
	ReadyVIP,
	ReadyTicket,
	Opening,
	Complete,
	Free,
	END,
}

public enum EAccountPostType
{
	get_info_auid,
	get_info_nickname,
	get_info_otheruser,
	signup,
	signin,
	modify_account,
	link_account,
	unlink_account,
	check_password,
	change_password,
	start_adventure,
	get_attendance_rewards,
	set_bonuspoint,
	set_obtain_chapter_weapon
}

public enum EDatabaseType
{
	inventory,
	box,
}

public enum EInventoryType
{
	weapon,
	material,
	gear,
	character,
	box,
}

public enum EInventoryPostType
{
	get_item_list,
	insert_item,
	delete_item,
	modify_item,
	insert_character,
	get_characterskill,
	upgrade_characterskill,
}

public enum EMapPostType
{
	get_map_version,
	set_map_version,
	start_hunt,
	start_defence,
	complete_map,

	complete_campaign,

	revive,

	start_adventureChapter,
	end_adventureChapter,
	skip_adventureChapter,

	end_defence,

	start_zombie,
	end_zombie,
}

public enum EQuestActionType
{
	KillMelee = 0,
	KillRange,
	ClearHunt,
	ClearAdventure,
	ClearDefence,
	ClearAbyss,
	CompleteCraft,
	CompleteRecycle,
	CompleteDailyDeal,
	ObtainWeapon,
	ObtainScrap,
	KillWithPistol,
	KillWithAR,
	KillWithGE,
	KillWithSR,
	KillWithSG,
}

public enum EShopPostType
{
	get_dailydeal_list,
	buy_dailydeal,

	get_gamemoneydeal_list,
	buy_gamemoneydeal,

	get_crystaldeal_list,
	buy_crystaldeal,

	get_adsdeal_list,
	buy_adsdeal,

	get_vipdeal_list,
	buy_vipdeal,

	set_battlepassactive,

	verify_receipt,
}

public enum EETCPostType
{
	get_notice,
	ads_view,
	get_battlepassinfo,
	set_battlepassinfo,
	get_questinfo,
	set_questinfo,
	create_questinfo,
	complete_quest,
	get_questrewardsinfo,
	set_questrewardsinfo,
}

public enum EAbyssPostType
{
	get_abyss_info,
	get_abyss_recordinfo,
	get_abyss_rankinfo,
	set_abyss_season_rewards_get,
	clear_floor,
}

public enum ECurrencyType
{
	money,
	crystal,
}

public enum ECompleteMapType
{
	None = 0,
	Start = 1,
	Suscces,
	Fail,
	Exit,

	skip = 14,
	revive = 15,
}

public enum ELanguage
{
	Korean = 1,
	English,
	Japanese,
	Chinese,
	German,
	Spanish,
	Portuguese,
	French,
	Italian,
	Dutch,
	Russian,
	Hungarian,
	Greek,
	Turkish,
	Vietnamese,
	// Persian,
	Malay,
	// Arabic,
	Indonesian,
	// Hebrew,
	END,
}

public enum EEventType
{
	EventDaily,
	EventWeaponBoxDeal,
	EventMaterialBoxDeal,
	EventMaterialPremiumBoxDeal,
	EventGameMoneyDeal,
	EventADSDeal,
	END,
}

public enum EDealType
{
	Weapon,
	PremiumWeapon,
	Material,
	PremiumMaterial,
	END,
}

public enum ERecipeType
{
	Start = 0,
	Produce,
	Reinforce,
	Merge,

	Head = 12,
	Upper,
	Hands,
	Lower,
	END,
}

public enum EADSViewType
{
	Start,
	ADSRewards,
	BoxOpen,
	Boost,
	MissingItemCollection,
	AdventureRoulette,
	Dice,
	DefenceSpeed,
    Roulette,
	InstanceSkill,
    Revive,
}

public enum EEffectOperationType
{
	ETC = 0,
	Replace,
	Ratio,
	Add
}

public enum EEquipEffectType
{
	None = 0,
	MoveSpeed,
	ReloadTime,
	InstanceReload,
	AimTime,

	AttackPowerRatio = 17,
	CriticalChance,
	CriticalRatio,
	SmallAttackChance,
	SmallAttackRatio,
	AttackRange,
	AccuracyRatio,
	ExplosionRangeRatio,
	AttackDelayRatio,
	RicochetChance,

	PenetrateChance = 29,
	AmmoSpeedRatio,
	DoubleShotChance,
	ReduceSpread,
	DefencePowerRatio,
	MaxHPRatio,

	MagazineSize = 49,
	KnockBackRange,

	AttackPowerToBNE = 65,
	AttackPowerToB,
	AttackPowerToE,

	AttackPowerPistol = 81,
	AttackPowerSMG,
	AttackPowerAR,
	AttackPowerMG,
	AttackPowerSR,
	AttackPowerSG,
	AttackPowerGE,

	ActiveSkillChargeRatio = 0x5A,

	DefencePowerMelee = 97,
	DefencePowerRange,
	DefencePowerExplosion,

	AttackPowerAfterKill = 113,

	MoveSpeedRatioAfterKill = 115,

	CureAfterKill = 117,

	BleedingAfterAttack = 122,
	MoveSpeedRatioAfterAttack,

	#region 전투
	HP = 100000,
	ATK,
	DEF,
	ACCURACY,

	ATK_DELAY,
	RELOAD_TIME,
	AIMING_DELAY,
	WARHEAD_SPEED,
	
	FREEZE,
	BLEEDING,

	ATTACK_POWER_UP,
	ATTACK_POWER_DOWN,

	MOVE_SPEED_UP,
	MOVE_SPEED_DOWN,

	FORCE_MIN,
	FORCE_MAX,
	FORCE_WALK,
	FORCE_RICOCHET_CHANCE,

	ATTACK_POWER_AFTER_KILL_DURATION = 200000,
	MOVE_SPEED_RATIO_AFTER_KILL_DURATION,

	DURATION_VAL = 0x1000000,
	OPERATION_TYPE_VAL = 0x2000000,

	SHOOT_HIT_EXPLOSION = 0x81,
	ATTACK_POWER_UP_BY_MAGAZIN = 0x82,
	ATTACK_POWER_UP_BY_EXPLOSION = 0x83,
	ATTACK_POWER_UP_BY_DISTANCE = 0x84,


	HP_RECOVERY = 1
	#endregion // 전투
}

#region 추가
/** 스테이지 타입 */
public enum EStageType
{
	NONE = -1,
	COMMON,
	SHELTER,
	BATTLE,
	MAX_VAL
}

/** 스킬 종류 */
public enum ESkillType
{
	NONE = -1,
	EMPTY,

	SUMMON = 0x0001,
	LOCK_WEAPON = 0x0002,

	FAN_SHOT = 0x0101,

	GRENADE = 0x0201,
	GRENADE_DELAY = 0x0202,
	GRENADE_SPLIT = 0x0203,

	BOMBING_REQUEST = 0x0301,
	JUMP_ATTACK = 0x0302,
	FLAME_FIELD = 0x0303,

	RICOCHET = 0x0306,
	UNTOUCHABLE = 0x0305,
	ICE = 0x0304,

	MAX_VAL
}

/** 스킬 사용 방식 */
public enum ESkillUseType
{
	NONE = -1,
	ETC,
	ACTIVE,
	PASSIVE,
	PASSIVE_GLOBAL,
	MAX_VAL
}

/** 스킬 애니메이션 종류 */
public enum ESkillAniType
{
	NONE = -1,
	EMPTY,
	FAN_SHOT,
	GRENADE_SHOT,
	PLACEHOLDERS_01,
	BOMBING_REQUEST,
	JUMP_ATTACK,
	MAX_VAL
}

/** 맵 정보 타입 */
public enum EMapInfoType
{
	NONE = -1,
	CAMPAIGN,
	HUNT,
	TUTORIAL,
	ADVENTURE,
	DEFENCE,
	BONUS,
	INFINITE,

	ABYSS = 8,

	MAX_VAL
}

/** 전투 방식 */
public enum EBattleType
{
	NONE = -1,
	NORM,
	INFINITE,
	MAX_VAL
}

/** 플레이 모드 */
public enum EPlayMode
{
	NONE = -1,
	TEST,
	CAMPAIGN,
	HUNT,
	TUTORIAL,
	ADVENTURE,
	DEFENCE,
	BONUS,
	INFINITE,

	ABYSS = 9,
	
	MAX_VAL
}

/** 조준 상태 */
public enum ELockOnState
{
	NONE = -1,
	NORM,
	WARNING,
	MAX_VAL
}

/** 교환 타입 */
public enum ESwapType
{
	NONE = -1,
	LESS,
	GREAT,
	ALWAYS,
	MAX_VAL
}

/** 마우스 버튼 */
public enum EMouseBtn
{
	NONE = -1,
	LEFT,
	RIGHT,
	MIDDLE,
	MAX_VAL
}

/** 피격 타입 */
public enum EHitType
{
	NONE = -1,
	NORM,
	CRITICAL,
	WEAK,
	MAX_VAL
}

/** 공격 타입 */
public enum EAttackType
{
	NONE = -1,
	SHOOT,
	THROW,
	DROP,
	MAX_VAL
}

/** 순찰 타입 */
public enum EPatrolType
{
	NONE = -1,
	IDLE,
	WAY_POINT,
	LOOK_AROUND,
	FIX,
	MAX_VAL
}

/** 이동 지점 타입 */
public enum EWayPointType
{
	NONE = -1,
	PASS,
	LOOK_AROUND,
	MAX_VAL
}

/** 범위 유형 */
public enum ERangeType
{
	NONE = -1,
	ETC,

	// 플레이어
	PLAYER,
	PLAYER_RANGE,

	// 사용자
	USER,
	USER_RANGE,

	// 피해 대상
	HIT_TARGET,
	HIT_TARGET_RANGE,

	// NPC
	NON_PLAYER,

	MAX_VAL
}

/** 효과 종류 */
public enum EEffectType
{
	NONE = -1,

	// 명중 {
	HIT_ETC = 0x00,
	HIT_EXPLOSION_DAMAGE,
	HIT_EXPLOSION_CREATE,
	HIT_EXPLOSION_DAMAGE_DELAY,

	HIT_FLAME_DAMAGE,
	// 명중 }

	// 사용 {
	LIMIT_ETC = 0x00,
	LIMIT_WEAPON_LOCK,
	LIMIT_FORCE_WALK,

	LIMIT_ICE,
	LIMIT_UNTOUCHABLE,
	LIMIT_JUMP_ATTACK,
	// 사용 }

	// 획득
	GAIN_ETC = 0x00,
	GAIN_RECOVERY,

	MAX_VAL
}

/** 효과 유형 */
public enum EEffectCategory
{
	NONE = -1,
	ETC,
	PARAM,
	HIT,
	LIMIT,
	GAIN,
	ADVENTURE_BONUS,
	GEAR_PARAM,
	TRAP_EFFECT,
	INST_EFFECT,
	ROULETTE_EFFECT,
	MAX_VAL
}

/** 행동 타입 */
public enum EActionType
{
	NONE = -1,
	AVOID = 2,
	ONLY_AVOID,
	MAX_VAL
}

/** 피해 타입 */
public enum EDamageType
{
	NONE = -1,
	MELEE,
	RANGE,
	EXPLOSION,
	MAX_VAL
}

/** NPC 타입 */
public enum ENPCType
{
	NONE = -1,
	ETC,
	NORM,
	FAT,
	TURRET,
	MAX_VAL
}

/** NPC 등급 */
public enum ENPCGrade
{
	NONE = -1,
	ETC,
	NORM,
	ELITE,
	BOSS,

	BONUS = 5,
	MAX_VAL
}

/** 튜토리얼 단계 */
public enum ETutorialStep
{
	NONE = -1,
	NPC_LOCK_ON,
	NPC_HELP_AROUND,
	TOUCH_REWARD_01,
	TOUCH_REWARD_02,
	TOUCH_REWARD_03,
	TOUCH_INVENTORY,
	TOUCH_WEAPON,
	EQUIP_WEAPON,
	TOUCH_BATTLE_LOBBY,
	ENTER_BATTLE,
	EQUIP_WEAPON_CHANGE,
	TOUCH_INVENTORY_FOR_UPGRADE,
	TOUCH_WEAPON_FOR_UPGRADE,
	UPGRADE_WEAPON,
	MAX_VAL
}

/** 도움말 튜토리얼 종류 */
public enum EHelpTutorialKinds
{
	NONE = -1,
	NPC_LOCK_ON,
	NPC_HELP_AROUND,
	EQUIP_WEAPON_CHANGE,
	NPC_SUMMON_SKILL,
	WEAPON_UPGRADE_GUIDE,
	BONUS_NPC_APPEAR,
	MAX_VAL
}

/** 연산 타입 */
public enum EOperationType
{
	NONE = -1,
	ETC,
	REPLACE,
	RATIO,
	ADD,
	MAX_VAL
}

/** 대상 그룹 */
public enum ETargetGroup
{
	NONE = -1,
	COMMON,
	PLAYER,
	NON_PLAYER_01,
	NON_PLAYER_02,
	NON_PLAYER_03,
	NON_PLAYER_04,
	NON_PLAYER_05,
	NON_PLAYER_06,
	NON_PLAYER_07,
	NON_PLAYER_08,
	NON_PLAYER_09,
	MAX_VAL
}

/** 필드 오브젝트 식별자 */
public enum EFieldObjKey
{
	NONE = -1,
	ROULETTE = 0x2F000002,
	WEAPON_BOX,
	MATERIAL_BOX,
	COIN_BOX,
	CRYSTAL_BOX,
	MAX_VAL
}
#endregion // 추가
