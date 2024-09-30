using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class UserAccount
{
	#region 프로퍼티
	public uint EpisodeKey => ComUtil.GetEpisodeKey(m_nEpisode + 1);
	public bool IsClearEpisode => m_nChapter >= EpisodeTable.GetData(this.EpisodeKey).MaxChapter;

	public int CorrectEpisode => this.IsClearEpisode ? m_nEpisode + 1 : m_nEpisode;
	public int CorrectChapter => this.IsClearEpisode ? 0 : m_nChapter;
	#endregion // 프로퍼티

	public long m_nUID = 0;
	public string m_strAccessToken = string.Empty;
	public string m_strNickname = "Guest";
	public int m_nLevel = 1;
	public int m_nExp = 0;

	public int m_nWSLevel = 1;
	public int m_nWSExp = 0;

	public bool[] m_bBuyDailyDeal;

	public long m_nCharacterID;
	public long[] m_nWeaponID;
	public long[] m_nGearID;

	public int m_nMoney = 0;

	public int m_nEpisode;
	public int m_nChapter;
	public int m_nStage;

	public int m_nHuntLevel;

	public int m_nPassLevel;
	public int m_nPassExp;
	public DateTime m_dtPass;
	public bool[][] m_bpPass = new bool[30][];

	public DateTime m_dtStartAdventure;
	public int m_nAdventureLevel;
	public int m_nCurrentAdventureKeyCount;
	public int m_nAdventureGroup;
	public DateTime m_dtAdventureKeyStart;

	public bool m_bIsElite;
	public bool m_bIsPlus;

	public int m_nMaxWeaponRepository;
	public int m_nMaxGearRepository;

	public DateTime m_dtDailyDealReset;

	public uint[] m_nDailyDealKey;
	public uint[] m_nGameMoneyDealKey;

	public int m_nCountBuyWeaponBox;
	public int m_nCountBuyMateerialBox;
	public int m_nCountBuyPremiumMaterialBox;

	public int m_nCountResetWeaponBox;
	public DateTime m_dtWeaponBoxReset;
	public uint m_nCurrentWeaponBoxRewardIndex;

	public int m_nCountResetWeaponPremiumBox;
	public DateTime m_dtWeaponPremiumBoxReset;
	public uint m_nCurrentWeaponPremiumBoxRewardIndex;

	public DateTime m_dtPMaterialBoxReset;

	public uint[] m_nCurrentPBoxG2Index = new uint[3];
	public uint[] m_nCurrentPBoxG3Index = new uint[3];

	public int m_nCountBuyPMaterialBox;
	public DateTime m_dtEndVip;

	public int[] m_nBuyADSDeal;
	public uint[] m_nADSDealKey;
	public DateTime m_dtADSDealReset;
	public DateTime[] m_dtADSDealSlot = new DateTime[4];

	public int[] m_nAbyssBestLap = new int[100];
	public DateTime m_dtAbyssEnd;
	public DateTime m_dtAbyssAdjustEnd;
	public int m_nAbyssSeason;
	public int m_nAbyssGroup;
	public int m_nAbyssCurRank;
	public bool m_bIsAbyssRewardsReceived;

	public DateTime m_dtAttendanceStart;
	public int m_nAttendanceContinuouslyCount;
	public bool m_bIsAbleToAttendanceReward = false;

	public int m_nCurDefenceDifficulty;

	public long[] m_nQuestID = new long[6];
	public uint[] m_nQuestKey = new uint[6];
	public int[] m_nQuestCount = new int[6];
	public bool[] m_bQuestIsComplete = new bool[6];
	public bool[] m_bUseQuestCard = new bool[6];
	public bool m_bIsRecieveQuestAccouuntRewardsMiddle = false;
	public bool m_bIsRecieveQuestAccouuntRewardsFull = false;

	public int m_nMaxDefeatZombie = 0;

	public float m_nCurrrentBonusPoint = 0;
	public bool m_bIsBonusTime = false;
	public DateTime m_dtBonusEnd = default;

	public string m_strLinkedMail;

	public int m_nAdditionalHP = 0;
	public int m_nAdditionalDP = 0;

	public float m_fDefencePowerRatio = 0;
	public float m_fMaxHPRatio = 0;
	public float m_fAttackPowerPistol = 0f;
	public float m_fAttackPowerSMG = 0f;
	public float m_fAttackPowerAR = 0f;
	public float m_fAttackPowerMG = 0f;
	public float m_fAttackPowerSR = 0f;
	public float m_fAttackPowerSG = 0f;
	public float m_fAttackPowerGE = 0f;
	public float m_fDefencePowerMelee = 0f;
	public float m_fDefencePowerRange = 0f;
	public float m_fDefencePowerExplosion = 0f;
	public float m_fAttackPowerAfterKill = 0f;
	public float m_nMsAttackPowerAfterKillDuration = 0f;
	public float m_nAttackPowerAfterKillMaxStack = 1.0f;
	public float m_fMoveSpeedRatioAfterKill = 0f;
	public float m_nMsMoveSpeedRatioAfterKillDuration = 0f;
	public float m_nMoveSpeedRatioAfterKillMaxStack = 1.0f;
	public float m_fCureAfterKill = 0f;
	public float m_fBleedingAfterAttack = 0f;
	public float m_nMsBleedingAfterAttackDuration = 0f;
	public float m_nMsBleedingAfterAttackInterval = 0f;
	public float m_nBleedingAfterAttackMaxStack = 1.0f;
	public float m_fMoveSpeedRatioAfterAttack = 0f;
	public float m_nMsMoveSpeedRatioAfterAttackDuration = 0f;
	public float m_nMoveSpeedRatioAfterAttackMaxStack = 1.0f;

	public UserAccount()
	{
		m_bBuyDailyDeal = new bool[4];
		m_nDailyDealKey = new uint[4];
		m_nWeaponID = new long[4];
		m_nGearID = new long[4];
		m_nBuyADSDeal = new int[4];
		m_nADSDealKey = new uint[4];
	}

	~UserAccount()
	{
	}

	public void Initialize(JObject result)
	{
		DateTime stime = DateTime.Now;

		try
		{
			m_nUID = (int)result.GetValue(ComType.STORAGE_UID);
			m_nLevel = (int)result.GetValue("currentLevel");
			m_nExp = (int)result.GetValue("currentExp");

			string accesstoken = result.GetValue("accessToken").ToString();

			if (accesstoken != PlayerPrefs.GetString(ComType.STORAGE_TOKEN))
			{
				GameManager.Log("accessToken Changed");
			}

			for (int i = 0; i < 4; i++)
			{
				m_nWeaponID[i] = (int)result.GetValue($"equipWeapon0{i}");
				m_nGearID[i] = (int)result.GetValue($"equipGear0{i}");
			}

			m_strAccessToken = accesstoken;
			m_strNickname = result.GetValue("nickname").ToString();

			m_nWSLevel = (int)result.GetValue("workshopLevel");
			m_nWSExp = (int)result.GetValue("workshopExp");

			m_nCharacterID = (int)result.GetValue("currentCharacter");
			m_nEpisode = (int)result.GetValue("currentEpisode");
			m_nChapter = (int)result.GetValue("currentChapter");
            m_nStage = (int)result.GetValue("currentStage");
            m_nHuntLevel = (int)result.GetValue("currentHunt");
			m_nPassLevel = (int)result.GetValue("passLevel");
			m_nPassExp = (int)result.GetValue("passExp");

			string dsa = (string)result.GetValue("startAdventure");
			string dak = (string)result.GetValue("adventureKeyStart");

			m_dtStartAdventure = ComUtil.String2Datetime(dsa); // string.IsNullOrEmpty(dsa) ? default : DateTime.Parse(dsa);
			m_dtAdventureKeyStart = ComUtil.String2Datetime(dak); // string.IsNullOrEmpty(dak) ? default : DateTime.Parse(dak);
			m_nAdventureLevel = (int)result.GetValue("adventureLevel");
			m_nCurrentAdventureKeyCount = (int)result.GetValue("currentAdventureKeyCount");
			m_nAdventureGroup = (int)result.GetValue("adventureGroup");

			string dp = (string)result.GetValue("passStartDatetime");
			m_dtPass = ComUtil.String2Datetime(dp); //  string.IsNullOrEmpty(dp) ? default : DateTime.Parse(dp);

			m_bIsElite = (int)result.GetValue("isElite") == 1;
			m_bIsPlus = (int)result.GetValue("isPlus") == 1;

			m_nMaxWeaponRepository = (int)result.GetValue("maxWeapon");
			m_nMaxGearRepository = (int)result.GetValue("maxGear");

			string dd = (string)result.GetValue("dailyDealResetDatetime");
			m_dtDailyDealReset = ComUtil.String2Datetime(dd); //  string.IsNullOrEmpty(dd) ? default : DateTime.Parse(dd);

			for (int i = 0; i < 4; i++)
				m_nDailyDealKey[i] = (uint)result.GetValue($"dailyDealItem{i}");

			m_nCountBuyWeaponBox = (int)result.GetValue("countBuyWeaponBox");
			m_nCountBuyMateerialBox = (int)result.GetValue("countBuyMaterialBox");
			m_nCountBuyPremiumMaterialBox = (int)result.GetValue("countBuyPremiumMaterialBox");

			m_nCountResetWeaponBox = (int)result.GetValue("countResetWeaponBox");
			string wb = (string)result.GetValue("weaponBoxResetDatetime");
			m_dtWeaponBoxReset = ComUtil.String2Datetime(wb); //  string.IsNullOrEmpty(wb) ? default : DateTime.Parse(wb);
			m_nCurrentWeaponBoxRewardIndex = (uint)result.GetValue("currentWeaponBoxRewardIndex");

			m_nCountResetWeaponPremiumBox = (int)result.GetValue("countResetWeaponPremiumBox");
			string wpb = (string)result.GetValue("weaponPremiumBoxResetDatetime");
			m_dtWeaponPremiumBoxReset = ComUtil.String2Datetime(wpb); //  string.IsNullOrEmpty(wpb) ? default : DateTime.Parse(wpb);
			m_nCurrentWeaponPremiumBoxRewardIndex = (uint)result.GetValue("currentWeaponPremiumBoxRewardIndex");

			string pmb = (string)result.GetValue("pMaterialBoxResetDatetime");
			m_dtPMaterialBoxReset = ComUtil.String2Datetime(pmb); //  string.IsNullOrEmpty(pmb) ? default : DateTime.Parse(pmb);

			string ad = (string)result.GetValue("adsDealResetDatetime");
			m_dtADSDealReset = ComUtil.String2Datetime(ad); //  string.IsNullOrEmpty(ad) ? default : DateTime.Parse(ad);

			for (int i = 0; i < 4; i++)
				m_nADSDealKey[i] = (uint)result.GetValue($"adsDealItem{i}");

			for (int i = 0; i < 3; i++)
			{
				m_nCurrentPBoxG2Index[i] = (uint)result.GetValue($"currentPBoxG2index{i}");
				m_nCurrentPBoxG3Index[i] = (uint)result.GetValue($"currentPBoxG3index{i}");
			}

			m_nCountBuyPMaterialBox = (int)result.GetValue("countBuyPremiumMaterialBox");

			string evp = (string)result.GetValue("vipEndDatetime");
			m_dtEndVip = ComUtil.String2Datetime(evp); // string.IsNullOrEmpty(evp) ? default : DateTime.Parse(evp);

			string ast = (string)result.GetValue("attendanceStartDatetime");
			m_dtAttendanceStart = ComUtil.String2Datetime(ast); //  string.IsNullOrEmpty(ast) ? default : DateTime.Parse(ast);
			m_nAttendanceContinuouslyCount = (int)result.GetValue("attendanceContinuouslyCount");

			// 서버에서 signin 시에만 ableToAttendanceReward 를 넣어서 보낸다.
			// 무기 / 장비 / 캐릭터 교체 대비.
			if (null != result.GetValue("ableToAttendanceReward"))
				m_bIsAbleToAttendanceReward = (int)result.GetValue("ableToAttendanceReward") == 1;

			m_nCurDefenceDifficulty = (int)result.GetValue("currentDefenceDifficulty");

			string absd = (string)result.GetValue("endBonusDatetime");
			m_dtBonusEnd = ComUtil.String2Datetime(absd); // string.IsNullOrEmpty(absd) ? default : DateTime.Parse(absd);
			m_nCurrrentBonusPoint = (int)result.GetValue("currentBonusPoint");
			m_bIsBonusTime = m_dtBonusEnd > DateTime.UtcNow;

			m_strLinkedMail = (string)result.GetValue("linkedEMail");

            m_nMaxDefeatZombie = (int)result.GetValue("countMaxDefeatZombie");

            CalcGearPower();
		}
		catch (Exception ex)
		{
			string msg = $"{ex.Message}\n\n{ex.StackTrace}";

			PopupSysMessage exPop = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
			exPop.InitializeInfo("Error", msg, "Exit", GameDataTableManager.Singleton.ExitApp);
		}

		GameManager.Log($"account info initialize time : {DateTime.Now - stime}", "green");
	}

	public void InitializeQuestInfo(string result)
    {
		JArray jaQuest = JArray.Parse(result);

		for ( int i = 0; i < jaQuest.Count; i++ )
        {
			JObject item = (JObject)jaQuest[i];

			m_nQuestID[i] = (long)item.GetValue("id");
			m_nQuestKey[i] = (uint)item.GetValue("questindex");
			m_nQuestCount[i] = (int)item.GetValue("count");
			m_bQuestIsComplete[i] = (int)item.GetValue("iscomplete") == 1;
			m_bUseQuestCard[i] = false;
		}
	}

	public void InitializeQuestRewardsInfo(string result)
    {
		JArray jaRewards = JArray.Parse(result);

		for ( int i = 0; i < jaRewards.Count; i++ )
        {
			JObject rewards = (JObject)jaRewards[i];

			if ((int)rewards.GetValue("type") == 0)
				m_bIsRecieveQuestAccouuntRewardsMiddle = true;
			else if ((int)rewards.GetValue("type") == 1)
				m_bIsRecieveQuestAccouuntRewardsFull = true;

		}
	}

	/// <summary>
	/// Abyss 기본 정보 초기화
	/// </summary>
	/// <param name="result"></param>
	public void InitializeAbyssInfo(string result)
	{
		JObject jAbyss = JObject.Parse(result);

		// m_nAbyssBestLap = new int[100];

		int edt = (int)jAbyss.GetValue("endDateTime");
		int adt = (int)jAbyss.GetValue("adjustDateTime");

		int adTime = GlobalTable.GetData<int>("countAbyssAdjustTime");

		m_dtAbyssAdjustEnd = new DateTime(DateTime.UtcNow.Year,
										  DateTime.UtcNow.Month,
										  DateTime.UtcNow.Day,
										  DateTime.UtcNow.Hour,
										  DateTime.UtcNow.Minute,
										  0).AddHours(edt + ( adt == adTime ? 1 : 0 ) + adt + ( adt == adTime ? 0 : 1 ));
		m_dtAbyssEnd = m_dtAbyssAdjustEnd.AddHours(-(adt + (adt == adTime ? 0 : 1)));
		m_nAbyssSeason = (int)jAbyss.GetValue("season");
		m_nAbyssGroup = (int)jAbyss.GetValue("abyssGroup");
		m_nAbyssCurRank = (int)jAbyss.GetValue("curRank");
		m_bIsAbyssRewardsReceived = (int)jAbyss.GetValue("isRewardsReceived") == 1;
	}

	/// <summary>
	/// Abyss 기록 정보 초기화
	/// </summary>
	/// <param name="result"></param>
	public void InitializeAbyssRecordInfo(string result)
	{
		JArray jaAbyss = JArray.Parse(result);

		foreach (JObject abyssRecord in jaAbyss)
		{
			m_nAbyssBestLap[(int)abyssRecord.GetValue("chapter") - 1] = (int)abyssRecord.GetValue("duration");
		}
	}

	public void CalcGearPower()
	{
		if (0 == GameManager.Singleton.invenGear.GetItemCount()) return;

		InitializeGearPower();

		Array.ForEach(m_nGearID, gearID =>
		{
			if (gearID > 0)
			{
				ItemGear gear = GameManager.Singleton.invenGear.GetItem(gearID);
				gear.CalcEffect();
			}
		});
	}

	public void InitializeGearPower()
	{
		m_nAdditionalHP = 0;
		m_nAdditionalDP = 0;

		m_fDefencePowerRatio = 0;
		m_fMaxHPRatio = 0;
		m_fAttackPowerPistol = 0f;
		m_fAttackPowerSMG = 0f;
		m_fAttackPowerAR = 0f;
		m_fAttackPowerMG = 0f;
		m_fAttackPowerSR = 0f;
		m_fAttackPowerSG = 0f;
		m_fAttackPowerGE = 0f;
		m_fDefencePowerMelee = 0f;
		m_fDefencePowerRange = 0f;
		m_fDefencePowerExplosion = 0f;
		m_fAttackPowerAfterKill = 0f;
		m_fMoveSpeedRatioAfterKill = 0f;
		m_fCureAfterKill = 0f;
		m_fBleedingAfterAttack = 0f;
		m_fMoveSpeedRatioAfterAttack = 0f;

		m_nMsAttackPowerAfterKillDuration = 0f;
		m_nAttackPowerAfterKillMaxStack = 0f;
		m_nMsMoveSpeedRatioAfterKillDuration = 0f;
		m_nMoveSpeedRatioAfterKillMaxStack = 0f;
		m_nMsBleedingAfterAttackDuration = 0f;
		m_nMsBleedingAfterAttackInterval = 0f;
		m_nBleedingAfterAttackMaxStack = 0;
		m_nMsMoveSpeedRatioAfterAttackDuration = 0f;
		m_nMoveSpeedRatioAfterAttackMaxStack = 0;
	}

	public bool IsEquipWeapon(long id)
	{
		for (int i = 0; i < m_nWeaponID.Length; i++)
		{
			if (m_nWeaponID[i] == id)
				return true;
		}

		return false;
	}

	public bool IsEquipGear(long id)
	{
		for (int i = 0; i < m_nGearID.Length; i++)
		{
			if (m_nGearID[i] == id)
				return true;
		}

		return false;
	}

	public bool IsVIP()
	{
		return m_dtEndVip > DateTime.UtcNow;
	}

	public void InitializeBattlePass(string result)
	{
		for (int i = 0; i < m_bpPass.Length; i++)
			m_bpPass[i] = new bool[3];

		JArray jResult = JArray.Parse(result);

		for (int i = 0; i < jResult.Count; i++)
		{
			JObject item = (JObject)jResult[i];
			m_bpPass[(int)item.GetValue("level") - 1][(int)item.GetValue("type")] = true;
		}
	}

	public void InitializeGameMoneyDeal(string result)
	{
		JArray jResult = JArray.Parse(result);

		m_nGameMoneyDealKey = new uint[jResult.Count];

		for (int i = 0; i < jResult.Count; i++)
		{
			JObject item = (JObject)jResult[i];
			m_nGameMoneyDealKey[i] = (uint)item.GetValue("dealKey");
		}

		ComShopGameMoney popup = GameObject.Find("PopupShopGameMoney")?.GetComponentInChildren<ComShopGameMoney>();
		ComShopGameMoney compo = GameObject.Find("ShopPage")?.GetComponentInChildren<ComShopGameMoney>();

		if (null != popup) popup.SetGoods();
		if (null != compo) compo.SetGoods();
	}

	public void InitializeDailyDeal(string result)
	{
		if (m_dtDailyDealReset == default)
		{
			Array.ForEach(m_bBuyDailyDeal, (e) => { e = false; });
			return;
		}

		DateTime bTime;
		JArray jResult = JArray.Parse(result);

		for (int i = 0; i < 4; i++)
			SetDailyDealCover(i, false);

		foreach (JObject item in jResult)
		{
			bTime = DateTime.Parse(item.GetValue("buyDatetime").ToString());
			SetDailyDealCover((int)item.GetValue("slotNumber"), bTime > m_dtDailyDealReset);
		}
	}

	public void SetDailyDealCover(int slotNumber, bool state)
	{
		m_bBuyDailyDeal[slotNumber] = state;
	}

	public void InitializeADSDeal(string result)
	{
		m_nBuyADSDeal = new int[4];
		m_dtADSDealSlot = new DateTime[4];

		if (m_dtADSDealReset == default) return;

		JArray jResult = JArray.Parse(result);

		foreach (JObject item in jResult)
		{
			int slot = (int)item.GetValue("slotNumber");

			m_nBuyADSDeal[slot]++;

			string btd = (string)item.GetValue("buyDatetime");

			m_dtADSDealSlot[slot] = string.IsNullOrEmpty(btd) ? default :
				(DateTime.Parse(btd) > m_dtADSDealSlot[slot] ? DateTime.Parse(btd) : m_dtADSDealSlot[slot]);
		}
	}

	public IEnumerator GainExp(int exp, int passExp = 0)
	{
		if ( m_nLevel < GlobalTable.GetData<int>("levelMaxAccount") )
        {
			int tTargetExp = GetAccountTargetExp(m_nLevel);

			if (m_nExp + exp >= tTargetExp)
			{
				m_nLevel++;
				m_nExp = (m_nExp + exp) - tTargetExp;

				GameManager.Singleton.StartCoroutine(GameManager.Singleton.AccountLevelUp(m_nLevel));
			}
			else
			{
				m_nExp += exp;
			}
		}

		if ( m_nPassLevel < BattlePassTable.GetList().Count )
        {
			if (passExp > 0 && m_nPassLevel < BattlePassTable.GetList().Count)
			{
				int tTargetPassExp = GetPassTargetExp(m_nPassLevel);

				if (m_nPassExp + passExp >= tTargetPassExp)
				{
					m_nPassLevel++;
					m_nPassExp = (m_nPassExp + passExp) - tTargetPassExp;
				}
				else
				{
					m_nPassExp += passExp;
				}
			}
		}

		yield return GameManager.Singleton.StartCoroutine(SetExp());

		if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
			GameObject.Find("LobbyTop").GetComponent<PageLobbyTop>().InitializeAccountInfo();
	}

	IEnumerator SetExp()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("currentLevel", m_nLevel.ToString());
		fields.Add("currentExp", m_nExp.ToString());
		fields.Add("passLevel", m_nPassLevel.ToString());
		fields.Add("passExp", m_nPassExp.ToString());

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
	}

	public IEnumerator IncrAdventureKeyCount(int a_nNumKeys)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>();
		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("adventureKeyStart", ComUtil.EnDatetime(m_dtAdventureKeyStart));
		fields.Add("currentAdventureKeyCount", Mathf.Max(0, m_nCurrentAdventureKeyCount + a_nNumKeys).ToString());

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
	}

	public int GetHP(int level)
	{
		return (int)((AccountLevelTable.GetData((uint)(0x01000000 + level)).HP + m_nAdditionalHP) * (1 + m_fMaxHPRatio));
	}

	public int GetDP(int level)
	{
		return (int)((AccountLevelTable.GetData((uint)(0x01000000 + level)).DP + m_nAdditionalDP) * (1 + m_fDefencePowerRatio));
	}

	public int GetAccountTargetExp(int level)
	{
		return AccountLevelTable.GetData((uint)(0x01000000 + level)).Exp;
	}

	public int GetPassTargetExp(int level)
	{
		return BattlePassTable.GetData((uint)(0x03000000 + level)).Exp;
	}

	public IEnumerator GainWorkshopExp(int exp)
	{
		if (m_nWSLevel < GlobalTable.GetData<int>("levelMaxWorkshop"))
		{
            int tTargetExp = GetWorkshopTargetExp(m_nWSLevel);

            if (m_nWSExp + exp >= tTargetExp)
            {
                m_nWSLevel++;
                m_nWSExp = (m_nWSExp + exp) - tTargetExp;
            }
            else
            {
                m_nWSExp += exp;
            }

            yield return GameManager.Singleton.StartCoroutine(SetWorkshopExp());
        }

		yield break;
	}

	IEnumerator SetWorkshopExp()
	{
		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("workshopLevel", m_nWSLevel.ToString());
		fields.Add("workshopExp", m_nWSExp.ToString());

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));
	}

	public int GetWorkshopTargetExp(int level)
	{
		return WorkshopLevelTable.GetData((uint)(0x02000000 + level)).Exp;
	}

	public IEnumerator ClearCampaign(int episode, int chapter, int stage, ECompleteMapType type = ECompleteMapType.None, bool isTutorial = false)
	{
		PopupWait4Response wait = MenuManager.Singleton.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);

		int cLv = m_nLevel;
		int cExp = m_nExp;
		int cPassLv = m_nPassLevel;
		int cPassExp = m_nPassExp;

		// 0 : exp, 1 : passExp
		int[] exp = StageTable.GetGainExp(episode + 1, chapter + 1, stage);

        if (ECompleteMapType.None == type)
        {
            type = StageTable.GetMax(episode + 1, chapter + 1) == stage ? ECompleteMapType.Suscces : ECompleteMapType.Fail;
            if (type == ECompleteMapType.Suscces)
            {
                chapter++;
                stage = 0;
            }
        }

		if ( isTutorial )
		{
			episode = -1;
			// chapter = m_nChapter;
			// stage = m_nStage;
		}

        Dictionary<string, string> fields = new Dictionary<string, string>();
 
		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("currentEpisode", episode.ToString());
        fields.Add("currentChapter", chapter.ToString());
        fields.Add("currentStage", stage.ToString());
        fields.Add("winState", ((int)type).ToString() );

        if ( m_nLevel < GlobalTable.GetData<int>("levelMaxAccount") )
        {
            int tTargetExp = GetAccountTargetExp(m_nLevel);

            if (cExp + exp[0] >= tTargetExp)
            {
                cLv++;
                cExp = (cExp + exp[0]) - tTargetExp;
            }
            else
            {
                cExp += exp[0];
            }
        }

        fields.Add("currentLevel", cLv.ToString());
        fields.Add("currentExp", cExp.ToString());

        if ( ( m_nPassLevel < BattlePassTable.GetList().Count ) &&
             ( CorrectEpisode + 1 ) >= GlobalTable.GetData<int>("valueBattlePassOpenEpisode") )
        {
            int tTargetPassExp = GetPassTargetExp(m_nPassLevel);

            if (cPassExp + exp[1] >= tTargetPassExp)
            {
                cPassLv++;
                cPassExp = (cPassExp + exp[1]) - tTargetPassExp;
            }
            else
            {
                cPassExp += exp[1];
            }
        }

        fields.Add("passLevel", cPassLv.ToString());
        fields.Add("passExp", cPassExp.ToString());

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.complete_campaign, fields, (result) =>
		{
			if (result == "renewal")
			{
				m_nEpisode = episode;
				m_nChapter = chapter;
				m_nStage = stage;
			}

            if (cLv > m_nLevel)
                GameManager.Singleton.StartCoroutine(GameManager.Singleton.AccountLevelUp(cLv));

            m_nLevel = cLv;
            m_nExp = cExp;

            m_nPassLevel = cPassLv;
            m_nPassExp = cPassExp;
        }));
		
        wait.Close();
    }

	public IEnumerator ClearHuntLevel(uint key, ECompleteMapType type)
	{
		HuntTable t = HuntTable.GetData(key);
		int level = (t.Episode - 1) * 10 + t.Order;

		if (level <= m_nHuntLevel) yield break;

		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("currentHunt", level.ToString());

		if (type == ECompleteMapType.Suscces)
			yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.ModifyAccount(fields));

		fields.Remove("currentHunt");
		fields.Add("currentEpisode", "0");
		fields.Add("currentChapter", level.ToString());
		fields.Add("winState", ((int)type).ToString());
		fields.Add("mapType", "1");

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.complete_map, fields));

		RefreshQuestCount(EQuestActionType.ClearHunt, 1);
	}

	public IEnumerator ClearAbyss(uint key, ECompleteMapType type, int msDuration = 0)
	{
		AbyssTable a = AbyssTable.GetData(key);
		int floor = a.Order;

		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("currentEpisode", "0");
		fields.Add("currentChapter", floor.ToString());
		fields.Add("winState", ((int)type).ToString());
		fields.Add("mapType", "8");

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.complete_map, fields));

		if (type == ECompleteMapType.Suscces && (m_nAbyssBestLap[floor - 1] > msDuration || m_nAbyssBestLap[floor - 1] == 0))
		{
			fields.Add("duration", msDuration.ToString());
			yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EAbyssPostType.clear_floor, fields, (result) =>
			{
				m_nAbyssBestLap[floor - 1] = msDuration;
			}));
		}

		RefreshQuestCount(EQuestActionType.ClearAbyss, 1);
	}

	public IEnumerator ClearDefence(MissionDefenceTable wave, Action gradeup = null)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("difficulty", wave.Difficulty.ToString());
		fields.Add("wave", wave.Order.ToString());

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.end_defence, fields, (result) =>
		{
			if ( result == "difficulty increase" )
			{ 
				m_nCurDefenceDifficulty++;
				gradeup?.Invoke();
			}

			RefreshQuestCount(EQuestActionType.ClearDefence, 1);
		}));
	}

	public IEnumerator Revive(EMapInfoType type, int ep, int chaper)
	{
		Dictionary<string, string> fields = new Dictionary<string, string>();

		fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
		fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
		fields.Add("mapType", ((int)type).ToString());
		fields.Add("currentEpisode", ep.ToString());
		fields.Add("currentChapter", chaper.ToString());

		yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.revive, fields));
	}

	public void RefreshQuestCount(EQuestActionType type, int count)
    {
		if ( m_nQuestKey[0] == 0 ) return;

		QuestTable quest;
		Dictionary<string, string> fields = new Dictionary<string, string>();

		for ( int i = 0; i < m_nQuestKey.Length; i++ )
        {
			quest = QuestTable.GetData(m_nQuestKey[i]);

			if (quest.Action == (int)type)
			{
				int questorder = i;

				int cnt = m_nQuestCount[i] + count;
				cnt = Math.Min(cnt, quest.RequireCount);

				fields.Add("id", m_nQuestID[i].ToString());
				fields.Add("count", cnt.ToString());

				GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EETCPostType.set_questinfo, fields, (result) =>
				{
					m_nQuestCount[questorder] = cnt;

					if (MenuManager.Singleton.CurScene == ESceneType.Lobby)
						GameObject.FindObjectOfType<PageLobbyBattle>().CheckQuestState();
				}));
			}
		}
    }

    public IEnumerator ClearZombie(int maxCount, int curWave)
    {
        Dictionary<string, string> fields = new Dictionary<string, string>();

        fields.Add("auid", PlayerPrefs.GetInt(ComType.STORAGE_UID).ToString());
        fields.Add("accessToken", PlayerPrefs.GetString(ComType.STORAGE_TOKEN));
        fields.Add("countMaxDefeatZombie", maxCount.ToString());
        fields.Add("wave", curWave.ToString());

        yield return GameDataManager.Singleton.StartCoroutine(GameDataManager.Singleton.SendRequestByPost(EMapPostType.end_zombie, fields, (result) =>
        {
			if ( result == "ok" )
				m_nMaxDefeatZombie = maxCount;
        }));
    }

    public void IncreaseBonusPoint(float point)
    {
		if (m_dtBonusEnd < DateTime.UtcNow &&
			CorrectEpisode + 1 >= GlobalTable.GetData<int>("valuBonusPointAdjustStartEpisode"))
			GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.SetBonusPoint(point));
	}

	public void ResetBonusPoint()
	{
		GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.SetBonusPoint(0));
	}

	/** NPC 기준 레벨을 반환한다 */
	public int GetStandardNPCLevel(int a_nLevel)
	{
		return AccountLevelTable.GetData((uint)(0x01000000 + a_nLevel)).StandardNPCLevel;
	}

	/** 보너스 NPC 기준 레벨을 반환한다 */
	public int GetStandardNPCLevelBonus(int a_nLevel)
	{
		return AccountLevelTable.GetData((uint)(0x01000000 + a_nLevel)).Standard;
	}

	/** 보너스 전투 플레이 시간을 반환한다 */
	public int GetMaxBattlePlayTimeBonus(int a_nLevel)
	{
		return AccountLevelTable.GetData((uint)(0x01000000 + a_nLevel)).BonusChapterMaintain;
	}

	/** 능력치를 반환한다 */
	public void GetAbilityValues(Dictionary<EEquipEffectType, float> a_oOutAbilityValDict)
	{
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerPistol, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerPistol) + m_fAttackPowerPistol);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerSMG, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerSMG) + m_fAttackPowerSMG);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerAR, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerAR) + m_fAttackPowerAR);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerMG, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerMG) + m_fAttackPowerMG);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerSR, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerSR) + m_fAttackPowerSR);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerSG, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerSG) + m_fAttackPowerSG);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerGE, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerGE) + m_fAttackPowerGE);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.DefencePowerMelee, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.DefencePowerMelee) + m_fDefencePowerMelee);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.DefencePowerRange, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.DefencePowerRange) + m_fDefencePowerRange);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.DefencePowerExplosion, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.DefencePowerExplosion) + m_fDefencePowerExplosion);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerAfterKill, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerAfterKill) + m_fAttackPowerAfterKill);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.MoveSpeedRatioAfterKill, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.MoveSpeedRatioAfterKill) + m_fMoveSpeedRatioAfterKill);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.CureAfterKill, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.CureAfterKill) + m_fCureAfterKill);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.BleedingAfterAttack, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.BleedingAfterAttack) + m_fBleedingAfterAttack);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.MoveSpeedRatioAfterAttack, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.MoveSpeedRatioAfterAttack) + m_fMoveSpeedRatioAfterAttack);

		int nDurationVal = (int)EOperationType.ADD * (int)EEquipEffectType.DURATION_VAL;

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerAfterKill + nDurationVal, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerAfterKill + nDurationVal) + (m_nMsAttackPowerAfterKillDuration * ComType.G_UNIT_MS_TO_S));
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.MoveSpeedRatioAfterKill + nDurationVal, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.MoveSpeedRatioAfterKill + nDurationVal) + (m_nMsMoveSpeedRatioAfterKillDuration * ComType.G_UNIT_MS_TO_S));
	}
}
