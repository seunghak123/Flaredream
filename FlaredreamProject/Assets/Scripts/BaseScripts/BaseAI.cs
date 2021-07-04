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
    public Vector3 movedirect = Vector3.forward;
    [Range(0, 1)]
    public float SlideRange;

    public delegate Coroutine UpdateFunction();
}
