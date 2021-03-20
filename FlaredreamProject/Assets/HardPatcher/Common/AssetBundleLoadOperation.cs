using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AssetBundleLoadOperation : IEnumerator
{
	public object Current
	{
		get
		{
			return null;
		}
	}
	public bool MoveNext()
	{
		return !IsDone();
	}
	
	public void Reset()
	{
	}
	
	abstract public bool Update ();
	
	abstract public bool IsDone ();
}

public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
{	
	public AssetBundleLoadLevelSimulationOperation ()
	{
	}
	
	public override bool Update ()
	{
		return false;
	}
	
	public override bool IsDone ()
	{		
		return true;
	}
}

public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_LevelName;
	protected bool 						m_IsAdditive;
	protected string 				m_DownloadingError;
	protected AsyncOperation		m_Request;

	public AssetBundleLoadLevelOperation (string assetbundleName, string levelName, bool isAdditive)
	{
		m_AssetBundleName = assetbundleName;
		m_LevelName = levelName;
		m_IsAdditive = isAdditive;
	}

	public override bool Update ()
	{
		if (m_Request != null)
			return false;
		
		LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null)
		{
            if (m_IsAdditive)
                m_Request = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_LevelName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            else
                m_Request = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(m_LevelName);


            return false;
		}
		else
			return true;
	}
	
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			Debug.LogError(m_DownloadingError);
			return true;
		}
		
		return m_Request != null && m_Request.isDone;
	}

    public float GetProgress()
    {
        float fValue = 0;
        if (m_Request != null)
            fValue = m_Request.progress;

        return fValue;
    }

}

public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
{
	public abstract T GetAsset<T>() where T : UnityEngine.Object;
    public virtual T GetAssetListData<T>(string assetName) where T : UnityEngine.Object
    {
        return null;
    }

}

public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
{
	Object							m_SimulatedObject;
	
	public AssetBundleLoadAssetOperationSimulation (Object simulatedObject)
	{
		m_SimulatedObject = simulatedObject;
	}
	
	public override T GetAsset<T>()
	{
		return m_SimulatedObject as T;
	}
	
	public override bool Update ()
	{
		return false;
	}
	
	public override bool IsDone ()
	{
		return true;
	}
}


public class AssetBundleLoadAssetListOperationSimulation : AssetBundleLoadAssetOperation
{
    List<Object> m_listSimulatedObject = new List<Object>();

    public AssetBundleLoadAssetListOperationSimulation()
    {
    }

    public void AddSimulatedobject( Object simulatedObject)
    {
        m_listSimulatedObject.Add(simulatedObject);
    }

    public override T GetAsset<T>()
    {
        return null;
    }

    public override T GetAssetListData<T>(string assetName)
    {
        for( int i = 0; i < m_listSimulatedObject.Count; ++i )
        {
            if( m_listSimulatedObject[i].name == assetName )
            {
                return m_listSimulatedObject[i] as T;
            }
        }

        return null;
    }


    public override bool Update()
    {
        return false;
    }

    public override bool IsDone()
    {
        return true;
    }
}



public class AssetBundleLoadAssetListOperationFull : AssetBundleLoadAssetOperation
{

    public string m_AssetBundleName;

    public struct stAssetRequestInfo
    {
        public string m_AssetName;
        public AssetBundleRequest m_Request;
    }

    public string m_DownloadingError;
    protected List<stAssetRequestInfo> m_listAsset;
    protected System.Type m_Type;

    bool m_bRequest = false;


    public AssetBundleLoadAssetListOperationFull(string assetBundleName, List<string> listAssetName, System.Type type)
    {
        m_AssetBundleName = assetBundleName;

        m_listAsset = new List<stAssetRequestInfo>();

        for( int i = 0; i < listAssetName.Count; ++i )
        {
            stAssetRequestInfo requestInfo = new stAssetRequestInfo();
            requestInfo.m_AssetName = listAssetName[i];
            m_listAsset.Add(requestInfo);
        }


        m_Type = type;
    }

    public override T GetAsset<T>()
    {
         return null;
    }

    public override T GetAssetListData<T>(string strAssetName)
    {

        AssetBundleRequest request = null;
        for( int i = 0; i < m_listAsset.Count; ++i )
        {
            if (m_listAsset[i].m_AssetName != strAssetName)
                continue;

            request = m_listAsset[i].m_Request;
        }

        if (request == null || request.isDone == false)
            return null;

        return request.asset as T;
    }



    // Returns true if more Update calls are required.
    public override bool Update()
    {
        if (m_bRequest == true)
            return false;

        LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
        if (bundle != null)
        {
            if (m_listAsset != null)
            {
                for (int i = 0; i < m_listAsset.Count; ++i)
                {
                    stAssetRequestInfo requestInfo = m_listAsset[i];
                    if (bundle.m_AssetBundle != null)
                        requestInfo.m_Request = bundle.m_AssetBundle.LoadAssetAsync(requestInfo.m_AssetName, m_Type);
                    //else
                    //    Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetListOperationFull.Update : excetpion 00, assetbundle name is " + m_AssetBundleName);

                    m_listAsset[i] = requestInfo;
                }
            }
            //else
            //{
            //    Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetListOperationFull.Update : exception 01, assetbundle name is " + m_AssetBundleName);
            //}

            m_bRequest = true;
            return false;
        }
        else
        {
            return true;
        }
    }

    public override bool IsDone()
    {
        // Return if meeting downloading error.
        // m_DownloadingError might come from the dependency downloading.
        if (m_bRequest == false && m_DownloadingError != null)
        {
            Debug.Log(m_DownloadingError);
            return true;
        }

        for( int i = 0; i < m_listAsset.Count; ++i )
        {
            stAssetRequestInfo requestInfo = m_listAsset[i];
            if (requestInfo.m_Request == null || requestInfo.m_Request.isDone == false)
                return false;
        }

        return true;
    }
}


public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
{
	protected string 				m_AssetBundleName;
	protected string 				m_AssetName;
	protected string 				m_DownloadingError;
	protected System.Type 			m_Type;
	protected AssetBundleRequest	m_Request = null;

	public AssetBundleLoadAssetOperationFull (string bundleName, string assetName, System.Type type)
	{
		m_AssetBundleName = bundleName;
		m_AssetName = assetName;
		m_Type = type;
	}
	
	public override T GetAsset<T>()
	{
        if (m_Request != null && m_Request.isDone)
        {
            if (m_Request.asset == null)
            {
                //Fabric.Crashlytics.Crashlytics.Log(string.Format("AssetBundle : {0}, AssetName : {1}", m_AssetBundleName, m_AssetName));
                //Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetOpertaionFull.GetAsset returned null : m_Request.asset is null");
            }

            T rAsset = m_Request.asset as T;

            if (rAsset == null)
            {
                //Fabric.Crashlytics.Crashlytics.Log(string.Format("AssetBundle : {0}, AssetName : {1}", m_AssetBundleName, m_AssetName));
                //Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetOpertaionFull.GetAsset returned null : failed to asset typecast");
            }

            return rAsset;
        }
        else
        {
            //if (m_Request == null)
            //{
            //    Fabric.Crashlytics.Crashlytics.Log(string.Format("AssetBundle : {0}, AssetName : {1}", m_AssetBundleName, m_AssetName));
            //    Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetOpertaionFull.GetAsset was failed : m_Request is null");
            //}
            //if (m_Request.isDone == false)
            //{
            //    Fabric.Crashlytics.Crashlytics.Log(string.Format("AssetBundle : {0}, AssetName : {1}", m_AssetBundleName, m_AssetName));
            //    Fabric.Crashlytics.Crashlytics.Log("AssetBundleLoadAssetOpertaionFull.GetAsset was failed : m_Request.isDone is false");
            //}

            return null;
        }
	}
	
	// Returns true if more Update calls are required.
	public override bool Update ()
	{
		if (m_Request != null)
			return false;

		LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle (m_AssetBundleName, out m_DownloadingError);
		if (bundle != null)
		{
			if (bundle.m_AssetBundle == null) {
				Debug.LogError (string.Format ("AssetBundle : {0}, AssetName : {1} load error", m_AssetBundleName, m_AssetName));
				Debug.LogError (string.Format ("ErrorMessage : {0}", (m_DownloadingError == null) ? string.Empty : m_DownloadingError));
			}
            else
                m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
			return false;
		}
		else
		{
			return true;
		}
	}
	
	public override bool IsDone ()
	{
		// Return if meeting downloading error.
		// m_DownloadingError might come from the dependency downloading.
		if (m_Request == null && m_DownloadingError != null)
		{
			Debug.Log(m_DownloadingError);
			return true;
		}

		return m_Request != null && m_Request.isDone;
	}
}

public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
{
	public AssetBundleLoadManifestOperation (string bundleName, string assetName, System.Type type)
		: base(bundleName, assetName, type)
	{
	}

	public override bool Update ()
	{
		base.Update();
		
		if (m_Request != null && m_Request.isDone)
		{
			AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
			return false;
		}
		else
			return true;
	}
}

