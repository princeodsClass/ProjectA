using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : SingletonMono<MenuManager>
{
    private Dictionary<int, UIDialog> m_dicUIPage = new Dictionary<int, UIDialog>();
    private Dictionary<int, UIDialog> m_dicUIPopup = new Dictionary<int, UIDialog>();
    private Dictionary<int, GameObject> m_dicUICompo = new Dictionary<int, GameObject>();

    private Transform m_tUIRoot = null;
    private Transform m_tRootPage = null;
    private Transform m_tRootAbove = null;
    private Transform m_tRootPopup = null;

    private GameResourceManager m_ResMgr = null;
    private Transform m_tfModelCam = null;
    private UIRootBase m_UIRoot = null;
    private TooltipMenu m_CurTooltip = null;

    private EUIPage m_eCurPage = EUIPage.End;
    // private EUIPopup m_eCurPopup = EUIPopup.End;

    public List<EUIPopup> m_liCurPopupType = new List<EUIPopup>();
    public List<UIDialog> m_liCurPopup = new List<UIDialog>();

    public bool IsLoadingComplete { get; private set; }
    public bool IsErrorWithLogout { get; private set; }

    public ESceneType CurScene { get; set; }

    public bool IsMapFirstMessage = false;
	public bool IsMoveToInventory { get; set; } = false;

	public bool IsMoveToAbyss { get; set; } = false;
	public bool IsMoveToMissionDefence { get; set; } = false;

	public bool IsClearMissionAdventure { get; set; } = false;
	public bool IsMoveToMissionAdventure { get; set; } = false;

    private void Awake()
    {
        if (null == m_ResMgr) m_ResMgr = GameResourceManager.Singleton;

        CurScene = ESceneType.Title;
        IsLoadingComplete = true;
        IsErrorWithLogout = false;
    }

    public void Clear()
    {
        m_eCurPage = EUIPage.End;
        // m_eCurPopup = EUIPopup.End;

        m_liCurPopupType.Clear();
        m_liCurPopup.Clear();

        IsErrorWithLogout = false;

        m_dicUIPage.Clear();
        m_dicUIPopup.Clear();
        m_dicUICompo.Clear();

        if (null != m_CurTooltip)
        {
            m_CurTooltip.Close();
            m_CurTooltip = null;
        }

        if (null != m_UIRoot)
            m_UIRoot.Clear();
    }

    public void Initialize()
    {
        Clear();

        GameObject goRoot = GameObject.Find(ComType.UI_ROOT_NAME);
        if (null == goRoot)
        {
            string strPath = $"{ComType.UI_PATH}/{ComType.UI_ROOT_NAME}";
            GameObject goPref = Resources.Load(strPath) as GameObject;
            if (null != goPref)
                goRoot = Instantiate(goPref) as GameObject;
        }

        if (null != goRoot)
        {
            m_tUIRoot = goRoot.transform;
            m_tRootPage = m_tUIRoot.Find(ComType.UI_ROOT_PAGE);
            m_tRootAbove = m_tUIRoot.Find(ComType.UI_ROOT_ABOVE);
            m_tRootPopup = m_tUIRoot.Find(ComType.UI_ROOT_POPUP);
        }
    }

    public void SetUIRoot(UIRootBase uiRoot)
    {
        m_UIRoot = uiRoot;
    }

    public void ShowBackground(bool bShow)
    {
        m_UIRoot.SetActiveBackground(bShow);
    }

    public void ShowPopupDimmed(bool bShow)
    {
        m_UIRoot.SetPopupDimmed(bShow);
    }

    public void SetCameraOption()
    {
        if (null != m_UIRoot) m_UIRoot.SetCameraOption();
    }

    public void SceneNext(ESceneType eScene)
    {
        if (!IsLoadingComplete) return;

        IsLoadingComplete = false;
        GameAudioManager.ChangeAudioMixSnapShot(eScene.ToString());

        SceneManager.LoadScene(eScene.ToString());
        CurScene = eScene;
    }

    public void SceneEnd()
    {
        IsLoadingComplete = true;
    }

    public void SetLogoutFlag(bool bSet)
    {
        IsErrorWithLogout = bSet;
    }

    public Transform GetUIRoot() { return m_tUIRoot; }
    public Canvas GetUICanvas() { return m_UIRoot.GetCanvas(); }
    public Vector2 GetUIRootSize() { return m_UIRoot.GetRectSize(); }
    public UIRootBase GetCurUIBase() { return m_UIRoot; }

    private UIDialog LoadPage(EUIPage ePage)
    {
        GameObject goPage = m_ResMgr.CreateObject(EResourceType.UIPage, ePage.ToString(), m_tRootPage);
        if (null == goPage) return null;

        int nKey = (int)ePage;
        UIDialog uiDlg = goPage.GetComponent<UIDialog>();
        if (m_dicUIPage.ContainsKey(nKey))
            m_dicUIPage.Remove(nKey);

        m_dicUIPage.Add(nKey, uiDlg);
        return uiDlg;
    }

    private UIDialog LoadAbove(EUIPage ePage)
    {
        GameObject goPage = m_ResMgr.CreateObject(EResourceType.UIPage, ePage.ToString(), m_tRootAbove);
        if (null == goPage) return null;

        int nKey = (int)ePage;
        UIDialog uiDlg = goPage.GetComponent<UIDialog>();
        if (m_dicUIPage.ContainsKey(nKey))
            m_dicUIPage.Remove(nKey);

        m_dicUIPage.Add(nKey, uiDlg);
        return uiDlg;
    }

    private UIDialog LoadPopup(EUIPopup ePopup)
    {
        GameObject goPopup = m_ResMgr.CreateObject(EResourceType.UIPopup, ePopup.ToString(), m_tRootPopup);
        if (null == goPopup) return null;

        int nKey = (int)ePopup;
        UIDialog uiDlg = goPopup.GetComponent<UIDialog>();
        if (m_dicUIPopup.ContainsKey(nKey))
            m_dicUIPopup.Remove(nKey);

        m_dicUIPopup.Add(nKey, uiDlg);
        return uiDlg;
    }

    public GameObject LoadComponent(Transform tRoot, EUIComponent eCompo)
    {
        GameObject goCompo = m_ResMgr.CreateObject(EResourceType.UIComponent, eCompo.ToString(), tRoot);
        return goCompo;
    }

    public T LoadComponent<T>(Transform tRoot, EUIComponent eCompo)
    {
        GameObject goCompo = LoadComponent(tRoot, eCompo);
        if (null != goCompo)
            return goCompo.GetComponent<T>();
        return default;
    }

    public void OpenPage(EUIPage ePage)
    {
        if (ePage != m_eCurPage) HidePage(m_eCurPage);
        m_eCurPage = ePage;

        UIDialog uiDlg = GetPage(ePage);
        if (null != uiDlg)
            uiDlg.Open();
    }

    public T OpenPage<T>(EUIPage ePage) where T : UIDialog
    {
        if (ePage != m_eCurPage) HidePage(m_eCurPage);
        m_eCurPage = ePage;

        UIDialog uiDlg = GetPage(ePage);
        if (null != uiDlg)
            uiDlg.Open();

        return uiDlg as T;
    }

    public T GetPage<T>(EUIPage ePage) where T : UIDialog
    {
        if (m_dicUIPage.TryGetValue((int)ePage, out UIDialog uiDlg))
            return uiDlg as T;
        return LoadPage(ePage) as T;
    }

    public UIDialog GetPage(EUIPage ePage)
    {
        if (m_dicUIPage.TryGetValue((int)ePage, out UIDialog uiDlg))
            return uiDlg;
        return LoadPage(ePage);
    }

    public UIDialog GetAbove(EUIPage ePage)
    {
        if (m_dicUIPage.TryGetValue((int)ePage, out UIDialog uiDlg))
            return uiDlg;
        return LoadAbove(ePage);
    }

    public T OpenAbove<T>(EUIPage ePage) where T : UIDialog
    {
        int nPage = (int)ePage;
        if (!m_dicUIPage.ContainsKey(nPage))
            LoadAbove(ePage);

        m_dicUIPage[nPage].Open();
        return m_dicUIPage[nPage] as T;
    }

    public T OpenPopup<T>(EUIPopup ePopup, bool overlap = false) where T : UIDialog
    {
        //if (ePopup != m_eCurPopup && !overlap)
        if (!overlap)
        {
            // HidePopup(m_eCurPopup);
            ComUtil.DestroyChildren(m_tRootPopup);

            m_liCurPopupType.Clear();
            m_liCurPopup.Clear();
        }

        UIDialog uiDlg = GetPopup(ePopup);

        if (null != uiDlg)
        {
            ComUtil.SetParent(m_tRootPopup, uiDlg.transform);
            // m_eCurPopup = ePopup;
            if ( !m_liCurPopupType.Contains(ePopup) )
            {
                m_liCurPopupType.Add(ePopup);
                m_liCurPopup.Add(uiDlg);
            }

            uiDlg.Open();
        }

        return uiDlg as T;
    }

    public void ClosePopup(UIDialog popup)
    {
        if ( m_liCurPopup.Contains(popup) )
        {
            int idx = m_liCurPopup.IndexOf(popup);

            m_liCurPopupType.RemoveAt(idx);
            m_liCurPopup.RemoveAt(idx);
        }
    }

    public T GetPopup<T>(EUIPopup ePopup) where T : UIDialog
    {
        if (m_dicUIPopup.TryGetValue((int)ePopup, out UIDialog uiDlg))
            return uiDlg as T;
        return LoadPopup(ePopup) as T;
    }

    public UIDialog GetPopup(EUIPopup ePopup)
    {
        if (m_dicUIPopup.TryGetValue((int)ePopup, out UIDialog uiDlg))
            return uiDlg;
        return LoadPopup(ePopup);
    }

    public void ShowUI(bool bShow)
    {
        m_tRootPage.gameObject.SetActive(bShow);
        m_tRootAbove.gameObject.SetActive(bShow);
        m_tRootPopup.gameObject.SetActive(bShow);
    }

    public void ShowUI(bool bPage, bool bAbove, bool bPopup, bool bShow)
    {
        if (bPage) m_tRootPage.gameObject.SetActive(bShow);
        if (bAbove) m_tRootAbove.gameObject.SetActive(bShow);
        if (bPopup) m_tRootPopup.gameObject.SetActive(bShow);
    }

    public void HidePage(EUIPage ePage)
    {
        int nKey = (int)ePage;
        if (m_dicUIPage.ContainsKey(nKey))
            m_dicUIPage[nKey].Close();
    }

    public void HidePopup(EUIPopup ePopup)
    {
        int nKey = (int)ePopup;
        if (m_dicUIPopup.ContainsKey(nKey))
            m_dicUIPopup[nKey].MuteClose();
    }

    public Transform GetUIModelCamera()
    {
        return m_tfModelCam;
    }

    public void SetModelCamera(Transform tfCam)
    {
        m_tfModelCam = tfCam;
    }

    public void OpenTooltipItem(ItemBase cItem, Vector2 vPos)
    {
        TooltipItem tooltip = LoadComponent<TooltipItem>(m_tUIRoot, EUIComponent.TooltipItem);
        tooltip.Initialize(cItem);
        tooltip.SetPos(vPos);

        m_CurTooltip = tooltip;
    }

    public void OpenTooltipData(TooltipItemData cItem, Vector2 vPos)
    {
        TooltipItem tooltip = LoadComponent<TooltipItem>(m_tUIRoot, EUIComponent.TooltipItem);
        tooltip.Initialize(cItem);
        tooltip.SetPos(vPos);

        m_CurTooltip = tooltip;
    }

    public void OpenTooltipMessage(string strMsg, Vector2 vPos)
    {
        TooltipMessage tooltip = LoadComponent<TooltipMessage>(m_tUIRoot, EUIComponent.TooltipMessage);
        tooltip.Initialize(strMsg);

        m_CurTooltip = tooltip;
    }

    public void OpenTooltipToast(string strMsg)
    {
        TooltipMessage tooltip = LoadComponent<TooltipMessage>(m_tUIRoot, EUIComponent.TooltipToast);
        tooltip.Initialize(strMsg);

        m_CurTooltip = tooltip;
    }

    public void ShowToastMessage(string text, RectTransform parentTransform )
    {
        OpenTooltipMessage(text, Vector2.zero);
        //ToastMessage tMsg = LoadComponent<ToastMessage>(parentTransform, EUIComponent.ToastMessage);
        //if (tMsg != null) tMsg.Initialize(text);
    }

    public void CloseTooltip()
    {
        if (null != m_CurTooltip)
            m_CurTooltip.Close();

        m_CurTooltip = null;
    }

    public bool IsCheckEscape()
    {
        if (null != m_CurTooltip)
        {
            CloseTooltip();
            return false;
        }

        for (int i = 0; i < (int)EUIPopup.End; ++i)
        {
            if (m_dicUIPopup.ContainsKey(i))
            {
                if (m_dicUIPopup[i].IsActivate)
                {
                    m_dicUIPopup[i].Escape();
                    return false;
                }
            }
        }

        if (m_dicUIPage.ContainsKey((int)m_eCurPage))
        {
            m_dicUIPage[(int)m_eCurPage].EscapePage();
            return false;
        }
        return true;
    }

    public bool IsUIPageShow(EUIPage page)
    {
        return m_dicUIPage.ContainsKey((int)page);
    }
}
