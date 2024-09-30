using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

/** 맵 객체 템플릿 정보 */
[JsonObject]
public class CMapObjTemplateInfo
{
	#region 변수
	[JsonProperty("N")] public string m_oName = string.Empty;
	[JsonProperty("MapOIL")] public List<CObjInfo> m_oMapObjInfoList = new List<CObjInfo>();
	#endregion // 변수

	#region 함수
	/** 역직렬화 되었을 경우 */
	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext a_oContext)
	{
		m_oName = m_oName ?? string.Empty;
	}
	#endregion // 함수
}

/** 맵 객체 템플릿 정보 테이블 */
public class CMapObjTemplateInfoTable : SingletonMono<CMapObjTemplateInfoTable>
{
	#region 프로퍼티
	public List<CMapObjTemplateInfo> MapObjTemplateInfo { get; private set; } = new List<CMapObjTemplateInfo>();
	#endregion // 프로퍼티

	#region 함수
	/** 맵 객체 템플릿 정보를 추가한다 */
	public void AddMapObjTemplateInfo(CMapObjTemplateInfo a_oMapObjTemplateInfo)
	{
		this.MapObjTemplateInfo.Add(a_oMapObjTemplateInfo);
	}

	/** 맵 객체 템플릿 정보를 제거한다 */
	public void RemoveMapObjTemplateInfo(CMapObjTemplateInfo a_oMapObjTemplateInfo)
	{
		this.MapObjTemplateInfo.Remove(a_oMapObjTemplateInfo);
	}

	/** 맵 객체 템플릿 정보를 로드한다 */
	public List<CMapObjTemplateInfo> LoadMapObjTemplateInfos()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		// 파일이 존재 할 경우
		if(File.Exists(ComType.G_RUNTIME_TABLE_P_MAP_OBJ_TEMPLATE_INFO))
		{
			string oTablePath = ComType.G_RUNTIME_TABLE_P_MAP_OBJ_TEMPLATE_INFO;
			this.MapObjTemplateInfo = ComUtil.ReadJSONObj<List<CMapObjTemplateInfo>>(oTablePath, false);
		}
#endif // #if UNITY_EDITOR || UNITY_STANDALONE

		return this.MapObjTemplateInfo;
	}

	/** 맵 객체 템플릿 정보를 저장한다 */
	public void SaveMapObjTemplateInfos()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		string oTablePath = ComType.G_RUNTIME_TABLE_P_MAP_OBJ_TEMPLATE_INFO;
		ComUtil.WriteJSONObj(oTablePath, this.MapObjTemplateInfo, false, false);
#endif // #if UNITY_EDITOR || UNITY_STANDALONE
	}
	#endregion // 함수
}
