using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EDevice { None = 0, iPhoneX, iPhoneXs_Max, Pixel3XL_LSL, Pixel3XL_LSR, END }

public class SafeRect : MonoBehaviour
{
    public static EDevice m_eDevice = EDevice.None;

    private Rect[] NSA_iPhoneX = new Rect[] {
        new Rect (0f, 102f / 2436f, 1f, 2202f / 2436f),  // Portrait
        new Rect (132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)  // Landscape
    };

    private Rect[] NSA_iPhoneXsMax = new Rect[] {
        new Rect (0f, 102f / 2688f, 1f, 2454f / 2688f),  // Portrait
        new Rect (132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)  // Landscape
    };

    private Rect[] NSA_Pixel3XL_LSL = new Rect[] {
        new Rect (0f, 0f, 1f, 2789f / 2960f),  // Portrait
        new Rect (0f, 0f, 2789f / 2960f, 1f)  // Landscape
    };

    private Rect[] NSA_Pixel3XL_LSR = new Rect[] {
            new Rect (0f, 0f, 1f, 2789f / 2960f),  // Portrait
            new Rect (171f / 2960f, 0f, 2789f / 2960f, 1f)  // Landscape
    };

    private RectTransform m_RectTransform = null;
    private Rect m_rtLastSafeArea = new Rect(0f, 0f, 0f, 0f);
    private Vector2Int m_vLastScreenSize = new Vector2Int(0, 0);
    private ScreenOrientation m_eLastOrientation = ScreenOrientation.Portrait;

    [SerializeField] bool bConformX = true;
    [SerializeField] bool bConformY = true;
    [SerializeField] bool bLogging = false;

    void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();

        Refresh();
    }

    void Update()
    {
        Refresh();

        if (Input.GetKeyUp(KeyCode.BackQuote))
        {
            m_eDevice++;
            if (m_eDevice == EDevice.END)
                m_eDevice = EDevice.None;
        }
    }

    void Refresh()
    {
        Rect rtArea = GetSafeArea();
        if (rtArea != m_rtLastSafeArea || Screen.width != m_vLastScreenSize.x || Screen.height != m_vLastScreenSize.y || Screen.orientation != m_eLastOrientation)
        {
            m_vLastScreenSize.x = Screen.width;
            m_vLastScreenSize.y = Screen.height;
            m_eLastOrientation = Screen.orientation;

            ApplySafeArea(rtArea);
        }
    }

    Rect GetSafeArea()
    {
        Rect rtSafeArea = Screen.safeArea;
        if (Application.isEditor && EDevice.None != m_eDevice)
        {
            Rect rtArea = new Rect(0f, 0f, Screen.width, Screen.height);
            switch (m_eDevice)
            {
                case EDevice.iPhoneX: rtArea = Screen.height > Screen.width ? NSA_iPhoneX[0] : NSA_iPhoneX[1]; break;
                case EDevice.iPhoneXs_Max: rtArea = Screen.height > Screen.width ? NSA_iPhoneXsMax[0] : NSA_iPhoneXsMax[1]; break;
                case EDevice.Pixel3XL_LSL: rtArea = Screen.height > Screen.width ? NSA_Pixel3XL_LSL[0] : NSA_Pixel3XL_LSL[1]; break;
                case EDevice.Pixel3XL_LSR: rtArea = Screen.height > Screen.width ? NSA_Pixel3XL_LSR[0] : NSA_Pixel3XL_LSR[1]; break;
            }

            rtSafeArea = new Rect(Screen.width * rtArea.x, Screen.height * rtArea.y, Screen.width * rtArea.width, Screen.height * rtArea.height);
        }
        return rtSafeArea;
    }

    void ApplySafeArea(Rect rt)
    {
        m_rtLastSafeArea = rt;

        // Ignore x-axis?
        if (!bConformX)
        {
            rt.x = 0;
            rt.width = Screen.width;
        }

        // Ignore y-axis?
        if (!bConformY)
        {
            rt.y = 0;
            rt.height = Screen.height;
        }

        // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
        Vector2 vAnchorMin = rt.position;
        Vector2 vAnchorMax = rt.position + rt.size;
        vAnchorMin.x /= Screen.width;
        vAnchorMin.y /= Screen.height;
        vAnchorMax.x /= Screen.width;
        vAnchorMax.y /= Screen.height;
        m_RectTransform.anchorMin = vAnchorMin;
        m_RectTransform.anchorMax = vAnchorMax;

        if (bLogging)
        {
            GameManager.Log($"New safe area applied to {name}: x={rt.x}, y={rt.y}, w={rt.width}, h={rt.height} on full extents w={Screen.width}, h={Screen.height}");
        }
    }
}
