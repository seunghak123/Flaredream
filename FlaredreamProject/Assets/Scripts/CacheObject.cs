using UnityEngine;
using System.Collections;

public class CacheObject : MonoBehaviour
{
    protected Transform m_CacheTransform = null;
    protected GameObject m_CacheGameObject = null;

    protected bool m_bGetCacheGameObject = false;
    protected bool m_bGetCacheTransform = false;


    virtual public GameObject SelfObject
    {
        get
        {
            if (m_CacheGameObject == null && m_bGetCacheGameObject == false)
            {
                m_CacheGameObject = gameObject;
                m_bGetCacheGameObject = true;
            }

            return m_CacheGameObject;
        }
    }
    virtual public Transform SelfTransform
    {
        get
        {
            if (m_CacheTransform == null && m_bGetCacheTransform == false)
            {
                m_CacheTransform = transform;
                m_bGetCacheTransform = true;
            }

            return m_CacheTransform;
        }
    }

}
