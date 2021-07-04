using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_FlatDirect
{
    e_Left,
    e_Right,
    e_Back,
    e_Forward,
}
public class Flatebed : MonoBehaviour
{
    bool movenext = false;
    public bool Alive = false;
    public E_FlatDirect flatdirect = E_FlatDirect.e_Forward;
    public float distance = 5.0f;
    public void ClearFlatbed()
    {
        this.gameObject.transform.position = Vector3.zero;
        this.gameObject.SetActive(false);
        Alive = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!movenext && collision.gameObject.tag.Equals("Player"))
        {
            movenext = true;
            FlatebedCreate.Instance.MakeFlat(this.transform.position, E_FlatDirect.e_Forward);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            //프리팹 삭제
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!movenext&&other.gameObject.tag.Equals("Player"))
        {
            movenext = true;
            FlatebedCreate.Instance.MakeFlat(this.transform.position,E_FlatDirect.e_Forward);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!movenext)
        {
            
        }
    }
}
