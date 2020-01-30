using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public GameObject Ball { get; set; }
    public Vector3 Target { get; set; }
    public int Owner { get; set; }
}