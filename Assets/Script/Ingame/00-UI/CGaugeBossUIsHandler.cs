using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 보스 게이지 UI 처리자 */
public class CGaugeBossUIsHandler : CGaugeUIsHandler
{
	/** 매개 변수 */
	public struct STParams
	{
		public NPCTable m_oTable;
	}

	/** 분할 UI */
	private struct STSplitUIs
	{
		public GameObject m_oOnUIs;
		public GameObject m_oOffUIs;
	}

	#region 변수
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oDescText = null;
	[SerializeField] private GameObject m_oSplitUIsRoot = null;

	private List<float> m_oPercentList = new List<float>();
	private List<STSplitUIs> m_oSplitUIsList = new List<STSplitUIs>();
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }

	public TMP_Text NameText => m_oNameText;
	public TMP_Text DescText => m_oDescText;

	public List<float> PercentList => m_oPercentList;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public override void Awake()
	{
		base.Awake();

		m_oPercentList.ExAddVal(0.25f);
		m_oPercentList.ExAddVal(0.5f);
		m_oPercentList.ExAddVal(0.75f);
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		m_oNameText.text = NameTable.GetValue(a_stParams.m_oTable.NameKey);
		m_oDescText.text = NameTable.GetValue(a_stParams.m_oTable.NameKey);

		// TODO: 보스 문구 사용 시 수정 필요
		m_oNameText.text = string.Empty;
		m_oDescText.text = string.Empty;
	}

	/** 초기화 */
	public void Start()
	{
		for (int i = 0; i < m_oSplitUIsRoot.transform.childCount; ++i)
		{
			var stSplitUIs = new STSplitUIs()
			{
				m_oOnUIs = m_oSplitUIsRoot.transform.GetChild(i).Find("IconImg_On")?.gameObject,
				m_oOffUIs = m_oSplitUIsRoot.transform.GetChild(i).Find("IconImg_Off")?.gameObject
			};

			m_oSplitUIsList.Add(stSplitUIs);
		}

		for (int i = 0; i < m_oSplitUIsRoot.transform.childCount; ++i)
		{
			var oSplitUIs = m_oSplitUIsRoot.transform.GetChild(i) as RectTransform;
			oSplitUIs.gameObject.SetActive(i < m_oPercentList.Count);

			// 퍼센트가 없을 경우
			if (i >= m_oPercentList.Count)
			{
				continue;
			}

			var oGaugeSliderTrans = this.GaugeSlider.transform as RectTransform;
			oSplitUIs.anchoredPosition = new Vector2(oGaugeSliderTrans.rect.width * m_oPercentList[i], oSplitUIs.anchoredPosition.y);
		}

		this.SetPercent(1.0f);
	}
	#endregion // 함수

	#region 접근 함수
	/** 퍼센트를 변경한다 */
	public override void SetPercent(float a_fPercent)
	{
		base.SetPercent(a_fPercent);

		for (int i = 0; i < m_oPercentList.Count; ++i)
		{
			// 분할 UI 가 없을 경우
			if (i >= m_oSplitUIsList.Count)
			{
				continue;
			}

			m_oSplitUIsList[i].m_oOnUIs?.SetActive(m_oPercentList[i].ExIsLess(a_fPercent));
			m_oSplitUIsList[i].m_oOffUIs?.SetActive(m_oPercentList[i].ExIsGreatEquals(a_fPercent));
		}
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(NPCTable a_oTable)
	{
		return new STParams()
		{
			m_oTable = a_oTable
		};
	}
	#endregion // 클래스 팩토리 함수
}
