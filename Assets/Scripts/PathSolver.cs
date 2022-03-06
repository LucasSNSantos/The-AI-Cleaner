using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathException : Exception
{
    public Stack<Vector2> PathCleaned { get; set; }
    public int EnergyLeft { get; set; }

    public PathException(Stack<Vector2> pathCleaned, int energyLeft)
    {
        PathCleaned = pathCleaned;
        EnergyLeft = energyLeft;
    }
}

public class PathSolver
{
    private static System.Random rng = new System.Random();
    public Vector2 StationPosition { get; private set; }

    public PathSolver(Vector2 startPosition)
    {
        StationPosition = startPosition;
    }

    private List<Vector2> GetAdjacentNodes(Vector2 node) => new List<Vector2>()
    {
        node + Vector2.up,
        node + Vector2.down,
        node + Vector2.left,
        node + Vector2.right
    };

    private List<Vector2> GetShorterStack(List<List<Vector2>> paths)
    {
        List<Vector2> shortPath = null;

        foreach(var path in paths)
        {
            if (shortPath == null)
            {
                shortPath = path;
                continue;
            }

            if (shortPath.Count > path.Count)
            {
                shortPath = path;
            }
        }

        return shortPath;
    }

    public List<Vector2> GetMinorPath(List<Vector2> pathCleaned, int energyLeft, List<Vector2> pathBlocked, Func<Vector2, bool> stopCondition = null)
    {
        if (energyLeft == 0) return null;

        var currentNode = pathCleaned.LastOrDefault();

        if (stopCondition != null && currentNode != null && stopCondition(currentNode))
        {
            return pathCleaned;
        }

        energyLeft--;

        var adjacentNodes = GetAdjacentNodes(currentNode).Where(x => !pathBlocked.Contains(x)).ToList();

        adjacentNodes = adjacentNodes.OrderBy(a => rng.Next()).ToList();

        var beforeLastNode = pathCleaned.ElementAtOrDefault(pathCleaned.Count - 2 >= 0 ? pathCleaned.Count - 2 : 0);

        if (beforeLastNode != null && adjacentNodes.Count >= 2)
        {
            var hasPrecedenteCriminal = adjacentNodes.FirstOrDefault(x => x == beforeLastNode);

            if (hasPrecedenteCriminal != null)
            {
                adjacentNodes.Remove(hasPrecedenteCriminal);
            }
        }

        var isAllVisited = adjacentNodes.All(x => pathCleaned.Contains(x));

        var pathGo = new List<List<Vector2>>();

        foreach(var node in adjacentNodes)
        {
            var notVisitedYet = !pathCleaned.Contains(node);

            if (notVisitedYet || isAllVisited)
            {
                var pathCleanedClone = pathCleaned.ToList();

                pathCleanedClone.Add(node);

                var minorPath = GetMinorPath(pathCleanedClone, energyLeft, pathBlocked);

                if (minorPath != null)
                {
                    pathGo.Add(minorPath);
                } else
                {
                    pathGo.Add(pathCleaned);
                } 
            }
        }

        return GetShorterStack(pathGo);
    }
}
