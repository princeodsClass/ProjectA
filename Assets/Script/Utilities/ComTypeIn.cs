using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static partial class ComType
{
	#region 프로퍼티
	public static List<EEquipEffectType> StandardAbilityTypeDict = new List<EEquipEffectType>()
	{
		EEquipEffectType.RicochetChance,
		EEquipEffectType.PenetrateChance,
		EEquipEffectType.ExplosionRangeRatio
	};

	public static Dictionary<EHelpTutorialKinds, string> HelpTutorialKindsKeyDict { get; } = new Dictionary<EHelpTutorialKinds, string>()
	{
		[EHelpTutorialKinds.NONE] = EHelpTutorialKinds.NONE.ToString(),
		[EHelpTutorialKinds.NPC_LOCK_ON] = EHelpTutorialKinds.NPC_LOCK_ON.ToString(),
		[EHelpTutorialKinds.NPC_HELP_AROUND] = EHelpTutorialKinds.NPC_HELP_AROUND.ToString(),
		[EHelpTutorialKinds.EQUIP_WEAPON_CHANGE] = EHelpTutorialKinds.EQUIP_WEAPON_CHANGE.ToString(),
		[EHelpTutorialKinds.NPC_SUMMON_SKILL] = EHelpTutorialKinds.NPC_SUMMON_SKILL.ToString(),
		[EHelpTutorialKinds.WEAPON_UPGRADE_GUIDE] = EHelpTutorialKinds.WEAPON_UPGRADE_GUIDE.ToString(),
		[EHelpTutorialKinds.BONUS_NPC_APPEAR] = EHelpTutorialKinds.BONUS_NPC_APPEAR.ToString()
	};
	#endregion // 프로퍼티

	#region 기본
	public static KeyCode DelKeyCode
	{
		get
		{
#if UNITY_EDITOR_WIN
			return KeyCode.Delete;
#elif UNITY_EDITOR_OSX
			return KeyCode.Backspace;
#else
			return (Application.platform == RuntimePlatform.WindowsPlayer) ? KeyCode.Delete : KeyCode.Backspace;
#endif // #if UNITY_EDITOR_WIN
		}
	}

	public static KeyCode CtrlKeyCode
	{
		get
		{
#if UNITY_EDITOR_WIN
			return KeyCode.LeftControl;
#elif UNITY_EDITOR_OSX
			return KeyCode.LeftCommand;
#else
			return (Application.platform == RuntimePlatform.WindowsPlayer) ? KeyCode.LeftControl : KeyCode.LeftCommand;
#endif // #if UNITY_EDITOR_WIN
		}
	}

	public static Vector3 DeviceScreenSize
	{
		get
		{
#if UNITY_EDITOR
			return new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0.0f);
#else
			return new Vector3(Screen.width, Screen.height, 0.0f);
#endif // #if UNITY_EDITOR
		}
	}

	public static float ResolutionScale => ComType.DeviceScreenSize.x.ExIsLess(ComType.ResolutionScreenSize.x) ? ComType.DeviceScreenSize.x / ComType.ResolutionScreenSize.x : 1.0f;
	public static float DesktopResolutionScale => ComType.DesktopScreenSize.x.ExIsLess(ComType.ResolutionDesktopScreenSize.x) ? ComType.DesktopScreenSize.x / ComType.ResolutionDesktopScreenSize.x : 1.0f;

	public static Vector3 DesktopScreenSize => new Vector3(Screen.currentResolution.width, Screen.currentResolution.height, ComType.DeviceScreenSize.z);
	public static Vector3 CorrectDesktopScreenSize => (ComType.ResolutionDesktopScreenSize * 0.9f) * ComType.DesktopResolutionScale;

	private static Vector3 ResolutionScreenSize => new Vector3(ComType.DeviceScreenSize.y * (1080.0f / 1920.0f), ComType.DeviceScreenSize.y, ComType.DeviceScreenSize.z);
	private static Vector3 ResolutionDesktopScreenSize => new Vector3(ComType.DesktopScreenSize.y * (1920.0f / 1080.0f), ComType.DesktopScreenSize.y, ComType.DesktopScreenSize.z);

	// 단위 {
	public const int G_COMPARE_LESS = -1;
	public const int G_COMPARE_GREAT = 1;
	public const int G_COMPARE_EQUALS = 0;

	public const int G_NUM_VERTICES_ON_FACE = 3;
	public const int G_MAX_NUM_EQUIP_WEAPONS = 4;
	public const int G_MAX_NUM_EPISODE_THEMES = 4;
	public const int G_MAX_NUM_OVERLAP_NON_ALLOC = 50;
	public const int G_MAX_NUM_RAYCAST_NON_ALLOC = 25;
	public const int G_OFFSET_FIND_AVOID_POS = 20;

	public const int G_TIMES_CONCURRENT = 10;
	public const int G_MAX_TRY_TIMES_FIND_SUMMON_POS = 999;

	public const int G_UNIT_IDS_PER_IDS_01 = 1;
	public const int G_UNIT_IDS_PER_IDS_02 = 0x100;
	public const int G_UNIT_IDS_PER_IDS_03 = 0x10000;

	public const float G_UNIT_SCALE = 0.01f;
	public const float G_UNIT_MS_TO_S = 0.001f;
	public const float G_UNIT_MM_TO_M = 0.001f;
	public const float G_UNIT_PLAYER_ROTATE_EPSILON = 0.999f;
	public const float G_UNIT_NON_PLAYER_ROTATE_EPSILON = 0.899f;

	public const float G_MIN_RANGE_SHELL_DISPOSE_DIRECTION = 0.25f;
	public const float G_MAX_RANGE_SHELL_DISPOSE_DIRECTION = 1.0f - ComType.G_MIN_RANGE_SHELL_DISPOSE_DIRECTION;

	public const float B_MIN_VAL_REAL = float.MinValue / 2.0f;
	public const float B_MAX_VAL_REAL = float.MaxValue / 2.0f;

	public const float G_LIFE_T_SHELL = 5.0f;
	public const float G_LIFE_T_SKILL = 30.0f;
	public const float G_LIFE_T_PROJECTILE = 10.0f;
	public const float G_LIFE_T_DAMAGE_FIELD = 25.0f;

	public const float G_LIFE_T_FX = 10.0f;
	public const float G_LIFE_T_HIT_FX = 5.0f;
	public const float G_LIFE_T_STEP_FX = 5.0f;
	public const float G_LIFE_T_RECOVERY_FX = 1.5f;

	public const float G_LIFE_T_DECAL = 5.0f;
	public const float G_LIFE_T_COMMON_FX = 5.0f;
	public const float G_LIFE_T_CONTINUOUS_FX = 3.5f;
	public const float G_LIFE_T_MUZZLE_FLASH = 1.0f;
	public const float G_LIFE_T_EXPLOSION_WARNING = 5.0f;

	public const float G_DELTA_T_SKILL = 0.125f;
	public const float G_DELTA_T_TEXT_ANI = 0.25f;

	public const float G_MAX_TIME_CONTINUE = 10.0f;
	public const float G_OFFSET_Y_WEAPON_UIS_SEL = 25.0f;

	public const float G_ANGLE_TEXT_ANI = 30.0f;
	public const float G_ANGLE_SUMMON_FX = 60.0f;
	public const float G_ANGLE_FIND_AVOID_POS = 240.0f;

	public const float G_SPEED_RUN = 0.65f;
	public const float G_SPEED_ACQUIRE_ITEM = 500.0f;

	public const float G_DURATION_TEXT_ANI = 0.25f;
	public const float G_DURATION_CAMERA_ANI = 1.0f;
	public const float G_DURATION_DAMAGE_ANI = 0.25f;
	public const float G_DURATION_TUTORIAL_FOCUS_ANI = 0.25f;
	public const float G_DURATION_SUMMON_ANI = 1.5f;
	public const float G_DURATION_EXPLOSION_ANI = 1.5f;
	public const float G_DURATION_GROUND_ITEM_ANI = 5.0f;

	public const float G_DURATION_CAMERA_SHAKING_ANI = 0.15f;
	public const float G_DURATION_WEAPON_UIS_SEL_ANI = 0.05f;
	public const float G_DURATION_LOADING_PROGRESS_ANI = 0.05f;

	public const float G_OFFSET_POA_FX = 0.1f;
	public const float G_OFFSET_RATE_HIT_FX = 0.15f;
	public const float G_OFFSET_ATTACK_RANGE_FX = 0.15f;
	public const float G_OFFSET_LOCK_ON_TARGET_FX = 0.1f;
	public const float G_MIN_OFFSET_THROW_PROJECTILE = 0.1f;

	public const float G_OFFSET_CAMERA_HEIGHT_FOR_FOCUS = 8.0f;
	public const float G_OFFSET_CAMERA_FORWARD_FOR_FOCUS = -10.5f;
	public const float G_OFFSET_CAMERA_DISTANCE_FOR_FOCUS = 1.5f;

	public const float G_DELAY_BATTLE_PLAY = 1.0f;
	public const float G_DELAY_RAGDOLL_ANI = 2.5f;
	public const float G_FORCE_ATTENUATION_HIT = 0.25f;
	public const float G_STRENGH_CAMERA_SHAKING_ANI = 0.2f;
	public const float G_DELAY_GROUND_ITEM_PHYSICS_STOP = 5.0f;

	public const float G_MIN_FORCE_GROUND_ITEM = 4.5f;
	public const float G_MAX_FORCE_GROUND_ITEM = 7.5f;

	public const float G_MIN_FORCE_SHELL_DISPOSE = 3.5f;
	public const float G_MAX_FORCE_SHELL_DISPOSE = 7.5f;

	public const string G_PREFIX_MAP_INFO = "311";
	public static readonly Bounds G_EMPTY_BOUNDS = new Bounds();
	public static readonly Vector3 G_OFFSET_BATTLE_TEXT = new Vector3(0.0f, 3.5f, 0.0f);

	public static readonly Dictionary<EMapInfoType, string> G_PREFIX_MAP_INFO_DICT = new Dictionary<EMapInfoType, string>()
	{
		[EMapInfoType.ADVENTURE] = "311",
		[EMapInfoType.ABYSS] = "311"
	};
	// 단위 }

	// 색상
	public static readonly Color G_WAVE_NPC_COLOR = new Color(0.3f, 0.3f, 0.3f, 1.0f);
	public static readonly STColor G_DEF_COLOR_LIGHT = new STColor(255.0f / 255.0f, 244.0f / 255.0f, 214.0f / 255.0f, 1.0f);

	// 키 {
	public const uint G_KEY_MAT_ADVENTURE_KEY = 0x221F3001;
	public const uint G_KEY_KNOCK_BACK_SKILL_KEY = 0x16020811;

	public const int G_MAX_AGENT_SKILL_LEVEL = 9;
	public const int G_OFFSET_AGENT_SKILL_LEVEL = 5;

	public const string G_ROBBERY_NPC_KEY = "RobberyNPCKey";
	public const string G_SAMI_BOSS_NPC_KEY = "SamiBossNPCKey";

	public const string G_DAMAGE_TINT = "_Tint";
	public const string G_DAMAGE_AMOUNT = "_demage_amount";

	public const string G_MAX_STEP_HEIGHT = "maxStepHeight";
	public const string G_RANGE_GET_GROUND_ITEM = "rangeGetGroundItem";
	public const string G_DISTANCE_GRENADE_OFFSET = "distanceGrenadeOffset";

	public const string G_TIME_CONTINUOUSLY_KILL = "timeContinuouslyKill";
	public const string G_TIME_IGNORE_CONTINUOUSLY_KILL = "timeIgnoreContinuouslyKill";
	public const string G_VALUE_PITCH_OFFSET = "valueContinuouslyKillAddPitch";
	public const string G_VALUE_INVINCIBLE_EXPLOSION_POWER = "valueInvincibleExplosionPower";

	public const string G_VALUE_SNIPER_RECOIL = "valueSniperRecoil";
	public const string G_VALUE_MAX_CHARACTER_LEVEL = "valueMaxCharacterLevel";
	public const string G_VALUE_STANDARD_ACCURACY = "valueStadardAccuracy";
	public const string G_VALUE_MIN_NPC_LEVEL = "valueMinNPCLevel";
	public const string G_VALUE_MAX_NPC_LEVEL = "valueMaxNPCLevel";
	public const string G_VALUE_GAME_MONEY_DIVIDE = "valueGamemoneyDivide";
	public const string G_VALUE_CRYSTAL_DIVIDE = "valueCrystalDivide";
	public const string G_VALUE_SAMI_BOSS_START_EPISODE = "valuSamiBossStartEpisode";

	public const string G_TIME_ADVENTURE_RESET = "timeAdventureReset";
	public const string G_TIME_ADVENTURE_KEY_OBTAIN = "timeAdventureKeyObtain";
	public const string G_TIME_INVINCIBILITY_AFTER_REVIVE = "timeInvincibilityAfterRevive";

	public const string G_RANGE_RICOCHET = "rangeRicochet";
	public const string G_RANGE_CAMERA_OFFSET_TOP = "rangeCameraOffsetTop";
	public const string G_RANGE_CAMERA_OFFSET_BOTTOM = "rangeCameraOffsetBottom";
	public const string G_RANGE_CAMERA_OFFSET_LEFT = "rangeCameraOffsetLeft";
	public const string G_RANGE_CAMERA_OFFSET_RIGHT = "rangeCameraOffsetRight";

	public const string G_TIME_DOUBLI_SHOT = "timeDoubliShot";
	public const string G_TIME_DELAY_TRIGGER = "timeDelayTrigger";
	public const string G_TIME_FOR_RELOAD_PRESS = "timeForReloadPress";
	public const string G_TIME_WEAPON_LOCK_SKILL = "timeWeaponLockSkill";
	public const string G_TIME_COLOR_CHANGE_TO_FIRE = "timeColorChangeToFire";

	public const string G_COUNT_NPC_CORPSE = "countNPCCorpse";
	public const string G_COUNT_KEEP_CORPSE = "countKeepCorpse";

	public const string G_RATIO_SKILL_POINT_CAMPAIGN = "ratioSkillPointinCampaign";
	public const string G_RATIO_SKILL_POINT_HUNT = "ratioSkillPointinHunt";
	public const string G_RATIO_SKILL_POINT_ADVENTURE = "ratioSkillPointinAdventure";
	public const string G_RATIO_SKILL_POINT_DEFENCE = "ratioSkillPointinDefence";
	public const string G_RATIO_SKILL_POINT_ABYSS = "ratioSkillPointinAbyss";
	public const string G_RATIO_SKILL_POINT_INFINITE = "ratioSkillPointinZombie";

	public const string G_RATIO_HUNT_REWARD = "ratioHuntReward";
	public const string G_RATIO_MAX_BLEEDING = "ratioMaxBleeding";
	public const string G_DEGREE_FOR_FIRE_GRENADE = "degreeForFireGrenade";
	public const string G_COUNT_MAX_ADVENTURE_KEY = "countMaxAdventureKey";
	public const string G_COUNT_CHECK_AI_CONDITION_PER_SECOND = "countCheckAIConditionPerSecond";

	public const string G_VALUE_TICKET_DEC_KEY = "valueTicketDecKey";
	public const string G_VALUE_GLOBAL_TICKET_DEC_KEY = "valueDefenceTicketDecKey";
	
	public const string G_VALUE_GRAVITY_FOR_PLAYER = "valueGravityForPlayer";
	public const string G_VALUE_GRAVITY_FOR_GRENADE = "valueGravityForGrenade";

	public const string G_HP_SKILL_GROUP_25 = "HPSkillGroup25";
	public const string G_HP_SKILL_GROUP_50 = "HPSkillGroup50";
	public const string G_HP_SKILL_GROUP_75 = "HPSkillGroup75";

	public const string G_MIDIUM_REWARD_GROUP_10 = "MidiumRewardGroup10";
	public const string G_MIDIUM_REWARD_GROUP_20 = "MidiumRewardGroup20";
	public const string G_MIDIUM_REWARD_GROUP_30 = "MidiumRewardGroup30";
	public const string G_MIDIUM_REWARD_GROUP_40 = "MidiumRewardGroup40";
	public const string G_MIDIUM_REWARD_GROUP_50 = "MidiumRewardGroup50";
	public const string G_MIDIUM_REWARD_GROUP_60 = "MidiumRewardGroup60";
	public const string G_MIDIUM_REWARD_GROUP_70 = "MidiumRewardGroup70";
	public const string G_MIDIUM_REWARD_GROUP_80 = "MidiumRewardGroup80";
	public const string G_MIDIUM_REWARD_GROUP_90 = "MidiumRewardGroup90";

	public const string G_KEY_MAP_VER = "version";
	public const string G_KEY_TUTORIAL_STEP = "TutorialStep";
	public const string G_KEY_FMT_NPC_STAT_TABLE = "{0}{1:X3}";
	public const string G_KEY_NOTI_BONUS_NPC_APPEAR = "NotiBonusNPCAppear";
	public const string G_KEY_IS_NEEDS_FOCUS_DIRECTING = "IsNeedsFocusDirecting";

	public const string G_KEY_FMT_HINT_GROUP = "HintGroup_{0:X3}";
	public static readonly string G_KEY_FMT_MAP_INFO = $"{"{0}"}{"{1:X7}"}";
	// 키 }

	// 태그
	public const string G_TAG_POA = "POA";
	public const string G_TAG_WARP_GATE = "WarpGate";

	public const string G_TAG_NPC = "Enemy";
	public const string G_TAG_PLAYER = "Player";
	public const string G_TAG_STRUCTURE = "Structure";
	public const string G_TAG_AUDIO_LISTENER = "AudioListener";
	public const string G_TAG_STRUCTURE_WOOD = "StructureWood";

	// 레이어 {
	public const string G_LAYER_COMMON = "Common";
	public const string G_LAYER_BOUNDS = "Bounds";
	public const string G_LAYER_PLAYER = "Player";
	public const string G_LAYER_STRUCTURE = "Structure";
	public const string G_LAYER_NON_PLAYER_01 = "NonPlayer_01";
	public const string G_LAYER_NON_PLAYER_02 = "NonPlayer_02";
	public const string G_LAYER_NON_PLAYER_03 = "NonPlayer_03";
	public const string G_LAYER_NON_PLAYER_04 = "NonPlayer_04";
	public const string G_LAYER_GROUND_OBJECT = "GroundObject";

	public const string G_NAV_MESH_AREA_JUMP = "Jump";
	public const string G_NAV_MESH_AREA_WALKABLE = "Walkable";
	public const string G_NAV_MESH_AREA_NOT_WALKABLE = "Not Walkable";

	public static List<string> G_UNIT_LAYER_LIST = new List<string>()
	{
		ComType.G_LAYER_COMMON,
		ComType.G_LAYER_PLAYER,
		ComType.G_LAYER_NON_PLAYER_01,
		ComType.G_LAYER_NON_PLAYER_02,
		ComType.G_LAYER_NON_PLAYER_03,
		ComType.G_LAYER_NON_PLAYER_04
	};
	// 레이어 }

	// 매개 변수 {
	public const string G_PARAMS_SHOT = "Shot";
	public const string G_PARAMS_RELOAD = "Reload";
	public const string G_PARAMS_UNLOCK = "Unlock";

	public const string G_PARAMS_OPEN = "Open";
	public const string G_PARAMS_CLOSE = "Close";
	public const string G_PARAMS_APPEAR = "Appear";
	public const string G_PARAMS_LOCK_ON = "LockOn";
	public const string G_PARAMS_LOCK_ON_IMMEDIATE = "LockOnImmediate";

	public const string G_PARAMS_SKILL_CAST = "SkillCast";
	public const string G_PARAMS_SKILL_FIRE = "SkillFire";
	public const string G_PARAMS_SKILL_FINISH = "SkillFinish";

	public const string G_PARAMS_IS_AIR = "IsAir";
	public const string G_PARAMS_IS_MOVE = "IsMove";
	public const string G_PARAMS_IS_ROTATE = "IsRotate";
	public const string G_PARAMS_IS_BATTLE = "IsBattle";
	public const string G_PARAMS_IS_FREEZE = "IsFreeze";
	public const string G_PARAMS_IS_CONTINUING_SKILL = "IsContinuingSkill";

	public const string G_PARAMS_RESTART = "Restart";
	public const string G_PARAMS_SKILL_TYPE = "SkillType";

	public const string G_PARAMS_OPEN_ANI_TYPE = "OpenAniType";
	public const string G_PARAMS_SKILL_ANI_TYPE = "SkillAniType";
	public const string G_PARAMS_WEAPON_ANI_TYPE = "WeaponAniType";
	// 매개 변수 }

	// 이름 {
	public const string G_FX_N_LOCK_ON = "Reticle";
	public const string G_FX_N_ATTACK_RANGE = "AttackRange";

	public const string G_BG_N_TEMP_CUBE = "01-BG_TempCube";
	public const string G_BG_N_PLAYER_POS = "BG_PlayerPos";
	public const string G_BG_N_ITEM_GROUND_WEAPON = "ITEM_GROUND_WEAPON";
	public const string G_BG_N_ITEM_GROUND_MATERIAL = "ITEM_GROUND_MATERIAL";
	// 이름 }

	// 애니메이션
	public static readonly List<EWeaponAnimationType> G_THROW_WEAPON_ANI_TYPE_LIST = new List<EWeaponAnimationType>()
	{
		EWeaponAnimationType.Grenade,
		EWeaponAnimationType.Mortar
	};

	// 경로 {
	public static readonly Dictionary<EItemType, Dictionary<int, string>> G_PREFAB_N_GROUND_ITEM_MATERIAL_DICT = new Dictionary<EItemType, Dictionary<int, string>>()
	{
		[EItemType.Currency] = new Dictionary<int, string>()
		{
			[1] = "ITEM_MATERIAL_CURRENCY_HARDP_01",
			[2] = "ITEM_MATERIAL_CURRENCY_HARDP_01",
			[3] = "ITEM_MATERIAL_CURRENCY_SOFT_01",
			[6] = "ITEM_MATERIAL_CURRENCY_TOOL_01",
			[7] = "ITEM_MATERIAL_CURRENCY_PMATERIAL_01"
		}
	};

	public const string G_BG_PATH_WALKABLE = "Prefabs/BG/01-Walkable/";
	public const string G_BG_PATH_NOT_WALKABLE = "Prefabs/BG/02-NotWalkable/";
	public const string G_BG_PATH_DECORATION = "Prefabs/BG/03-Decoration/";
	public const string G_BG_PATH_ETC = "Prefabs/BG/04-Etc/";
	public const string G_BG_PATH_TEMP = "Prefabs/BG/05-Temp/";
	public const string G_BG_PATH = "Prefabs/BG/";
	public const string G_ETC_PATH = "Prefabs/Etc/";

	public const string G_TABLE_P_MAP_INFO = "Tables/Global/MapInfo/G_MapInfoTable";
	public const string G_TABLE_P_MAP_OBJ_TEMPLATE_INFO = "Tables/Global/G_MapObjTemplateInfoTable";

	public const string G_DATA_P_G_MAP_EDITOR_PREFAB_GROUPS = "Datas/Global/MapEditorPrefabGroups";
	public const string CHARACTER_EXTRA_PATH_NPC = "01-NPC/";

	public static readonly string G_DATA_P_FMT_MAP_INFO = $"Tables/Global/MapInfo/G_MapInfo_{ComType.G_KEY_FMT_MAP_INFO}";
	public static readonly string G_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD = $"G_MapInfo_{ComType.G_KEY_FMT_MAP_INFO}";

#if UNITY_IOS || UNITY_ANDROID
	public static string G_DIR_P_WRITABLE => $"{Application.persistentDataPath}/{Application.identifier}/";
#else
	public static string G_DIR_P_WRITABLE => $"{Application.persistentDataPath}/PersistentDatas/";
#endif // #if UNITY_IOS || UNITY_ANDROID

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
	public static string G_ABS_DIR_P_EXTERNAL_DATAS => $"{Application.dataPath}/../ExternalDatas/";

#if UNITY_STANDALONE_WIN
	public static string G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS => $"{Application.dataPath}/../ExternalDatas/";
#else
	public static string G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS => $"{Application.dataPath}/../../ExternalDatas/";
#endif // #if UNITY_STANDALONE_WIN
#else
	public static string G_ABS_DIR_P_EXTERNAL_DATAS => ComType.G_DIR_P_WRITABLE;
	public static string G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS => ComType.G_DIR_P_WRITABLE;
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)

#if UNITY_EDITOR || UNITY_STANDALONE
	public static readonly string G_RUNTIME_TABLE_P_MAP_INFO = $"{ComType.G_ABS_DIR_P_EXTERNAL_DATAS}{ComType.G_TABLE_P_MAP_INFO}.json";
	public static readonly string G_RUNTIME_TABLE_P_MAP_OBJ_TEMPLATE_INFO = $"{ComType.G_ABS_DIR_P_EXTERNAL_DATAS}{ComType.G_TABLE_P_MAP_OBJ_TEMPLATE_INFO}.json";

	public static readonly string G_RUNTIME_DATA_P_FMT_MAP_INFO = $"{ComType.G_ABS_DIR_P_EXTERNAL_DATAS}{ComType.G_DATA_P_FMT_MAP_INFO}.json";
	public static readonly string G_RUNTIME_DATA_P_MAP_EDITOR_PREFAB_GROUPS = $"{ComType.G_ABS_DIR_P_EXTERNAL_DATAS}{ComType.G_DATA_P_G_MAP_EDITOR_PREFAB_GROUPS}.json";
	public static readonly string G_RUNTIME_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD = $"{ComType.G_ABS_DIR_P_EXTERNAL_DATAS}../../Server/Data/Map/{ComType.G_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD}.json";
#else
	public static readonly string G_RUNTIME_TABLE_P_MAP_INFO = $"{ComType.G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS}{ComType.G_TABLE_P_MAP_INFO}.json";

	public static readonly string G_RUNTIME_DATA_P_FMT_MAP_INFO = $"{ComType.G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS}{ComType.G_DATA_P_FMT_MAP_INFO}.json";
	public static readonly string G_RUNTIME_DATA_P_MAP_EDITOR_PREFAB_GROUPS = $"{ComType.G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS}{ComType.G_DATA_P_G_MAP_EDITOR_PREFAB_GROUPS}.json";
	public static readonly string G_RUNTIME_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD = $"{ComType.G_ABS_DIR_P_RUNTIME_EXTERNAL_DATAS}../../Server/Data/Map/{ComType.G_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD}.json";
#endif // #if UNITY_EDITOR || UNITY_STANDALONE
	// 경로 }
	#endregion // 기본

	#region 조건부 상수
#if UNITY_EDITOR || UNITY_STANDALONE
	// 경로
	public const string G_TOOL_P_SHELL = "/bin/zsh";
	public const string G_TOOL_P_CMD_PROMPT = "cmd.exe";

	// 커맨드 라인 {
	public const string G_CMD_LINE_PARAMS_FMT_SHELL = "-c \"{0}\"";
	public const string G_CMD_LINE_PARAMS_FMT_CMD_PROMPT = "/c \"{0}\"";

	public const string G_BUILD_CMD_INTEL_EXPORT_PATH = "export PATH=\"${PATH}:/usr/local/bin\"";
	public const string G_BUILD_CMD_SILICON_EXPORT_PATH = "export PATH=\"${PATH}:/opt/homebrew/bin\"";
	// 커맨드 라인 }
#endif // #if UNITY_EDITOR || UNITY_STANDALONE
	#endregion // 조건부 상수
}
