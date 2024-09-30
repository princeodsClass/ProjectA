using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScanner : MonoBehaviour
{
    LayerMask _TargetLayer;
    RaycastHit[] _Targets;
    Transform _tCurrentTarget;

    public float _fScanRange;

    private void FixedUpdate()
    {
        _Targets = Physics.SphereCastAll(transform.position, _fScanRange, Vector3.zero, 0, _TargetLayer);
    }

    public Transform GetCurrentTarget()
    {
        Transform curtarget = null;
        float fCurDistance = _fScanRange;

        foreach ( RaycastHit target in _Targets )
        {
            Vector3 mpos = transform.position;
            Vector3 tpos = target.transform.position;

            float targetDistance = Vector3.Distance(mpos, tpos);

            if ( targetDistance < fCurDistance)
            {
                fCurDistance = targetDistance;
                curtarget = target.transform;
            }
        }

        return curtarget;
    }

    public bool IsExistTarget()
    {
        return _Targets.Length > 0;
    }

    public bool ContainsIndex(int index)
    {
        return false;
    }
}
