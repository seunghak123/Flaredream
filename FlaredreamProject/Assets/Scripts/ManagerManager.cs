using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
//using System.Threading;

public enum eManagerResetType
{
    VOLATILE,   // 어플리케이션 재시작 시 파괴하고 재생성
    STABLE,     // 어플리케이션 재시작 되어도 남아있는
}

public enum eDataLoadMode
{
    MultiThread,
    Coroutine,
}

public class ManagerManager : BaseManager<ManagerManager>
{

    protected override eManagerResetType GetResetType()
    {
        return eManagerResetType.STABLE;
    }

#if UNITY_IOS
	[DllImport("__Internal")]
    private extern static void GetSafeAreaImpl(out float x, out float y, out float w, out float h);
    public static eDataLoadMode DATA_LOAD_MODE = eDataLoadMode.Coroutine;
#else
    public static eDataLoadMode DATA_LOAD_MODE = eDataLoadMode.MultiThread;
#endif

    public Rect GetSafeArea()
    {
        float x, y, w, h;
#if UNITY_IOS && !UNITY_EDITOR
		GetSafeAreaImpl(out x, out y, out w, out h);
#else
        x = 0;
        y = 0;
        w = Screen.width;
        h = Screen.height;
#endif
        //return new Rect(0, 0, 1280, 720); 
        return new Rect(x, y, w, h);
    }

    Rect m_SafeRect = new Rect(0, 0, 1280, 720);
    public Rect SAFE_RECT
    {
        get
        {
            return m_SafeRect;
        }
        set
        {
            m_SafeRect = value;
        }
    }

    //public static eDataLoadMode DATA_LOAD_MODE = eDataLoadMode.Coroutine;
    public static bool UPDATE_OK = true;

    Dictionary<Type, BaseObject> m_Dict_Type = new Dictionary<Type, BaseObject>();
    List<BaseObject> m_List_Volatile = new List<BaseObject>();
    List<BaseObject> m_List_Stable = new List<BaseObject>();

    List<Type> m_List_Cleaned = new List<Type>();

    GameObject m_BlindObject = null;

    GameObject BLIND_OBJECT
    {
        get
        {
            if (m_BlindObject == null)
                m_BlindObject = GetComponentInChildren<Camera>(true).gameObject;

            return m_BlindObject;
        }
    }

    public static Dictionary<Type, BaseObject> TYPE_DICT
    {
        get
        {
            if (Instance == null)
                return null;

            if (Instance.m_Dict_Type == null)
                Instance.m_Dict_Type = new Dictionary<Type, BaseObject>();

            return Instance.m_Dict_Type;
        }
    }

    static List<BaseObject> VOLATILE_LIST
    {
        get
        {
            if (Instance.m_List_Volatile == null)
                Instance.m_List_Volatile = new List<BaseObject>();
            return Instance.m_List_Volatile;
        }
    }

    static List<BaseObject> STABLE_LIST
    {
        get
        {
            if (Instance.m_List_Stable == null)
                Instance.m_List_Stable = new List<BaseObject>();
            return Instance.m_List_Stable;
        }
    }

    public void OnEnable()
    {
#if UNITY_IOS && !UNITY_EDITOR
		Camera.onPreRender += OnPreRenderManagerManager;
#endif
    }

    public void OnDisable()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Camera.onPreRender -= OnPreRenderManagerManager;
#endif
    }

    private void OnApplicationQuit()
    {
        string log = "Havana OnApplicationQuit called";
        Debug.Log(log);
        //Fabric.Crashlytics.Crashlytics.Log(log);
    }

    public void ApplySafeArea()
    {
        m_SafeRect = GetSafeArea();

    }

    public void InitSafeArea()
    {
        //m_SafeRect = new Rect(0, 0, 1280, 720);
        m_SafeRect = new Rect(0, 0, Screen.width, Screen.height);
    }


    private void OnPreRenderManagerManager(Camera cam)
    {
#if UNITY_IOS && !UNITY_EDITOR
		if (cam == null)
			return;

		if (cam.pixelRect != m_SafeRect)
			cam.pixelRect = m_SafeRect;
#endif
    }

    public static bool AddManager(Type _Managertype, BaseObject _manager, eManagerResetType _type = eManagerResetType.VOLATILE)
    {
        if (TYPE_DICT.ContainsKey(_Managertype))
            return false;

        TYPE_DICT.Add(_Managertype, _manager);

        if (_type == eManagerResetType.VOLATILE)
            VOLATILE_LIST.Add(_manager);
        else if (_type == eManagerResetType.STABLE)
            STABLE_LIST.Add(_manager);

        return true;
    }

    public static bool DeleteManager(Type _Managertype)
    {
        if (Instance == null)
            return false;

        if (TYPE_DICT.ContainsKey(_Managertype) == false)
            return false;

        BaseObject _curObject = TYPE_DICT[_Managertype];

        TYPE_DICT.Remove(_Managertype);

        if (VOLATILE_LIST.Contains(_curObject))
        {
            VOLATILE_LIST.Remove(_curObject);
            return true;
        }

        if (STABLE_LIST.Contains(_curObject))
        {
            STABLE_LIST.Remove(_curObject);
            return true;
        }

        Debug.LogError("whole list has manager but no other has");
        return false;
    }

    void _PauseManagers()
    {
        foreach (KeyValuePair<Type, BaseObject> kvPair in TYPE_DICT)
        {
            BaseObject.ThrowObject(kvPair.Value, eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_PAUSE);
        }
    }

    public static void CleanCompleteCall(Type _managertype)
    {
        Instance.m_List_Cleaned.Add(_managertype);
    }

    IEnumerator CleanManagers()
    {

        Instance.m_List_Cleaned = new List<Type>();

        foreach (KeyValuePair<Type, BaseObject> kvPair in TYPE_DICT)
        {
            BaseObject.ThrowObject(kvPair.Value, eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_CLEAN);
        }

        while (true)
        {
            if (Instance.m_List_Cleaned.Count == TYPE_DICT.Count)
                break;

            foreach (var kvPair in TYPE_DICT)
            {
                if (Instance.m_List_Cleaned.Contains(kvPair.Key) == false)
                    Debug.LogError(kvPair.Key);
            }

            yield return null;
        }

        Instance.m_List_Cleaned.Clear();
    }

    static void ResetManagers()
    {
        foreach (BaseObject _manager in VOLATILE_LIST)
        {
            if (_manager == null)
                continue;

            TYPE_DICT.Remove(_manager.GetType());
            //VOLATILE_LIST.Remove(_manager);
            BaseObject.ThrowObject(_manager, eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_RESET);

            GameObject _container = _manager.SelfObject;
            GameObject.Destroy(_manager);

            if (_container != null)
                GameObject.Destroy(_container);
        }

        VOLATILE_LIST.Clear();

        foreach (BaseObject _manager in STABLE_LIST)
        {
            BaseObject.ThrowObject(_manager, eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_RESET);
        }
    }

    IEnumerator _DeleteDestroyable()
    {
        AsyncOperation changeEmpty = SceneManager.LoadSceneAsync("EmptyScene");  //SceneManager.LoadSceneAsync(1);    // scene이 바뀌면서 없어질 오브젝트 미리 삭제(emptyScene 호출)

        if (changeEmpty == null)
        {
            Debug.LogError("No Scene Index : 1");
            changeEmpty = SceneManager.LoadSceneAsync("BaseScene");

            if (changeEmpty == null)
            {
                Debug.LogError("No Scene Index : 0");
                yield break;
            }
        }

        while (changeEmpty.isDone == false)
        {
            yield return null;
        }
    }

    IEnumerator _AppRestartOperation()
    {
        Debug.Log("_AppRestartOperation || UPDATE_OK = " + UPDATE_OK);

        if (UPDATE_OK == false) // 이미 해당 작업이 진행중이면 또다시 하지 않도록 한다
            yield break;

        UPDATE_OK = false;


        //yield return new WaitForEndOfFrame();


        Debug.Log("reset op 01 : Delete Destroyable objects by changing scene ...");

        yield return StartCoroutine(_DeleteDestroyable());  // DontDestroyOnLoad 가 아닌 대상 삭제 (빈 씬으로 전환)

        yield return new WaitForEndOfFrame();  // 파괴되는 오브젝트들의 OnDestroy , Update 등 대기


        Debug.Log("reset op 02 : Let Managers Clean ...");

        yield return StartCoroutine(CleanManagers());    // 각 매니저별 정해진 OnCleanManager 호출 : 제거 전 정리작업 실행

        yield return new WaitForEndOfFrame();  // clean 후 파괴되는 오브젝트 대기


        Debug.Log("reset op 03 : Deleting VOLATILE managers ...");

        ResetManagers();    // volatile 매니저만 OnResetManager 호출 및 파괴, stable : ResetManager 호출

        yield return new WaitForEndOfFrame();


        Debug.Log("reset op 04 : Load new BaseScene ...");

        UPDATE_OK = true;   // BaseScene 호출


        SceneManager.LoadScene("BaseScene");
        _ReleaseManagers();

        Debug.Log("reset op Finished : Disable Blind Object");  // 가림막 제거
        Blind(false);

    }

    void _ReleaseManagers()
    {
        foreach (KeyValuePair<Type, BaseObject> kvPair in TYPE_DICT)
        {
            BaseObject.ThrowObject(kvPair.Value, eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_RELEASE);
        }
    }

    public static void ApplicationReset()
    {
        Debug.Log("ApplicationReset ");

        Instance.StartCoroutine(Instance._AppRestartOperation());
    }

    public static void resetThreadData()
    {
        if (DATA_LOAD_MODE == eDataLoadMode.MultiThread)
        {
            if (m_threadLock == null)
                m_threadLock = new object();
            if (m_runningLock == null)
                m_runningLock = new object();
            m_bRunningThread = false;
            if (m_listThreadDataHigh == null)
                m_listThreadDataHigh = new List<DataLoadThreadData>();
            if (m_listThreadDataNormal == null)
                m_listThreadDataNormal = new List<DataLoadThreadData>();
            if (m_listThreadDataLow == null)
                m_listThreadDataLow = new List<DataLoadThreadData>();

            startloadCount = 0;
            endloadCount = 0;
        }


    }
    public static void Blind(bool enabled)
    {
        if (Instance.BLIND_OBJECT != null)
            Instance.BLIND_OBJECT.SetActive(enabled);
    }

    public static Action onPauseMethod = null;

    public static void Pause()
    {
        Debug.Log("reset op 00 : Pause Managers ...");

        Instance._PauseManagers();   // 매니저 Update 중지 : 각 매니저 Update 과정에서 삭제되는 오브젝트들을 호출하지 않도록
    }

    public static void Release()
    {
        Debug.Log("reset op Canceled : Release Managers ...");
        Instance._ReleaseManagers();

    }


    public static bool IsLoadingProcessEnd()
    {
        if (DATA_LOAD_MODE == eDataLoadMode.Coroutine)
        {
            if (CoroutineRunningCount > 0)
                return false;
            else if (CoroutineRunningCount < 0)
            {
                Debug.LogError("Invalid coroutine running count : " + CoroutineRunningCount.ToString());
                return false;
            }
            else
                return true;
        }
        else if (DATA_LOAD_MODE == eDataLoadMode.MultiThread)
        {
            if (RunningDataCount == 0 && ThreadDataCount == 0)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    static int CoroutineRunningCount = 0;
    public static int GetCoroutineRunningCount()
    {
        return CoroutineRunningCount;
    }

    public static int startloadCount = 0;
    public static int endloadCount = 0;
    public static void LoadDataFile(eLoadDataPriority priority, string strFileName, DataFileLoaderFunc completeFunc, Action startFunc = null, Action endFunc = null)
    {
        AssetBundleLoadAssetOperation operation = AssetBundleManager.LoadAssetAsync("setting_data", strFileName, typeof(TextAsset));
        if (operation == null)
        {
            if (completeFunc != null)
                completeFunc(null);
            return;
        }

        if (DATA_LOAD_MODE == eDataLoadMode.Coroutine)
        {
            ++CoroutineRunningCount;
            //Debug.LogError(" ++ Coroutine running Added : " + CoroutineRunningCount.ToString());
        }
        else
            startloadCount++;

        if (startFunc != null)
        {
            startFunc();
        }
        Instance.StartCoroutine(LoadDataFile(priority, operation, completeFunc, endFunc));
    }


    static List<DataLoadThreadData> m_listThreadDataHigh = new List<DataLoadThreadData>();
    static List<DataLoadThreadData> m_listThreadDataNormal = new List<DataLoadThreadData>();
    static List<DataLoadThreadData> m_listThreadDataLow = new List<DataLoadThreadData>();

    static List<DataLoadThreadData> m_listRunningData = new List<DataLoadThreadData>();

    static bool m_bRunningThread = false;

    static object m_threadLock = new object();
    static object m_runningLock = new object();

    static public void ClearThreadData()
    {
        if (m_listThreadDataHigh != null)
        {
            m_listThreadDataHigh.Clear();
            m_listThreadDataHigh = null;
        }

        if (m_listThreadDataNormal != null)
        {
            m_listThreadDataNormal.Clear();
            m_listThreadDataNormal = null;
        }

        if (m_listThreadDataLow != null)
        {
            m_listThreadDataLow.Clear();
            m_listThreadDataLow = null;
        }

        RunningDataCount = 0;
        ThreadDataCount = 0;
        CoroutineRunningCount = 0;

        m_threadLock = null;
        m_runningLock = null;
        m_bRunningThread = false;

    }

    static int ThreadDataCount = 0;
    static public int GetThreadDataCount()
    {
        //int nCount = 0;

        //if( m_threadLock != null )
        //{
        //    lock (m_threadLock)
        //    {
        //        nCount += m_listThreadDataHigh.Count;
        //        nCount += m_listThreadDataNormal.Count;
        //        nCount += m_listThreadDataLow.Count;
        //    }
        //}


        //return nCount;


        int nCount = 0;

        if (m_threadLock != null)
        {
            lock (m_threadLock)
            {
                nCount = ThreadDataCount;
            }
        }

        return nCount;
    }

    static int RunningDataCount = 0;
    static public int GetRunningDataCount()
    {
        //int nCount = 0;

        //if( m_runningLock != null )
        //{
        //    lock (m_runningLock)
        //    {
        //        if (m_listRunningData != null)
        //            nCount = m_listRunningData.Count;
        //    }
        //}


        //return nCount;

        int nCount = 0;

        if (m_runningLock != null)
        {
            lock (m_runningLock)
            {
                nCount = RunningDataCount;
            }
        }

        return nCount;
    }


    static void ParseDataImmediately(eLoadDataPriority priority, DataLoadThreadData threadData)
    {
        ParseThreadData(threadData);
    }

    static void PutDataIntoThreadList(eLoadDataPriority priority, DataLoadThreadData threadData)
    {
        if (m_threadLock != null)
        {
            lock (m_threadLock)
            {
                switch (priority)
                {
                    case eLoadDataPriority.HIGH:
                        {
                            m_listThreadDataHigh.Add(threadData);
                            ++ThreadDataCount;
                        }
                        break;

                    case eLoadDataPriority.NORMAL:
                        {
                            m_listThreadDataNormal.Add(threadData);
                            ++ThreadDataCount;
                        }
                        break;

                    case eLoadDataPriority.LOW:
                        {
                            m_listThreadDataLow.Add(threadData);
                            ++ThreadDataCount;
                        }
                        break;
                }
            }
        }
    }

    static IEnumerator LoadDataFile(eLoadDataPriority priority, AssetBundleLoadAssetOperation operation, DataFileLoaderFunc completeFunc, Action endFunc = null)
    {
        yield return operation;
        TextAsset textAsset = operation.GetAsset<TextAsset>();

        //long lStartTime = DateTime.Now.Ticks;
        string strText = string.Empty;
        string strName = string.Empty;

        if (textAsset != null)
        {
            strText = textAsset.text;
            strName = textAsset.name;
        }

        AssetBundleManager.UnloadAssetBundle("setting_data");

        DataLoadThreadData threadData = new DataLoadThreadData();

        threadData.m_name = strName;
        threadData.m_Text = strText;
        threadData.m_CompleteFunc = completeFunc;
        threadData.m_EndFunc = endFunc;

        //thread.Priority = System.Threading.ThreadPriority.Lowest;

        if (DATA_LOAD_MODE == eDataLoadMode.MultiThread)
        {
            PutDataIntoThreadList(priority, threadData);
            endloadCount++;
        }
        else if (DATA_LOAD_MODE == eDataLoadMode.Coroutine)
            ParseDataImmediately(priority, threadData);

        yield break;
    }

    public static void StartLoadingThread(int nThreadCount)
    {
        //ClearThreadData();

        if (DATA_LOAD_MODE == eDataLoadMode.MultiThread)
        {
            //m_threadLock = new object();
            //m_runningLock = new object();

            m_bRunningThread = true;

            //m_listThreadDataHigh = new List<DataLoadThreadData>();
            //m_listThreadDataNormal = new List<DataLoadThreadData>();
            //m_listThreadDataLow = new List<DataLoadThreadData>();

            if (nThreadCount > 0)
            {
                for (int i = 0; i < nThreadCount; ++i)
                {
                    try
                    {
                        System.Threading.Thread thread = new System.Threading.Thread(RequestComplete);
                        thread.Priority = System.Threading.ThreadPriority.BelowNormal;
                        thread.Start();
                    }
                    catch (System.Threading.ThreadStateException e)
                    {
                        Debug.LogError("ThreadStateException : " + e.Message);
                    }
                    catch (OutOfMemoryException e)
                    {
                        Debug.LogError("OutOfMemoryException : " + e.Message);
                    }
                    catch (InvalidOperationException e)
                    {
                        Debug.LogError("InvalidOperationException : " + e.Message);
                    }

                }
            }
        }
    }

    public static void debuglog()
    {
        if (m_listRunningData != null)
        {
            Debug.LogError("m_listRunningData count : " + m_listRunningData.Count);
            for (int i = 0; i < m_listRunningData.Count; ++i)
            {
                if (m_listRunningData[i] != null)
                {
                    Debug.LogError("running data " + m_listRunningData[i].m_name);

                }
            }
        }
        if (m_listThreadDataHigh != null)
        {
            Debug.LogError("m_listThreadDataHigh count : " + m_listThreadDataHigh.Count);
            for (int i = 0; i < m_listThreadDataHigh.Count; ++i)
            {
                if (m_listThreadDataHigh[i] != null)
                {
                    Debug.LogError("remain : " + m_listThreadDataHigh[i].m_name);
                }
            }
        }
        if (m_listThreadDataNormal != null)
        {
            Debug.LogError("m_listThreadDataNormal count : " + m_listThreadDataNormal.Count);
            for (int i = 0; i < m_listThreadDataNormal.Count; ++i)
            {
                if (m_listThreadDataNormal[i] != null)
                {
                    Debug.LogError("remain : " + m_listThreadDataNormal[i].m_name);
                }
            }
        }
        if (m_listThreadDataLow != null)
        {
            Debug.LogError("m_listThreadDataLow count : " + m_listThreadDataLow.Count);
            for (int i = 0; i < m_listThreadDataLow.Count; ++i)
            {
                if (m_listThreadDataLow[i] != null)
                {
                    Debug.LogError("remain : " + m_listThreadDataLow[i].m_name);
                }
            }
        }
    }

    static void _GetWaitData()
    {

        //DataLoadThreadData threadData = null;
        if (m_threadLock == null)
        {
            return;
        }


        lock (m_threadLock)
        {
            //  Debug.LogError("m_threadLock : " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < (int)eLoadDataPriority.NONE; ++i)
            {
                bool bbreak = false;
                switch ((eLoadDataPriority)i)
                {
                    case eLoadDataPriority.HIGH:
                        {
                            if (m_listThreadDataHigh == null || m_listThreadDataHigh.Count == 0)
                                continue;
                            ParseThreadData(m_listThreadDataHigh[0]);
                            m_listThreadDataHigh[0].Dispose();
                            m_listThreadDataHigh.RemoveAt(0);
                            --ThreadDataCount;
                            bbreak = true;
                        }
                        break;

                    case eLoadDataPriority.NORMAL:
                        {
                            if (m_listThreadDataNormal == null || m_listThreadDataNormal.Count == 0)
                                continue;
                            ParseThreadData(m_listThreadDataNormal[0]);

                            m_listThreadDataNormal[0].Dispose();
                            m_listThreadDataNormal.RemoveAt(0);
                            --ThreadDataCount;
                            bbreak = true;
                        }
                        break;
                    case eLoadDataPriority.LOW:
                        {
                            if (m_listThreadDataLow == null || m_listThreadDataLow.Count == 0)
                                continue;
                            ParseThreadData(m_listThreadDataLow[0]);
                            m_listThreadDataLow[0].Dispose();
                            m_listThreadDataLow.RemoveAt(0);
                            --ThreadDataCount;
                            bbreak = true;
                        }
                        break;

                }


                if (bbreak == true)
                {
                    break;
                }
            }
        }

    }

    static void ParseThreadData(DataLoadThreadData threadData)
    {
        if (threadData != null)
        {
            //UnityEngine.Debug.Log("@@@@@ START ---- " + threadData.m_name);
            SimpleJSON.JSONNode rootNode = null;

            //long lStartTime = DateTime.Now.Ticks;

            if (string.IsNullOrEmpty(threadData.m_Text) == false)
                rootNode = SimpleJSON.JSON.Parse(threadData.m_Text);

            try
            {
                if (threadData.m_CompleteFunc != null)
                    threadData.m_CompleteFunc(rootNode);

                if (threadData.m_EndFunc != null)
                    threadData.m_EndFunc();
            }

            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            if (DATA_LOAD_MODE == eDataLoadMode.Coroutine)
            {
                --CoroutineRunningCount;
                //Debug.LogError(" -- Coroutine running Removed : " + CoroutineRunningCount.ToString());
            }
        }
        else
        {
            //Fabric.Crashlytics.Crashlytics.Log("threadData is null");
        }
    }

    static void RequestComplete()
    {
        while (m_bRunningThread)
        {
            _GetWaitData();
            System.Threading.Thread.Sleep(10);
        }
    }
}

public class DataLoadThreadData : IDisposable
{
    public string m_Text = string.Empty;
    public string m_name = string.Empty;
    public DataFileLoaderFunc m_CompleteFunc = null;
    public Action m_EndFunc = null;

    public void Dispose()
    {
        m_Text = string.Empty;
        m_name = string.Empty;
        m_CompleteFunc = null;
        m_EndFunc = null;
    }
}
