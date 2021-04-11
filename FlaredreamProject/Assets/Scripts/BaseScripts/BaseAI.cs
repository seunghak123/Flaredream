using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State {None, Idle, taurt, Move, Smile, Thank, Fighting }
public enum e_StatType
{
    e_Speed,

}
public class BaseAI : CacheObject
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
    public Vector3 movedirect;
    [Range(0, 1)]
    public float SlideRange;


}
