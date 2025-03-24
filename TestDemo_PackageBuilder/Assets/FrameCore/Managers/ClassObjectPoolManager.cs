using System;
using System.Collections.Generic;


public interface IClassObjectPoolDispose
{
    void Clear(bool force);
}
/// <summary>
/// �����ع�����
/// </summary>
public class ClassObjectPoolManager : Singletion<ClassObjectPoolManager>
{
    protected Dictionary<Type, object> m_ObjectPoolDict = new Dictionary<Type, object>();

    /// <summary>
    /// ����������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateObjectPool<T>(int maxCount, bool initCreate = true) where T : class,new()
    {
        Type type = typeof(T);
        if (!m_ObjectPoolDict.TryGetValue(type, out object obj) || obj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount, initCreate);
            m_ObjectPoolDict.Add(type, newPool);
            return newPool;
        }

        return obj as ClassObjectPool<T>;
    }

  
}
