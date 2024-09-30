using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/** 재료 아이템 처리자 */
public class CGroundMaterialHandler : CGroundItemHandler
{
	#region 변수
	private MaterialTable m_oTable = null;
	#endregion // 변수

	#region 프로퍼티
	public override int Grade => m_oTable.Grade;
	public override string ItemName => NameTable.GetValue(m_oTable.NameKey);
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(MaterialTable a_oTable, int a_nNumMaterials)
	{
		base.Init(a_oTable.PrimaryKey, a_nNumMaterials);
		m_oTable = a_oTable;

		// 부품 일 경우
		if ((EItemType)a_oTable.Type == EItemType.Material && a_oTable.SubType != 0)
		{
			var oMaterial = GameResourceManager.Singleton.CreateObject(EResourceType.BG, a_oTable.Prefab, this.Dummy.transform);
			this.SetExtraGroundItem(oMaterial);
		}
	}

	/** UI 상태를 갱신한다 */
	protected override void UpdateUIsState()
	{
		base.UpdateUIsState();

		// 이미지를 갱신한다
		for (int i = 0; i < this.GradeMakrImgList.Count; ++i)
		{
			this.GradeMakrImgList[i].gameObject.SetActive(false);
		}
	}

	/** 획득 연출을 시작한다 */
	protected override void StartAcquireDirecting()
	{
		base.StartAcquireDirecting();

		// 재화 일 경우
		if(m_oTable.Type ==  (int)EItemType.Currency && m_oTable.SubType == 3)
		{
			this.ExtraGroundItem.transform.SetParent(Camera.main.transform);
		}
		else
		{
		}
	}
	#endregion // 함수
}
