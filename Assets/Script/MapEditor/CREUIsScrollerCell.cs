using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 스크롤러 셀 */
	public class CREUIsScrollerCell : MonoBehaviour
	{
		#region 변수
		[Header("=====> UIs <=====")]
		[SerializeField] private Text m_oNameText = null;
		[SerializeField] private Button m_oSelBtn = null;
		#endregion // 변수

		#region 프로퍼티
		public Text NameText => m_oNameText;
		public Button SelBtn => m_oSelBtn;
		#endregion // 프로퍼티
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
