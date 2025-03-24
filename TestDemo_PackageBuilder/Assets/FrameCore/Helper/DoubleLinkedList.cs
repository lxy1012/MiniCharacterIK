using System;
using System.Collections.Generic;


// 节点类
public class Node<T> where T : class, new()
{
    public T Value { get; set; } = null;          // 节点值
    public Node<T> Previous { get; set; } = null;    // 前驱节点
    public Node<T> Next { get; set; } = null;        // 后继节点

    public Node() { }
    public Node(T value)
    {
        Value = value;
        Previous = null;
        Next = null;
    }
}

public class DoubleLinkedList<T> : IClassObjectPoolDispose where T : class, new()
{

    protected ClassObjectPool<Node<T>> m_doubleLinkedNodePool = ClassObjectPoolManager.Instance.GetOrCreateObjectPool<Node<T>>(500);
    public Node<T> Head { get; set; }           // 头节点
    public Node<T> Tail { get; set; }           // 尾节点
    public int Count { get; private set; }    // 链表长度


    // 构造函数
    public DoubleLinkedList()
    {
        Head = null;
        Tail = null;
        Count = 0;
    }
    ~DoubleLinkedList()
    {
        Clear(true);
    }

    // 从头添加节点
    public Node<T> AddToHeader(T value)
    {
        Node<T> newNode = m_doubleLinkedNodePool.Spawn(true);
        newNode.Value = value;
        newNode.Previous = null;
        newNode.Next = null;
        AddToHeader(newNode);
        return newNode;
    }

    public Node<T> AddToHeader(Node<T> newNode)
    {
        if (Head == null) // 链表为空
        {
            Head = newNode;
            Tail = newNode;
        }
        else // 链表不为空
        {
            newNode.Next = Head;
            Head.Previous = newNode;
            Head = newNode;
        }

        Count++;
        return Head;
    }
    public Node<T> AddToTail(T value)
    {
        Node<T> newNode = m_doubleLinkedNodePool.Spawn(true);
        newNode.Value = value;
        newNode.Previous = null;
        newNode.Next = null;
        AddToTail(newNode);
        return newNode;
    }
    // 从尾添加节点
    public Node<T> AddToTail(Node<T> newNode)
    {

        if (Tail == null) // 链表为空
        {
            Head = newNode;
            Tail = newNode;
        }
        else // 链表不为空
        {
            newNode.Previous = Tail;
            Tail.Next = newNode;
            Tail = newNode;
        }

        Count++;
        return Tail;
    }

    // 从头删除节点
    public void RemoveFirst()
    {
        if (Head == null) // 链表为空
        {
            throw new InvalidOperationException("链表为空，无法删除");
        }
        if (Head == Tail) // 链表只有一个节点
        {
            m_doubleLinkedNodePool.Release(Head);
            Head = null;
            Tail = null;
        }
        else // 链表有多个节点
        {
            m_doubleLinkedNodePool.Release(Head.Previous);
            Head = Head.Next;
            Head.Previous = null;
        }

        Count--;
    }

    // 从尾删除节点
    public void RemoveLast()
    {
        if (Tail == null) // 链表为空
        {
            throw new InvalidOperationException("链表为空，无法删除");
        }

        if (Head == Tail) // 链表只有一个节点
        {
            m_doubleLinkedNodePool.Release(Head);
            Head = null;
            Tail = null;
        }
        else // 链表有多个节点
        {
            m_doubleLinkedNodePool.Release(Head.Next);
            Tail = Tail.Previous;
            Tail.Next = null;
        }

        Count--;
    }

    // 将指定节点移动到头部
    public void MoveToHead(Node<T> node)
    {
        if (node == null || Head == null)
        {
            throw new ArgumentNullException("节点不能为空");
        }

        if (node == Head) // 节点已经是头节点
        {
            return;
        }

        // 从链表中移除节点
        if (node.Previous != null)
        {
            node.Previous.Next = node.Next;
        }
        if (node.Next != null)
        {
            node.Next.Previous = node.Previous;
        }

        // 如果节点是尾节点，更新尾节点
        if (node == Tail)
        {
            Tail = node.Previous;
        }

        // 将节点移动到头部
        node.Previous = null;
        node.Next = Head;
        Head.Previous = node;
        Head = node;
    }


    // 移除指定节点
    public void RemoveNode(Node<T> node)
    {
        if (node == null)
        {
            throw new ArgumentNullException(nameof(node), "节点不能为空");
        }

        // 如果链表为空
        if (Head == null)
        {
            throw new InvalidOperationException("链表为空，无法删除");
        }

        // 如果移除的是头节点
        if (node == Head)
        {
            RemoveFirst();
            return;
        }

        // 如果移除的是尾节点
        if (node == Tail)
        {
            RemoveLast();
            return;
        }

        // 从链表中移除节点
        if (node.Previous != null)
        {
            node.Previous.Next = node.Next;
        }
        if (node.Next != null)
        {
            node.Next.Previous = node.Previous;
        }

        // 清理引用
        node.Previous = null;
        node.Next = null;

        Count--;
        m_doubleLinkedNodePool.Release(node);
    }

    public void Clear(bool force)
    {
        m_doubleLinkedNodePool.Clear(force);
    }
}

public class LRUMapList<T> where T : class, new()
{
    DoubleLinkedList<T> m_DLink = new DoubleLinkedList<T>();
    Dictionary<T, Node<T>> m_FindMap = new Dictionary<T, Node<T>>();

    public void InsertToHead(T t)
    {
        Node<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DLink.AddToHeader(node);
            return;
        }
        m_DLink.AddToHeader(t);
        m_FindMap.Add(t, m_DLink.Head);
    }

    /// <summary>
    /// 表尾弹出
    /// </summary>
    public void Pop()
    {
        if (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.Value);
        }
    }

    public void Remove(T t)
    {
        Node<T> node = null;
        if (!m_FindMap.TryGetValue((T)t, out node) || node == null)
        {
            return;
        }
        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    public T Back()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.Value;
    }

    public int Size()
    {
        return m_FindMap.Count;
    }

    public bool Find(T t)
    {
        Node<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) || node == null)
            return false;
        return true;
    }

    /// <summary>
    /// 刷新节点，放到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Reflesh(T t)
    {
        Node<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) || node == null)
            return false;

        m_DLink.MoveToHead(node);
        return true;
    }

    public void Clear()
    {
        while (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.Value);
        }
    }
}
