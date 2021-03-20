using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using UnityEngine.Networking;
using SimpleJSON;

#if UNITY_EDITOR	
using UnityEditor;
#endif

public enum eVCstate
{
	APP_STARTED,

	TRY_LOAD_SERVER_XML,
	FAIL_LOAD_SERVER_XML,
	FINISH_LOAD_SERVER_XML,

	ASSET_DOWNLOADING,
	ASSET_DOWN_FAILED,
}

public class VersionChecker : MonoBehaviour
{
#if BUILD_ANDROID
    const String DebugPlatform = "Android";
#elif BUILD_IOS
    const String DebugPlatform = "iOS";
#else
    const String DebugPlatform = "Android";
#endif

    bool isValidInstance = false;

	static VersionChecker m_Instance;
	public static VersionChecker Instance
	{
		get
		{
			if(m_Instance == null)
			{
				GameObject newObject = new GameObject();
				m_Instance = newObject.AddComponent<VersionChecker>();
				m_Instance.isValidInstance = true;
			}
			return m_Instance;
		}
	}

	eVCstate m_CurState = eVCstate.APP_STARTED;

    public eVCstate AssetState { get { return m_CurState; } }

	VersionInfo m_LoadedServerInfo = new VersionInfo();
	VersionInfo m_CurHavingInfo = new VersionInfo();

	//public bool IsLatest { get; private set; }

	public Dictionary<string, stAssetInfo> AssetInfoDict
	{
		get
		{
			return m_LoadedServerInfo.AssetDict;
		}
	}

	double m_FullAssetSize = 0.0f;
	double m_HavingAssetSize = 0.0f;

	public double NEED_SIZE { get; private set; }
	public double DOWNLOADED_SIZE { get; private set; }

	static public string GetSizeString(double byteCount)
	{
		return String.Format("{0:F2}", byteCount / 1048576.0) + " MB";
	}

	public string NEED_SIZE_MB
	{
		get
		{
			return GetSizeString(NEED_SIZE);
		}
	}
	public string DOWN_SIZE_MB
	{
		get
		{
			return GetSizeString(DOWNLOADED_SIZE);
		}
	}

	public int NEED_DOWNLOAD { get; private set; }

	//public int MAJOR
	//{
	//	get
	//	{
	//		int outInt = 0;
	//		int.TryParse(m_CurHavingInfo.Major, out outInt);
	//		return outInt;
	//	}
	//}

	//public int MINOR
	//{
	//	get
	//	{
	//		int outInt = 0;
	//		int.TryParse(m_CurHavingInfo.Minor, out outInt);
	//		return outInt;
	//	}
	//}

	//public int REVISION
	//{
	//	get
	//	{
	//		int outInt = 0;
	//		int.TryParse(m_CurHavingInfo.Revision, out outInt);
	//		return outInt;
	//	}
	//}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
			isValidInstance = true;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		GameObject.DontDestroyOnLoad(this);
		//StartCoroutine(CheckVersion(defaultURL));
	}

	private void Update()
	{
		//int a = 1;
	}

	void CheckAPKstep()
	{
		//Debug.Log(PlayerSettings.Android.bundleVersionCode.ToString());
		//Debug.Log(PlayerSettings.bundleVersion);
		//m_CurHavingInfo.SetVersion(PlayerSettings.bundleVersion);
	}

	public string GetCurVerString()
	{
//#if UNITY_EDITOR
//		return Application.version;
//#else
//		return string.Format("{0}.{1}.{2}",
//		m_CurHavingInfo.Major,
//		m_CurHavingInfo.Minor,
//		m_CurHavingInfo.Revision
//			);
//#endif
        return Application.version;
    }

	public string CachedBaseURL = string.Empty;
	public void SetBaseURL(string baseCDNurl)
	{
		string trimURL = baseCDNurl;
		if (string.IsNullOrEmpty(trimURL) == true)
			trimURL = string.Empty;
		trimURL = trimURL.TrimEnd('/');

        CachedBaseURL = trimURL + "/";// + DebugPlatform.ToString() + "/";
		//CachedBaseURL = trimURL + "/" + Application.platform.ToString() + "/";

		AssetBundleManager.BaseDownloadingURL = CachedBaseURL;
		Debug.Log("base asset url cached : " + CachedBaseURL);
	}
    public void SetBaseCDNURL(string baseCDNrul)
    {
        AssetBundleManager.BaseCDNAddressURL = baseCDNrul + "/" + DebugPlatform.ToString() + "/";
    }

    public System.Action CDN_CONNECT_FAIL_CALLBACK = null;
	bool CDN_CONNECT_FAIL_FORCE_END = false;



    public IEnumerator CheckAssetStremaingVersion()
    {
        NEED_SIZE = 0;
        DOWNLOADED_SIZE = 0;
        m_FullAssetSize = 0.0f;
        m_HavingAssetSize = 0.0f;
        NEED_DOWNLOAD = int.MaxValue;   // ���ǰ� �ʱ�ȭ

        //LoadLocalPatchXML();

        string fileURL = GetLoaclPathXMLPath() + VersionXML.VersionFileName;

        //WWW serverXMLwww = new WWW(fileURL);
        UnityWebRequest localxmlwww = UnityWebRequest.Get(fileURL);

        yield return localxmlwww.SendWebRequest();

        if (localxmlwww.error != null)
        {
            Debug.LogError("Load local XML failed : " + localxmlwww.error);
            yield break;
        }

        XmlDocument localXML = XmlTool.loadXml(localxmlwww.downloadHandler.data);
        if (localXML == null)
        {
            Debug.LogError("localXML is null");
            yield break;
        }

        m_CurHavingInfo.ParseXML(localXML, GetLoaclPathXMLPath());
    }


    public IEnumerator CheckVersion(string cdnURL)
	{
        yield return LoadServerPatchXML();

        if (m_CurState == eVCstate.FAIL_LOAD_SERVER_XML)
        {
            if (CDN_CONNECT_FAIL_CALLBACK != null)
                CDN_CONNECT_FAIL_CALLBACK();

            while (CDN_CONNECT_FAIL_FORCE_END == false)
                yield return null;

            yield break;
        }

        CheckAPKstep();         // apk ���� ��?

        var havDict = m_CurHavingInfo.AssetDict;
        var serverDict = m_LoadedServerInfo.AssetDict;

        NEED_DOWNLOAD = 0;
        foreach (var kvPair in serverDict)
        {

            m_FullAssetSize += kvPair.Value.FileSize;

            if (Caching.IsVersionCached(kvPair.Key, (int)kvPair.Value.Version) == true)
            {
                m_HavingAssetSize += kvPair.Value.FileSize;
                m_CurHavingInfo.AssetDict[kvPair.Key] = kvPair.Value;
                Caching.MarkAsUsed(kvPair.Key, (int)kvPair.Value.Version);
                continue;
            }

            // cdn 주소가 streaming-> cdn으로가져옴
            if (havDict.ContainsKey(kvPair.Key) == true)
            {
                // stremaing asset patch 랑 cdn patch랑 겹치는데, streaming 버전이 같거나 작으면 pass.
                if (havDict[kvPair.Key].Version >= (int)kvPair.Value.Version)
                {
                    continue;
                }
            }

            NEED_SIZE += kvPair.Value.FileSize;
            NEED_DOWNLOAD++;
        }

        Debug.Log("Total Need Size : " + NEED_SIZE.ToString());








        //NEED_SIZE = 0;
        //DOWNLOADED_SIZE = 0;
        //m_FullAssetSize = 0.0f;
        //m_HavingAssetSize = 0.0f;
        //NEED_DOWNLOAD = int.MaxValue;	// ���ǰ� �ʱ�ȭ



        //yield return LoadServerPatchXML(CachedBaseURL);

        //if (m_CurState == eVCstate.FAIL_LOAD_SERVER_XML)
        //{
        //	if (CDN_CONNECT_FAIL_CALLBACK != null)
        //		CDN_CONNECT_FAIL_CALLBACK();

        //	while (CDN_CONNECT_FAIL_FORCE_END == false)
        //		yield return null;

        //	yield break;
        //}

        //CheckAPKstep();         // apk ���� ��?

        //var havDict = m_CurHavingInfo.AssetDict;
        //var serverDict = m_LoadedServerInfo.AssetDict;


        //NEED_DOWNLOAD = 0;
        //foreach (var kvPair in serverDict)
        //{
        //	m_FullAssetSize += kvPair.Value.FileSize;

        //          if (Caching.IsVersionCached(kvPair.Key, (int)kvPair.Value.Version) == true)
        //          {
        //              m_HavingAssetSize += kvPair.Value.FileSize;
        //              m_CurHavingInfo.AssetDict[kvPair.Key] = kvPair.Value;
        //              Caching.MarkAsUsed(kvPair.Key, (int)kvPair.Value.Version);
        //              continue;
        //          }
        //          NEED_SIZE += kvPair.Value.FileSize;
        //	NEED_DOWNLOAD++;
        //}

        //Debug.Log("Total Need Size : " + NEED_SIZE.ToString());
    }

    public Action OnDisconnectNetwork;
    void _OnDisconnectNetwork()
    {
        if (OnDisconnectNetwork != null)
            OnDisconnectNetwork();
    }

	public IEnumerator LoadAssets(Action onDisconnectMethod = null)
	{

        m_CurState = eVCstate.ASSET_DOWNLOADING;
        DOWNLOADED_SIZE = 0;

        OnDisconnectNetwork = onDisconnectMethod;
        var havDict = m_CurHavingInfo.AssetDict;
        var serverDict = m_LoadedServerInfo.AssetDict;

        foreach (var kvPair in serverDict)
        {
            string assetName = kvPair.Key;

            bool isNewDownload = true;

            if (Caching.IsVersionCached(kvPair.Key, (int)kvPair.Value.Version) == true)
            {
                isNewDownload = false;
            }

            if (havDict.ContainsKey(kvPair.Key))
            {
                if (havDict[kvPair.Key].Version >= kvPair.Value.Version)
                {
                    continue;
                }
            }

            bool isPlatformManifest = false;
            if (assetName.Equals(AssetBundleManager.strPlatformManifestName) == true)
            {
                isPlatformManifest = true;
            }

            string assetURL = "datapath";

            double startFileSize = m_HavingAssetSize;
            double startDownloadedSize = DOWNLOADED_SIZE;

            using (UnityWebRequest loadAsset_u = UnityWebRequestAssetBundle.GetAssetBundle(assetURL, (uint)kvPair.Value.Version, 0))
            {
                loadAsset_u.SendWebRequest();

                while (loadAsset_u.isDone == false)
                {
                    double increasedSize = kvPair.Value.FileSize * (double)loadAsset_u.downloadProgress;
                    m_HavingAssetSize = startFileSize + increasedSize; 

                    if (isNewDownload == true)
                        DOWNLOADED_SIZE = startDownloadedSize + increasedSize;

                    if (Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        _OnDisconnectNetwork();
                        m_CurState = eVCstate.ASSET_DOWN_FAILED;
                        yield break;
                    }

                    if (loadAsset_u.error != null)
                    {
                        if (!loadAsset_u.error.Contains("another AssetBundle with the same files is already loaded"))
                        {
                            Debug.LogError("WWW Error : " + loadAsset_u.error);
                            m_CurState = eVCstate.ASSET_DOWN_FAILED;
                            yield break;
                        }
                    }

                    yield return null;
                }

                if (loadAsset_u.error != null)
                {
                    if (!loadAsset_u.error.Contains("another AssetBundle with the same files is already loaded"))
                    {
                        Debug.LogError("WWW Error : " + loadAsset_u.error);
                        m_CurState = eVCstate.ASSET_DOWN_FAILED;
                        yield break;
                    }
                }

                m_HavingAssetSize = startFileSize + kvPair.Value.FileSize;

                if (isNewDownload == true)
                {
                    DOWNLOADED_SIZE = startDownloadedSize + kvPair.Value.FileSize;
                    NEED_DOWNLOAD--;
                    if (NEED_DOWNLOAD <= 0)
                        NEED_DOWNLOAD = 0;
                }

                m_CurHavingInfo.AssetDict[kvPair.Key] = kvPair.Value;
  

                if (isPlatformManifest == false)
                {
                    AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(loadAsset_u);

                    if (loadedBundle != null)
                        loadedBundle.Unload(true);
                }
            }

            SaveCurVerInfo();
        }

        OnFinishDownAsset();











        //      m_CurState = eVCstate.ASSET_DOWNLOADING;
        //      DOWNLOADED_SIZE = 0;

        //      OnDisconnectNetwork = onDisconnectMethod;
        //var havDict = m_CurHavingInfo.AssetDict;
        //var serverDict = m_LoadedServerInfo.AssetDict;
        //      foreach (var kvPair in serverDict)
        //{
        //	string assetName = kvPair.Key;

        //          bool isNewDownload = true;

        //          if (Caching.IsVersionCached(kvPair.Key, (int)kvPair.Value.Version) == true)
        //              isNewDownload = false;

        //          bool isPlatformManifest = false;
        //          if (assetName.Equals(AssetBundleManager.strPlatformManifestName) == true)
        //          {
        //              isPlatformManifest = true;
        //          }

        //	string assetURL = CachedBaseURL + assetName;

        //	double startFileSize = m_HavingAssetSize;
        //	double startDownloadedSize = DOWNLOADED_SIZE;

        //          using (UnityWebRequest loadAsset_u = UnityWebRequest.GetAssetBundle(assetURL, (uint)kvPair.Value.Version, 0))
        //	{
        //              loadAsset_u.SendWebRequest();

        //		while (loadAsset_u.isDone == false)
        //		{
        //			double increasedSize = kvPair.Value.FileSize * (double)loadAsset_u.downloadProgress;
        //			m_HavingAssetSize = startFileSize + increasedSize; // ���� ��� ������ ����

        //			if (isNewDownload == true)
        //				DOWNLOADED_SIZE = startDownloadedSize + increasedSize;

        //                  //Debug.Log(Application.internetReachability.ToString());
        //                  if( Application.internetReachability == NetworkReachability.NotReachable)
        //                  {
        //                      _OnDisconnectNetwork();
        //                      m_CurState = eVCstate.ASSET_DOWN_FAILED;
        //                      yield break;
        //                  }

        //                  if (loadAsset_u.error != null)
        //                  {
        //                      if (!loadAsset_u.error.Contains("another AssetBundle with the same files is already loaded"))
        //                      {
        //                          Debug.LogError("WWW Error : " + loadAsset_u.error);
        //                          m_CurState = eVCstate.ASSET_DOWN_FAILED;
        //                          yield break;
        //                      }
        //                  }

        //                  yield return null;
        //		}

        //		if (loadAsset_u.error != null)
        //		{
        //			if (!loadAsset_u.error.Contains("another AssetBundle with the same files is already loaded"))
        //			{
        //				Debug.LogError("WWW Error : " + loadAsset_u.error);
        //				m_CurState = eVCstate.ASSET_DOWN_FAILED;
        //				yield break;
        //			}
        //		}

        //		m_HavingAssetSize = startFileSize + kvPair.Value.FileSize;

        //		if (isNewDownload == true)
        //		{
        //			DOWNLOADED_SIZE = startDownloadedSize + kvPair.Value.FileSize;
        //			//Debug.Log("Cur Downloaded Size : " + DOWNLOADED_SIZE.ToString());
        //			NEED_DOWNLOAD--;
        //		}

        //		m_CurHavingInfo.AssetDict[kvPair.Key] = kvPair.Value;

        //              if (isPlatformManifest == false)
        //              {
        //                  AssetBundle loadedBundle = DownloadHandlerAssetBundle.GetContent(loadAsset_u);

        //                  if (loadedBundle != null)
        //                      loadedBundle.Unload(true);
        //              }
        //	}

        //          SaveCurVerInfo();
        //      }

        //      OnFinishDownAsset();


    }

    public void OnFinishDownAsset()
    {
        m_CurHavingInfo.AssetCRC = m_LoadedServerInfo.AssetCRC;
        m_CurHavingInfo.AssetVersion = m_LoadedServerInfo.AssetVersion;

        SaveCurVerInfo();
    }

    public IEnumerator CheckSettingData(string strCDNurl)
    {
        string fileurl = strCDNurl + "TCPsetting.json";
        UnityWebRequest TCPsettingjsonWWW = UnityWebRequest.Get(fileurl);
        yield return TCPsettingjsonWWW.SendWebRequest();
        if (TCPsettingjsonWWW.error != null)
        {
            Debug.LogError("TCPsettingjsonWWW error : " + TCPsettingjsonWWW.error);
            yield break;
        }

        string strTcpSettingText = TCPsettingjsonWWW.downloadHandler.text;
        if (string.IsNullOrEmpty(strTcpSettingText) == false)
        {
            var dicsetting = JSON.Parse(strTcpSettingText) as JSONObject;
            if (dicsetting != null)
            {
                int nTCPmode = 0;
                nTCPmode = JSONDataStruct.GetDataInt(dicsetting, "TCPMODE");
            }
        }

    }

	IEnumerator LoadServerPatchXML()
	{
		m_CurState = eVCstate.TRY_LOAD_SERVER_XML;
        string strCDNurl = "Path";

        if (string.IsNullOrEmpty (strCDNurl) == true)
        {
            Debug.LogError("strCDNurl is empty");
            m_CurState = eVCstate.FAIL_LOAD_SERVER_XML;
			yield break;
		}

		string fileURL = strCDNurl + VersionXML.VersionFileName;

        //WWW serverXMLwww = new WWW(fileURL);
        UnityWebRequest serverXMLwww = UnityWebRequest.Get(fileURL);

        yield return serverXMLwww.SendWebRequest();
		
		if(serverXMLwww.error != null)
		{
			Debug.LogError("Load XML failed : " + serverXMLwww.error);
			m_CurState = eVCstate.FAIL_LOAD_SERVER_XML;
			yield break;
		}

		XmlDocument serverXML = XmlTool.loadXml(serverXMLwww.downloadHandler.data);
		if (serverXML == null)
        {
            Debug.LogError("serverXML is null");
            m_CurState = eVCstate.FAIL_LOAD_SERVER_XML;
			yield break;
		}

		m_LoadedServerInfo.ParseXML(serverXML, strCDNurl);
		
		m_CurState = eVCstate.FINISH_LOAD_SERVER_XML;
	}

	public static string GetLoaclPathXMLPath()
	{
        //string localPath = Application.persistentDataPath + "/" + VersionXML.VersionFileName;
        //Debug.Log("localPatchXML : " + localPath);


        string localPath = string.Empty;
#if UNITY_EDITOR
        localPath = Application.dataPath + "/StreamingAssets" + "/" + DebugPlatform + "/";
#elif UNITY_ANDROID 
        localPath = Application.streamingAssetsPath + "/" + DebugPlatform + "/";
#elif UNITY_IOS
        localPath = "file://"+Application.streamingAssetsPath + "/" + DebugPlatform + "/";
#else
      localPath ="file://"+Application.streamingAssetsPath + "/" +DebugPlatform+"/" ;
#endif
        return localPath;
	}

	void LoadLocalPatchXML()
	{
		XmlDocument localDoc = XmlTool.loadXml(GetLoaclPathXMLPath());

		if(localDoc != null)
		{
			m_CurHavingInfo.ParseXML(localDoc);
		}

		string strVer = Application.version;
		

		Debug.Log("local app version : " + strVer);

		//SimpleVersion simVer = new SimpleVersion(strVer);

		//m_CurHavingInfo.Major = simVer.Major;
		//m_CurHavingInfo.Minor = simVer.Minor;
		//m_CurHavingInfo.Revision = simVer.Revision;
	}

	struct SimpleVersion
	{
		public string Major;
		public string Minor;
		public string Revision;

		public SimpleVersion(string attachedStr)
		{
			Major = string.Empty;
			Minor = string.Empty;
			Revision = string.Empty;

			if (string.IsNullOrEmpty(attachedStr) == true)
				return;

			string[] tokenArr = attachedStr.Split('_', '.');
			if (tokenArr.Length > 0)
				Major = tokenArr[0];
			if (tokenArr.Length > 1)
				Minor = tokenArr[1];
			if (tokenArr.Length > 2)
				Revision = tokenArr[2];
		}
	}

	void SaveCurVerInfo()
    {
        if (m_CurState == eVCstate.FAIL_LOAD_SERVER_XML || m_CurState == eVCstate.APP_STARTED)
            return;

        //���� ���� ������ ������ ���� �ʿ䰡 ������
  //      string localPath = Application.persistentDataPath + "/" + VersionXML.VersionFileName;

		//if (File.Exists(localPath) == true)
		//	File.Delete(localPath);

		//XmlDocument curXML = m_CurHavingInfo.ToXML();
		//XmlTool.writeXml(localPath, curXML);
  //      //Debug.LogError("Save in SaveCurVerInfo");
    }

	private void OnDisable()
	{
		if (isValidInstance == true)
			SaveCurVerInfo();
	}

    const int BIT_YEAR = 10;
    const int BIT_MONTH = 4;
    const int BIT_DAY = 5;
    const int BIT_HOUR = 5;
    const int BIT_MIN = 6;

	public static int BitVersionCode(int iYear, int iMonth, int iDay, int iHour, int iMin)
	{
		int retSum = 0;

		int tempYear = iYear - 2000;
		//retSum = retSum << 10;  // temp year : 0~1023 (+2000)
		retSum += tempYear;

		retSum = retSum << BIT_MONTH;   // month : 0~15
		retSum += iMonth;

		retSum = retSum << BIT_DAY;   // day : 0~31
		retSum += iDay;

		retSum = retSum << BIT_HOUR;   // hour : 0~31
		retSum += iHour;

		retSum = retSum << BIT_MIN;   // min : 0~63
		retSum += iMin;

		// ���� 30bit
		return retSum;
	}

    public static DateTime ReverseVersionBit(int bitVersion)
    {
        int copyBit = bitVersion;

        int iMin = GetIntFromBitAndPrepareNextOperation(BIT_MIN, ref copyBit);
        int iHour = GetIntFromBitAndPrepareNextOperation(BIT_HOUR, ref copyBit);
        int iDay = GetIntFromBitAndPrepareNextOperation(BIT_DAY, ref copyBit);
        int iMonth = GetIntFromBitAndPrepareNextOperation(BIT_MONTH, ref copyBit);
        int iYear = copyBit + 2000;

        DateTime retDateTime = new DateTime(iYear, iMonth, iDay, iHour, iMin, 0);
        return retDateTime;
    }

    static int GenerateBitMask(int bitCount)
    {
        int iMask = 0;
        for(int i =0; i<bitCount; ++i)
        {
            iMask = iMask << 1;
            iMask++;
        }
        return iMask;
    }

    static int GetIntFromBitAndPrepareNextOperation(int bitCount, ref int srcBit)
    {
        int retInt = srcBit & GenerateBitMask(bitCount);
        srcBit = srcBit >> bitCount;
        return retInt;
    }
}

