using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public interface IContainerObject
{
    object This { get; }
}

public class ContainerObject<T> : IDisposable, IContainerObject where T : new()
{
    object IContainerObject.This { get { return This; } }

    public T This { get; private set; }

    bool m_bDisposed = false;
    public bool DISPOSED { get { return m_bDisposed; } set { m_bDisposed = value; } }

    public ContainerObject()
    {
        This = new T();
    }

    public void Dispose()
    {
        if (DISPOSED == true)
            return;

        RecycledObjectPool.ReleaseContainer(this);
        DISPOSED = true;

        GC.SuppressFinalize(this);
    }

    public static implicit operator T(ContainerObject<T> obj)
    {
        return obj.This;
    }
}

public static class RecycledObjectPool
{
    private static Dictionary<Type, Queue<IContainerObject>> ContainerObjectQueueContainer = new Dictionary<Type, Queue<IContainerObject>>();
    private static Dictionary<Type, List<IContainerObject>> ContainerObjectUseContainer = new Dictionary<Type, List<IContainerObject>>();

    private static object containerLockObject = new object();

    public static ContainerObject<T> GetContainer<T>() where T : new()
    {
        ContainerObject<T> retContainer = null;
        Queue<IContainerObject> queue = null;
        List<IContainerObject> list = null;
        lock (containerLockObject)
        {
            if (ContainerObjectQueueContainer.TryGetValue(typeof(ContainerObject<T>), out queue) == false)
            {
                queue = new Queue<IContainerObject>();
                ContainerObjectQueueContainer[typeof(ContainerObject<T>)] = queue;
            }

            if (ContainerObjectUseContainer.TryGetValue(typeof(ContainerObject<T>), out list) == false)
            {
                list = new List<IContainerObject>();
                ContainerObjectUseContainer[typeof(ContainerObject<T>)] = list;
            }

            if (queue.Count == 0)
            {
                retContainer = new ContainerObject<T>();
            }
            else
            {
                retContainer = queue.Dequeue() as ContainerObject<T>;
            }

            list.Add(retContainer);

        }

        MethodInfo methodInfo = retContainer.This.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        if (null != methodInfo)
        {
            methodInfo.Invoke(retContainer.This, null);
        }

        retContainer.DISPOSED = false;
        return retContainer;
    }

    public static void ReleaseContainer<T>(T containerObj) where T : IContainerObject
    {
        if (null == containerObj)
            return;
        Queue<IContainerObject> queue = null;
        List<IContainerObject> list = null;
        lock (containerLockObject)
        {
            if (false == ContainerObjectQueueContainer.TryGetValue(typeof(T), out queue))
            {
                queue = new Queue<IContainerObject>();
                ContainerObjectQueueContainer[typeof(T)] = queue;
            }

            if (ContainerObjectUseContainer.TryGetValue(typeof(T), out list) == false)
            {
                list = new List<IContainerObject>();
                ContainerObjectUseContainer[typeof(T)] = list;
            }


            if (null == queue)
            {
                Debug.LogException(new Exception("RecycledObjectPool.ReleaseContainer : queue is null"));
                return;
            }

            if (list.Contains(containerObj) == false)
            {
                Debug.LogError("already released : " + containerObj.GetType().ToString());
                return;
            }

            list.Remove(containerObj);

            MethodInfo methodInfo = containerObj.This.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (null == methodInfo)
                return;
            methodInfo.Invoke(containerObj.This, null);

            queue.Enqueue(containerObj);
        }

    }

    private static Dictionary<Type, ICollection> DataObjectQueueContainer = new Dictionary<Type, ICollection>();
    private static object dataLockObject = new object();

    public static T GetData<T>() where T : class, new()
    {
        T retData = default(T);
        ICollection collection = null;
        lock (dataLockObject)
        {
            try
            {
                if (false == DataObjectQueueContainer.TryGetValue(typeof(T), out collection) || 0 == collection.Count)
                {
                    retData = new T();
                    //#if UNITY_EDITOR || UNITY_STANDALONE
                    //					Debug.LogWarning("RecycledObjectPool.GetData new : " + retData.GetType().ToString());
                    //#endif
                    return retData;
                }
                MethodInfo methodInfo = collection.GetType().GetMethod("Dequeue");
                if (null == methodInfo)
                {
                    retData = new T();
                    //#if UNITY_EDITOR || UNITY_STANDALONE
                    //					Debug.LogWarning("RecycledObjectPool.GetData new (null == methodInfo) : " + retData.GetType().ToString());
                    //#endif
                    return retData;
                }
                retData = methodInfo.Invoke(collection, null) as T;
                if (null == retData)
                {
                    retData = new T();
                    //#if UNITY_EDITOR || UNITY_STANDALONE
                    //					Debug.LogWarning("RecycledObjectPool.GetData new (null == retData) : " + retData.GetType().ToString());
                    //#endif
                }
                //#if UNITY_EDITOR || UNITY_STANDALONE
                //				Debug.LogWarning("RecycledObjectPool.GetData " + retData.GetType());
                //#endif
                return retData;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new T();
            }
        }
    }

    public static void ReleaseData<T>(T data) where T : class
    {
        if (null == data)
            return;
        ICollection collection = null;
        Queue<T> queue = null;
        lock (dataLockObject)
        {
            try
            {
                if (false == DataObjectQueueContainer.TryGetValue(data.GetType(), out collection))
                {
                    queue = new Queue<T>();
                    //#if UNITY_EDITOR || UNITY_STANDALONE
                    //					Debug.LogWarning("RecycledObjectPool.ReleaseData queue new : " + queue.GetType().ToString());
                    //#endif
                    DataObjectQueueContainer[data.GetType()] = queue;
                }
                else
                    queue = collection as Queue<T>;

                if (null == queue)
                {
                    Debug.LogException(new Exception("RecycledObjectPool.ReleaseData : queue is null"));
                    return;
                }

                if (0 < queue.Count && null != queue.FirstOrDefault((findObj) => { return findObj.Equals(data); }))
                {
                    Debug.LogError("already released : " + data.GetType().ToString());
                    return;
                }

                MethodInfo methodInfo = data.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (null == methodInfo)
                    return;
                methodInfo.Invoke(data, null);
            }
            finally
            {
                if (null != queue)
                {
                    //#if UNITY_EDITOR || UNITY_STANDALONE
                    //					Debug.LogWarning("RecycledObjectPool.ReleaseData " + data.GetType());
                    //#endif
                    queue.Enqueue(data);
                }
            }
        }
    }

    public static void PrintPoolStatistics()
    {
#if UNITY_EDITOR
        lock (containerLockObject)
        {
            Debug.LogWarning("### ContainerQueue Count");
            foreach (var pair in ContainerObjectQueueContainer)
                Debug.LogWarning(pair.Key.ToString() + " : " + pair.Value.Count);
        }

        lock (dataLockObject)
        {
            Debug.LogWarning("### DataQueue Count");
            foreach (var pair in DataObjectQueueContainer)
                Debug.LogWarning(pair.Key.ToString() + " : " + pair.Value.Count);
        }
#endif
    }
}