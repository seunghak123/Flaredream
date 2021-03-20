using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

public struct stAssetInfo
{
	public string Name;

	public uint CRC;
	public uint Version; // AssetVersion
	public double FileSize; // byte
    public string URL;

    public stAssetInfo(string nName, uint nCRC, uint nVersion, double nFileSize, string strURL)
	{
		Name = nName;
		CRC = nCRC;
		Version = nVersion;
		FileSize = nFileSize;
        URL = strURL;
    }
}

public class VersionInfo
{
	//public string Major = string.Empty;
	//public string Minor = string.Empty;
	//public string Revision = string.Empty;

	public uint AssetCRC = 0;
	public uint AssetVersion = 0;

	public Dictionary<string, stAssetInfo> AssetDict;

	//public void SetVersion(string versionCode)
	//{
	//	if (string.IsNullOrEmpty(versionCode) == true)
	//		return;

	//	string[] splitVer = versionCode.Split('.', '_');

	//	if (splitVer.Length > 0)
	//		Major = splitVer[0];
	//	if (splitVer.Length > 1)
	//		Minor = splitVer[1];
	//	if (splitVer.Length > 2)
	//		Revision = splitVer[2];

	//	Debug.Log(GetVersionString(Major, Minor, Revision));
	//}

	//public static string GetVersionString(string major, string minor, string revision)
	//{
	//	string strMajor = (string.IsNullOrEmpty(major)) ? string.Empty : major;
	//	string strMinor = (string.IsNullOrEmpty(minor)) ? string.Empty : minor;
	//	string strRevision = (string.IsNullOrEmpty(revision)) ? string.Empty : revision;
	//	return strMajor + "_" + strMinor + "_" + strRevision;
	//}

	public void Init()
	{
		//Major = string.Empty;
		//Minor = string.Empty;
		//Revision = string.Empty;
		AssetDict = new Dictionary<string, stAssetInfo>();
		AssetVersion = 0;
	}

	//XmlNode MakeApkNode(XmlDocument xmlDoc)
	//{
	//	if (xmlDoc == null)
	//		return null;

	//	XmlNode majorNode = xmlDoc.CreateElement("MAJOR");
	//	XmlNode minorNode = xmlDoc.CreateElement("MINOR");
	//	XmlNode revisionNode = xmlDoc.CreateElement("REVISION");

	//	XmlAttribute majorValue = xmlDoc.CreateAttribute("value");
	//	XmlAttribute minorValue = xmlDoc.CreateAttribute("value");
	//	XmlAttribute revisionValue = xmlDoc.CreateAttribute("value");

	//	majorValue.Value = Major;
	//	minorValue.Value = Minor;
	//	revisionValue.Value = Revision;

	//	majorNode.Attributes.Append(majorValue);
	//	minorNode.Attributes.Append(minorValue);
	//	revisionNode.Attributes.Append(revisionValue);

	//	XmlNode versionNode = xmlDoc.CreateElement("LastApkVersion");
	//	versionNode.AppendChild(majorNode);
	//	versionNode.AppendChild(minorNode);
	//	versionNode.AppendChild(revisionNode);

	//	return versionNode;
	//}
	
	XmlNode MakeAssetNode(XmlDocument xmlDoc, stAssetInfo assetInfo)
	{
		if (xmlDoc == null)
			return null;

		XmlNode assetNode = xmlDoc.CreateElement(assetInfo.Name);

		XmlAttribute crcAttr = xmlDoc.CreateAttribute("CRC");
		crcAttr.Value = assetInfo.CRC.ToString();

		XmlAttribute verAttr = xmlDoc.CreateAttribute("VERSION");
		verAttr.Value = assetInfo.Version.ToString();

		XmlAttribute sizeAttr = xmlDoc.CreateAttribute("FILESIZE");
		sizeAttr.Value = assetInfo.FileSize.ToString();

		assetNode.Attributes.Append(crcAttr);
		assetNode.Attributes.Append(verAttr);
		assetNode.Attributes.Append(sizeAttr);

		return assetNode;
	}

	public XmlDocument ToXML()
	{
		XmlDocument xmlDoc = new XmlDocument();
		XmlNode root = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
		xmlDoc.AppendChild(root);

		//string versionStr = GetVersionString(Major, Minor, Revision);
		XmlNode verNode = xmlDoc.CreateElement("VERSION_INFO");
		xmlDoc.AppendChild(verNode);

		//XmlNode apkNode = verNode.AppendChild(MakeApkNode(xmlDoc));

		XmlNode assetNode = xmlDoc.CreateElement("AssetVersion");
		{

			XmlAttribute assetCRCAttr = xmlDoc.CreateAttribute("CRC");
			assetCRCAttr.Value = AssetCRC.ToString();
			assetNode.Attributes.Append(assetCRCAttr);
		}
		{
			XmlAttribute assetVerAttr = xmlDoc.CreateAttribute("VERSION");
			assetVerAttr.Value = AssetVersion.ToString();
			assetNode.Attributes.Append(assetVerAttr);
		}

		foreach (var kvPair in AssetDict)
		{
			XmlNode tempNode = MakeAssetNode(xmlDoc, kvPair.Value);
			assetNode.AppendChild(tempNode);
		}
		
		verNode.AppendChild(assetNode);

		return xmlDoc;
	}

	public void ParseXML(XmlDocument xmlDoc, string strURL = "")
	{
		Init();

		if (xmlDoc == null)
			return;

		XmlElement verElem = xmlDoc["VERSION_INFO"];
		if (verElem == null)
			return;

		XmlElement assetListElem = verElem["AssetVersion"];
		if (assetListElem != null)
		{
			string strAssetVer = assetListElem.GetAttribute("VERSION");
			uint.TryParse(strAssetVer, out AssetVersion);

			string strAssetCRC = assetListElem.GetAttribute("CRC");
			uint.TryParse(strAssetCRC, out AssetCRC);

			XmlNodeList assetList = assetListElem.ChildNodes;

			if (assetList != null)
			{
				for (int i = 0; i < assetList.Count; ++i)
				{
					stAssetInfo assetInfo = new stAssetInfo();
					assetInfo.Name = assetList[i].Name;
                    assetInfo.URL = strURL;

                    XmlAttributeCollection attributeList = assetList[i].Attributes;
					if (attributeList != null)
					{
						if (attributeList["CRC"] != null)
							uint.TryParse(attributeList["CRC"].Value, out assetInfo.CRC);
						if (attributeList["VERSION"] != null)
							uint.TryParse(attributeList["VERSION"].Value, out assetInfo.Version);
						if (attributeList["FILESIZE"] != null)
							double.TryParse(attributeList["FILESIZE"].Value, out assetInfo.FileSize);
						//assetInfo.Version = attributeList["VERSION"].Value;
					}

					AssetDict[assetInfo.Name] = assetInfo;
				}
			}
		}
	}

	public VersionInfo()
	{
		Init();
	}

	public VersionInfo(XmlDocument xmlDoc)
	{
		ParseXML(xmlDoc);
	}
}

public static class VersionXML
{
	public const string VersionFileName = "patch.xml";
	public const string MajorVerKey = "VER_MAJOR";
	public const string MinorVerKey = "VER_MINOR";
	public const string RevisionKey = "VER_REVISION";
	public const string AssetVerKey = "VER_ASSET";

	static XmlDocument LoadedVerDoc = null;
	public static VersionInfo CachedVerInfo = null;

	static VersionInfo LoadVersionFile(string srcPath)
	{
		string targetPath = GetVersionFilePath(srcPath);

		LoadedVerDoc = XmlTool.loadXml(targetPath);

		if (LoadedVerDoc == null)
		{
			return null;
		}

		CachedVerInfo = new VersionInfo(LoadedVerDoc);

		return CachedVerInfo;
	}

	static string GetVersionFilePath(string srcPath)
	{
		if (string.IsNullOrEmpty(srcPath) == true)
			return VersionFileName;

		if (srcPath.EndsWith("/") == true)
			srcPath.TrimEnd('/');

		return srcPath + "/" + VersionFileName;
	}

#if UNITY_EDITOR
	public static XmlDocument MakeVersionFile(uint totalCRC, string srcPath,  List<stAssetInfo> listAssetNotVersioned = null, bool isFirst = false)
	{
		string targetPath = GetVersionFilePath(srcPath);

		if (isFirst == false)
		{
			LoadVersionFile(srcPath);
			if (CachedVerInfo == null)
			{
				Debug.Log("No Cached File, make new ..");
				LoadedVerDoc = MakeVersionFile(0, srcPath, null, true); // ���� ����X�� �⺻��
																		//CachedVerInfo = new VersionInfo(LoadedVerDoc);
			}
		}
		else
		{
			CachedVerInfo = new VersionInfo();
		}
		
		//string strMajor = EnvVariable.GetEnvStr(MajorVerKey);
		//string strMinor = EnvVariable.GetEnvStr(MinorVerKey);
		//string strRevision = EnvVariable.GetEnvStr(RevisionKey);
		//string strAssetVer = EnvVariable.GetEnvStr(AssetVerKey);

		uint uAssetVer = 0;
		uAssetVer = CachedVerInfo.AssetVersion;

		//uint.TryParse(strAssetVer, out uAssetVer);

		VersionInfo newVerInfo = new VersionInfo();
		{
			newVerInfo.Init();
			//newVerInfo.Major = strMajor;
			//newVerInfo.Minor = strMinor;
			//newVerInfo.Revision = strRevision;
			newVerInfo.AssetVersion = uAssetVer;
			newVerInfo.AssetCRC = totalCRC;
			
			//newVerInfo.SetVersion(UnityEditor.PlayerSettings.bundleVersion);
			Debug.Log("new version is now " + UnityEditor.PlayerSettings.bundleVersion);

			if (newVerInfo.AssetCRC == CachedVerInfo.AssetCRC) // CRC ������ ���� �ø��� �ʴ´�
			{
				newVerInfo.AssetDict = CachedVerInfo.AssetDict;
			}
			else
			{																		// �Ȱ����� �־��� �����ͷ� ���� �ø���
				if (listAssetNotVersioned != null)
				{
					newVerInfo.AssetVersion++;

					for (int i = 0; i < listAssetNotVersioned.Count; ++i)
					{
						stAssetInfo tempAssetInfo = listAssetNotVersioned[i];           // �� ���� ��� ������ ���� ���� ����
						tempAssetInfo.Version = newVerInfo.AssetVersion;

						if (CachedVerInfo.AssetDict.ContainsKey(tempAssetInfo.Name))        // �ڽ��� �����ִ� ����߿� ���� ����� ���� ��
						{
							stAssetInfo cachedAssetInfo = CachedVerInfo.AssetDict[tempAssetInfo.Name];

							if (tempAssetInfo.CRC == cachedAssetInfo.CRC)               // �� ��ü�� CRC ���� ������ ������ �ٲ��� �ʴ´�
								tempAssetInfo.Version = cachedAssetInfo.Version;
						}

						newVerInfo.AssetDict[tempAssetInfo.Name] = tempAssetInfo;
					}
				}
			}
		}

		XmlDocument newDocu = newVerInfo.ToXML();
		CachedVerInfo = newVerInfo;

		if (File.Exists(targetPath) == true)			// ������ ���� ����
			File.Delete(targetPath);

		XmlTool.writeXml(targetPath, newDocu);          // ���� ����

        Debug.LogError("Save in MakeVersionFile");

        return newDocu;
	}

	
#endif
		}

