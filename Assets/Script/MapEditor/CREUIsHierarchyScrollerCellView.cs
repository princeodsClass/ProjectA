using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 계층 스크롤러 셀 뷰 */
public class CREUIsHierarchyScrollerCellView : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public bool m_bIsSel;
		public GameObject m_oPrefabObj;
		public System.Action<CREUIsHierarchyScrollerCellView, GameObject> m_oCallback;
	}

	#region 변수
	private Button m_oSelBtn = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		// 버튼을 설정한다
		m_oSelBtn = this.GetComponent<Button>();
	}

	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;

		m_oSelBtn?.onClick.RemoveAllListeners();
		m_oSelBtn?.onClick.AddListener(this.OnTouchSelBtn);

		// 텍스트가 존재 할 경우
		if (m_oSelBtn.TryGetComponent(out Text oText))
		{
			oText.text = a_stParams.m_oPrefabObj.name;
			oText.color = a_stParams.m_bIsSel ? Color.red : Color.black;
		}
	}

	/** 선택 버튼을 눌렀을 경우 */
	private void OnTouchSelBtn()
	{
		this.Params.m_oCallback?.Invoke(this, this.Params.m_oPrefabObj);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(GameObject a_oPrefabObj, bool a_bIsSel, System.Action<CREUIsHierarchyScrollerCellView, GameObject> a_oCallback)
	{
		return new STParams()
		{
			m_bIsSel = a_bIsSel,
			m_oPrefabObj = a_oPrefabObj,
			m_oCallback = a_oCallback
		};
	}
	#endregion // 클래스 팩토리 함수
}
