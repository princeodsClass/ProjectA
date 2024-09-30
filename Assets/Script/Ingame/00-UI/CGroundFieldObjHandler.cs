using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 필드 오브젝트 아이템 처리자 */
public class CGroundFieldObjHandler : CGroundItemHandler
{
	#region 변수
	private FieldObjectTable m_oTable = null;
	#endregion // 변수

	#region 프로퍼티
	public override int Grade => 1;
	public override string ItemName => NameTable.GetValue(m_oTable.NameKey);
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(FieldObjectTable a_oTable, int a_nNumFieldObjs)
	{
		base.Init(a_oTable.PrimaryKey, a_nNumFieldObjs);
		m_oTable = a_oTable;
	}
	#endregion // 함수
}
