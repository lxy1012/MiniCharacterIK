using System;

public class Singletion<T> where T : class, new()
{
    private static readonly Lazy<T> m_instance = new Lazy<T>(new T());
    public static T Instance
    {
        get { return m_instance.Value; }
    }
}
