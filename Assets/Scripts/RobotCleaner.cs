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

        foreach(var wall in walls)
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

        if (RemoteControl)
        {
            MoveWithKeyboard();
        }

        switch(RoboState)
        {
            case RoboState.InStation:

                if (IsOn)
                {
                    CalculateRoute(EnergyStep);

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

    private void CalculateRoute(int howManyEnergy)
    {
        if (Energy > 0)
        {
            var currentCleanedTiles = PathCleaned.Count;

            PathCleaned = Solver.GetMinorPath(PathCleaned, howManyEnergy, PathBlocked);

            CurrentPath = PathCleaned.Skip(currentCleanedTiles).ToList();

            StartCoroutine(MoveOnPath());
        }
    }

    private IEnumerator MoveOnPath()
    {
        if (CurrentPath.Count > 0)
        {
            bool ignoreIfNotMyPosition = false;

            foreach(var currentNode in CurrentPath)
            {
                if (currentNode == (Vector2)transform.position)
                {
                    ignoreIfNotMyPosition = true;
                }

                if (ignoreIfNotMyPosition) continue;

                print(currentNode.ToString());

                var tile = TilesCleaned.FirstOrDefault(x => (Vector2)x.transform.position == currentNode);

                if (tile == null)
                {
                    var tileInstanciatade = Instantiate(TileCleaned);
                    tileInstanciatade.transform.position = currentNode;
                    TilesCleaned.Add(tileInstanciatade);
                }

                AreaUI.UpdateArea(TilesCleaned.Count.ToString());

                var canMove = Move(currentNode);

                if (!canMove)
                {
                    // adicionar aos caminhos bloqueados
                    // recalcular
                }

                // checar a energia q falta pra voltar pra casa
                // se nao tiver suficiente, volte

                Timestamp += MoveSpeed;

                TimeSpan span = new TimeSpan(0, 0, (int)Timestamp);

                TempoUI.UpdateArea($"{span.Minutes.ToString("00")}:{span.Seconds.ToString("00")}");

                yield return new WaitForSeconds(MoveSpeed);
            }

            if (Energy > EnergyStep)
            {
                CalculateRoute(EnergyStep);
            }
            else
            {
                print("ACABOU ENERGIA IRMAO!");
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
        } else if(Energy > (int) BatteryState.Mid && Energy < (int) BatteryState.Full)
        {
            Battery = BatteryState.Mid;
        } else
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
