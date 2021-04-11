using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.IO;

#if UNITY_EDITOR 
using UnityEditor;
#endif

public class JSONDataStruct
{
    protected JSONNode m_RootNode = null;

    static Dictionary<string, string> s_dicTextAsset = new Dictionary<string, string>();

#if UNITY_EDITOR
    static bool m_bLoadData = false;
#endif
    static public IEnumerator LoadTextAssets()
    {

        s_dicTextAsset.Clear();


        if (AssetBundleManager.SimulateAssetBundleInEditor == true)
        {
            string[] arrSettingData = AssetDatabase.GetAssetPathsFromAssetBundle("setting_data");
            if (arrSettingData == null)
                yield break;

            string stringData = string.Empty;
            for (int i = 0; i < arrSettingData.Length; ++i)
            {
                stringData = Path.GetFileName(arrSettingData[i]);
                stringData = stringData.Replace(".json", "");
                TextAsset assetData = AssetDatabase.LoadAssetAtPath<TextAsset>(arrSettingData[i]);
                s_dicTextAsset.Add(stringData, assetData.text);
            }

            yield break;
        }



        {
            while (AssetBundleManager.AssetBundleManifestObject == null)
            {
                yield return new WaitForSeconds(1f);
            }


            AssetBundleManager.LoadAssetBundle("setting_data");


            string strError = string.Empty;


            LoadedAssetBundle loadBundle = AssetBundleManager.GetLoadedAssetBundle("setting_data", out strError);
            while (loadBundle == null)
            {
                yield return new WaitForSeconds(0.1f);
                loadBundle = AssetBundleManager.GetLoadedAssetBundle("setting_data", out strError);
            }


            string[] strAssetData = loadBundle.m_AssetBundle.GetAllAssetNames();

            if (strAssetData != null)
            {
                string stringData = string.Empty;
                for (int i = 0; i < strAssetData.Length; ++i)
                {
                    stringData = Path.GetFileName(strAssetData[i]);
                    stringData = stringData.Replace(".json", "");
                    stringData = stringData.ToUpper();

                    AssetBundleLoadAssetOperation operation = AssetBundleManager.LoadAssetAsync("setting_data", stringData, typeof(TextAsset));
                    if (operation != null)
                    {
                        while (operation.IsDone() == false)
                        {
                            yield return new WaitForEndOfFrame();
                        }

                        TextAsset assetData = operation.GetAsset<TextAsset>();
                        s_dicTextAsset.Add(stringData, assetData.text);
                    }

                    AssetBundleManager.UnloadAssetBundle("setting_data");
                }
            }
        }

    }

    private static void GetTextAssetsFromDirectory(DirectoryInfo directoryInfo)
    {
        if (null == directoryInfo)
            return;
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            if (file.Extension != ".json")
                continue;
            Debug.Log(file.Name);
            using (StreamReader reader = file.OpenText())
            {
                string readData = reader.ReadToEnd();

                string strFileName = file.Name;
                strFileName = strFileName.Replace(".json", "");
                strFileName = strFileName.ToUpper();

                s_dicTextAsset.Add(strFileName, readData);
                Debug.Log(strFileName + "'s Length : " + readData.Length);
            }
        }
    }


    // 개발용 INITUPDATE 치트키용 TextAsset Reload ( 서버에서만 사용 ) /////////////////
    private class UpdatedTextAssetData
    {
        public long LastUpdateTicks { get; set; }
        public JSONNode JsonNodeData { get; set; }
    }

    private static Dictionary<string, UpdatedTextAssetData> s_dicUpdatedTextAssetData = new Dictionary<string, UpdatedTextAssetData>();

    public static bool UpdateTextAssetFromDirectory(DirectoryInfo directoryInfo, string assetName, out JSONNode outputNode)
    {
        outputNode = null;
#if UNITY_EDITOR || UNITY_STANDALONE  // 에디터 또는 서버 빌드일 경우
		if (null == directoryInfo || false == directoryInfo.Exists || true == string.IsNullOrEmpty(assetName))
			return false;

		string fileFullPath = directoryInfo.FullName + "/" + assetName + ".json";
		try
		{
			FileInfo fileInfo = new FileInfo(fileFullPath);
			if (false == fileInfo.Exists || 0L == fileInfo.Length)
				return false;

			UpdatedTextAssetData updatedTextAssetData = null;
			if (false == s_dicUpdatedTextAssetData.TryGetValue(fileFullPath, out updatedTextAssetData) || null == updatedTextAssetData)
			{
				updatedTextAssetData = new UpdatedTextAssetData();
				s_dicUpdatedTextAssetData[fileFullPath] = updatedTextAssetData;
			}

			if (null == updatedTextAssetData)
				return false;

			if (fileInfo.LastWriteTime.Ticks > updatedTextAssetData.LastUpdateTicks)
			{
				using (StreamReader streamReader = fileInfo.OpenText())
				{
					string readData = streamReader.ReadToEnd();
					updatedTextAssetData.JsonNodeData = JSON.Parse(readData);
					updatedTextAssetData.LastUpdateTicks = fileInfo.LastWriteTime.Ticks;
				}
				outputNode = updatedTextAssetData.JsonNodeData;
				return true;
			}
			else
			{
				outputNode = updatedTextAssetData.JsonNodeData;
				return false;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
			return false;
		}
#else                     // 클라 빌드에서 Virtual 모드일 경우
        JSONDataStruct dataStruct = new JSONDataStruct();
        UpdatedTextAssetData updatedTextAssetData = null;
        if (true == s_dicUpdatedTextAssetData.TryGetValue(assetName, out updatedTextAssetData))
        {
            outputNode = updatedTextAssetData.JsonNodeData;
            return true;
        }

        if (false == dataStruct.LoadJSONFile(assetName))
            return false;
        updatedTextAssetData = new UpdatedTextAssetData();
        outputNode = updatedTextAssetData.JsonNodeData = dataStruct.RootNode;
        s_dicUpdatedTextAssetData[assetName] = updatedTextAssetData;
        return true;
#endif
    }

    // 개발용 INITUPDATE 치트키용 TextAsset Reload End /////////////////


    public JSONNode RootNode
    {
        get { return m_RootNode; }
    }

    public bool IsValidRootNode()
    {
        if (m_RootNode == null)
            return false;

        return true;
    }

    public bool IsValidMainNode(string strMainKey)
    {
        if (!IsValidRootNode())
            return false;

        return !(m_RootNode[strMainKey] is JSONLazyCreator);
    }

    public void MakeMainNode(string strMainKey)
    {
        if (m_RootNode == null)
        {
            m_RootNode = JSONDataStruct.MakeDictionary();
        }

        JSONObject classNode = JSONDataStruct.MakeDictionary();
        m_RootNode.Add(strMainKey, classNode);
    }

    public JSONNode GetMainNode(string strMainKey)
    {
        if (!IsValidRootNode())
            return null;

        return m_RootNode[strMainKey];
    }


    public void SaveJSONFile(string strDataName)
    {
        if (m_RootNode != null)
        {
            //m_RootNode.SaveToFile(strDataName);

            using (FileStream fStream = new FileStream(strDataName, FileMode.Create, FileAccess.Write, FileShare.Read, 8, FileOptions.WriteThrough))
            {
                using (StreamWriter streamWriter = new StreamWriter(fStream))
                {
                    streamWriter.Write(m_RootNode.ToString(5));

                    //streamWriter.Close();
                }
            }
        }
    }


    public void SaveJSONFile_THAI(string strDataName)
    {
        if (m_RootNode != null)
        {
            //m_RootNode.SaveToFile(strDataName);

            using (FileStream fStream = new FileStream(strDataName, FileMode.Create, FileAccess.Write, FileShare.Read, 16, FileOptions.WriteThrough))
            {
                using (StreamWriter streamWriter = new StreamWriter(fStream))
                {
                    streamWriter.Write(m_RootNode.ToString(5));

                    //streamWriter.Close();
                }
            }
        }
    }

    public bool LoadJSONFile(string strDataName)
    {
        if (string.IsNullOrEmpty(strDataName))
            return false;

#if UNITY_EDITOR
        if (AssetBundleManager.SimulateAssetBundleInEditor == true)
        {
            if (m_bLoadData == false)
            {
                s_dicTextAsset.Clear();

                string[] arrSettingData = AssetDatabase.GetAssetPathsFromAssetBundle("setting_data");
                if (arrSettingData == null)
                    return false;

                string stringData = string.Empty;
                for (int i = 0; i < arrSettingData.Length; ++i)
                {
                    stringData = Path.GetFileName(arrSettingData[i]);
                    stringData = stringData.Replace(".json", "");
                    TextAsset assetData = AssetDatabase.LoadAssetAtPath<TextAsset>(arrSettingData[i]);
                    s_dicTextAsset.Add(stringData, assetData.text);
                }

                m_bLoadData = true;
            }
        }
#endif


        string templateText = null;
        s_dicTextAsset.TryGetValue(strDataName, out templateText);
        if (templateText == null)
        {
            Debug.LogError(strDataName + " templateText == null");
            return false;
        }

        m_RootNode = JSON.Parse(templateText);
        s_dicTextAsset.Remove(strDataName);

        if (m_RootNode != null)
            return true;

        Debug.LogError(strDataName + " m_RootNode == null");
        return false;
    }

    public bool LoadJSONText(string strText)
    {
        m_RootNode = JSON.Parse(strText);

        if (m_RootNode != null)
            return true;

        return false;
    }

    public bool SetJSONData(JSONNode rootNode)
    {
        m_RootNode = rootNode;
        if (m_RootNode != null)
            return true;

        return false;
    }

    public bool LoadJSONText(TextAsset textAsset)
    {
        if (textAsset == null)
            return false;

        return LoadJSONText(textAsset.text);
    }


    static public int GetDataCount(JSONNode dataSet, string strKey)
    {
        if (dataSet == null)
            return 0;

        if (dataSet[strKey] != null)
        {
            return dataSet[strKey].Count;
        }

        return 0;
    }
    //public int GetDataCount( int nIndex , string strKey )
    //{
    //    if (m_RootNode == null)
    //        return 0;

    //    foreach (JSONNode nodeData in m_RootNode.Children)
    //    {
    //        JSONNode nodeIndex = nodeData[nIndex];
    //        if (nodeIndex != null)
    //        {
    //            if (nodeIndex[strKey] != null)
    //            {
    //                return nodeIndex[strKey].Count;
    //            }
    //        }
    //    }


    //    return 0;
    //}

    //static public ArrayList GetDataArrayToInt(JSONNode dataSet, string strKey)
    //{
    //    ArrayList arrayData = new ArrayList();
    //
    //    if (dataSet != null && dataSet[strKey] != null)
    //    {
    //        foreach (JSONNode childData in dataSet[strKey].Childs)
    //        {
    //            int nValue = Convert.ToInt32(childData.Value);
    //            arrayData.Add(nValue);
    //        }
    //    }
    //
    //    return arrayData;
    //}

    //static public ArrayList GetDataArrayToFloat(JSONNode dataSet, string strKey)
    //{
    //    ArrayList arrayData = new ArrayList();
    //
    //    if (dataSet != null && dataSet[strKey] != null)
    //    {
    //        foreach (JSONNode childData in dataSet[strKey].Childs)
    //        {
    //            float fValue = Convert.ToSingle(childData.Value);
    //            arrayData.Add(fValue);
    //        }
    //    }
    //
    //    return arrayData;
    //}

    static public List<string> GetDataArrayString(JSONNode dataSet, string strkey)
    {
        List<string> listTempString = new List<string>();

        if (dataSet != null && dataSet[strkey] != null)
        {
            JSONArray arrData = dataSet[strkey] as JSONArray;
            if (arrData != null)
            {
                for (int i = 0; i < arrData.Count; ++i)
                {
                    listTempString.Add(arrData[i].Value);
                }
            }
        }

        return listTempString;
    }

    static public void GetDataArrayString(JSONNode dataSet, string strkey, ref List<string> listString, bool bClear = false)
    {
        if (bClear)
            listString.Clear();

        if (dataSet != null && dataSet[strkey] != null)
        {
            JSONArray arrData = dataSet[strkey] as JSONArray;
            if (arrData == null)
                return;

            for (int i = 0; i < arrData.Count; ++i)
            {
                listString.Add(arrData[i].Value);
            }
        }
    }

    static public void GetDataArrayInt(JSONNode dataSet, string strkey, ref List<int> listInt, bool bClear = false)
    {
        if (bClear)
            listInt.Clear();

        if (dataSet != null && dataSet[strkey] != null)
        {
            JSONArray arrData = dataSet[strkey] as JSONArray;
            if (arrData == null)
                return;

            try
            {
                for (int i = 0; i < arrData.Count; ++i)
                {
                    listInt.Add(EnumData.GetInteger(arrData[i].Value));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                for (int i = 0; i < arrData.Count; ++i)
                {
                    Debug.LogError("Elem " + i.ToString() + " : " + arrData[i].Value);
                }
            }

        }
    }


    //static public ArrayList GetDataArray<DataType>(JSONNode dataSet, string strKey)
    //{
    //    ArrayList arrayData = new ArrayList();
    //
    //    if ( dataSet != null && dataSet[strKey] != null)
    //    {
    //
    //        JSONArray arrData = dataSet[strKey] as JSONArray;
    //        if( arrData != null )
    //        {
    //            for (int i = 0; i < arrData.Count; ++i)
    //            {
    //                arrayData.Add(arrData[i].Value);
    //            }
    //        }
    //    }
    //
    //    return arrayData;
    //}

    //public ArrayList GetDataArray<DataType>( int nIndex , string strKey )
    //{
    //    ArrayList arrayData = new ArrayList();
    //
    //    if (m_RootNode != null)
    //    {
    //        foreach (JSONNode nodeData in m_RootNode.Childs)
    //        {
    //            JSONNode nodeIndex = nodeData[nIndex];
    //            if (nodeIndex != null)
    //            {
    //                if (nodeIndex[strKey] != null)
    //                {
    //                    foreach (JSONNode childData in nodeIndex[strKey].Childs)
    //                    {
    //                        arrayData.Add(childData.Value);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //
    //    return arrayData;
    //}


    static public bool ContainsKey(JSONNode dataSet, string strKey)
    {
        if (dataSet == null)
            return false;

        JSONNode nodeData = dataSet[strKey];

        if (nodeData.Tag == JSONNodeType.None)
            return false;

        return true;
    }


    static public JSONNode GetDataObject(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            JSONNode nodeData = dataSet[strKey];
            return nodeData;
        }

        return null;
    }

    static public T GetDataObject<T>(JSONNode dataSet, string strKey) where T : JSONNode
    {
        if (dataSet != null)
        {
            JSONNode nodeData = dataSet[strKey];
            return nodeData as T;
        }

        return null;
    }


    static public bool GetDataBool(JSONNode dataSet, string strkey)
    {
        if (dataSet != null)
        {
            if (dataSet[strkey] != null)
            {
                //if( dataSet[strkey].IsBoolean == true )

                return dataSet[strkey].AsBool;

                //int nValue = dataSet[strkey].AsInt;
                //return nValue != 0;
            }
        }

        return false;
    }

    static public float GetDataFloat(JSONObject dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                return dataSet[strKey].AsFloat;
            }
        }

        return 0;
    }

    static public float GetDataFloat(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                return dataSet[strKey].AsFloat;
            }
        }

        return 0;
    }

    static public float GetDataFloat(JSONNode dataSet, Enum key)
    {

        return GetDataFloat(dataSet, key.ToString("F"));
    }

    public float GetDataFloat(int nIndex, string strKey)
    {
        if (m_RootNode == null)
            return 0;

        foreach (JSONNode nodeData in m_RootNode.Children)
        {
            JSONNode nodeIndex = nodeData[nIndex];
            if (nodeIndex != null)
            {
                if (nodeIndex[strKey] != null)
                {
                    return nodeIndex[strKey].AsFloat;
                }
            }
        }


        return 0;
    }

    static public void SetDataBool(JSONNode dataSet, string strkey, bool bValue)
    {
        if (dataSet != null)
        {
            if (dataSet[strkey] != null)
            {
                dataSet[strkey] = bValue.ToString();
            }
            else
            {
                dataSet.Add(strkey, bValue.ToString());
            }
        }
    }

    static public void SetDataInt(JSONNode dataSet, Enum keyData, int nValue)
    {
        SetDataInt(dataSet, keyData.ToString("F"), nValue);
    }

    static public void SetDataInt(JSONNode dataSet, string strkey, int nValue)
    {
        if (dataSet != null)
        {
            if (dataSet[strkey] != null)
            {
                dataSet[strkey] = nValue.ToString();
            }
            else
            {
                dataSet.Add(strkey, nValue.ToString());
            }
        }
    }

    static public void SetDataFloat(JSONNode dataSet, string strkey, float fValue)
    {
        if (dataSet != null)
        {
            if (dataSet[strkey] != null)
            {
                dataSet[strkey] = fValue.ToString();
            }
            else
            {
                dataSet.Add(strkey, fValue.ToString());
            }
        }
    }

    static public int GetDataInt(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                return dataSet[strKey].AsInt;
            }
        }

        return 0;
    }

    static public int GetDataInt(JSONNode dataSet, Enum keyData)
    {
        return GetDataInt(dataSet, keyData.ToString("F"));
    }

    static public long GetDataLong(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                return dataSet[strKey].AsLong;
            }
        }

        return 0;
    }

    static public double GetDataDouble(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                return dataSet[strKey].AsDouble;
            }
        }

        return 0;
    }

    //public int GetDataInt( int nIndex , string strKey )
    //{
    //    if (m_RootNode == null)
    //        return 0;

    //    foreach (JSONNode nodeData in m_RootNode.Children)
    //    {
    //        JSONNode nodeIndex = nodeData[nIndex];
    //        if (nodeIndex != null)
    //        {
    //            if (nodeIndex[strKey] != null)
    //            {
    //                return nodeIndex[strKey].AsInt;
    //            }
    //        }
    //    }


    //    return 0;
    //}


    static public Vector3 GetDataVector3(JSONNode dataSet, string strKey)
    {
        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                SimpleJSON.JSONVector3 tempVector3 = dataSet[strKey].AsVector3;
                return new Vector3(tempVector3.x, tempVector3.y, tempVector3.z);
            }
        }

        return Vector3.zero;
    }
    //public Vector3 GetDataVector3(int nIndex, string strKey)
    //{
    //    if (m_RootNode == null)
    //        return Vector3.zero;

    //    foreach (JSONNode nodeData in m_RootNode.Children)
    //    {
    //        JSONNode nodeIndex = nodeData[nIndex];
    //        if (nodeIndex != null)
    //        {
    //            if (nodeIndex[strKey] != null)
    //            {
    //                SimpleJSON.JSONVector3 tempVector3 = nodeIndex[strKey].AsVector3;
    //                return new Vector3(tempVector3.x, tempVector3.y, tempVector3.z);
    //            }
    //        }
    //    }


    //    return Vector3.zero;
    //}

    static public string GetDataString(JSONNode dataSet, Enum keyData)
    {
        return GetDataString(dataSet, keyData.ToString("F"));
    }

    //static public string GetDataString(JSONObject dataSet, string strKey )
    //{
    //    string strData = string.Empty;

    //    if (dataSet != null)
    //    {
    //        if (dataSet[strKey] != null)
    //        {
    //            strData = dataSet[strKey];
    //        }
    //    }

    //    return strData;
    //}


    static public string GetDataString(JSONNode dataSet, string strKey)
    {
        string strData = string.Empty;

        if (dataSet != null)
        {
            if (dataSet[strKey] != null)
            {
                strData = dataSet[strKey];
            }
        }

        return strData;
    }

    public static void JSONToListString(JSONArray arrData, List<string> outList)
    {
        if (arrData != null && outList != null)
        {
            for (int i = 0; i < arrData.Count; ++i)
            {
                outList.Add(arrData[i]);
            }
        }
    }

    public static JSONArray ListToJSON(List<int> listData)
    {
        JSONArray arr = JSONDataStruct.MakeArray();
        if (listData == null)
            return arr;

        for (int i = 0; i < listData.Count; ++i)
        {
            arr.Add(listData[i]);
        }

        return arr;
    }

    public static JSONArray ListToJSON(List<string> listData)
    {
        JSONArray arr = JSONDataStruct.MakeArray();
        if (listData == null)
            return arr;

        for (int i = 0; i < listData.Count; ++i)
        {
            arr.Add(listData[i]);
        }

        return arr;
    }

    public static JSONArray ArrayToJSON(int[] arrData)
    {
        JSONArray arr = JSONDataStruct.MakeArray();
        if (arrData == null)
            return arr;

        for (int i = 0; i < arrData.Length; ++i)
        {
            arr.Add(arrData[i]);
        }

        return arr;
    }

    public static JSONArray ArrayToJSON(string[] arrData)
    {
        JSONArray arr = JSONDataStruct.MakeArray();
        if (arrData == null)
            return arr;

        for (int i = 0; i < arrData.Length; ++i)
        {
            arr.Add(arrData[i]);
        }

        return arr;
    }

    public static JSONObject DicToJSON(Dictionary<int, string> dicData)
    {
        JSONObject dic = JSONDataStruct.MakeDictionary();
        if (dicData == null)
            return dic;

        var enumerator = dicData.GetEnumerator();

        while (enumerator.MoveNext())
        {
            dic.Add(enumerator.Current.Key.ToString(), enumerator.Current.Value);
        }

        return dic;
    }

    public static JSONObject DicToJSON(Dictionary<int, float> dicData)
    {
        JSONObject dic = JSONDataStruct.MakeDictionary();
        if (dicData == null)
            return dic;

        var enumerator = dicData.GetEnumerator();

        while (enumerator.MoveNext())
        {
            dic.Add(enumerator.Current.Key.ToString(), enumerator.Current.Value);
        }

        return dic;
    }

    public static JSONObject DicToJSON(Dictionary<int, int> dicData)
    {
        JSONObject dic = JSONDataStruct.MakeDictionary();
        if (dicData == null)
            return dic;

        var enumerator = dicData.GetEnumerator();

        while (enumerator.MoveNext())
        {
            dic.Add(enumerator.Current.Key.ToString(), enumerator.Current.Value);
        }

        return dic;
    }

    public static JSONObject DicToJSON(Dictionary<string, int> dicData)
    {
        JSONObject dic = JSONDataStruct.MakeDictionary();
        if (dicData == null)
            return dic;

        var enumerator = dicData.GetEnumerator();

        while (enumerator.MoveNext())
        {
            dic.Add(enumerator.Current.Key.ToString(), enumerator.Current.Value);
        }

        return dic;
    }

    //public string GetDataString( int nIndex , string strKey )
    //{
    //    string strData = string.Empty;
    //    if (m_RootNode != null)
    //    {

    //        foreach (JSONNode nodeData in m_RootNode.Children)
    //        {
    //            JSONNode nodeIndex = nodeData[nIndex];
    //            if (nodeIndex != null)
    //            {
    //                if (nodeIndex[strKey] != null)
    //                {
    //                    strData = nodeIndex[strKey];
    //                    break;
    //                }
    //            }
    //        }


    //    }

    //    return strData;
    //}

    public JSONNode GetDataSet(int nIndex, bool bMake = false)
    {
        if (m_RootNode == null)
            return null;

        foreach (JSONNode nodeData in m_RootNode.Children)
        {
            JSONObject data = nodeData[nIndex.ToString()] as JSONObject;
            if (data != null)
                return data;
            else if (bMake)
            {
                JSONObject classData = JSONDataStruct.MakeDictionary();
                nodeData[nIndex.ToString()] = classData;

                return classData;
            }
        }

        return null;
    }

    public JSONNode GetDataSet(string strKey, bool bMake = false)
    {
        if (m_RootNode == null)
            return null;

        foreach (JSONNode nodeData in m_RootNode.Children)
        {
            JSONObject data = nodeData[strKey] as JSONObject;
            if (data != null)
                return data;
            else if (bMake)
            {
                JSONObject classData = JSONDataStruct.MakeDictionary();
                nodeData[strKey] = classData;

                return classData;
            }
        }

        return null;
    }

    public JSONNode AddDataSet(int nIndex)
    {
        if (m_RootNode == null)
        {
            return null;
        }

        JSONNode newData = JSONDataStruct.MakeDictionary();

        foreach (JSONNode nodeData in m_RootNode.Children)
        {
            nodeData.Add(nIndex.ToString(), newData);
            break;
        }

        return GetDataSet(nIndex);
    }


    public JSONNode AddDataSet(string strKey)
    {
        if (m_RootNode == null)
        {
            return null;
        }

        JSONNode newData = JSONDataStruct.MakeDictionary();

        foreach (JSONNode nodeData in m_RootNode.Children)
        {
            nodeData.Add(strKey, newData);
            break;
        }

        return GetDataSet(strKey);
    }



    public void AddData(string strKey, string strData)
    {
        if (m_RootNode == null)
        {
            m_RootNode = JSONDataStruct.MakeDictionary();
        }
        m_RootNode.Add(strKey, strData);
    }

    public void AddData(string strKey, JSONNode stNode)
    {
        if (m_RootNode == null)
        {
            m_RootNode = JSONDataStruct.MakeDictionary();
        }

        m_RootNode.Add(strKey, stNode);
    }

    public void AddData(string strKey, JSONArray stNode)
    {
        if (m_RootNode == null)
        {
            m_RootNode = JSONDataStruct.MakeDictionary();
        }

        m_RootNode.Add(strKey, stNode);
    }


    public static JSONObject MakeDictionary()
    {
        //ContainerObject<JSONObject> conJSONObject = RecycledObjectPool.GetContainer<JSONObject>();
        //JSONObject dic = conJSONObject;
        //ContainerObject<JSONObject> container = conJSONObject;
        //dic.CONTAINER = container;

        //return dic;

        return new JSONObject();
    }

    public static JSONArray MakeArray()
    {
        //ContainerObject<JSONArray> conJSONArray = RecycledObjectPool.GetContainer<JSONArray>();
        //JSONArray arr = conJSONArray;
        //ContainerObject<JSONArray> container = conJSONArray;
        //arr.CONTAINER = container;

        //return arr;
        return new JSONArray();
    }


}




