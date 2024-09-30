using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 이벤트 전달자 */
public partial class CEventDispatcher : MonoBehaviour {
	/** 콜백 */
	private enum ECallback {
		NONE,
		ANI_EVENT,
		VISIBLE_EVENT,
		INVISIBLE_EVENT,
		PARTICLE_FX_EVENT,
		[HideInInspector] MAX_VAL
	}

	#region 변수
	private Dictionary<ECallback, System.Action<CEventDispatcher>> m_oCallbackDict01 = new Dictionary<ECallback, System.Action<CEventDispatcher>>();
	private Dictionary<ECallback, System.Action<CEventDispatcher, object>> m_oCallbackDict02 = new Dictionary<ECallback, System.Action<CEventDispatcher, object>>();
	#endregion // 변수

	#region 함수
	/** 애니메이션 이벤트를 수신했을 경우 */
	public void OnReceiveAniEvent(object a_oParams) {
		m_oCallbackDict02.GetValueOrDefault(ECallback.ANI_EVENT)?.Invoke(this, a_oParams);
	}

	/** 등장 이벤트를 수신했을 경우*/
	public void OnBecameVisible() {
		m_oCallbackDict01.GetValueOrDefault(ECallback.VISIBLE_EVENT)?.Invoke(this);
	}

	/** 사라짐 이벤트를 수신했을 경우 */
	public void OnBecameInvisible() {
		m_oCallbackDict01.GetValueOrDefault(ECallback.INVISIBLE_EVENT)?.Invoke(this);
	}
	
	/** 파티클 이벤트를 수신했을 경우 */
	public void OnParticleSystemStopped() {
		m_oCallbackDict01.GetValueOrDefault(ECallback.PARTICLE_FX_EVENT)?.Invoke(this);
	}
	#endregion // 함수

	#region 접근 함수
	/** 애니메이션 콜백을 변경한다 */
	public void SetAniCallback(System.Action<CEventDispatcher, object> a_oCallback) {
		m_oCallbackDict02.ExReplaceVal(ECallback.ANI_EVENT, a_oCallback);
	}

	/** 등장 콜백을 변경한다 */
	public void SetVisibleCallback(System.Action<CEventDispatcher> a_oCallback) {
		m_oCallbackDict01.ExReplaceVal(ECallback.VISIBLE_EVENT, a_oCallback);
	}

	/** 사라짐 콜백을 변경한다 */
	public void SetInvisibleCallback(System.Action<CEventDispatcher> a_oCallback) {
		m_oCallbackDict01.ExReplaceVal(ECallback.INVISIBLE_EVENT, a_oCallback);
	}

	/** 파티클 효과 콜백을 변경한다 */
	public void SetParticleFXCallback(System.Action<CEventDispatcher> a_oCallback) {
		m_oCallbackDict01.ExReplaceVal(ECallback.PARTICLE_FX_EVENT, a_oCallback);
	}
	#endregion // 접근 함수
}
