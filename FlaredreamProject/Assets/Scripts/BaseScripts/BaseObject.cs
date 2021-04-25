using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using SimpleJSON;


public struct stGetCustomData
{
    public int Temp_Int1;
    public int Temp_Int2;
    public int Temp_Int3;

    public float Temp_Float1;
    public float Temp_Float2;
    public float Temp_Float3;

    public bool Temp_Bool1;
    public bool Temp_Bool2;
    public bool Temp_Bool3;

    public string Temp_string1;
    public string Temp_string2;
    public string Temp_string3;

    public object Temp_Object1;
    public object Temp_Object2;
    public object Temp_Object3;
    public object Temp_Object4;

    public void Clear()
    {
        Temp_Int1 = 0;
        Temp_Int2 = 0;
        Temp_Int3 = 0;

        Temp_Float1 = 0.0f;
        Temp_Float2 = 0.0f;
        Temp_Float3 = 0.0f;

        Temp_Bool1 = false;
        Temp_Bool2 = false;
        Temp_Bool3 = false;

        Temp_string1 = string.Empty;
        Temp_string2 = string.Empty;
        Temp_string3 = string.Empty;

        Temp_Object1 = null;
        Temp_Object2 = null;
        Temp_Object3 = null;
        Temp_Object4 = null;
    }

}
public class BaseObject : CacheObject
{
    protected eBaseObjectState m_objectState = eBaseObjectState.E_OBJECT_STATE_ACTIVE;
    protected BaseObject m_Parent = null;

    private string m_UUID = string.Empty; 
    private Transform m_animationTransform = null;

    Dictionary<string, object> m_dicUserData = new Dictionary<string, object>();

    public Vector3 AnimationPos { get; set; }


    public eBaseObjectState ObjectState
    {
        get { return m_objectState; }
        set { m_objectState = value; }
    }
    public BaseObject Parent { get { return m_Parent; } set { m_Parent = value; } }

    [SerializeField]
    eFrozenMode m_isFrozen = eFrozenMode.e_FREE;
    public eFrozenMode isFrozen
    {
        get
        {
            return m_isFrozen;
        }

        set
        {
            m_isFrozen = value;
        }
    }

    public T GetParent<T>() where T : BaseObject
    {
        return Parent as T;
    }

    virtual public string UUID
    {
        get
        {
            return m_UUID;
        }
        set
        {
            m_UUID = value;
        }
    }

    override public GameObject SelfObject
    {
        get
        {
            if (m_CacheGameObject == null)
            {
                if (Parent == null)
                {
                    if (this != null)
                        m_CacheGameObject = gameObject;
                    else
                        m_CacheGameObject = null;
                }

                if (m_CacheGameObject == null && Parent != null)
                {
                    m_CacheGameObject = Parent.SelfObject;
                }
            }

            return m_CacheGameObject;
        }
    }
    override public Transform SelfTransform
    {
        get
        {
            if (m_CacheTransform == null)
            {
                if (Parent == null)
                {
                    if (this != null)
                        m_CacheTransform = transform;
                    else
                        m_CacheTransform = null;
                }

                if (m_CacheTransform == null && Parent != null)
                {
                    m_CacheTransform = Parent.SelfTransform;
                }
            }

            return m_CacheTransform;
        }
    }

    public Transform AnimationTransform
    {
        get
        {
            if (m_animationTransform == null)
            {
               // m_animationTransform = BaseObject.GetChildByName(SelfTransform, HavanaGlobal.BONE_ROOT);
            }

            return m_animationTransform;
        }

    }

    virtual public void Init() { }
    virtual public eObjectType GetObjectType() { return eObjectType.E_OBJECT_TYPE_NONE; }


    virtual public Vector3 GetAnimationPosition(bool bInitPosition = true)
    {
        if (AnimationTransform != null)
        {
            Vector3 returnVector = AnimationTransform.localPosition;

            if (bInitPosition)
            {
                AnimationTransform.position = Vector3.zero;
                AnimationTransform.localPosition = Vector3.zero;
            }

            return returnVector;
        }

        return Vector3.zero;
    }


    static List<Transform> listTempTransform = new List<Transform>();
    static public Transform[] GetChildsByContains(Transform form, string strName)
    {
        listTempTransform.Clear();

        _GetChildsByContains(form, strName);

        return listTempTransform.ToArray();
    }

    static void _GetChildsByContains(Transform form, string strName)
    {
        if (form != null)
        {
            Transform child = null;
            for (int i = 0; i < form.childCount; ++i)
            {
                child = form.GetChild(i);

                if (child.name.Contains(strName))
                {
                    listTempTransform.Add(child);
                }

                _GetChildsByContains(child, strName);
            }
        }
    }


    static public Transform GetChildByName(Transform form, string strName)
    {
        if (form != null)
        {
            Transform child = null;
            //foreach (Transform child in form)
            for (int i = 0; i < form.childCount; ++i)
            {
                child = form.GetChild(i);

                if (child.name == strName)
                {
                    return child;
                }

                Transform data = GetChildByName(child, strName);
                if (data)
                {
                    return data;
                }
            }
        }

        return null;
    }

    static public Transform GetChildByNameContains(Transform form, string strName)
    {
        if (form == null)
            return null;

        Transform child = null;
        //foreach (Transform child in form)
        for (int i = 0; i < form.childCount; ++i)
        {
            child = form.GetChild(i);
            if (child.name.Contains(strName))
            {
                return child;
            }

            Transform data = GetChildByNameContains(child, strName);
            if (data)
            {
                return data;
            }
        }
        return null;
    }

    static public void ChangeObjectLayer(GameObject objectData, int nLayer)
    {
        objectData.layer = nLayer;
        foreach (Transform child in objectData.transform)
        {
            ChangeObjectLayer(child.gameObject, nLayer);
        }
    }

    static List<Renderer> listExceptRenderer = new List<Renderer>();
    static public void VisibleObject(GameObject objectData, bool bVisible)
    {
        if (objectData == null)
            return;

        Renderer render = objectData.GetComponent<Renderer>();
        if (render != null)
        {
            if (bVisible == false)
            {
                if (render.enabled == false)
                    listExceptRenderer.Add(render);

            }
            else
            {
                if (listExceptRenderer.Contains(render) == true)
                {
                    listExceptRenderer.Remove(render);
                    bVisible = false;
                }

            }
            render.enabled = bVisible;
        }
        foreach (Transform child in objectData.transform)
        {
            VisibleObject(child.gameObject, bVisible);
        }
    }



    virtual public void SetState(eBaseObjectState objectState) { ObjectState = objectState; }

    virtual public object GetCustomData(eCustomDataType customData, object TempData1 = null, object TempData2 = null, object TempData3 = null)
    {
        return 0;
    }

    virtual public int GetCustomDataInt(eCustomDataType customData, stGetCustomData stgetCustomData = default(stGetCustomData))
    {
        return 0;
    }

    virtual public float GetCustomDataFloat(eCustomDataType customData, stGetCustomData stgetCustomData = default(stGetCustomData))
    {
        return 0f;
    }
    virtual public bool GetCustomDataBool(eCustomDataType customData, stGetCustomData stgetCustomData = default(stGetCustomData))
    {

        return false;
    }

    virtual public object GetCustomDataObject(eCustomDataType customData, stGetCustomData stgetCustomData = default(stGetCustomData))
    {
        return 0;
    }




    public static void ThrowAll(eThrowEventType eventType, stGetCustomData stThrowData = default(stGetCustomData))
    {
        BaseObject[] arrayObject = FindObjectsOfType<BaseObject>();
        foreach (BaseObject baseObject in arrayObject)
        {
            baseObject.ThrowEvent(eventType, stThrowData);
        }
    }


    public static void ThrowObject(BaseObject baseObject, eThrowEventType eventType, stGetCustomData stThrowData = default(stGetCustomData))
    {
        if (baseObject != null)
        {
            baseObject.ThrowEvent(eventType, stThrowData);
        }
    }

    virtual protected void ThrowEvent(eThrowEventType eventType, stGetCustomData stThrowData = default(stGetCustomData))
    {

    }

    protected string GetEnumString(Enum enumData)
    {
        return enumData.ToString("F");
    }

    public object GetUserData(string strKey)
    {
        object objectData;
        m_dicUserData.TryGetValue(strKey, out objectData);
        return objectData;
    }

    public void AddUserData(string strKey, object objectData)
    {
        RemoveUserData(strKey);
        m_dicUserData.Add(strKey, objectData);
    }

    public void RemoveUserData(string strKey)
    {
        if (m_dicUserData.ContainsKey(strKey))
            m_dicUserData.Remove(strKey);
    }

    public void ClearUserData()
    {
        m_dicUserData.Clear();
    }

}
