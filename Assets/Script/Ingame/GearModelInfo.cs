using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GearModelInfo : MonoBehaviour
{
	[Header("ui 중심 게임 오브젝트")]
	[SerializeField]
	GameObject _goUIDummy;

	/// <summary>
	/// ui 중심으로 돌릴 ventor3 ui 에서 보여질 scale / rotation 모두 처리.
	/// </summary>
	/// <returns></returns>
	public GameObject GetUIDummy()
	{
		return _goUIDummy;
	}
}
