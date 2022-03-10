using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    int m_xPosition;
    public int PositionX { get { return m_xPosition; } }
    int m_yPosition;
    public int PositionY { get { return m_yPosition; } }

    public Node previousNode;

    public float priority;

    public float distanceFromStart = Mathf.Infinity;
    public float costsFromStart = Mathf.Infinity;   //this way you can track hazardous terrain
    public List<Node> GetAdjacentNodes()
    {
        return new List<Node>
        {
            new Node(PositionX,PositionY + 1),
            new Node(PositionX,PositionY - 1),
            new Node(PositionX + 1,PositionY),
            new Node(PositionX - 1,PositionY)
        };
    }

    public Node(int xPosition, int yPosition)
    {
        this.m_xPosition = xPosition;
        this.m_yPosition = yPosition;
    }

    //public static bool operator == (Node v1, Node v2)
    //{
    //    return (v1 != null && v2 != null &&
    //        (v1.PositionX ==  v2.PositionX) && (v1.PositionY == v2.PositionY))
    //        || (v1 == null && v2 == null);
    //}
    //public static bool operator != (Node lhs, Node rhs) => !(lhs == rhs);


    // used for sorting order priorityqueue
    public int CompareTo(Node other)
    {
        if (this.priority < other.priority)
        {
            return -1;
        }
        else if (this.priority > other.priority)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

}
