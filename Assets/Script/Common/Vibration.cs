using UnityEngine;


public class Vibration 
{

#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
	private static AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
	private static AndroidJavaObject AndroidVibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
	private static AndroidJavaClass VibrationEffectClass;
	private static AndroidJavaObject VibrationEffect;
	private static int DefaultAmplitude;
#else
    private static AndroidJavaClass UnityPlayer;
    private static AndroidJavaObject CurrentActivity;
    private static AndroidJavaObject AndroidVibrator = null;
    private static AndroidJavaClass VibrationEffectClass = null;
    private static AndroidJavaObject VibrationEffect;
    private static int DefaultAmplitude;
#endif
 
   private static bool _AbleToVibrate = true;

    public enum VibrateTypes { BattleBubble, CubeMinigame, BattleDown, GachaSpecial }

    private EOSType OSYype;

    private static int _sdkVersion = -1;

    private static long[] _DownPatternDuration = { 100, 100, 100, 100 };
    private static int[] _DownPatternAmplitude = { 0, 40, 0, 40 };

    private static long[] _GachaSpecialPatternDuration = { 100, 100, 100, 100 };
    private static int[] _GachaSpecialPatternAmplitude = { 0, 50, 0, 100 };

    public void ChangeVibrateState()
    {
        _AbleToVibrate = !_AbleToVibrate;
    }    

    public void SetVibrateState(bool state)
    {
        _AbleToVibrate = state;
    }

    public void Initialize()
    {
        OSYype = ComUtil.CheckOS();

        if (VibrationEffectClass == null)
        {
            if (OSYype == EOSType.Android)
                VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
        }

        int.TryParse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3),out _sdkVersion);
    }       

    public void Vibrate(VibrateTypes type)
    {
        if (_AbleToVibrate)
        {
            if (OSYype == EOSType.Android)
            {
                switch (type)
                {
                    case VibrateTypes.BattleBubble:
                        AndroidVibrate(50, 20);
                        break;

                    case VibrateTypes.CubeMinigame:
                        AndroidVibrate(50, 20);
                        break;

                    case VibrateTypes.BattleDown:
                        AndroidVibrate(_DownPatternDuration, _DownPatternAmplitude, -1);
                        break;
                }
            }
            else if (OSYype == EOSType.iOS)
            {
                //iOSTriggerHaptics(type);
            }
        }
    }

    private void AndroidVibrate(long milliseconds)
    {
        AndroidVibrator.Call("vibrate", milliseconds);
    }

    private void AndroidVibrate(long milliseconds, int amplitude)
    {
        // amplitude : 0 ~ 255

        if (_sdkVersion < 26)
        {
            AndroidVibrate(milliseconds);
        }
        else
        {
            VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", new object[] { milliseconds, amplitude });
            AndroidVibrator.Call("vibrate", VibrationEffect);
        }
    }

    private static void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat)
    {
        if (_sdkVersion < 26)
        {
            AndroidVibrator.Call("vibrate", pattern, repeat);
        }
        else
        {
            VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", new object[] { pattern, amplitudes, repeat });
            AndroidVibrator.Call("vibrate", VibrationEffect);
        }
    }
}
