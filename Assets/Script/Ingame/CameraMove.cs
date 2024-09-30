using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMove : MonoBehaviour
{
	[SerializeField]
	public Image m_oFocusImg = null;

	[SerializeField]
	private GameObject m_oBlindUIs = null;

    [SerializeField]
    private Transform _DummyTransform, _tCharacter;

    public float _fCamHeight = 15f;
    public float _fCamForward = -15f;

    public bool _isBack = false;
    public bool _isMenual = false;

    // Transform _tarTransform;
    private CamDummy m_oCamDummy = null;

	#region 프로퍼티
	public bool IsFocus { get; set; } = false;
	public bool IsDimensional { get; set; } = false;
	public GameObject BlindUIs => m_oBlindUIs;
	#endregion // 프로퍼티

    private void Awake()
    {
#if DISABLE_THIS
        _fCamHeight = 15f;
        _fCamForward = -15f;
#endif // #if DISABLE_THIS

		m_oCamDummy = _DummyTransform.GetComponent<CamDummy>();
        // _tarTransform = transform;

		this.transform.position = _DummyTransform.position + 
			_DummyTransform.forward * _fCamForward + _DummyTransform.up * _fCamHeight;
    }

    public void SetPlayer(GameObject go)
    {
        if (null != _tCharacter ||
             null == go) return;

        _tCharacter = go.transform;
    }

    public void LateUpdate()
    {
        if ( _isMenual )
            return;

        if (null != _DummyTransform && !_isBack && m_oCamDummy != null && m_oCamDummy.Target != null)
        {
			var stPos = Vector3.zero;
			var stOffset = this.IsFocus ? new Vector3(0.0f, 0.5f, 0.0f) : Vector3.zero;

			// 방향 무시 모드 일 경우
			if(this.IsFocus)
			{
				stPos = m_oCamDummy.Target.transform.position + 
					m_oCamDummy.Target.transform.forward * -_fCamForward + m_oCamDummy.Target.transform.up * _fCamHeight / 2.0f;
			}
			else
			{
				stPos = _DummyTransform.position + 
					_DummyTransform.forward * _fCamForward + _DummyTransform.up * _fCamHeight;
			}

			this.transform.position = (this.IsFocus || this.IsDimensional) ? 
				Vector3.Lerp(this.transform.position, stPos, this.IsFocus ? 0.2f : 0.35f) : stPos;

            transform.LookAt(this.IsFocus ? 
				m_oCamDummy.Target.transform.position + stOffset : _DummyTransform.transform.position + stOffset);
        }
        else if ( null != _tCharacter && _isBack)
        {
            FOVController(70f);
            transform.position = _tCharacter.position
                                 + new Vector3(1f, 1.5f, -3.5f);
            transform.LookAt(transform.position
                             + new Vector3(0, 0, 1f));
        }
    }

    void FOVController(float fov)
    {
        Camera.main.fieldOfView = fov;
    }
}
