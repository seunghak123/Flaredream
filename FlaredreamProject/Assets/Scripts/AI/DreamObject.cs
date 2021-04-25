using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DreamObject : BaseObject
{
    public BaseAI m_baseAI = null;

    [SerializeField]
    GameObject m_PrefabShadowObject = null;

    [NonSerialized]
    GameObject m_ShadowObject = null;

    public void InitObject()
    {
        if (m_PrefabShadowObject != null)
        {
            m_ShadowObject = Instantiate(m_PrefabShadowObject) as GameObject;
            if (m_ShadowObject != null)
            {
                m_ShadowObject.transform.SetParent(SelfTransform, false);
            }
        }
    }
}
