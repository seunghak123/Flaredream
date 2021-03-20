using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class EnvVariable
{
    const string strEnvPropertiesPath = "EnvVariable.buildsetting";
    const string strTargetBuildingAssets = "BuildAssetNames.buildsetting";

	static IDictionary m_EnvDict = null;

	static IDictionary EnvDict
	{
		get
		{
            if (m_EnvDict == null)
                GetVariablesFromProperties();
			return m_EnvDict;
		}
	}

    public static List<string> GetTargetAssets()
    {
        List<string> listTargetAssets = new List<string>();


        if (File.Exists(strTargetBuildingAssets) == false)
        {
            Debug.Log("No env properties file");
        }
        else
        {
            try
            {
                using (StreamReader tr = new StreamReader(strTargetBuildingAssets))
                {
                    while (tr.Peek() >= 0)
                    {
                        string preParseLine = tr.ReadLine();

                        Debug.Log("Load environment variables Line string : " + preParseLine);

                        if (preParseLine.StartsWith(@"//") == true)
                            continue;

                        listTargetAssets.Add(preParseLine);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("The process failed 1: {0}", e.ToString()));
            }
        }

        return listTargetAssets;
    }

    static void GetVariablesFromProperties()
    {
        Debug.Log("Load environment variables...");

        if (m_EnvDict == null)
            m_EnvDict = System.Environment.GetEnvironmentVariables();

        if (File.Exists(strEnvPropertiesPath) == false)
        {
            Debug.Log("No env properties file");
        }
        else
        {
            try
            {
                using (StreamReader tr = new StreamReader(strEnvPropertiesPath))
                {
                    while (tr.Peek() >= 0)
                    {
                        string preParseLine = tr.ReadLine();

                        //Debug.Log("Load environment variables Line string : " + preParseLine);

                        string[] parsedTokens = preParseLine.Split('=');
                        if (parsedTokens == null || parsedTokens.Length < 2)
                            continue;

                        if (parsedTokens[0].StartsWith(@"//") == true)
                            continue;
                        m_EnvDict[parsedTokens[0]] = parsedTokens[1];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("The process failed 1: {0}", e.ToString()));
            }
        }

        //try
        //{
        //    ICollection keys = m_EnvDict.Keys;
            
        //    foreach(var col in keys)
        //    {
        //        Debug.Log(string.Format("EnvKey : {0}, Value : {1}", col.ToString(), m_EnvDict[col].ToString()));
        //    }
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError(string.Format("The process failed 2: {0}", e.ToString()));
        //}
    }

	static object GetEnvObj(object envKey)
	{
		if (EnvDict == null)
			return null;
		
		if (EnvDict.Contains(envKey) == true)
			return EnvDict[envKey];

		return null;
	}

	public static string GetEnvStr(string strEnvKey)
	{
		object envObj = GetEnvObj(strEnvKey);

		if (envObj == null)
			return string.Empty;

		return envObj.ToString();
	}
}

