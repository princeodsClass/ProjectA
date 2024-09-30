using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGear : ItemBase
{
	public enum EquipState
	{
		equip,
		unequip,
	}

	public EGearType eSubType = 0;
	public int nMoveSpeed = 0;
	public int nExchangeCount = 0;
	public int nFeeItemKey = 0;
	public int nFeeItemCount = 0;
	public int nExchangeItemKey = 0;
	public int nExchangeAfterCount = 0;
	public int nRecipeKey = 0;
	public string strPrefab = string.Empty;

	public uint nUpgradeMaterialKey = 0;
	public uint nReinforceMaterialKey = 0;

	public int nDefencePowerStandard = 0;
	public int nDefencePower = 0;

	public int nHPStandard = 0;
	public int nHP = 0;

	public uint[] nLimitbreakMaterialKey = new uint[7];

	public uint[] nEffectKey = new uint[6];
	public int[] nEffectFixxed = new int[6];

	public float[] fEffectValue = new float[6];

	public ItemGear() { }
	public ItemGear(long id, uint primaryKey, int count, int upgrade = 0, int reinforce = 0, int limitbreak = 0, bool islock = false, bool isNew = false, uint[] effectkey = null, float[] effectvalue = null, int[] effectFixxed = null)
	{
		Initialize(id, primaryKey, count, upgrade, reinforce, limitbreak, islock, isNew, effectkey, effectvalue, effectFixxed);
	}

	public void Initialize(long nID, uint primaryKey, int count, int upgrade, int reinforce, int limitbreak, bool islock, bool isNew = false, uint[] effectkey = null, float[] effectvalue = null, int[] effectFixxed = null)
	{
		id = nID;

		nCurUpgrade = upgrade;
		nCurReinforce = reinforce;
		nCurLimitbreak = limitbreak;

		bIsLock = islock;
		bNew = (isNew && !GameManager.Singleton.user.m_nGearID.Contains(nID));

		if (null != effectkey)
		{
			nEffectKey = effectkey;
			nEffectFixxed = effectFixxed;

			fEffectValue = effectvalue;
		}

		InitializeTableData(primaryKey);

		ReCalcPower();
	}

	void InitializeTableData(uint primaryKey)
	{
		GearTable cTbl = GearTable.GetData(primaryKey);

		if (null == cTbl)
		{
			GameManager.Log("Item Init Failed.. Key:" + primaryKey, "red");
			return;
		}

		nKey = cTbl.PrimaryKey;
		eType = EItemType.Weapon;
		eSubType = (EGearType)cTbl.Type;
		nMoveSpeed = cTbl.MoveSpeed;
		nGrade = cTbl.Grade;
		nVolume = 1;

		strName = NameTable.GetValue(cTbl.NameKey);
		strDesc = DescTable.GetValue(cTbl.DescKey);
		nMaxStackCount = 1;

		strIcon = cTbl.Icon;
		strPrefab = cTbl.Prefab;

		nUpgradeMaterialKey = cTbl.UpgradeMaterialKey;
		nReinforceMaterialKey = cTbl.ReinforceMaterialKey;

		nDefencePowerStandard = cTbl.DefencePowerStandard;
		nHPStandard = cTbl.HPStandard;

		nLimitbreakMaterialKey[0] = cTbl.LimitbreakMaterialKey00;
		nLimitbreakMaterialKey[1] = cTbl.LimitbreakMaterialKey01;
		nLimitbreakMaterialKey[2] = cTbl.LimitbreakMaterialKey02;
		nLimitbreakMaterialKey[3] = cTbl.LimitbreakMaterialKey03;
		nLimitbreakMaterialKey[4] = cTbl.LimitbreakMaterialKey04;
		nLimitbreakMaterialKey[5] = cTbl.LimitbreakMaterialKey05;
		nLimitbreakMaterialKey[6] = cTbl.LimitbreakMaterialKey06;

		nRecycleMaterialKey[0] = cTbl.RecycleMaterialKey00;
		nRecycleMaterialKey[1] = cTbl.RecycleMaterialKey01;
		nRecycleMaterialKey[2] = cTbl.RecycleMaterialKey02;
		nRecycleMaterialKey[3] = cTbl.RecycleMaterialKey03;
		nRecycleMaterialKey[4] = cTbl.RecycleMaterialKey04;

		RecycleMaterialCount[0] = cTbl.RecycleMaterialCount00;
		RecycleMaterialCount[1] = cTbl.RecycleMaterialCount01;
		RecycleMaterialCount[2] = cTbl.RecycleMaterialCount02;
		RecycleMaterialCount[3] = cTbl.RecycleMaterialCount03;
		RecycleMaterialCount[4] = cTbl.RecycleMaterialCount04;
	}

	public void ReCalcPower()
	{
		CalcDefencePower(nCurUpgrade, nCurReinforce, nCurLimitbreak, true);
		CalcHP(nCurUpgrade, nCurReinforce, nCurLimitbreak, true);
	}

	public override Sprite GetIcon()
	{
		EAtlasType eAtlas = EAtlasType.Icons;
		return GameResourceManager.Singleton.LoadSprite(eAtlas, strIcon);
	}

	public Dictionary<uint, int> CalcLimitbreakMaterial()
	{
		return CalcLimitbreakMaterial(nCurLimitbreak);
	}

	public Dictionary<uint, int> CalcLimitbreakMaterial(int limitbreak)
	{
		Dictionary<uint, int> material = new Dictionary<uint, int>();

		int t;
		string sTC, sT;

		for (int i = 0; i <= limitbreak; i++)
		{
			uint[] tt = new uint[7];

			sT = nLimitbreakMaterialKey[i].ToString("X");
			sTC = sT.Substring(2, 1);

			t = int.Parse(sTC, System.Globalization.NumberStyles.HexNumber) + limitbreak;
			sTC = sT.Substring(0, 2) + t.ToString("X") + sT.Substring(3, 5);

			tt[i] = uint.Parse(sTC, System.Globalization.NumberStyles.HexNumber);

			material.Add(tt[i], 1);
		}

		return material;
	}

	public Dictionary<uint, int> CalcReinforceMaterial()
	{
		return CalcReinforceMaterial(nCurLimitbreak);
	}

	public Dictionary<uint, int> CalcReinforceMaterial(int limitbreak)
	{
		Dictionary<uint, int> material = new Dictionary<uint, int>();

		int cost, count, countReinforce;
		int standardCost, additionalMaterialCount, standardMaterialCount, standardReinforceTCount;

		standardCost = GlobalTable.GetData<int>("costStandardReinforce");
		standardMaterialCount = GlobalTable.GetData<int>("countStandardMaterialForReinforce");
		additionalMaterialCount = GlobalTable.GetData<int>("countAdditionalMaterialForReinforce");
		standardReinforceTCount = GlobalTable.GetData<int>("countToolForReinforce");

		cost = standardCost;
		count = standardMaterialCount + ((limitbreak - 1) * additionalMaterialCount);
		countReinforce = limitbreak * standardReinforceTCount;

		material.Add(ComType.KEY_ITEM_GOLD, cost);
		material.Add(nUpgradeMaterialKey, count);
		material.Add(nReinforceMaterialKey, countReinforce);

		return material;
	}

	public Dictionary<uint, int> CalcUpgradeMaterial()
	{
		return CalcUpgradeMaterial(nCurUpgrade, nCurLimitbreak);
	}

	public Dictionary<uint, int> CalcUpgradeMaterial(int upgrade, int limitbreak)
	{
		Dictionary<uint, int> material = new Dictionary<uint, int>();

		int cost, count;
		int standardCost, additionalCost, additionalMaterialCount, standardMaterialCount;

		standardCost = GlobalTable.GetData<int>("costStandardUpgrade");
		additionalCost = GlobalTable.GetData<int>("costAdditionalUpgrade");
		standardMaterialCount = GlobalTable.GetData<int>("countMaterialForUpgradeStandard");
		additionalMaterialCount = GlobalTable.GetData<int>("countMaterialForUpgradeAdditional");

		cost = (upgrade / 5 + 1) * standardCost + (additionalCost * limitbreak);
		count = (upgrade + Math.Max(limitbreak * additionalMaterialCount, 1)) * standardMaterialCount;

		material.Add(ComType.KEY_ITEM_GOLD, cost);
		material.Add(nUpgradeMaterialKey, count);

		return material;
	}

	public void CalcEffect()
	{
		UserAccount account = GameManager.Singleton.user;

		account.m_nAdditionalHP += nHP;
		account.m_nAdditionalDP += nDefencePower;

		for (int i = 0; i < nEffectKey.Length; i++)
		{
			if (nEffectKey[i] > 0)
			{
				EffectTable ef = EffectTable.GetData(nEffectKey[i]);

				switch ((EEquipEffectType)ef.Type)
				{
					case EEquipEffectType.DefencePowerRatio:
						account.m_fDefencePowerRatio += fEffectValue[i];
						break;
					case EEquipEffectType.MaxHPRatio:
						account.m_fMaxHPRatio += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerPistol:
						account.m_fAttackPowerPistol += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerSMG:
						account.m_fAttackPowerSMG += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerAR:
						account.m_fAttackPowerAR += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerMG:
						account.m_fAttackPowerMG += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerSR:
						account.m_fAttackPowerSR += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerSG:
						account.m_fAttackPowerSG += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerGE:
						account.m_fAttackPowerGE += fEffectValue[i];
						break;
					case EEquipEffectType.DefencePowerMelee:
						account.m_fDefencePowerMelee += fEffectValue[i];
						break;
					case EEquipEffectType.DefencePowerRange:
						account.m_fDefencePowerRange += fEffectValue[i];
						break;
					case EEquipEffectType.DefencePowerExplosion:
						account.m_fDefencePowerExplosion += fEffectValue[i];
						break;
					case EEquipEffectType.AttackPowerAfterKill:
						account.m_fAttackPowerAfterKill+= fEffectValue[i];
						account.m_nMsAttackPowerAfterKillDuration = ef.Duration;
						account.m_nAttackPowerAfterKillMaxStack = ef.MaxStackCount;
						break;
					case EEquipEffectType.MoveSpeedRatioAfterKill:
						account.m_fMoveSpeedRatioAfterKill += fEffectValue[i];
						account.m_nMsMoveSpeedRatioAfterKillDuration = ef.Duration;
						account.m_nMoveSpeedRatioAfterKillMaxStack = ef.MaxStackCount;
						break;
					case EEquipEffectType.CureAfterKill:
						account.m_fCureAfterKill += fEffectValue[i];
						break;
					case EEquipEffectType.BleedingAfterAttack:
						account.m_fBleedingAfterAttack += fEffectValue[i];
						account.m_nMsBleedingAfterAttackDuration = ef.Duration;
						account.m_nMsBleedingAfterAttackInterval = ef.Inteval;
						account.m_nBleedingAfterAttackMaxStack = ef.MaxStackCount;
						break;
					case EEquipEffectType.MoveSpeedRatioAfterAttack:
						account.m_fMoveSpeedRatioAfterAttack += fEffectValue[i];
						account.m_nMsMoveSpeedRatioAfterAttackDuration = ef.Duration;
						account.m_nMoveSpeedRatioAfterAttackMaxStack = ef.MaxStackCount;
						break;
				}
			}
		}
	}

	public int CalcHP(int upgrade, int reinforce, int limitbreak, bool isSet = false)
	{
		int u = GlobalTable.GetData<int>("countStandardUpgrade");
		float ratioUpgrade = GlobalTable.GetData<float>("ratioUpgrade");
		float ratioReinforce = GlobalTable.GetData<float>("ratioReinforce");
		float ratioLimitbreak = GlobalTable.GetData<float>("ratioLimitbreak");

		int hp = 0;
		int umax = 0;
		int rmax = 0;

		for (int i = 0; i <= limitbreak; i++)
			if (i == limitbreak)
			{
				umax += upgrade;
				rmax += reinforce;
			}
			else
			{
				umax += (u * i + u);
				rmax = rmax + (i + 1);
			}

		hp = (int)(nHPStandard * (1 + umax * ratioUpgrade + rmax * ratioReinforce + limitbreak * ratioLimitbreak));

		if (isSet) nHP = hp;

		return hp;
	}

	public int CalcDefencePower(int upgrade, int reinforce, int limitbreak, bool isSet = false)
	{
		int u = GlobalTable.GetData<int>("countStandardUpgrade");
		float ratioUpgrade = GlobalTable.GetData<float>("ratioUpgrade");
		float ratioReinforce = GlobalTable.GetData<float>("ratioReinforce");
		float ratioLimitbreak = GlobalTable.GetData<float>("ratioLimitbreak");

		int dp = 0;
		int umax = 0;
		int rmax = 0;

		for (int i = 0; i <= limitbreak; i++)
			if (i == limitbreak)
			{
				umax += upgrade;
				rmax += reinforce;
			}
			else
			{
				umax += (u * i + u);
				rmax = rmax + (i + 1);
			}

		dp = (int)(nDefencePowerStandard * (1 + umax * ratioUpgrade + rmax * ratioReinforce + limitbreak * ratioLimitbreak));

		if (isSet) nDefencePower = dp;

		return dp;
	}

	public int CalcCP(int attackPower, bool isSet = false)
	{
		/*
        float standardTime = GlobalTable.GetData<int>("timeStandardAttack");
        float standardRange = GlobalTable.GetData<int>("rangeStandardAttack");
        float standardSpeed = GlobalTable.GetData<int>("speedStandardMove");
        float standatdAPRatio = GlobalTable.GetData<float>("ratioStandardAttackPower");
        float standatdSGHitRatio = GlobalTable.GetData<float>("ratioStandardSGHit");
        float standatdGRAimFrame = GlobalTable.GetData<int>("frameGRAimTime");
        float standatdGRAttackFrame = GlobalTable.GetData<int>("frameGRAttackDelay");

        // 샷건의 기준 명중 탄환 비율 적용.
        standatdAPRatio = eSubType == EWeaponType.SG ? (standatdAPRatio / standatdSGHitRatio) : standatdAPRatio;
        // 수류탄은 애니메이션에서 16 프레임을 잡아먹는 것 처리.
        int corAimTime = eSubType == EWeaponType.Grenade ? (int)((standatdGRAimFrame / 30f) * 1000) : nAimTime;
        // 수류탄은 공격 딜레이가 없는 것을 애니 길이로 처리.
        int corAttackDelay = eSubType == EWeaponType.Grenade ? (int)((standatdGRAttackFrame / 30f) * 1000) : nAttackDelay;


        float a1 = (float)standardTime / ((corAttackDelay * nMagazineSize) + nReloadTime);
        float a2 = a1 * nMagazineSize;
        float high = a2 * attackPower * standatdAPRatio;

        float b1 = (float)(nAttackRange - standardRange) /
                   (standardSpeed + nMoveSpeed) /
                   (corAttackDelay + corAimTime);
        float b2 = b1 * attackPower * standatdAPRatio;
        float low = a2 * b2;

        float denominator = nMagazineSize * a1;

        int r = (int)(high * ((denominator - 1) / denominator) + low * (1 / denominator));

        if (isSet) nCP = r;

        return r;
        */
		return 0;
	}
}