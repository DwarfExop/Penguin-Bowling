using UnityEngine;

public class BallMovement
{
    public GameObject ball;
    public Vector3 targetPosition;

    public BallMovement (Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}