using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAI : BaseAI
{
    protected bool m_PauseUpdate = false;
    public void Update()
    {
        RunningAI();
    }
    public void RunningAI()
    {
        ProcessMove();
    }
  
    protected void ProcessMove()
    {
        if(movedirect!= Vector3.zero)
        {
            this.transform.Translate(movedirect * Time.deltaTime);
            //StopDestination(false);
        }
        else
        {

        }
    }
}
