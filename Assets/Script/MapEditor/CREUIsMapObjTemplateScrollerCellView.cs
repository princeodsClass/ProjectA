using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 스크롤러 셀 뷰 */
	public class CREUIsMapObjTemplateScrollerCellView : MonoBehaviour
	{
		#region 변수
		[Header("=====> Scroller Cell View - UIs <=====")]
		[SerializeField] private Text m_oNameText = null;
		[SerializeField] private Button m_oSelBtn = null;
		
		[SerializeField] private Button m_oRemoveBtn = null;
		#endregion // 변수

		#region 프로퍼티
		public Text NameText => m_oNameText;
		public Button SelBtn => m_oSelBtn;

		public Button RemoveBtn => m_oRemoveBtn;
		#endregion // 프로퍼티
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
