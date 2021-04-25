using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum e_State {None, Idle, Walk, Talk, InterAct,Max }
public enum e_StatType
{
    e_Speed,

}

public struct stNextAI
{
    public e_State nextStatus;
    public stGetCustomData stSetAiData;
}
public class BaseAI : BaseObject
{
    //Dictionary<string,>
    private Animator Animation;//현재 애니메이션
    private bool isPause = true;
    private string ControlId;
    [Range(1, 2)]
    public float runvalue = 1.5f;
    [Range(40, 150)]
    public float vspeed;
    public float accelator = 0.0f;
    public float SightY;
    [Range(2, 6)]
    public float movespeed;
    public GameObject Body;
    public GameObject Head;
    public Vector3 movedirect = Vector3.zero;
    [Range(0, 1)]
    public float SlideRange;

    public delegate Coroutine UpdateFunction();
    UpdateFunction[] m_arrUpdateFunction = new UpdateFunction[(int)e_State.Max];
    protected void Awake()
    {
        m_arrUpdateFunction[(int)e_State.Idle] = _Idle;
        m_arrUpdateFunction[(int)e_State.Walk] = _Walk;
        m_arrUpdateFunction[(int)e_State.Talk] = _Talk;
        m_arrUpdateFunction[(int)e_State.InterAct] = _Interact;
        InitAI();
    }
    public virtual void InitAI()
    {

    }
    Coroutine _Idle()
    {
        return StartCoroutine(Idle());
    }
    Coroutine _Walk()
    {
        return StartCoroutine(Walk());
    }
    Coroutine _Talk()
    {
        return StartCoroutine(Talk());
    }
    Coroutine _Interact()
    {
        return StartCoroutine(Interact());

    }

    protected IEnumerator Idle()
    {
        yield return null;
    }

    protected IEnumerator Walk()
    {
        yield return null;
    }
    protected IEnumerator Talk()
    {
        yield return null;
    }
    protected IEnumerator Interact()
    {
        yield return null;
    }
    public void AnimationChange()
    {

    }

    public void NextAI(e_State state, int nTempData1 = 0)
    {
        switch (state)
        {
            case e_State.None:
                break;
            case e_State.Idle:
                break;
            case e_State.Walk:
                break;
            case e_State.InterAct:
                break;
            default:
                break;
        }
         m_arrUpdateFunction[(int)state]();
    }

    protected virtual void ProcessMove()
    {

    }
    virtual protected void ProcessDie()
    {
        //MoveDestination(m_baseObject.SelfObject.transform.position);
        //StopDestination(false);
        //m_fElapsedTime = 0;
        //m_DiePosition = SelfTransform.position + (SelfTransform.forward * -3.0f);

        //// 죽을때 예약됫던 스킬 취소.
        //if (m_nReservationSkill != 0 || m_bSkillID != false)
        //{
        //    m_bSkillID = false;
        //    m_nReservationSkill = 0;
        //}
        //BaseObject.ThrowObject(Parent, eThrowEventType.E_THROW_EVENT_TYPE_CHARACTER_DIE);

        //ChangeStatus(eAIStatus.E_AI_STATUS_DIE);
        //ChangeAniState(eAIStatus.E_AI_STATUS_DIE);
    }
    protected virtual void ProcessInterAct()
    {

    }

    //protected Vector3 MoveDestination(Vector3 position, float fSpeed = 0)
    //{
    //    if (m_objectState == eBaseObjectState.E_OBJECT_STATE_DEACTIVE)
    //        return SelfTransform.position;

    //    if (Parent == null)
    //        return SelfTransform.position;

    //    if (fSpeed > 0)
    //    {
    //        MOVE_COMPONENT.SetSpeed(fSpeed * HavanaGlobal.GAME_SPEED_WEIGHT);
    //    }
    //    else
    //    {
    //        fSpeed = Parent.GetCustomDataFloat(eCustomDataType.E_CUSTOM_DATA_TYPE_RUN_SPEED);
    //        MOVE_COMPONENT.SetSpeed(fSpeed * HavanaGlobal.GAME_SPEED_WEIGHT);
    //    }

    //    UnityEngine.AI.NavMeshHit navHit;
    //    if (UnityEngine.AI.NavMesh.SamplePosition(position, out navHit, 5, UnityEngine.AI.NavMesh.AllAreas))
    //    {
    //        position = navHit.position;
    //    }

    //    m_prevMovePosition = position;
    //    MOVE_COMPONENT.MoveTo(position);

    //    return position;
    //}

    //virtual public bool StopDestination(bool bNextAICheck = true)
    //{
    //    if (LockMove)
    //        return false;

    //    if (m_objectState == eBaseObjectState.E_OBJECT_STATE_DEACTIVE)
    //        return false;

    //    if (Parent == null)
    //        return false;

    //    if (MOVE_COMPONENT)
    //        MOVE_COMPONENT.Stop();

    //    if (m_autoType != eAIAutoType.E_AI_AUTO_ENABLE_JOYSTICK)
    //        ClearNextAI(eAIStatus.E_AI_STATUS_MOVE);

    //    m_movePosition = Vector3.zero;
    //    m_prevMovePosition = Vector3.zero;

    //    if (bNextAICheck == true)
    //    {
    //        if (m_listNextAI.Count == 0 && CurrentStatus == eAIStatus.E_AI_STATUS_MOVE)
    //        {
    //            AddNextAI(eAIStatus.E_AI_STATUS_IDLE);
    //        }
    //    }


    //    return true;
    //}
}
