using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BatteryState
{
    Full = 80,
    Mid = 50,
    Low = 20,
    Recharging = 0
}

public enum RoboState
{
    InStation,
    OnRoute,
    Charging
}

public class RobotCleaner : MonoBehaviour
{
    public int Energy;
    [Range(0, 100)]
    public int MaxEnergy = 10;
    public int EnergyStep = 15;

    public float Timestamp = 0;
    public float MoveSpeed = 0.5f;

    public string WallTag;

    public bool IsRightFree;
    public bool IsLeftFree;
    public bool IsUpFree;
    public bool IsDownFree;

    public bool IsOn = false;
    public bool RemoteControl = true;

    public BatteryState Battery;
    public RoboState RoboState;

    public Transform StationTransform;
    public Bar ProgressBar;
    public PathSolver Solver;
    public GameObject TileCleaned;
    public AreaUI AreaUI;
    public AreaUI TempoUI;
    public List<GameObject> TilesCleaned { get; set; }

    public List<Vector2> PathCleaned;
    public List<Vector2> CurrentPath;
    public List<Vector2> PathBlocked;
    public List<Vector2> Learn;

    private void Awake()
    {
        Solver = new PathSolver(StationTransform.position);

        PathCleaned = new List<Vector2>();
        CurrentPath = new List<Vector2>();
        PathBlocked = new List<Vector2>();
        TilesCleaned = new List<GameObject>();

        PathCleaned.Add(StationTransform.position);

        Energy = MaxEnergy;
    }

    private void Start()
    {
        var walls = FindObjectsOfType(typeof(BoxCollider2D)) as BoxCollider2D[];

        foreach (var wall in walls)
        {
            if (wall.CompareTag("Wall"))
            {
                PathBlocked.Add(wall.transform.position);
            }
        }
    }

    void Update()
    {
        IdentifyPath();

        //if (RemoteControl)
        //{
        //    MoveWithKeyboard();
        //}

        switch (RoboState)
        {
            case RoboState.InStation:

                if (IsOn)
                {
                    StartCoroutine(ualqui());
                    RoboState = RoboState.OnRoute;
                }

                break;
            case RoboState.OnRoute:

                if (IsOn)
                {

                }

                break;
            case RoboState.Charging:

                break;
        }

        PrintRay();
    }

    private List<Vector2> GetAdjacentNodes(Vector2 node, Func<Vector2, bool> filter = null)
    {
        return new List<Vector2>
        {
            new Vector2(node.x,node.y + 1),
            new Vector2(node.x,node.y - 1),
            new Vector2(node.x + 1,node.y),
            new Vector2(node.x - 1,node.y)
        }.Where((filter ?? ((x) => true))).ToList();
    }

    private void MoveWithKeyboard()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            Move(Vector2.up, IsUpFree);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            Move(Vector2.down, IsDownFree);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            Move(Vector2.left, IsLeftFree);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            Move(Vector2.right, IsRightFree);
        }
    }



    private bool equalVectors(Vector2 v1, Vector2 v2)
    {
        return (v1.x == v2.x) && (v1.y == v2.y);
    }
    private string stringfyVector(Vector2 vector)
    {
        return "{" + "x: " + vector.x + " y: " + vector.y ;
    }

    private int CalcHeuristicCost(Vector2 origin, Vector2 goal)
    {
        return (int)((Math.Abs(origin.x - goal.x) + Math.Abs(origin.y - origin.y)));
    }

    private List<Vector2> buildPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        var path = new List<Vector2>() { current };
        current = cameFrom[current];

        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];

            path.Add(current);
        }

        var paz = path.LastOrDefault();

        path.Remove(paz);
        
        path.Reverse();

        return path;
    }

    private List<Vector2> AStar(Vector2 start, Vector2 goal, Predicate<Vector2> restriction)
    {
        var openSet = new List<Vector2>() { start };
        var cameFrom = new Dictionary<Vector2, Vector2>();

        var startCost = new Dictionary<Vector2, int>();
        startCost.Add(start, 0);
        
        var fullCost = new Dictionary<Vector2, int>();
        fullCost.Add(start, CalcHeuristicCost(start, goal));

        var count = 0;
        while(openSet.Count > 0)
        {
            if (count++ > 500) throw new Exception();

            Vector2 current = start;

            foreach(var node in openSet)
            {
                if (fullCost[node] == default)
                {
                    continue;
                }

                if (fullCost[current] == default)
                {
                    current = node;
                    continue;
                }

                var res = fullCost[node] < fullCost[current];
                if (res)
                {
                    current = node;
                }
            }

            if (current == goal) return buildPath(cameFrom, current);

            openSet.Remove(current);

            var adjs = GetAdjacentNodes(current);

            foreach (var neighbor in adjs)
            {
                var restrc = restriction(neighbor);
                if (restriction != null && !restriction(neighbor))
                {
                    continue;
                }

                var neighborStartCost = startCost[current] + 1;

                var restartHasKey = startCost.ContainsKey(neighbor);

                var cost = int.MaxValue;

                if (restartHasKey)
                {
                    cost = startCost[neighbor];
                }

                if (cost == int.MaxValue || neighborStartCost < cost)
                {
                    cameFrom.Add(neighbor, current);
                    startCost.Add(neighbor, neighborStartCost);
                    fullCost.Add(neighbor, neighborStartCost + CalcHeuristicCost(neighbor, goal));

                    if (!openSet.Any(x => x == neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        throw new Exception("no impossivel de ser alcancado");


    }

    private IEnumerator ualqui()
    {
        Vector2 current = StationTransform.position;
        List<Vector2> nexts = new List<Vector2>();
        List<Vector2> visited = new List<Vector2>();

        while (Energy > 0)
        {
            List<Vector2> adjs = GetAdjacentNodes(current, (node) => !visited.Contains(node) && !Learn.Contains(node));
            foreach (var i in adjs)
            {
                if (!nexts.Contains(i)) nexts.Add(i);
            }
            List<Vector2> bestPathForward = null;
            foreach (var node in nexts)
            {
                List<Vector2> pathForward = AStar(current, node, (x) => !Learn.Contains(x));
                if (bestPathForward == null || pathForward.Count < bestPathForward.Count)
                {
                    bestPathForward = pathForward;
                }
                if (bestPathForward.Count == 1)
                {
                    break;
                }
            }

            Vector2 goal = bestPathForward.LastOrDefault();
            List<Vector2> bestPathReturning = AStar(goal, StationTransform.position, (x) => visited.Contains(x));
            if (Energy > bestPathForward.Count + bestPathReturning.Count)
            {
                bool success = true;
                foreach (var i in bestPathForward)
                {
                    success = Move(i);
                    yield return new WaitForSeconds(0.5f);
                    nexts = nexts.Where(x => x == i).ToList();
                    if (success)
                    {
                        visited.Add(i);
                        adjs = GetAdjacentNodes(i, (x) => !visited.Contains(x) && !Learn.Contains(x));
                        foreach (var j in adjs)
                        {
                            if (!nexts.Contains(j)) nexts.Add(j);
                        }
                        current = i;
                    }
                    else
                    {
                        Learn.Add(i);
                        break;
                    }
                }

                if (!success) continue;

            }
            else
            {
                foreach (var i in bestPathReturning)
                {
                    Move(i);
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            }
        }

    }

    public bool Move(Vector2 direction)
    {
        var normalized = direction.normalized;

        if ((normalized.y == 1 && !IsUpFree) || (normalized.y == -1 && !IsDownFree))
        {
            return false;
        }

        if ((normalized.x == 1 && !IsRightFree) || (normalized.x == -1 && !IsLeftFree))
        {
            return false;
        }

        Move(direction, true);

        return true;
    }

    private void Move(Vector2 walkStep, bool isFree)
    {
        if (!isFree) return;

        transform.position = walkStep;

        HandleEnergy();

        ProgressBar.UpdateProgress((float)Energy / MaxEnergy, Battery.ToString());
    }

    private void HandleEnergy()
    {
        Energy--;

        if (Energy >= (int)BatteryState.Full)
        {
            Battery = BatteryState.Full;
        }
        else if (Energy > (int)BatteryState.Mid && Energy < (int)BatteryState.Full)
        {
            Battery = BatteryState.Mid;
        }
        else
        {
            Battery = BatteryState.Low;
        }
    }

    private void IdentifyPath()
    {
        var currentPosition = transform.position;

        var hitUp = Physics2D.Raycast(currentPosition, Vector2.up, 1f);
        var hitDown = Physics2D.Raycast(currentPosition, Vector2.down, 1f);
        var hitRight = Physics2D.Raycast(currentPosition, Vector2.right, 1f);
        var hitLeft = Physics2D.Raycast(currentPosition, Vector2.left, 1f);

        IsUpFree = !(hitUp.collider != null && hitUp.collider.CompareTag(WallTag));
        IsDownFree = !(hitDown.collider != null && hitDown.collider.CompareTag(WallTag));
        IsLeftFree = !(hitLeft.collider != null && hitLeft.collider.CompareTag(WallTag));
        IsRightFree = !(hitRight.collider != null && hitRight.collider.CompareTag(WallTag));
    }

    private void PrintRay()
    {
        Debug.DrawRay(transform.position, Vector2.up, IsUpFree ? Color.green : Color.red);
        Debug.DrawRay(transform.position, Vector2.down, IsDownFree ? Color.green : Color.red);
        Debug.DrawRay(transform.position, Vector2.right, IsRightFree ? Color.green : Color.red);
        Debug.DrawRay(transform.position, Vector2.left, IsLeftFree ? Color.green : Color.red);
    }
}
