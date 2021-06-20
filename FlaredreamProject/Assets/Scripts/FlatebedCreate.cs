using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatebedCreate : MonoBehaviour
{
    public static FlatebedCreate Instance = null;
    [SerializeField]
    FlatebedPool flatpool;
    E_FlatDirect m_RecentDirect = E_FlatDirect.e_Forward;
    [SerializeField]
    GameObject m_startpos;
    [SerializeField]
    List<GameObject> m_FlatObjects;
    int startcount = 5;
    public void Awake()
    {
        if (Instance != null)
            Destroy(this.gameObject);
        
        Instance = this;
        MakesFlats();
        DontDestroyOnLoad(this.gameObject);
    }
    public void MakesFlats()
    {
        if (m_startpos != null)
        {
            for(int i = 0; i < startcount; i++)
            {
                MakeFlat(m_startpos.transform.position + new Vector3(0, 0, 5 * i), E_FlatDirect.e_Forward);
            }
        }
    }
    public void RestartFlatebed()
    {
        m_RecentDirect = E_FlatDirect.e_Forward;
        flatpool.ClearPool();
    } 
    public void MakeFlat(Vector3 passedObject, E_FlatDirect flat_type)
    {

    }


}
