using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BatteryState
{
    Full = 80,
    Mid = 50,
    Low = 20,
    Recharging = 0
}

public class RobotCleaner : MonoBehaviour
{
    [Range(0, 100)]
    public int Energy = 100;

    public string WallTag;

    public bool IsRightFree;
    public bool IsLeftFree;
    public bool IsUpFree;
    public bool IsDownFree;

    public BatteryState Battery;

    public Bar ProgressBar;

    // Update is called once per frame
    void Update()
    {
        IdentifyPath();

        WalkWithKeyboard();

        PrintRay();
    }

    private void WalkWithKeyboard()
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

    private void Move(Vector2 walkStep, bool isFree)
    {
        if (!isFree) return;

        transform.Translate(walkStep);

        HandleEnergy();

        ProgressBar.UpdateProgress(Energy / 100f);
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
