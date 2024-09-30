using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/** 무기 아이템 처리자 */
public class CGroundWeaponHandler : CGroundItemHandler
{
	#region 변수
	private WeaponTable m_oTable = null;
	[SerializeField] private List<GameObject> m_oGradeFXList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public override int Grade => m_oTable.Grade;
	public override string ItemName => NameTable.GetValue(m_oTable.NameKey);
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(WeaponTable a_oTable, int a_nNumWeapons)
	{
		base.Init(a_oTable.PrimaryKey, a_nNumWeapons);
		m_oTable = a_oTable;

		// 연출을 설정한다
		for (int i = 0; i < m_oGradeFXList.Count; ++i)
		{
			m_oGradeFXList[i].SetActive(i == a_oTable.Grade - 1);
		}

		// 트랜스 폼을 설정한다 {
		var oWeapon = GameResourceManager.Singleton.CreateObject(EResourceType.Weapon, 
			a_oTable.Prefab, this.Dummy.transform);
		
		var oWeaponModelInfo = oWeapon.GetComponent<WeaponModelInfo>();
		this.PageBattle.PlayerController.ResetEquipWeapon(oWeapon);

		var oUIDummy = oWeaponModelInfo.GetUIDummy();
		this.SetExtraGroundItem(oWeapon);

		oWeapon.transform.localPosition = new Vector3(0,
			-oUIDummy.transform.localPosition.z * oUIDummy.transform.localScale.z, oUIDummy.transform.localPosition.y * oUIDummy.transform.localScale.y);

		oWeapon.transform.eulerAngles = oUIDummy.transform.eulerAngles;
		// 트랜스 폼을 설정한다 }
	}
	#endregion // 함수
}
