using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/** 전투 제어자 - 팩토리 */
public partial class BattleController : MonoBehaviour
{
	#region 팩토리 함수
	/** NPC 를 생성한다 */
	public NonPlayerController CreateNonPlayer(NPCTable a_oNPCTable,
		Vector3 a_stPos, GameObject a_oMapObjsRoot, CObjInfo a_oObjInfo, bool a_bIsAdd = true, List<EffectTable> a_oEffectTableList = null)
	{
		var oStatTableKey = string.Format(ComType.G_KEY_FMT_NPC_STAT_TABLE,
			a_oNPCTable.StatsValue, this.BattlePlayInfo.m_nStandardNPCLevel);

		uint nStatTableKey = uint.Parse(oStatTableKey, NumberStyles.HexNumber);

		var oNonPlayerController = GameResourceManager.Singleton.CreateObject<NonPlayerController>(EResourceType.Character_NPC,
			a_oNPCTable.Prefab, a_oMapObjsRoot.transform, a_nTheme: a_oNPCTable.Theme - 1);

		oNonPlayerController.transform.position = a_stPos;
		oNonPlayerController.transform.localScale = Vector3.one;
		oNonPlayerController.transform.localEulerAngles = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

		oNonPlayerController.Init(a_oNPCTable, NPCStatTable.GetData(nStatTableKey), a_oObjInfo);
		oNonPlayerController.AddPassiveEffects(a_oEffectTableList);

		// 추가 모드 일 경우
		if (a_bIsAdd)
		{
			this.AddUnitController(oNonPlayerController);
		}

		oNonPlayerController.SetupAbilityValues(true);
		return oNonPlayerController;
	}

	/** 아이템 객체를 생성한다 */
	public void CreateGroundItemObjs(Dictionary<uint, int> a_oItemInfoDict, List<GameObject> a_oOutItemObjList)
	{
		foreach (var stKeyVal in a_oItemInfoDict)
		{
			string oKey = stKeyVal.Key.ToString("X");

			// 더미 데이터 일 경우
			if (oKey.Length < 2)
			{
				continue;
			}

			// 방어전 일 경우
			if (GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.DEFENCE)
			{
				continue;
			}

			string oType = oKey.Substring(0, 2);
			GameObject oItemObj = null;

			bool bIsEnableOnlyFieldItem = GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ADVENTURE ||
				GameDataManager.Singleton.PlayMapInfoType == EMapInfoType.ABYSS;

			// 필드 아이템만 드랍 가능 할 경우
			if (bIsEnableOnlyFieldItem)
			{
				switch (oType)
				{
					case "2F": oItemObj = this.CreateGroundItemObjField(stKeyVal.Key, stKeyVal.Value); break;
				}
			}
			else
			{
				switch (oType)
				{
					case "20": oItemObj = this.CreateGroundItemObjWeapon(stKeyVal.Key, stKeyVal.Value); break;
					case "22":
						var oMatTable = MaterialTable.GetData(stKeyVal.Key);

						bool bIsNeedsDivide = oMatTable.Type == (int)EItemType.Currency && oMatTable.SubType == 1;
						bIsNeedsDivide = bIsNeedsDivide || (oMatTable.Type == (int)EItemType.Currency && oMatTable.SubType == 2);
						bIsNeedsDivide = bIsNeedsDivide || (oMatTable.Type == (int)EItemType.Currency && oMatTable.SubType == 3);

						// 분할이 필요 할 경우
						if (bIsNeedsDivide)
						{
							int nDivideVal = GlobalTable.GetData<int>(ComType.G_VALUE_GAME_MONEY_DIVIDE);
							nDivideVal = (oMatTable.SubType == 3) ? nDivideVal : GlobalTable.GetData<int>(ComType.G_VALUE_CRYSTAL_DIVIDE);

							int nTimes = Mathf.Max(1, stKeyVal.Value / nDivideVal);

							for (int i = 0; i < nTimes; ++i)
							{
								var oCoinItemObj = this.CreateGroundItemObjMaterial(stKeyVal.Key, stKeyVal.Value / nTimes);
								a_oOutItemObjList.Add(oCoinItemObj);
							}
						}
						else
						{
							oItemObj = this.CreateGroundItemObjMaterial(stKeyVal.Key, stKeyVal.Value);
						}

						break;
					case "2F": oItemObj = this.CreateGroundItemObjField(stKeyVal.Key, stKeyVal.Value); break;
				}
			}

			// 아이템 객체가 존재 할 경우
			if (oItemObj != null)
			{
				a_oOutItemObjList.Add(oItemObj);
			}
		}
	}

	/** 무기 아이템 객체를 생성한다 */
	private GameObject CreateGroundItemObjWeapon(uint a_nKey, int a_nNumWeapons)
	{
		var oGroundWeapon = GameResourceManager.Singleton.CreateObject(EResourceType.BG, ComType.G_BG_N_ITEM_GROUND_WEAPON, this.PathObjRoot);
		oGroundWeapon.GetComponent<CGroundWeaponHandler>()?.Init(WeaponTable.GetData(a_nKey), a_nNumWeapons);

		return oGroundWeapon;
	}

	/** 재료 아이템 객체를 생성한다 */
	private GameObject CreateGroundItemObjMaterial(uint a_nKey, int a_nNumMaterials)
	{
		string oPrefabName = this.GetGroundItemObjMaterialPrefabName(a_nKey);

		// 프리팹 이름이 유효 할 경우
		if (!string.IsNullOrEmpty(oPrefabName))
		{
			var oGroundMaterial = GameResourceManager.Singleton.CreateObject(EResourceType.BG, oPrefabName, this.PathObjRoot);
			oGroundMaterial.GetComponent<CGroundMaterialHandler>()?.Init(MaterialTable.GetData(a_nKey), a_nNumMaterials);

			return oGroundMaterial;
		}

		return null;
	}

	/** 필드 오브젝트 아이템 객체를 생성한다 */
	private GameObject CreateGroundItemObjField(uint a_nKey, int a_nNumFieldObjs)
	{
		var oTable = FieldObjectTable.GetData(a_nKey);

		var oGroundFieldObj = GameResourceManager.Singleton.CreateObject(EResourceType.BG, oTable.Prefab, this.PathObjRoot);
		oGroundFieldObj.GetComponent<CGroundFieldObjHandler>()?.Init(oTable, a_nNumFieldObjs);

		return oGroundFieldObj;
	}
	#endregion // 팩토리 함수
}
