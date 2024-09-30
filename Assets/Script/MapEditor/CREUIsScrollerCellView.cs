using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
namespace MapEditor
{
	/** 스크롤러 셀 뷰 */
	public class CREUIsScrollerCellView : MonoBehaviour
	{
		/** 매개 변수 */
		public struct STParams
		{
			public int m_nTheme;
			public int m_nGroup;
			public int m_nStart;
			public List<GameObject> m_oPrefabList;
			public System.Action<CREUIsScrollerCellView, int, int, int> m_oCallback;
		}

		#region 변수
		[SerializeField] private List<CREUIsScrollerCell> m_oScrollerCellList = new List<CREUIsScrollerCell>();
		#endregion // 변수

		#region 프로퍼티
		public STParams Params { get; private set; }

		public PageMapEditor PageMapEditor => MenuManager.Singleton.GetPage<PageMapEditor>(EUIPage.PageMapEditor);
		public List<CREUIsScrollerCell> ScrollerCellList => m_oScrollerCellList;
		#endregion // 프로퍼티

		#region 함수
		/** 초기화 */
		public virtual void Init(STParams a_stParams)
		{
			this.Params = a_stParams;

			for (int i = 0; i < m_oScrollerCellList.Count; ++i)
			{
				// 프리팹이 존재 할 경우
				if (this.Params.m_nStart + i < this.Params.m_oPrefabList.Count)
				{
					int nIdx = this.Params.m_nStart + i;
					int nResult = this.PageMapEditor.NPCTableList.FindIndex((a_oTable) => a_oTable.Prefab.Equals(this.Params.m_oPrefabList[nIdx].name));

					m_oScrollerCellList[i].NameText.text = (nResult >= 0 && nResult < this.PageMapEditor.NPCTableList.Count) ? NameTable.GetValue(this.PageMapEditor.NPCTableList[nResult].NameKey) : this.Params.m_oPrefabList[nIdx].name;
					m_oScrollerCellList[i].SelBtn.onClick.AddListener(() => this.Params.m_oCallback?.Invoke(this, this.Params.m_nTheme, this.Params.m_nGroup, nIdx));
				}
			}
		}

		/** 초기화 */
		public void Start()
		{
			for (int i = 0; i < m_oScrollerCellList.Count; ++i)
			{
				bool bIsValid = this.Params.m_nStart + i < this.Params.m_oPrefabList.Count;

				m_oScrollerCellList[i].SelBtn.enabled = bIsValid;
				m_oScrollerCellList[i].SelBtn.image.enabled = bIsValid;
				m_oScrollerCellList[i].SelBtn.interactable = bIsValid;
				m_oScrollerCellList[i].NameText.enabled = bIsValid;

				// 프리팹이 존재 할 경우
				if (bIsValid)
				{
					bool bIsLight = this.Params.m_oPrefabList[this.Params.m_nStart + i].name.Equals("BG_Light");
					bool bIsWayPoint = this.Params.m_oPrefabList[this.Params.m_nStart + i].name.Equals("BG_WayPoint");
					bool bIsStartPoint = this.Params.m_oPrefabList[this.Params.m_nStart + i].name.Equals("BG_StartPoint");
					bool bIsPlayerPos = this.Params.m_oPrefabList[this.Params.m_nStart + i].name.Equals(ComType.G_BG_N_PLAYER_POS);

					m_oScrollerCellList[i].SelBtn.interactable = !bIsLight && !bIsWayPoint && !bIsStartPoint && !bIsPlayerPos;
					m_oScrollerCellList[i].SelBtn.image.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.MapEditor, this.Params.m_oPrefabList[this.Params.m_nStart + i].name);
				}
			}
		}
		#endregion // 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
