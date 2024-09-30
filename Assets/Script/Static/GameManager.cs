using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Newtonsoft.Json.Linq;
using System.Globalization;

public class GameManager : SingletonMono<GameManager>
{
	public UserAccount user { get; private set; }
	public Dictionary<long, OtherUser> otherUser { get; set; }
	public InventoryData<ItemMaterial> invenMaterial { get; private set; }
	public InventoryData<ItemWeapon> invenWeapon { get; private set; }
	public InventoryData<ItemGear> invenGear { get; private set; }
	public InventoryData<ItemBox> invenBox { get; private set; }
	public InventoryData<ItemCharacter> invenCharacter { get; private set; }
	public AboveTutorial tutorial { get; private set; }
	public AboveQuestCard questCard { get; private set; }

	public enum GameState
	{
		none = 0,
		title,
		lobby,
		battle,
	}

	public GameState _gameState;

	public Vibration _vibration = null;
	public EventController eventController = null;
	private OptionData m_Option = null;

	public int _curLanguage;
	public int _nPreLevel { get; set; }
	public int _nPreEpisode { get; set; }
	public int _nPreChapter { get; set; }
	public List<long> _liPreBoxID { get; set; } = new List<long>();

	public long _tempWeaponID = 0;

	public string _enKey = string.Empty;

	public struct DeviceSpec
	{
		public int processorFrequency;
		public int processorCount;
		public int systemMemorySize;
		public int graphicsMemorySize;

		public string processorType;
		public GraphicsDeviceType graphicsDeviceType;
	}

	public static DeviceSpec _deviceSpec;
	public bool _isAttendanceShow = false;
	public bool _isShowAlertGear = false;
    public bool _isShowAlertWeapon = false;

    Dictionary<EQuestActionType, int> _dicQuestCounterKill = new Dictionary<EQuestActionType, int>();

	public void ApplyQuestCounter()
    {
		foreach ( KeyValuePair<EQuestActionType,int> el in _dicQuestCounterKill )
			user.RefreshQuestCount(el.Key, el.Value);

		_dicQuestCounterKill.Clear();
	}

	public void SetQuestCounter(EQuestActionType type, int count)
    {
		if (_dicQuestCounterKill.ContainsKey(type))
			_dicQuestCounterKill[type] += count;
		else
			_dicQuestCounterKill.Add(type, count);
    }

	/** 탐험 키 개수를 증가시킨다 */
	public void IncrAdventureKeyCount(int a_nNumKeys)
	{
		int nMaxKeyCount = GlobalTable.GetData<int>(ComType.G_COUNT_MAX_ADVENTURE_KEY);
		int nPrevKeyCount = this.user.m_nCurrentAdventureKeyCount;

		this.user.m_nCurrentAdventureKeyCount = Mathf.Clamp(nPrevKeyCount + a_nNumKeys, 0, nMaxKeyCount);

		// 키 충전이 필요 할 경우
		if (nPrevKeyCount >= nMaxKeyCount && this.IsEnableChargeAdventureKey())
		{
			this.user.m_dtAdventureKeyStart = System.DateTime.UtcNow;
		}
	}

	/** 탐험 미션 시작 가능 여부를 검사한다 */
	public bool IsEnableStartMissionAdventure()
	{
		int nResetCycle = GlobalTable.GetData<int>(ComType.G_TIME_ADVENTURE_RESET);
		var stFinishTime = GameManager.Singleton.user.m_dtStartAdventure.AddMilliseconds(nResetCycle);

		var stDeltaTime = stFinishTime - System.DateTime.UtcNow;
		return stDeltaTime.TotalSeconds.ExIsLessEquals(0.0f);
	}

	/** 탐험 미션 키 충전 가능 여부를 검사한다 */
	public bool IsEnableChargeAdventureKey()
	{
		int nMaxNumKeys = GlobalTable.GetData<int>(ComType.G_COUNT_MAX_ADVENTURE_KEY);
		return this.user.m_nCurrentAdventureKeyCount < nMaxNumKeys;
	}

	public void Awake()
	{
		CheckDevice();
		Screen.sleepTimeout = SleepTimeout.SystemSetting;

		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;
		
		#region 추가
		ComUtil.SetTimeScale(1.0f);
		#endregion // 추가

		if (PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT) == 0)
			ComUtil.DetectLanguage();

		SetLanguage();

		_gameState = GameState.none;

        _vibration = new Vibration();
        _vibration.Initialize();

		user = new UserAccount();
		invenMaterial = new InventoryData<ItemMaterial>();
		invenWeapon = new InventoryData<ItemWeapon>();
		invenGear = new InventoryData<ItemGear>();
		invenBox = new InventoryData<ItemBox>();
		invenCharacter = new InventoryData<ItemCharacter>();

		otherUser = new Dictionary<long, OtherUser>();

		InitializeTutorial();
		InitializeQuestCard();
		CheckOption();

		StartCoroutine(Timer());
	}

	private IEnumerator Timer()
	{
		while (true)
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime nextMidnight = utcNow.Date.AddDays(1);

			TimeSpan timeUntilMidnight = nextMidnight - utcNow;

			yield return new WaitForSecondsRealtime((float)timeUntilMidnight.TotalSeconds);

			OnMidnight();
		}
	}

	private void OnMidnight()
	{
		PopupQuest popup = FindObjectOfType<PopupQuest>();

		if (popup == null)
			popup = new PopupQuest();

		StartCoroutine(popup.SetQuest());
	}

	public void InitializeTutorial()
    {
		if ( null == tutorial )
        {
			tutorial = MenuManager.Singleton.OpenAbove<AboveTutorial>(EUIPage.AboveTutorial);
		}

		tutorial.Activate(false);
	}

	public void InitializeQuestCard()
    {
		if ( null == questCard )
		{
			questCard = MenuManager.Singleton.OpenAbove<AboveQuestCard>(EUIPage.AboveQuestCard);
		}
	}

	void CheckDevice()
	{
#if DEBUG
		_deviceSpec.processorCount = SystemInfo.processorCount;
		_deviceSpec.processorFrequency = SystemInfo.processorFrequency;

		_deviceSpec.systemMemorySize = SystemInfo.systemMemorySize;
		_deviceSpec.graphicsMemorySize = SystemInfo.graphicsMemorySize;

		_deviceSpec.processorType = SystemInfo.processorType;
		_deviceSpec.graphicsDeviceType = SystemInfo.graphicsDeviceType;

		Log($"processorCount : {_deviceSpec.processorCount}");
		Log($"processorFrequency : {_deviceSpec.processorFrequency}");
		Log($"systemMemorySize : {_deviceSpec.systemMemorySize}");
		Log($"graphicsMemorySize : {_deviceSpec.graphicsMemorySize}");
		Log($"processorType : {_deviceSpec.processorType}");
		Log($"graphicsDeviceType : {_deviceSpec.graphicsDeviceType}");
#endif
	}

	public Dictionary<string, int> GetIAPID()
	{
		// List<string> returnValue = new List<string>();
		Dictionary<string, int> returnValue = new Dictionary<string, int>();

		List<CrystalDealTable> crystal = CrystalDealTable.GetList();
		List<VIPDealTable> vip = VIPDealTable.GetList();
		List<PassDealTable> pass = PassDealTable.GetList();

#if UNITY_ANDROID
		crystal.ForEach(t => returnValue.Add(t.IAPAOS, t.RewardsBonus));
		vip.ForEach(t =>
		{
			if ( !returnValue.ContainsKey(t.IAPAOS_Original) )
				returnValue.Add(t.IAPAOS_Original, t.RewardsBonus);
			if ( !returnValue.ContainsKey(t.IAPAOS) )
				returnValue.Add(t.IAPAOS, t.RewardsBonus);
		});
		pass.ForEach(t => returnValue.Add(t.IAPAOS, t.RewardsBonus));
#elif UNITY_IOS
        crystal.ForEach(t => returnValue.Add(t.IAPiOS, t.RewardsBonus));
        vip.ForEach(t =>
		{
			if ( !returnValue.ContainsKey(t.IAPiOS_Original) )
				returnValue.Add(t.IAPiOS_Original, t.RewardsBonus);
			if ( !returnValue.ContainsKey(t.IAPiOS) )
				returnValue.Add(t.IAPiOS, t.RewardsBonus);
		});
		pass.ForEach(t => returnValue.Add(t.IAPiOS, t.RewardsBonus));
#endif

		return returnValue;
	}

	public void EventStart()
	{
		eventController = new EventController();
		eventController.Initialize();
	}

	public void SetLanguage()
	{
		_curLanguage = PlayerPrefs.GetInt(ComType.STORAGE_LANGUAGE_INT);
	}

	public static void Log(string msg, string color = "white")
	{
#if DEBUG || DEVELOPMENT_BUILD
		Debug.Log($"<color={color}>{msg}</color>");
#endif
	}

	public static void Log(string titleColor, string title, string msgColor, string msg)
	{
#if DEBUG || DEVELOPMENT_BUILD
		Debug.Log($"<color={titleColor}>[{title}]</color> : <color={msgColor}>{msg}</color>");
#endif
	}

	public void InitializeInventory(string result)
	{
		//if ( string.IsNullOrEmpty(result) ) return;

		//ClearInventory();

		JArray jInven = JArray.Parse(result);

		foreach (JObject item in jInven)
		{
			AddItemSC(item);
		}
	}

	public IEnumerator EquipWeapon(int slot, long id, int slot2, long id2)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ $"equipWeapon0{slot}", id2.ToString() },
			{ $"equipWeapon0{slot2}", id.ToString() },
		};

		yield return StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			user.Initialize(jResult);

			if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			{
				GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeWeapon();
				GameObject.Find("Character").GetComponentInChildren<PlayerController>().InitializeEquipWeapon(slot2);
			}
		}));
	}

	public IEnumerator EquipWeapon(int slot, long id, System.Action a_oCallback = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ $"equipWeapon0{slot}", id.ToString() },
		};

		yield return StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			user.Initialize(jResult);

			if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			{
				GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeWeapon();
				GameObject.Find("Character").GetComponentInChildren<PlayerController>().InitializeEquipWeapon(slot);
			}

			a_oCallback?.Invoke();
		}));
	}

	public IEnumerator EquipGear(int type, long id)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ $"equipGear0{type}", id.ToString() },
		};

		yield return StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			user.Initialize(jResult);

			if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			{
				GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeGear();
			}
		}));
	}

	public IEnumerator EquipCharacter(long id)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ $"currentCharacter", id.ToString() },
		};

		yield return StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EAccountPostType.modify_account, fields, (result) =>
		{
			JObject jResult = JObject.Parse(result);

			user.Initialize(jResult);
		}));
	}

	public IEnumerator AccountLevelUp(int level)
	{
		// to do : 계정 정보 수정을 추가하자.

		// 여기부터는 아이템 지급

		AccountLevelTable tarLevel = AccountLevelTable.GetData((uint)(0x01000000 + level));

		if (tarLevel.RewardCharacterKey > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewardCharacterKey, 1));

		if (tarLevel.RewardWeaponKey > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewardWeaponKey, 1));

		if (tarLevel.RewarditemKey00 > 0 && tarLevel.Rewarditemcount00 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey00, tarLevel.Rewarditemcount00));

		if (tarLevel.RewarditemKey01 > 0 && tarLevel.Rewarditemcount01 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey01, tarLevel.Rewarditemcount01));

		if (tarLevel.RewarditemKey02 > 0 && tarLevel.Rewarditemcount02 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey02, tarLevel.Rewarditemcount02));

		if (tarLevel.RewarditemKey03 > 0 && tarLevel.Rewarditemcount03 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey03, tarLevel.Rewarditemcount03));

		if (tarLevel.RewarditemKey04 > 0 && tarLevel.Rewarditemcount04 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey04, tarLevel.Rewarditemcount04));

		if (tarLevel.RewarditemKey05 > 0 && tarLevel.Rewarditemcount05 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey05, tarLevel.Rewarditemcount05));

		if (tarLevel.RewarditemKey06 > 0 && tarLevel.Rewarditemcount06 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey06, tarLevel.Rewarditemcount06));

		if (tarLevel.RewarditemKey07 > 0 && tarLevel.Rewarditemcount07 > 0)
			yield return StartCoroutine(AddItemCS(tarLevel.RewarditemKey07, tarLevel.Rewarditemcount07));

		yield return null;
	}

	public IEnumerator AddItemCS(uint primaryKey, int count, Action callback = null)
	{
		Dictionary<string, string> baseFields = new Dictionary<string, string>
		{
			{ "accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN) },
			{ "auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString() },
			{ "table", EDatabaseType.inventory.ToString() },
			{ "createDatetime", ComUtil.EnUTC() },
		};

		switch (ComUtil.GetItemType(primaryKey))
		{
			case EItemType.Weapon:
				for ( int i = 0; i < count; i++ )
				{
					Dictionary<string, string> fields = new Dictionary<string, string>(baseFields);
					yield return StartCoroutine(AddWeaponCS(fields, primaryKey));
				}
				break;
			case EItemType.Gear:
				for ( int i = 0; i < count; i++ )
                {
					Dictionary<string, string> fields = new Dictionary<string, string>(baseFields);
					yield return StartCoroutine(AddGearCS(fields, primaryKey));
				}
				break;
			case EItemType.Character:
				for ( int i = 0; i < count; i++ )
				{
					Dictionary<string, string> fields = new Dictionary<string, string>(baseFields);
					yield return StartCoroutine(AddCharacterCS(fields, primaryKey));
				}
				break;
			case EItemType.Material:
				yield return StartCoroutine(AddmaterialCS(baseFields, primaryKey, count));
				break;
		}

		if ( null != callback ) callback.Invoke();
	}

	IEnumerator AddmaterialCS(Dictionary<string, string> fields, uint primaryKey, int count)
    {
		bool isNew = true;

		foreach (ItemMaterial material in invenMaterial)
		{
			if (primaryKey == material.nKey)
			{
				isNew = false;
				count += material.nVolume;
				fields.Add("id", material.id.ToString());
				fields.Add("count", count.ToString());

				break;
			}
		}

		if (isNew)
		{
			fields.Add("count", count.ToString());
			fields.Add("primaryKey", primaryKey.ToString());
			yield return StartCoroutine(GameDataManager.Singleton.InsertItem(fields));
		}
		else
		{
			// fields.Add("table", EDatabaseType.inventory.ToString());
			yield return StartCoroutine(GameDataManager.Singleton.ModifyItem(fields));
		}
	}

	IEnumerator AddCharacterCS(Dictionary<string, string> fields, uint primaryKey)
	{
		fields.Add("count", "1");
		fields.Add("primaryKey", primaryKey.ToString());

		yield return StartCoroutine(GameDataManager.Singleton.InsertCharacter(fields));
	}

	IEnumerator AddGearCS(Dictionary<string, string> fields, uint primaryKey)
	{
		GearTable ge = new GearTable();
		ge = GearTable.GetData(primaryKey);

		if (0 < ge.EquipEffectGroup || 0 < ge.EquipEffectSelectionCount)
		{
			int c = 0;

			Dictionary<EffectTable, float> eft = new Dictionary<EffectTable, float>();
			eft = EffectGroupTable.RandomEffectInGroup(ge.EquipEffectGroup, ge.EquipEffectSelectionCount);

			foreach (KeyValuePair<EffectTable, float> e in eft)
			{
				fields.Add($"effect0{c}", e.Key.PrimaryKey.ToString());
				fields.Add($"effectValue0{c}", e.Value.ToString(CultureInfo.InvariantCulture));
                c++;
			}
		}

		fields.Add("count", "1");
		fields.Add("primaryKey", primaryKey.ToString());

		yield return StartCoroutine(GameDataManager.Singleton.InsertItem(fields));
	}

	IEnumerator AddWeaponCS(Dictionary<string, string> fields, uint primaryKey)
	{
		WeaponTable we = new WeaponTable();
		we = WeaponTable.GetData(primaryKey);

		if (0 < we.EquipEffectGroup)
		{
			int c = 0;

			Dictionary<EffectTable, float> eft = new Dictionary<EffectTable, float>();
			eft = EffectGroupTable.RandomEffectInGroup(we.EquipEffectGroup, we.EquipEffectSelectionCount);

			foreach (KeyValuePair<EffectTable, float> e in eft)
			{
				fields.Add($"effect0{c}", e.Key.PrimaryKey.ToString());
				fields.Add($"effectValue0{c}", e.Value.ToString(CultureInfo.InvariantCulture));
                c++;
			}
		}

		fields.Add("count", "1");
		fields.Add("primaryKey", primaryKey.ToString());

		yield return StartCoroutine(GameDataManager.Singleton.InsertItem(fields));
	}

	public void AddItemSC(JObject item, bool isNew = false)
	{
		int upgrade, reinforce, limitbreak;
		bool isLock;

		int count = 0;
		int id = (int)item.GetValue("id");
		uint primaryKey = (uint)item.GetValue("primaryKey");

		string type = primaryKey.ToString("X").Substring(0, 2);

		uint[] effectkey = new uint[6];
		int[] effectfixxed = new int[6];

		float[] effectvalue = new float[6];

		switch (type)
		{
			case "20": // weapon
				_tempWeaponID = id;

				count = (int)item.GetValue("count");

				if ( count < 1 ) break;

				upgrade = (int)item.GetValue("upgrade");
				reinforce = (int)item.GetValue("reinforce");
				limitbreak = (int)item.GetValue("limitbreak");
				isLock = (bool)item.GetValue("isLock");

				for (int i = 0; i < 6; i++)
				{
					effectkey[i] = (uint)item.GetValue($"effect0{i}");
					effectfixxed[i] = (int)item.GetValue($"effectIsFixed0{i}");

					effectvalue[i] = (float)item.GetValue($"effectValue0{i}");
				}

				AddWeaponSC(id, primaryKey, count, upgrade, reinforce, limitbreak,
							isLock, isNew, effectkey, effectvalue, effectfixxed);
				break;
			case "22": // material
				count = (int)item.GetValue("count");

				if ( count < 0 ) break;

				AddMaterialSC(id, primaryKey, count);
				break;
			case "23": // gear
				count = (int)item.GetValue("count");

				if ( count < 1 ) break;

				upgrade = (int)item.GetValue("upgrade");
				reinforce = (int)item.GetValue("reinforce");
				limitbreak = (int)item.GetValue("limitbreak");
				isLock = (bool)item.GetValue("isLock");

				for (int i = 0; i < 6; i++)
				{
					effectkey[i] = (uint)item.GetValue($"effect0{i}");
					effectfixxed[i] = (int)item.GetValue($"effectIsFixed0{i}");

					effectvalue[i] = (float)item.GetValue($"effectValue0{i}");
				}

				AddGearSC(id, primaryKey, count, upgrade, reinforce, limitbreak,
						  isLock, isNew, effectkey, effectvalue, effectfixxed);
				break;
			case "24": // box
				isNew = item.TryGetValue("isNew", out JToken isNewToken) ? (bool)isNewToken : false;
				count = (int)item.GetValue("slotNumber");
				DateTime openDatetime = string.IsNullOrEmpty(item.GetValue("openDatetime").ToString()) ?
										DateTime.MinValue :
										((DateTime)item.GetValue("openDatetime"));
				AddBoxSC(id, primaryKey, count, openDatetime, isNew);
				break;
			case "11": // character
				upgrade = (int)item.GetValue("upgrade");
				AddCharacterSC(id, primaryKey, upgrade);
				break;
		}
	}

	public void AddWeaponSC(int id, uint primaryKey, int count, int countUpgrade, int countReinforce, int countLimitbreak,
							bool isLock, bool isNew, uint[] effectkey = null, float[] effectvalue = null, int[] effectfixxed = null)
	{
		StartCoroutine(AddWeapon(id, primaryKey, count, countUpgrade, countReinforce, countLimitbreak,
								 isLock, isNew, effectkey, effectvalue, effectfixxed));
	}

	IEnumerator AddWeapon(int id, uint primaryKey, int count, int countUpgrade, int countReinforce, int countLimitbreak,
						  bool isLock, bool isNew, uint[] effectkey = null, float[] effectvalue = null, int[] effectfixxed = null)
	{
		foreach (ItemWeapon weapon in invenWeapon)
		{
			if (weapon.id == id)
			{
				Log($"duplicate ID : {weapon.id} / {weapon.nKey}", "red");
				yield break;
			}
		}

		invenWeapon.AddItem(id, primaryKey, count, countUpgrade, countReinforce, countLimitbreak, isLock,
							default, isNew, effectkey, effectvalue, effectfixxed);

		int index = Array.IndexOf(user.m_nWeaponID, 0);

		if (index > -1 && Array.IndexOf(user.m_nWeaponID, id) == -1 )
			yield return StartCoroutine(EquipWeapon(index, id));
		/* 
		if (user.m_nWeaponID.Count(e => e == 0) == 4)
		{
			yield return StartCoroutine(EquipWeapon(0, id));
		}
		*/

		// RefreshInventory(EInvenType.Weapon);

		user.RefreshQuestCount(EQuestActionType.ObtainWeapon, 1);
	}

	public void AddGearSC(int id, uint primaryKey, int count, int countUpgrade, int countReinforce, int countLimitbreak,
						  bool isLock, bool isNew, uint[] effectkey = null, float[] effectvalue = null, int[] effectfixxed = null)
	{
		foreach (ItemGear gear in invenGear)
		{
			if (gear.id == id)
			{
				Log($"duplicate ID : {gear.id} / {gear.nKey}", "red");
				return;
			}
		}

		invenGear.AddItem(id, primaryKey, count, countUpgrade, countReinforce, countLimitbreak, isLock,
						  default, isNew, effectkey, effectvalue, effectfixxed);
	}

	public void AddMaterialSC(int id, uint primaryKey, int count)
	{
		if (false == MaterialTable.IsContainsKey(primaryKey)) return;

		MaterialTable tt = MaterialTable.GetData(primaryKey);
		int tempCount = count - invenMaterial.GetItemCount(id);

		if ( invenMaterial.ContainID(id) )
        {
			ItemMaterial material = invenMaterial.GetItem(id);

			if (material.nMaxStackCount < count)
				Log("Exceeded maximum amount.", "red");
			else
				material.Initialize(id, primaryKey, count);
		}
		else
        {
			invenMaterial.AddItem(id, primaryKey, count);
		}

		/*
		foreach (ItemMaterial material in invenMaterial)
		{
			if (material.id == id)
			{
				if (material.nMaxStackCount < count)
					Log("Exceeded maximum amount.", "red");
				else
                {
					material.Initialize(id, primaryKey, count);
					SetADSSlotMaker(primaryKey);

					if (tt.Type == 0xD || tt.Type == 0x9)
						user.RefreshQuestCount(EQuestActionType.ObtainScrap, count);
				}
				return;
			}
		}

		if ( invenMaterial.ContainKey(primaryKey) == -1 )
			invenMaterial.AddItem(id, primaryKey, count);
		*/

		SetADSSlotMarker(primaryKey);

		if ( tempCount > 0 && ( tt.Type == 0xD || tt.Type == 0x9 ) )
			user.RefreshQuestCount(EQuestActionType.ObtainScrap, tempCount);

		// RefreshInventory(EInvenType.Material);
	}

	void SetADSSlotMarker(uint key)
    {
		if (key == GlobalTable.GetData<uint>("valueAdsRemoveTicketDecKey"))
			GameObject.Find("ComShopADS")?.GetComponent<ComShopADS>().SetSlotMaker();
	}

	public void AddBoxSC(int id, uint primaryKey, int count, DateTime openDatetime, bool isNew = false)
	{
		ItemBox temp = null;

		if (invenBox.GetItemCount() >= 4 && !invenBox.ContainID(id))
		{
			Log("slot full", "red");
			return;
		}

		foreach (ItemBox box in invenBox)
		{
			if (box.id == id)
			{
				if (box.nSlotNumber == count)
				{
					box.Initialize(id, primaryKey, count, openDatetime, isNew);
					break;
				}
				else
				{
					Log("slot Error", "red");
					return;
				}
			}
		}

		if (temp == null)
			invenBox.AddItem(id, primaryKey, count, 0, 0, 0, false, openDatetime, isNew);

		RefreshBox();

	}

	public void AddCharacterSC(int id, uint primaryKey, int countEnchant, bool isNew = false)
	{
		StartCoroutine(AddCharacter(id, primaryKey, countEnchant, isNew));
	}

	IEnumerator AddCharacter(int id, uint primaryKey, int countEnchant, bool isNew = false)
	{
		if ( -1 < invenCharacter.ContainKey(primaryKey) )
        {
			Log("duplicate Character ( primaryKey )", "red");
			yield break;
		}
		else if ( invenCharacter.ContainID(id) )
        {
			Log("duplicate Character ( id )", "red");
			yield break;
		}

		ItemCharacter.stSkillUpgrade upInfo = new ItemCharacter.stSkillUpgrade();
		JObject jResult;

		if ( user.m_nCharacterID == 0 )
			yield return StartCoroutine(EquipCharacter(id));

		if (false == isNew)
        {
			yield return StartCoroutine(GameDataManager.Singleton.GetCharacterSkillUpgradeInfo(id, (result) =>
			{
				jResult = JObject.Parse(result);

				upInfo.Upgrade05 = (int)jResult.GetValue($"upgrade05");
				upInfo.Upgrade10 = (int)jResult.GetValue($"upgrade10");
				upInfo.Upgrade15 = (int)jResult.GetValue($"upgrade15");
				upInfo.Upgrade20 = (int)jResult.GetValue($"upgrade20");
				upInfo.Upgrade25 = (int)jResult.GetValue($"upgrade25");
				upInfo.Upgrade30 = (int)jResult.GetValue($"upgrade30");
				upInfo.Upgrade35 = (int)jResult.GetValue($"upgrade35");
				upInfo.Upgrade40 = (int)jResult.GetValue($"upgrade40");
				upInfo.Upgrade45 = (int)jResult.GetValue($"upgrade45");
				upInfo.Upgrade50 = (int)jResult.GetValue($"upgrade50");
				upInfo.Upgrade55 = (int)jResult.GetValue($"upgrade55");
				upInfo.Upgrade60 = (int)jResult.GetValue($"upgrade60");
				upInfo.Upgrade65 = (int)jResult.GetValue($"upgrade65");
			}));
		}
		
		invenCharacter.AddCharacter(id, primaryKey, countEnchant, upInfo);
	}

	public void RefreshBox()
	{
		if (MenuManager.Singleton.CurScene != ESceneType.Lobby) return;

		PageLobbyBattle page = GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>();

		page.InitializeBox();
	}

	public enum EInvenType
	{
		All,
		Weapon,
		Gear,
		Material,
		Character,
		Box,
	}

	public void RefreshInventory(EInvenType type)
	{
		if (MenuManager.Singleton.CurScene != ESceneType.Lobby) return;

		PageLobbyInventory page = GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>();

		if (type == EInvenType.All)
		{
			page.InitializeWeapon();
			page.InitializeGear();
			page.InitializeMaterial();
			page.InitializeCharacter();
		}
		else if (type == EInvenType.Weapon)
			page.InitializeWeapon();
		else if (type == EInvenType.Gear)
			page.InitializeGear();
		else if (type == EInvenType.Material)
			page.InitializeMaterial();
		else if (type == EInvenType.Character)
			page.InitializeCharacter();

		page.ReSize();

		GameObject.Find("PopupWeapon")?.GetComponent<PopupWeapon>().RefreshInfo();
		GameObject.Find("PopupGear")?.GetComponent<PopupGear>().RefreshInfo();
	}

	public int IsExistOpeningBox()
	{
		foreach (ItemBox box in invenBox)
		{
			if (box._openStartDatetime > DateTime.MinValue &&
				 box._openStartDatetime.AddMilliseconds(box.nOpenDatetime) > DateTime.UtcNow)
				return box.nSlotNumber;
		}

		return -1;
	}

	public int IsExistReadyBox()
	{
		foreach (ItemBox box in invenBox)
		{
			if (box._openStartDatetime == DateTime.MinValue)
				return box.nSlotNumber;
		}

		return -1;
	}

	public void DeleteItem(JObject item)
	{
		int count = 0;
		int id = (int)item.GetValue("id");
		int primaryKey = (int)item.GetValue("primaryKey");

		string type = primaryKey.ToString("X").Substring(0, 2);

		switch (type)
		{
			case "20":
				count = (int)item.GetValue("count");
				DelWeapon(id);
				break;
			case "22":
				count = (int)item.GetValue("count");
				DelMaterial(id, count);
				break;
			case "23":
				count = (int)item.GetValue("count");
				DelGear(id);
				break;
			case "24":
				count = (int)item.GetValue("slotNumber");
				DateTime openDatetime = string.IsNullOrEmpty(item.GetValue("openDatetime").ToString()) ?
										DateTime.MinValue : (DateTime)item.GetValue("openDatetime");
				DelBox(id);
				break;
		}
	}

	void DelWeapon(long id)
	{
		invenWeapon.RemoveItem(id);

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeWeapon();
	}

	void DelMaterial(long id, int count)
	{
		invenMaterial.RemoveItem(id);

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeMaterial();
	}

	void DelGear(long id)
	{
		invenGear.RemoveItem(id);

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			GameObject.Find("InventoryPage").GetComponent<PageLobbyInventory>().InitializeGear();
	}

	void DelBox(long id)
	{
		invenBox.RemoveItem(id);

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			GameObject.Find("BattlePage").GetComponent<PageLobbyBattle>().DeleteBox(id);
	}

	public OptionData GetOption()
	{
		return m_Option;
	}

	public void SaveOption()
	{
		ComUtil.SavePreference(ComType.STORAGE_OPTION, m_Option);
	}

	public void ResetOption()
	{
		m_Option = new OptionData();
		SaveOption();
		CheckOption();
	}

	public void CheckOption()
	{
		m_Option = ComUtil.LoadPreference<OptionData>(ComType.STORAGE_OPTION);
		if (null == m_Option) m_Option = new OptionData();

#if !UNITY_EDITOR
		QualitySettings.SetQualityLevel(m_Option.m_Game.nQuality);
#endif

		// _vibration.SetVibrateState(m_Option.m_Game.bVibration);

		MenuManager.Singleton.SetCameraOption();
		GameAudioManager.Singleton.SetOption(m_Option.m_Audio);
	}

	public void PauseGame()
	{
		Time.timeScale = 0f;
	}

	public void ResumeGame()
	{
		Time.timeScale = 1f;
	}

	public void ClearInventory()
	{
		invenMaterial.Clear();
		invenWeapon.Clear();
		invenGear.Clear();
		invenBox.Clear();
		invenCharacter.Clear();
	}

	public void Restart()
	{
		ClearInventory();
		SetLanguage();

		Awake();
		MenuManager.Singleton.SceneEnd();
		MenuManager.Singleton.SceneNext(ESceneType.Title);
	}

	private void OnDestroy()
	{
		user = null;

		invenMaterial = null;
		invenWeapon = null;
		invenGear = null;
		invenBox = null;
		invenCharacter = null;
	}

#if DEBUG
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftCurlyBracket)) Time.timeScale -= 0.05f;
		if (Input.GetKeyDown(KeyCode.RightCurlyBracket)) Time.timeScale += 0.05f;
		if (Input.GetKeyDown(KeyCode.Backspace)) Time.timeScale = 1.0f;
	}
#endif
}
