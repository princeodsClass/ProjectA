using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 인벤토리 스크롤러 셀 뷰 */
public class BattleInventoryScrollerCellView : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public List<long> m_oIDList;
	}

	#region 변수
	[Header("=====> Popup Battle Inventory Scroller Cell View <=====")]
	[SerializeField] private List<GameObject> m_oSlotUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public List<GameObject> SlotUIsList => m_oSlotUIsList;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
	}
	#endregion // 함수
}
