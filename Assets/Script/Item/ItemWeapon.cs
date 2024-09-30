using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : ItemBase
{
	public EWeaponType eSubType = 0;
	public EWeaponAnimationType eAnimationType = 0;
	public int nProjectileCount = 0;
	public int nAttackPowerStandard = 0;
	public int nAttackPower = 0;
	public int nAimTime = 0;
	public int nAttackRangeStandard = 0;
	public int nAttackRange = 0;
	public int nMagazineSize = 0;
	public int nReloadTime = 0;
	public int nAttackDelay = 0;
	public int nMoveSpeed = 0;
	public int nAccuracy = 0;
	public int nExchangeCount = 0;
	public int nFeeItemKey = 0;
	public int nFeeItemCount = 0;
	public int nExchangeItemKey = 0;
	public int nExchangeAfterCount = 0;
	public int nRecipeKey = 0;
	public string strPrefab = string.Empty;

	public uint nUpgradeMaterialKey = 0;
	public uint nReinforceMaterialKey = 0;

	public float ForceMin = 0f;
	public float ForceMax = 0f;

	public uint[] nLimitbreakMaterialKey = new uint[7];

	public uint[] nEffectKey = new uint[6];
	public int[] nEffectFixxed = new int[6];

	public float[] fEffectValue = new float[6];

	public int nKnockBackRange = 0;
	public float fRicochetChance = 0f;
	public float fCriticalChance = 0f;
	public float fCriticalRatio = 0f;
	public float fPenetrateChance = 0f;
	public float fDoubliShotChance = 0f;
	public float fSmallAttackChance = 0f;
	public float fSmallAttackRatio = 0f;
	public float fReloadInstanceChance = 0f;
	public float fAP2BossRatio = 0f;
	public float fAP2EliteRatio = 0f;
	public float fExplosionRangeRatio = 0f;
	public float fShotHitExplosion = 0f;
	public float fAPUpByRemainMagazin = 1f;
	public float fAPUpByExplosionTarget = 1f;
	public float fAPUpByDistance = 1f;

	public ItemWeapon() { }
	public ItemWeapon(long id, uint primaryKey, int count, int upgrade, int reinforce, int limitbreak, bool islock, bool isNew = false, uint[] effectkey = null, float[] effectvalue = null, int[] effectFixxed = null)
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
		bNew = (isNew && !GameManager.Singleton.user.m_nWeaponID.Contains(nID));

		if (null != effectkey)
		{
			nEffectKey = effectkey;
			nEffectFixxed = effectFixxed;

			fEffectValue = effectvalue;
		}

		InitializeTableData(primaryKey);
		InitializeGlobalData();

		CalcEffect();
		ReCalcPower();
	}

	void InitializeGlobalData()
	{
		fRicochetChance = GlobalTable.GetData<float>("perRicochet");
		fCriticalChance = GlobalTable.GetData<float>("perCritical");
		fCriticalRatio = GlobalTable.GetData<float>("ratioCritical");
		fPenetrateChance = GlobalTable.GetData<float>("perPenetrate");
		fDoubliShotChance = GlobalTable.GetData<float>("perDoubleShot");
		fSmallAttackChance = GlobalTable.GetData<float>("perSmallAttack");
		fSmallAttackRatio = GlobalTable.GetData<float>("ratioSmallAttack");
		fReloadInstanceChance = GlobalTable.GetData<float>("perReloadInstance");
	}

	void InitializeTableData(uint primaryKey)
	{
		WeaponTable cTbl = WeaponTable.GetData(primaryKey);

		if (null == cTbl)
		{
			GameManager.Log("Item Init Failed.. Key:" + primaryKey, "red");
			return;
		}

		nKey = cTbl.PrimaryKey;
		eType = EItemType.Weapon;
		eSubType = (EWeaponType)cTbl.Type;
		eAnimationType = (EWeaponAnimationType)cTbl.AnimationType;
		nProjectileCount = cTbl.ProjectileCount;
		nAttackPowerStandard = cTbl.AttackPowerStandard;
		nAimTime = cTbl.AimTime;
		nAttackRangeStandard = cTbl.Range;
		nAttackRange = cTbl.Range;
		nMagazineSize = cTbl.MagazineSize;
		nReloadTime = cTbl.ReloadTime;
		nAttackDelay = cTbl.AttackDelay;
		nMoveSpeed = cTbl.MoveSpeed;
		nAccuracy = cTbl.Accuracy;
		nGrade = cTbl.Grade;
		nVolume = 1;

		strName = NameTable.GetValue(cTbl.NameKey);
		strDesc = DescTable.GetValue(cTbl.DescKey);
		nMaxStackCount = 1;

		strIcon = cTbl.Icon;
		strPrefab = cTbl.Prefab;

		ForceMin = cTbl.ForceMin;
		ForceMax = cTbl.ForceMax;

		nUpgradeMaterialKey = cTbl.UpgradeMaterialKey;
		nReinforceMaterialKey = cTbl.ReinforceMaterialKey;

		m_nWarheadSpeed = cTbl.WarheadSpeed;
		m_eAttackType = (EAttackType)cTbl.AttackType;

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
		CalcAttackPower(nCurUpgrade, nCurReinforce, nCurLimitbreak, true);
		CalcCP(nAttackPower, true);
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

	void CalcEffect()
	{
		for (int i = 0; i < nEffectKey.Length; i++)
		{
			if (nEffectKey[i] > 0)
			{
				EffectTable ef = EffectTable.GetData(nEffectKey[i]);
				EEquipEffectType efType = (EEquipEffectType)ef.Type;
				EEffectOperationType efOpType = (EEffectOperationType)ef.OperationType;

				switch (efType)
				{
					case EEquipEffectType.AttackPowerRatio:
						//CalcAttackPower() 참고
						break;
					case EEquipEffectType.AttackDelayRatio:
						ProcessEffectValue(ref nAttackDelay, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AttackRange:
						ProcessEffectValue(ref nAttackRange, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.RicochetChance:
						ProcessEffectValue(ref fRicochetChance, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.CriticalChance:
						ProcessEffectValue(ref fCriticalChance, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.DoubleShotChance:
						ProcessEffectValue(ref fDoubliShotChance, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.ReloadTime:
						ProcessEffectValue(ref nReloadTime, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.PenetrateChance:
						ProcessEffectValue(ref fPenetrateChance, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.MagazineSize:
						ProcessEffectValue(ref nMagazineSize, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.KnockBackRange:
						ProcessEffectValue(ref nKnockBackRange, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AccuracyRatio:
					case EEquipEffectType.ReduceSpread:
						ProcessEffectValue(ref nAccuracy, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AmmoSpeedRatio:
						ProcessEffectValue(ref m_nWarheadSpeed, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.CriticalRatio:
						ProcessEffectValue(ref fCriticalRatio, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AimTime:
						ProcessEffectValue(ref nAimTime, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.InstanceReload:
						ProcessEffectValue(ref fReloadInstanceChance, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.ExplosionRangeRatio:
						ProcessEffectValue(ref fExplosionRangeRatio, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AttackPowerToB:
						ProcessEffectValue(ref fAP2BossRatio, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AttackPowerToE:
						ProcessEffectValue(ref fAP2EliteRatio, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.AttackPowerToBNE:
						ProcessEffectValue(ref fAP2BossRatio, fEffectValue[i], efOpType);
						ProcessEffectValue(ref fAP2EliteRatio, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.SHOOT_HIT_EXPLOSION:
						ProcessEffectValue(ref fShotHitExplosion, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN:
						ProcessEffectValue(ref fAPUpByRemainMagazin, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION:
						ProcessEffectValue(ref fAPUpByExplosionTarget, fEffectValue[i], efOpType);
						break;
					case EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE:
						ProcessEffectValue(ref fAPUpByDistance, fEffectValue[i], efOpType);
						break;
				}
            }
		}
	}

	void ProcessEffectValue(ref int targetValue, float effectValue, EEffectOperationType opType)
	{
		if (opType == EEffectOperationType.Add)
			targetValue = (int)(targetValue + effectValue);
		else if (opType == EEffectOperationType.Ratio)
			targetValue = (int)(targetValue * (1 + effectValue));
		else if (opType == EEffectOperationType.Replace)
			targetValue = (int)effectValue;
	}

	void ProcessEffectValue(ref float targetValue, float effectValue, EEffectOperationType opType)
	{
		if (opType == EEffectOperationType.Add)
			targetValue = (targetValue + effectValue);
		else if (opType == EEffectOperationType.Ratio)
			targetValue = (targetValue * (1 + effectValue));
		else if (opType == EEffectOperationType.Replace)
			targetValue = effectValue;
	}

	public int CalcAttackPower(int upgrade, int reinforce, int limitbreak, bool isSet = false)
	{
		int u = GlobalTable.GetData<int>("countStandardUpgrade");
		float ratioUpgrade = GlobalTable.GetData<float>("ratioUpgrade");
		float ratioReinforce = GlobalTable.GetData<float>("ratioReinforce");
		float ratioLimitbreak = GlobalTable.GetData<float>("ratioLimitbreak");

		int ap = 0;
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

		ap = (int)(nAttackPowerStandard * (1 + umax * ratioUpgrade + rmax * ratioReinforce + limitbreak * ratioLimitbreak));

		for (int i = 0; i < nEffectKey.Length; i++)
		{
			if (nEffectKey[i] > 0)
			{
				EffectTable ef = EffectTable.GetData(nEffectKey[i]);
				if ((EEquipEffectType)ef.Type == EEquipEffectType.AttackPowerRatio)
					ap = (int)(ap * (1 + fEffectValue[i]));
			}
		}

		if (isSet) nAttackPower = ap;

		return ap;
	}

	public int CalcCP(int attackPower, bool isSet = false)
	{
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
	}

	#region 변수
	public int m_nWarheadSpeed = 0;
	public EAttackType m_eAttackType = EAttackType.NONE;
	#endregion // 변수

	#region 함수
	/** 능력치를 반환한다 */
	public void GetAbilityValues(Dictionary<EEquipEffectType, float> a_oOutAbilityValDict)
	{
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ATK, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK) + nAttackPower);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ACCURACY, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.ACCURACY) + nAccuracy);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ATK_DELAY, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.ATK_DELAY) + nAttackDelay);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.RELOAD_TIME, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.RELOAD_TIME) + nReloadTime);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.WARHEAD_SPEED, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.WARHEAD_SPEED) + m_nWarheadSpeed);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackRange, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackRange) + nAttackRange);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.RicochetChance, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.RicochetChance) + fRicochetChance);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.CriticalChance, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.CriticalChance) + fCriticalChance);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.DoubleShotChance, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.DoubleShotChance) + fDoubliShotChance);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.PenetrateChance, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.PenetrateChance) + fPenetrateChance);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.MagazineSize, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.MagazineSize) + nMagazineSize);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.KnockBackRange, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.KnockBackRange) + nKnockBackRange);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.CriticalRatio, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.CriticalRatio) + fCriticalRatio);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AIMING_DELAY, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AIMING_DELAY) + nAimTime);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.InstanceReload, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.InstanceReload) + fReloadInstanceChance);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ExplosionRangeRatio, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.ExplosionRangeRatio) + fExplosionRangeRatio);

		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.FORCE_MIN, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.FORCE_MIN) + ForceMin);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.FORCE_MAX, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.FORCE_MAX) + ForceMax);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerToB, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerToB) + fAP2BossRatio);
		a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.AttackPowerToE, a_oOutAbilityValDict.GetValueOrDefault(EEquipEffectType.AttackPowerToE) + fAP2EliteRatio);

		var oTable = WeaponTable.GetData(nKey);
		var oEffectGroupList = EffectGroupTable.GetGroup(oTable.EquipEffectGroup);

		for (int i = 0; i < oEffectGroupList.Count; ++i)
		{
			var oEffectTable = EffectTable.GetData(oEffectGroupList[i].EffectKey);

			switch((EEquipEffectType)oEffectTable.Type)
			{
				case EEquipEffectType.SHOOT_HIT_EXPLOSION: 
					a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.SHOOT_HIT_EXPLOSION, fShotHitExplosion * ComType.G_UNIT_MM_TO_M);
					break;

				case EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN: 
					a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ATTACK_POWER_UP_BY_MAGAZIN, fAPUpByRemainMagazin - 1.0f);
					break;

				case EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION: 
					a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ATTACK_POWER_UP_BY_EXPLOSION, fAPUpByExplosionTarget - 1.0f);
					break;

				case EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE: 
					a_oOutAbilityValDict.ExReplaceVal(EEquipEffectType.ATTACK_POWER_UP_BY_DISTANCE, fAPUpByDistance - 1.0f);
					break;
			}
		}
	}
	#endregion // 함수
}