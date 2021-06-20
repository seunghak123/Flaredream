using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OguAI : MonoBehaviour
{
    [SerializeField]
    float Speed = 3.0f;
    [SerializeField]
    float Jumpheight = 4.0f;
    void Update()
    {
        
    }
    public void ProcessMove()
    {
        transform.Translate(transform.forward*Time.deltaTime);
    }

    public void ProcessDead()
    {

    }

    public void ProcessEmotion()
    {

    }
    public void ProcessIdle()
    {

    }
}
