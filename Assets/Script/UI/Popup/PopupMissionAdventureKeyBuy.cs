using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 탐험 미션 키 구입 팝업 */
public class PopupMissionAdventureKeyBuy : UIDialog
{
	/** 매개 변수 */
	public struct STParams
	{
		public MissionAdventureTable m_oTable;
		public PopupMissionAdventure m_oPopup;
		public System.Action<PopupMissionAdventureKeyBuy, bool> m_oCallback;
	}

	#region 변수
	[Header("=====> PopupMissionAdventureKeyBuy - UIs <=====")]
	[SerializeField] private TMP_Text m_oTimeText = null;
	[SerializeField] private TMP_Text m_oTitleText = null;
	[SerializeField] private TMP_Text m_oPriceText = null;
	[SerializeField] private TMP_Text m_oNumCrystalText = null;

	[SerializeField] private List<LayoutGroup> m_oLayoutGroupList = new List<LayoutGroup>();
	[SerializeField] private List<ContentSizeFitter> m_oSizeFitterList = new List<ContentSizeFitter>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		base.Initialize();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.UpdateUIsState();
	}

	/** 초기화 */
	public void Start()
	{
		this.RebuildLayouts();
	}

	/** 레이아웃을 재정렬한다 */
	public void RebuildLayouts()
	{
		for (int i = 0; i < m_oSizeFitterList.Count; ++i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oSizeFitterList[i].transform as RectTransform);
		}

		for (int i = 0; i < m_oLayoutGroupList.Count; ++i)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_oLayoutGroupList[i].transform as RectTransform);
		}
	}

	/** 팝업을 닫는다 */
	public override void Close()
	{
		base.Close();
		this.Params.m_oPopup.KeyBuyPopup = null;
	}

	/** 구입 버튼을 눌렀을 경우 */
	public void OnTouchBuyBtn()
	{
		// 구입이 불가능 할 경우
		if (!GameManager.Singleton.IsEnableChargeAdventureKey())
		{
			return;
		}

		int nPrice = GlobalTable.GetData<int>("countKeyCardBuyMaterial");

		// 크리스탈이 부족 할 경우
		if (GameManager.Singleton.invenMaterial.CalcTotalCrystal() < nPrice)
		{
			MenuManager.Singleton.OpenPopup<PopupShopCrystal>(EUIPopup.PopupShopCrystal, true);
		}
		else
		{
			StartCoroutine(this.ConsumeCrystal(nPrice));
		}
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		int nPrice = GlobalTable.GetData<int>("countKeyCardBuyMaterial");
		var oKeyMatTable = MaterialTable.GetData(ComType.G_KEY_MAT_ADVENTURE_KEY);
		
		m_oPriceText.text = $"{nPrice}";
		m_oNumCrystalText.text = $"{GameManager.Singleton.invenMaterial.CalcTotalCrystal()}";
		
		m_oTitleText.text = NameTable.GetValue(oKeyMatTable.NameKey);
	}
	#endregion // 함수

	#region 접근 함수
	/** 충전 시간을 설정한다 */
	public void SetChargeTime(string a_oTimeStr)
	{
		m_oTimeText.text = a_oTimeStr;
		this.RebuildLayouts();
	}
	#endregion // 접근 함수

	#region 코루틴 함수
	/** 크리스탈을 소비한다 */
	private IEnumerator ConsumeCrystal(int a_nNumCrystals)
	{
		var wait = m_MenuMgr.OpenPopup<PopupWait4Response>(EUIPopup.PopupWait4Response, true);
		yield return GameManager.Singleton.user.IncrAdventureKeyCount(1);

		yield return m_InvenMaterial.ConsumeCrystal(a_nNumCrystals);
		m_GameMgr.RefreshInventory(GameManager.EInvenType.Material);

		wait.Close();
		this.Params.m_oCallback?.Invoke(this, true);
	}
	#endregion // 코루틴 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(PopupMissionAdventure a_oPopup,
		MissionAdventureTable a_oTable, System.Action<PopupMissionAdventureKeyBuy, bool> a_oCallback)
	{
		return new STParams()
		{
			m_oTable = a_oTable,
			m_oPopup = a_oPopup,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클랙스 팩토리 함수
}
