using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 전투 인벤토리 */
public class BattleInventory : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public PageBattle m_oPageBattle;
	}

	#region 변수
	[Header("=====> Popup Battle Inventory - Etc <=====")]
	[SerializeField] private Animator m_oAnimator = null;
	private List<ItemWeapon> m_oItemWeaponList = new List<ItemWeapon>();

	private List<ItemWeapon> m_oInventoryItemWeaponList = new List<ItemWeapon>();

	[Header("=====> Popup Battle Inventory - UIs <=====")]
	[SerializeField] private Image m_oBGImg = null;
	[SerializeField] private ScrollRect m_oScrollRect = null;

	[Header("=====> Popup Battle Inventory - Game Objects <=====")]
	[SerializeField] private GameObject m_oWeaponPrefab = null;
	[SerializeField] private GameObject m_oScrollerCellViewPrefab = null;

	[SerializeField] private GameObject m_oWaitUIs = null;
	[SerializeField] private GameObject m_oScrollViewContents = null;

	[SerializeField] private List<GameObject> m_oSlotUIsList = new List<GameObject>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public PopupWeaponChange PopupWeaponChange { get; private set; } = null;

	public ScrollRect ScrollRect => m_oScrollRect;

	public List<ItemWeapon> ItemWeaponList => m_oItemWeaponList;
	public List<ItemWeapon> InventoryItemWeaponList => m_oInventoryItemWeaponList;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Start()
	{
		m_oBGImg.gameObject.SetActive(false);
	}

	/** 상태를 리셋한다 */
	public virtual void Reset()
	{
		this.SetupScrollViewContents();
		this.UpdateUIsState();

		this.Params.m_oPageBattle.PlayerController.UpdateEquipWeapons();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		this.SetupScrollViewContents();
		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		for (int i = 0; i < m_oSlotUIsList.Count; ++i)
		{
			var oItemWeapon = this.FindEquipWeapon(i);
			var oSlotWeapon = m_oSlotUIsList[i].GetComponentInChildren<SlotWeapon>();

			oSlotWeapon?.gameObject.SetActive(oItemWeapon != null && !this.Params.m_oPageBattle.BattleController.IsUpdateWeapons);

			// 무기가 존재 할 경우
			if (oItemWeapon != null)
			{
				oSlotWeapon.Initialize(oItemWeapon, a_oPopupInventory: this);
				oSlotWeapon.SetState(SlotWeapon.SlotState.equiped);
			}
		}

		int nNumChildren = m_oInventoryItemWeaponList.Count / ComType.G_MAX_NUM_EQUIP_WEAPONS;
		int nExtraNumChildren = (m_oInventoryItemWeaponList.Count % ComType.G_MAX_NUM_EQUIP_WEAPONS != 0) ? 1 : 0;

		int nTotalNumInventoryItemWeapons = (nNumChildren + nExtraNumChildren) * ComType.G_MAX_NUM_EQUIP_WEAPONS;
		m_oWaitUIs.SetActive(this.Params.m_oPageBattle.BattleController.IsUpdateWeapons);

		for (int i = 0; i < nTotalNumInventoryItemWeapons; ++i)
		{
			int nIdx = i / ComType.G_MAX_NUM_EQUIP_WEAPONS;
			int nSlotIdx = i % ComType.G_MAX_NUM_EQUIP_WEAPONS;

			var oChild = m_oScrollViewContents.transform.GetChild(nIdx);
			var oScrollerCellView = oChild.GetComponentInChildren<BattleInventoryScrollerCellView>();

			var oSlotWeapon = oScrollerCellView.SlotUIsList[nSlotIdx].GetComponentInChildren<SlotWeapon>();
			oSlotWeapon?.gameObject.SetActive(i < m_oInventoryItemWeaponList.Count && !this.Params.m_oPageBattle.BattleController.IsUpdateWeapons);

			// 무기가 존재 할 경우
			if (oSlotWeapon != null && m_oInventoryItemWeaponList.ExIsValidIdx(i))
			{
				oSlotWeapon.Initialize(m_oInventoryItemWeaponList[i], a_oPopupInventory: this);
				oSlotWeapon.SetState(SlotWeapon.SlotState.unequiped);
			}
		}

		for (int i = 0; i < m_oScrollViewContents.transform.childCount; ++i)
		{
			var oScrollerCellView = m_oScrollViewContents.transform.GetChild(i);
			oScrollerCellView.gameObject.SetActive(i < nNumChildren + nExtraNumChildren);
		}

		ComUtil.RebuildLayouts(this.gameObject);
	}

	/** 인벤토리 닫기 버튼을 눌렀을 경우 */
	public void OnTouchCloseBtn()
	{
		this.gameObject.SetActive(false);
	}

	/** 인벤토리를 출력한다 */
	public void OpenInventory()
	{
		m_oAnimator.SetTrigger(ComType.G_PARAMS_OPEN);
		m_oScrollRect.verticalNormalizedPosition = 1.0f;

		this.gameObject.SetActive(true);
	}
	
	/** 장착 무기 여부를 검사한다 */
	public bool IsEquipWeapon(ItemWeapon a_oItemWeapon)
	{
		return this.FindEquipWeaponIdx(a_oItemWeapon) >= 0;
	}

	/** 무기를 탐색한다 */
	public ItemWeapon FindWeapon(int a_nID)
	{
		for (int i = 0; i < m_oItemWeaponList.Count; ++i)
		{
			// 식별자가 동일 할 경우
			if (a_nID == m_oItemWeaponList[i].id)
			{
				return m_oItemWeaponList[i];
			}
		}

		return null;
	}

	/** 장착 무기를 탐색한다 */
	public ItemWeapon FindEquipWeapon(int a_nIdx)
	{
		for (int i = 0; i < m_oItemWeaponList.Count; ++i)
		{
			// 장착 무기 일 경우
			if (GameManager.Singleton.user.m_nWeaponID[a_nIdx] == m_oItemWeaponList[i].id)
			{
				return m_oItemWeaponList[i];
			}
		}

		return null;
	}

	/** 장착 무기 인덱스를 탐색한다 */
	public int FindEquipWeaponIdx(ItemWeapon a_oItemWeapon)
	{
		for (int i = 0; i < ComType.G_MAX_NUM_EQUIP_WEAPONS; ++i)
		{
			// 장착 무기 일 경우
			if (a_oItemWeapon.id == GameManager.Singleton.user.m_nWeaponID[i])
			{
				return i;
			}
		}

		return -1;
	}

	/** 스크롤 뷰 컨텐츠를 설정한다 */
	private void SetupScrollViewContents()
	{
		this.SetupItemWeapons();
		this.SetupEquipWeapons();

		for (int i = 0; i < m_oItemWeaponList.Count; i += ComType.G_MAX_NUM_EQUIP_WEAPONS)
		{
			int nIdx = i / ComType.G_MAX_NUM_EQUIP_WEAPONS;

			var oScrollerCellView = (nIdx < m_oScrollViewContents.transform.childCount) ?
				m_oScrollViewContents.transform.GetChild(nIdx).GetComponentInChildren<BattleInventoryScrollerCellView>() : GameResourceManager.Singleton.CreateObject<BattleInventoryScrollerCellView>(m_oScrollerCellViewPrefab, m_oScrollViewContents.transform, null);

			for (int j = 0; j < ComType.G_MAX_NUM_EQUIP_WEAPONS && j + i < m_oItemWeaponList.Count; ++j)
			{
				this.SetupWeapon(m_oItemWeaponList[j + i], oScrollerCellView.SlotUIsList[j]);
			}
		}
	}

	/** 무기를 설정한다 */
	private void SetupItemWeapons()
	{
		m_oItemWeaponList.Clear();
		m_oInventoryItemWeaponList.Clear();

		GameManager.Singleton.invenWeapon.SortItem();

		foreach (var oItemWeapon in GameManager.Singleton.invenWeapon)
		{
			m_oItemWeaponList.Add(oItemWeapon);

			// 장착 무기가 아닐 경우
			if (!this.IsEquipWeapon(oItemWeapon))
			{
				m_oInventoryItemWeaponList.Add(oItemWeapon);
			}
		}
	}

	/** 장착 무기를 설정한다 */
	private void SetupEquipWeapons()
	{
		foreach (var oItemWeapon in GameManager.Singleton.invenWeapon)
		{
			// 장착 무기가 아닐 경우
			if (!this.IsEquipWeapon(oItemWeapon))
			{
				continue;
			}

			int nIdx = this.FindEquipWeaponIdx(oItemWeapon);
			this.SetupWeapon(oItemWeapon, m_oSlotUIsList[nIdx]);
		}
	}

	/** 무기를 설정한다 */
	public void SetupWeapon(ItemWeapon a_oItemWeapon, GameObject a_oSlotUIs)
	{
		var oSlotWeapon = a_oSlotUIs.GetComponentInChildren<SlotWeapon>();

		// 무기 슬롯이 존재 할 경우
		if (oSlotWeapon != null)
		{
			return;
		}

		oSlotWeapon = this.CreateSlotWeapon(a_oItemWeapon, m_oScrollViewContents);
		ComUtil.SetParent(a_oSlotUIs?.transform, oSlotWeapon.transform);
	}
	#endregion // 함수

	#region 접근 함수
	/** 무기 변경 팝업을 변경한다 */
	public void SetPopupWeaponChange(PopupWeaponChange a_oPopup)
	{
		this.PopupWeaponChange = a_oPopup;
	}
	#endregion // 접근 함수

	#region 팩토리 함수
	/** 무기를 생성한다 */
	public SlotWeapon CreateSlotWeapon(ItemWeapon a_oItemWeapon, GameObject a_oParent)
	{
		var oSlotWeapon = MenuManager.Singleton.LoadComponent<SlotWeapon>(a_oParent?.transform, EUIComponent.BattleSlotWeapon);
		oSlotWeapon.Initialize(a_oItemWeapon, a_oPopupInventory: this);
		oSlotWeapon.SetNewTag(false);
		oSlotWeapon.SetTargetFX(false);
		oSlotWeapon.SetEquipReady(false);
		oSlotWeapon.SetSelected(SlotWeapon.SlotState.normal);

		return oSlotWeapon;
	}
	#endregion // 팩토리 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(PageBattle a_oPageBattle)
	{
		return new STParams()
		{
			m_oPageBattle = a_oPageBattle
		};
	}
	#endregion // 클래스 팩토리 함수
}
