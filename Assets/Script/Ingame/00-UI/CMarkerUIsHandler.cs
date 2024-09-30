using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 마커 UI 처리자 */
public class CMarkerUIsHandler : MonoBehaviour
{
	/** 마커 타입 */
	public enum EMarkerType
	{
		NONE = -1,
		NORM,
		BOSS,
		GATE,
		BOX,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	[SerializeField] private Image m_oBossImg = null;
	[SerializeField] private Image m_oGateImg = null;
	[SerializeField] private Image m_oNormImg = null;
	[SerializeField] private Image m_oWarningImg = null;
	[SerializeField] private Image m_oBoxImg = null;

	private List<Image> m_oImgList = new List<Image>();
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		// 이미지를 설정한다
		this.GetComponentsInChildren<Image>(true, m_oImgList);
	}

	/** 초기화 */
	public void Start()
	{
		this.SetIsWarning(false);
		this.SetMarkerType(EMarkerType.NORM);
	}

	/** 상태를 갱신한다 */
	public void Update()
	{
		m_oBossImg.transform.rotation = Quaternion.identity;
		m_oGateImg.transform.rotation = Quaternion.identity;
		m_oBoxImg.transform.rotation = Quaternion.identity;
	}
	#endregion // 함수

	#region 접근 함수
	/** 마커 타입을 변경한다 */
	public void SetMarkerType(EMarkerType a_eType)
	{
		m_oBossImg.gameObject.SetActive(a_eType == EMarkerType.BOSS);
		m_oGateImg.gameObject.SetActive(a_eType == EMarkerType.GATE);
		m_oBoxImg.gameObject.SetActive(a_eType == EMarkerType.BOX);
	}

	/** 경고 여부를 변경한다 */
	public void SetIsWarning(bool a_bIsWarning)
	{
		m_oNormImg.gameObject.SetActive(!a_bIsWarning);
		m_oWarningImg.gameObject.SetActive(a_bIsWarning);
	}

	/** 투명도를 변경한다 */
	public void SetAlpha(float a_fAlpha)
	{
		for (int i = 0; i < m_oImgList.Count; ++i)
		{
			var stColor = m_oImgList[i].color;
			stColor.a = Mathf.Clamp01(a_fAlpha);

			m_oImgList[i].color = stColor;
		}
	}
	#endregion // 접근 함수
}
