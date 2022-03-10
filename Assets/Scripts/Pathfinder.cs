using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinder
{

    Node m_startNode;
    Node m_goalNode;

    PriorityQueue<Node> m_frontierNodes;
    List<Node> m_exploredNodes;
    List<Node> m_pathNodes;

    int m_iterations;
    public bool showIterations = false;
    public float timeStepIterations = .1f;

    bool isComplete = false;


    public void Init()
    {

    }

    public List<Vector2> GeneratePath(Vector2 origin, Vector2 destiny, Predicate<Vector2> predicate = null)
    {
        int startPosX = (int) origin.x;
        int startPosY = (int) origin.y;
        int goalPosX = (int) destiny.x;
        int goalPosY  = (int) destiny.y;
        m_startNode = new Node(startPosX, startPosY);
        m_startNode.distanceFromStart = 0;
        m_startNode.costsFromStart = 0;
        m_goalNode = new Node(goalPosX, goalPosY);

        m_frontierNodes = new PriorityQueue<Node>();
        m_frontierNodes.Enqueue(m_startNode);
        m_exploredNodes = new List<Node>();
        m_pathNodes = new List<Node>();

        isComplete = false;
        m_iterations = 0;
        Debug.Log("Pathfinder is initialized");

        Debug.Log("Starting path generating...");

        while (!isComplete)
        {
            if (m_frontierNodes.Count > 0)
            {

                Node currentNode = (Node)m_frontierNodes.Dequeue();
                m_iterations++;

                if (!m_exploredNodes.Any(x => x.PositionX == currentNode.PositionX && x.PositionY == currentNode.PositionY))
                {
                    m_exploredNodes.Add(currentNode);
                }

                ExpandFrontierAStar(currentNode, predicate);

                if (m_frontierNodes.ToList().Any(x => x.PositionX == m_goalNode.PositionX && x.PositionY == m_goalNode.PositionY))
                {
                    m_pathNodes = GetPathNodes(m_goalNode);
                    isComplete = true;
                    Debug.Log("Path has been found in " + m_iterations + " iterations!");
                    Debug.Log("Path distance " + m_goalNode.distanceFromStart + "!");
                    Debug.Log("Path costs " + m_goalNode.costsFromStart + "!");
                }
            }
            else
            {
                isComplete = true;
                Debug.Log("No Path has been found. " + m_iterations + " iterations.");
            }
        }
        return m_pathNodes.Select(x => new Vector2(x.PositionX, x.PositionY)).ToList();
    }
   

    //see https://en.wikipedia.org/wiki/A*_search_algorithm
    void ExpandFrontierAStar(Node node, Predicate<Vector2> predicate = null)
    {
        if (node == null)
        {
            return;
        }
        var adj = node.GetAdjacentNodes();
        for (int i = 0; i < adj.Count; i++)
        {
            if (predicate != null && !predicate(new Vector2(node.GetAdjacentNodes()[i].PositionX, adj[i].PositionY)))
                continue;
            if (!m_exploredNodes.Any(x => x.PositionX == adj[i].PositionX && x.PositionY == adj[i].PositionY))
            {
                float distanceToAdjacent = Math.Abs(node.PositionX - adj[i].PositionX) + Math.Abs(node.PositionY - adj[i].PositionY);
                float newDistanceFromStart = node.distanceFromStart + distanceToAdjacent;
                float newCostsFromStart = node.costsFromStart + distanceToAdjacent; //this way the terraincosts get included

                if (float.IsPositiveInfinity(adj[i].costsFromStart)
                        || newCostsFromStart < adj[i].costsFromStart)

                {
                    adj[i].previousNode = node;
                    adj[i].distanceFromStart = newDistanceFromStart;
                    adj[i].costsFromStart = newCostsFromStart;
                }

                if (!m_frontierNodes.ToList().Any(x => x.PositionX == adj[i].PositionX && x.PositionY == adj[i].PositionY))
                {
                    float distanceToGoal = Math.Abs(m_goalNode.PositionX - adj[i].PositionX) + Math.Abs(m_goalNode.PositionY - adj[i].PositionY);
                    adj[i].priority = adj[i].costsFromStart + distanceToGoal;
                    m_frontierNodes.Enqueue(adj[i]);
                }
            }
        }
    }

    List<Node> GetPathNodes(Node endNode)
    {
        List<Node> path = new List<Node>();

        if (endNode == null)
        {
            return path;
        }

        Node currentNode = endNode;

        while (currentNode.previousNode != null)
        {
            path.Insert(0, currentNode);
            currentNode = currentNode.previousNode;
        }

        return path;
    }

}
