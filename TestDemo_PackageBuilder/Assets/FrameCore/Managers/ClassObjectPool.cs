using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ClassObjectPool<T> : IClassObjectPoolDispose where T : class, new()
{
    protected Stack<T> m_Pool = new Stack<T>();
    protected int m_MaxCount = 0;
    protected int m_NotRecycleCount = 0;

    public ClassObjectPool(int capacity, bool isInitCreate = true)
    {
        m_MaxCount = capacity;
        if (isInitCreate)
        {
            for (int i = 0; i < m_MaxCount; ++i)
            {
                m_Pool.Push(new T());
            }
        }
    }

    public T Spawn(bool creatIfPoolEmpty)
    {
        T obj = null;

        if (m_Pool.Count > 0)
        {
            obj = m_Pool.Pop();
        }

        if (creatIfPoolEmpty && obj == null)
        {
            obj = new T();
        }

        if (obj != null)
        {
            ++m_NotRecycleCount;
        }

        return obj;
    }

    public bool Release(T obj)
    {
        if (obj == null)
            return false;

        --m_NotRecycleCount;
        if (m_Pool.Count >= m_MaxCount && m_MaxCount > 0)
        {
            obj = null;
            return false;
        }
        m_Pool.Push(obj);
        return true;
    }

    public void Clear(bool force)
    {
       while(m_Pool.Count>0)
        {
            m_Pool.Pop();
        }
    }
}
