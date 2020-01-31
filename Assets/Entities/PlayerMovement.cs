using UnityEngine;

public class PlayerMovement
{
    public Vector3 targetPosition;
    public Quaternion playerRot;
    public Vector3 currentPosition;

    public PlayerMovement(Vector3 targetPosition, Quaternion playerRot, Vector3 currentPosition)
    {
        this.targetPosition = targetPosition;
        this.playerRot = playerRot;
        this.currentPosition = currentPosition;
    }
}