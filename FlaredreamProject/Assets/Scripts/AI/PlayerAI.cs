using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerAI : BaseAI
{
    protected bool m_PauseUpdate = false;
    [SerializeField]
    NavMeshAgent m_navigate = null;
    public void Update()
    {
        Inputdirect = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
    }
    public override void InitAI()
    {
        StartCoroutine(InitSuccess());
    }
    private Vector3 Inputdirect = Vector3.zero;
    IEnumerator InitSuccess()
    {
        float fPrevTime = Time.time;
        float fPrevUnscaleTime = Time.unscaledTime;
        float fCurrentTime = 0;
        float fCurrentUnscaleTime = 0;
        while (true)
        {
            fCurrentTime = Time.time;
            fCurrentUnscaleTime = Time.unscaledTime;

            float deltaTime = fCurrentTime - fPrevTime;
            float unscaleDeltaTime = fCurrentUnscaleTime - fPrevUnscaleTime;

            fPrevTime = fCurrentTime;
            fPrevUnscaleTime = fCurrentUnscaleTime;

            while (m_PauseUpdate == true)
            {
                yield return null;
            }


            yield return new WaitForSeconds(0.05f);
        }
    }
    protected override void ProcessMove()
    {
        if(movedirect!= Vector3.zero)
        {
            Vector3 m_movePosition = SelfTransform.position + (Inputdirect * Time.deltaTime);
            m_navigate.Move(m_movePosition);//MoveDestination(m_movePosition);
            m_navigate.isStopped = false;
            //StopDestination(false);
        }
        else
        {
            NextAI(e_State.Idle);
        }
    }
    protected override void ProcessDie()
    {

    }
    protected override void ProcessInterAct()
    {

    }
}
