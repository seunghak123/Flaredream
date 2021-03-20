using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void DataFileLoaderFunc(SimpleJSON.JSONNode root);

public static class ManagerController
{
    public static List<ManagerData> s_listManager = new List<ManagerData>();

    public static void AddManager(ManagerData managerData)
    {
        if (s_listManager.Contains(managerData) == true)
            return;

        s_listManager.Add(managerData);
    }


    public static bool IsInitDataComplete()
    {
        for (int i = 0; i < s_listManager.Count; ++i)
        {
            ManagerData managerData = s_listManager[i];
            if (managerData == null)
                continue;

            if (managerData.LOAD_COUNT > 0)
                return false;
        }

        return true;
    }

    public static int NonCompleateLoadCount()
    {
        int nTotalCount = 0;
        for (int i = 0; i < s_listManager.Count; ++i)
        {
            ManagerData managerData = s_listManager[i];
            if (managerData == null)
                continue;

            if (managerData.LOAD_COUNT > 0)
                ++nTotalCount;
        }
        return nTotalCount;
    }

}

public class BaseManager<ManagerType> : ManagerData where ManagerType : ManagerData
{
    static bool m_bShutDown = false;

    public static ManagerType s_Manager = null;

    public override eObjectType GetObjectType() { return eObjectType.E_OBJECT_TYPE_MANAGER; }

    protected static GameObject s_container = null;

    protected virtual eManagerResetType GetResetType() { return eManagerResetType.VOLATILE; }

    protected static bool m_isFirstMade = false;       // 최초 생성 혹은 리셋 후 최초 생성인지. 리셋 되지 않은 매니저는 false로 기록

    protected bool m_PauseUpdate = false;

    protected List<GameObject> m_DontDestroyObjects = new List<GameObject>();
    public List<GameObject> CLEAN_TARGET
    {
        get

        {
            if (m_DontDestroyObjects == null)
                m_DontDestroyObjects = new List<GameObject>();

            return m_DontDestroyObjects;
        }
    }

    public bool IsFirst
    {
        get
        {
            return m_isFirstMade;
        }
    }

    public virtual void Awake()
    {
        m_isFirstMade = ManagerManager.AddManager(typeof(ManagerType), this, GetResetType());

        if (m_isFirstMade == false)
        {
            //Debug.Log(this.ToString() + " No First");
            Destroy(this.SelfObject);
        }
        else
        {
            //Debug.Log(this.ToString() + " First");
            DontDestroyOnLoad(this);
        }
    }

    void OnApplicationQuit()
    {
        m_bShutDown = true;
    }

    public static ManagerType Instance
    {
        get
        {
            if (s_Manager == null)
            {
                if (ManagerManager.UPDATE_OK == false)
                    return null;

                s_Manager = FindObjectOfType(typeof(ManagerType)) as ManagerType;
                if (s_Manager == null)
                {
                    if (m_bShutDown == false)
                    {
                        s_container = new GameObject();
                        s_container.name = typeof(ManagerType).Name;
                        s_Manager = s_container.AddComponent(typeof(ManagerType)) as ManagerType;
                        s_Manager.Init();

                        ManagerController.AddManager(s_Manager);
                    }
                }
            }

            return s_Manager;
        }
    }

    protected override void ThrowEvent(eThrowEventType eventType, stGetCustomData stThrowData = default(stGetCustomData))
    {
        switch (eventType)
        {
            case eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_PAUSE:
                OnPauseManager();
                break;
            case eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_CLEAN:
                OnCleanManager();
                break;
            case eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_RESET:
                OnResetManager();
                break;
            case eThrowEventType.E_THROW_EVENT_TYPE_MANAGER_RELEASE:
                OnReleaseManager();
                break;
        }
    }

    protected virtual void OnPauseManager()
    {
        //Debug.Log(SelfObject.ToString() + " on Pause");
        m_PauseUpdate = true;
    }

    protected virtual void OnReleaseManager()
    {
        //Debug.Log(SelfObject.ToString() + " on Release");
        m_PauseUpdate = false;
    }

    protected virtual void OnCleanManager()
    {
        //Debug.Log(SelfObject.ToString() + " on Clean");

        for (int i = 0; i < CLEAN_TARGET.Count; ++i)
        {
            GameObject _tempObj = CLEAN_TARGET[i];
            if (_tempObj != null)
                Destroy(_tempObj);
        }
        CLEAN_TARGET.Clear();

        if (GetResetType() == eManagerResetType.VOLATILE)
            StopAllCoroutines();

        ManagerManager.CleanCompleteCall(typeof(ManagerType));
    }

    protected virtual void OnResetManager()
    {
        //Debug.Log(SelfObject.ToString() + " on Reset");
        if (GetResetType() == eManagerResetType.VOLATILE)
            ManagerManager.DeleteManager(typeof(ManagerType));
    }

    public void OnDestroy()
    {
        if (GetResetType() == eManagerResetType.VOLATILE)
            ManagerManager.DeleteManager(typeof(ManagerType));
        else if (GetResetType() == eManagerResetType.STABLE)
            if (Instance == this)
                ManagerManager.DeleteManager(typeof(ManagerType));
    }
}
