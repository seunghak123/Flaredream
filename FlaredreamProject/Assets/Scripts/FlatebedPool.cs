using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class FlateMake
{
    [SerializeField]
    public Flatebed instaiatebed;
    [SerializeField]
    public int count;
}
public class FlatebedPool : MonoBehaviour
{
    [SerializeField]
    List<FlateMake> FlatPrefabLists = new List<FlateMake>();

    List<Flatebed> FlatLists = new List<Flatebed>();

    public void ClearPool()
    {
        foreach(Flatebed flat in FlatLists)
        {
            flat.ClearFlatbed();
            //FlatLists.Remove(flat);
            //GameObject.Destroy(flat);
        }
    }
    public void Start()
    {
        
    }
    public void Awake()
    {
        InitPool();
    }
    public void InitPool()
    {
        foreach (FlateMake flat in FlatPrefabLists)
        {
            for(int i=0;i<flat.count;i++)
                Instantiate(flat.instaiatebed, this.transform);
        }
    }
    public void MakePoolObject(E_FlatDirect type)
    {

    }
    public Flatebed GetNextFlat(E_FlatDirect type)
    {
        foreach (Flatebed flat in FlatLists)
        {
            if (flat.flatdirect.Equals(type))
            {

            }
        }
        //만들어주기
        return null;
    }
    public void InsertPool(Flatebed newflat)
    {
        if (newflat == null)
            return;

        FlatLists.Add(newflat);
    }
}
