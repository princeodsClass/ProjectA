using System.Collections.Generic;
using UnityEngine;

internal static class YieldInstructionCache
{
	class FloatComparer : IEqualityComparer<float>
	{
		bool IEqualityComparer<float>.Equals(float x, float y)
		{
			return x == y;
		}
		int IEqualityComparer<float>.GetHashCode(float obj)
		{
			return obj.GetHashCode();
		}
	}

	public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
	public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

	private static readonly Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());
	private static readonly Dictionary<float, WaitForSecondsRealtime> _timeIntervalRealtime = new Dictionary<float, WaitForSecondsRealtime>();

	public static void Clear()
	{
		_timeInterval.Clear();
		_timeIntervalRealtime.Clear();
	}

	public static WaitForSeconds WaitForSeconds(float seconds)
	{
		WaitForSeconds wfs;
		if (!_timeInterval.TryGetValue(seconds, out wfs))
			_timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
		return wfs;
	}

	public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
	{
		WaitForSecondsRealtime wfs;
		if (!_timeIntervalRealtime.TryGetValue(seconds, out wfs))
			_timeIntervalRealtime.Add(seconds, wfs = new WaitForSecondsRealtime(seconds));
		return wfs;
	}
}