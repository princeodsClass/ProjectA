using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/** 전투 페이지 - 인벤토리 */
public partial class PageBattle : UIDialog
{
	#region 변수
	[Header("=====> Page Battle (Inventory) - Etc <=====")]
	[SerializeField] private BattleInventory m_oBattleInventory = null;

	[Header("=====> Page Battle (Inventory) - UIs <=====")]
	[SerializeField] private GameObject m_oBattleInventoryUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public BattleInventory BattleInventory => m_oBattleInventory;
	public GameObject BattleInventoryUIs => m_oBattleInventoryUIs;
	#endregion // 프로퍼티
}
