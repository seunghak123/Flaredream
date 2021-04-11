using UnityEngine;
using System.Collections;


public enum eLoadDataPriority
{
	HIGH,
	NORMAL,
	LOW,

	NONE,
}

public class ManagerData : BaseObject
{

	int m_nLoadCount = 0;

	public int LOAD_COUNT
	{
		get { return m_nLoadCount; }
		set { m_nLoadCount = value; }
	}

	public void LoadDataFile(string strFileName, DataFileLoaderFunc completeFunc, eLoadDataPriority loadDataPriority = eLoadDataPriority.NORMAL)
	{
		ManagerManager.LoadDataFile(loadDataPriority, strFileName, completeFunc);
	}
}
