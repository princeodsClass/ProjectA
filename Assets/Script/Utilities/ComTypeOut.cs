using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static partial class ComType
{
	public const string GATEKEEPER_URL = "http://3.34.40.127:29661/";

	public const string SERVER_CONTROLLER_DATATABLE_VERSION = "checkversion";
	public const string SERVER_CONTROLLER_DATATABLE_DOWNLOAD = "datatable";
	public const string SERVER_CONTROLLER_MAP_DOWNLOAD = "mapdata";

	public const string SERVER_ACCOUNT_POST_PATH = "account";
	public const string SERVER_INVENTORY_POST_PATH = "inventory";
	public const string SERVER_SHOP_POST_PATH = "shop";
	public const string SERVER_MAP_POST_PATH = "map";
	public const string SERVER_ETC_POST_PATH = "etc";
	public const string SERVER_ABYSS_POST_PATH = "abyss";

	public const string MARKET_URL_AOS = "https://play.google.com/store/apps/details?id=com.ninetap.gunshotrumblebeside";
	public const string MARKET_URL_iOS = "https://itunes.apple.com/app/id6476123067";

	public static string[] AudioMixPaths = new string[]
	{
		"Master/SFX/ETC",
		"Master/SFX/UI",
		"Master/SFX/Notice",
		"Master/SFX/Voice",
		"Master/SFX/Gacha",
		"Master/SFX/Battle",
		"Master/Music/BGM",
		"Master/Music/Movie",
	};

	public static readonly string BGM_MIX = "Master/Music/BGM";
	public static readonly string UI_MIX = "Master/SFX/UI";
	public static readonly string BATTLE_MIX = "Master/SFX/Battle";

	public const string STORAGE_OPTION = "Option";
	public const string STORAGE_TOKEN = "accessToken";
	public const string STORAGE_UID = "auid";
	public const string STORAGE_MAIL = "mail";
	public const string STORAGE_LEVEL = "level";
	public const string STORAGE_EXP = "exp";

	public const string STORAGE_TERMS_AGREE = "TermsAgree";
	public const string STORAGE_DATAVERSION = "DataVersion";

	public const string STORAGE_LANGUAGE_INT = "LanguageInt";

	public const string OBJECT_ROOT_NAME = "ObjsRoot";
	public const string LEVEL_LOADER_NAME = "LevelLoader";
	public const string MAP_OBJECT_ROOT_NAME_FMT = "MapObjsRoot_{0:00}";

	public const string OBJECT_PATH_LEVEL = "Prefabs/Level/";

	public const string UI_ROOT_NAME = "UIsRoot";
	public const string UI_ROOT_PAGE = "Page";
	public const string UI_ROOT_ABOVE = "Above";
	public const string UI_ROOT_POPUP = "Popup";

	public const string UI_PATH = "Prefabs/UI/Common/";
	public const string UI_PATH_PAGE = "Prefabs/UI/Page/";
	public const string UI_PATH_POPUP = "Prefabs/UI/Popup/";
	public const string UI_PATH_COMPO = "Prefabs/UI/Slot/";
	public const string UI_PATH_BATTLE = "Prefabs/UI/Battle/";
	public const string UI_PATH_BUTTON = "Prefabs/UI/Button/";
	public const string UI_PATH_ETC = "Prefabs/UI/ETCComponent/";

	public const string CHARACTER_PATH = "Prefabs/Character/";
	public const string WEAPON_PATH = "Prefabs/Weapons/";
	public const string GEAR_PATH = "Prefabs/Gears/";
	public const string BOX_PATH = "Prefabs/Boxes/";
	public const string ATLAS_PATH = "SpriteAtlas/";
	public const string AUDIO_PATH = "Audios/";
	public const string MATERIAL_PATH = "Materials/";
	public const string OBJECT_PATH = "Objects/";
	public const string TEXTURE_PATH = "Textures/";
	public const string DATA_PATH = "Data";

	public const string EFFECT_PATH = "Prefabs/FX/";
	public const string EFFECT_PATH_BG = "Prefabs/FX/BG/";
	public const string EFFECT_PATH_CH = "Prefabs/FX/CH/";
	public const string EFFECT_PATH_UI = "Prefabs/FX/UI/";

	public const string APP_VERSION = "ApplicationVersion";

	public const int MAX_DISPLAY_GOODS = 999999;
	public const int MAX_SLOT_VOLUME = 999;

	public const int FILE_NAME_LEN_MAX = 127;

	public const int KEY_ITEM_GOLD					= 0x221C3001;
	public const int KEY_ITEM_CRYSTAL_FREE			= 0x221C2001;
	public const int KEY_ITEM_CRYSTAL_PAY			= 0x221C1001;
	public const int KEY_ITEM_TOKEN_NORMALWEAPON	= 0x221B1001;
	public const int KEY_ITEM_TOKEN_REREWEAPON		= 0x221B2001;
	public const int KEY_ITEM_TOKEN_MATERIAL		= 0x221B3001;

    public const int KEY_BOX_G1 = 0x241E1011;
    public const int KEY_BOX_G2 = 0x242E1011;
    public const int KEY_BOX_G3 = 0x243E1011;
    public const int KEY_BOX_G4 = 0x244E1011;

    public const string DEFAULT_NICKNAME = "GUEST";

	public const string NAME_WEAPON_DUMMY = "Dummy_WeaponHand";

	public const string SHOP_DAILY_GOODS_LIST		= "Shop_Daily_Goods_List";
	public const string SHOP_ADS_GOODS_LIST			= "Shop_Ads_Goods_List";
	public const string SHOP_WEAPON_BOX_START_TIME	= "Shop_Weapon_Box_Start_Time";
	public const string SHOP_WEAPON_BOX_RESET_COUNT	= "Shop_Weapon_Box_Reset_Count";
	public const string SHOP_WEAPON_BOX_RESET_TIME	= "Shop_Weapon_Box_Reset_Time";

	public const string EXCEPTION_ACCESSTOKEN		= "accessToken does not match";

	// Iron Source {
#if UNITY_IOS
	public const string IRON_SRC_APP_KEY = "1b856a435";
#else
	public const string IRON_SRC_APP_KEY = "1f0b92465";
#endif // #if UNITY_IOS

	public static readonly List<string> IRON_SRC_ADS_ID_LIST = new List<string>() {
		IronSourceAdUnits.REWARDED_VIDEO
	};
	// Iron Source }

	// Apps Flyer
	public const string APPS_FLYER_APP_ID = "6476123067";
	public const string APPS_FLYER_DEV_KEY = "J7eXAem8sBRuHTr3iX58d5";
}
