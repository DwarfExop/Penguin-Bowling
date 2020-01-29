using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Quaternion youPlayerRot;
    private Vector3 youTargetPosition;

    private Client client;
    private int healthPoints = 99;
    private MovementData otherMovementData;

    public GameObject other;
    public GameObject health;
    public float speed = 10;
    public float cameraFollowOffset = 40;
    public float ballSpeed = 20;
    public GameObject ball;
    public List<BallMovement> balls = new List<BallMovement>();

    private void Start()
    {
        client = Client.GetInstance();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetPosition();
        }

        var (data, type) = client.CheckMessages();

        if (data != null)
        {
            if (type.Value == MovementType.Player)
            {
                otherMovementData = data;
            }
            else if (type.Value == MovementType.Ball)
            {
                CreateBall(other, data.targetPosition);
            }
        }

        if (Input.GetKeyDown("space"))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 1000))
            {
                Vector3 normalized = (hit.point - transform.position).normalized;
                var target = transform.position + normalized * 30;
                CreateBall(gameObject, target);
                client.SendBall(other, target, default, gameObject.transform.position);
            }
        }

        MoveBall();

        if (youTargetPosition != default && youTargetPosition != transform.position)
        {
            MovePlayer(gameObject, youPlayerRot, youTargetPosition);
        }

        if (otherMovementData != default && otherMovementData.targetPosition != other.transform.position)
        {
            MovePlayer(other, otherMovementData.playerRot, otherMovementData.targetPosition);
        }
    }

    private void CreateBall(GameObject gameObject, Vector3 target)
    {
        var newBall = Instantiate(ball);
        newBall.SetActive(true);

        var ballComponent = newBall.GetComponent<Ball>();
        ballComponent.OwnerId = gameObject.GetInstanceID();

        newBall.transform.position = gameObject.transform.position;
        balls.Add(new BallMovement { Owner = gameObject.GetInstanceID(), Ball = newBall, Target = target });
    }

    private void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            youTargetPosition = hit.point;
            youPlayerRot = Quaternion.LookRotation(Vector3.forward, (youTargetPosition - transform.position) * -1);

            client.SendMovement(other, youTargetPosition, youPlayerRot, transform.position);
        }
    }

    private void MovePlayer(GameObject player, Quaternion rotation, Vector3 targetPosition)
    {
        player.transform.rotation = rotation;
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void MoveBall()
    {
        var toRemoves = new List<BallMovement>();
        foreach (var movement in balls)
        {
            if (movement.Ball == null)
            {
                toRemoves.Add(movement);
                continue;
            }

            movement.Ball.transform.position = Vector3.MoveTowards(movement.Ball.transform.position, movement.Target, ballSpeed * Time.deltaTime);

            if (movement.Ball.transform.position == movement.Target)
            {
                Destroy(movement.Ball.gameObject);
                toRemoves.Add(movement);
            }
        }
        toRemoves.ForEach(t => balls.Remove(t));
    }

    public void OnCollisionEnter(Collision collision)
    {
        var ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null && ball.OwnerId != gameObject.GetInstanceID())
        {
            // Remove 10 hp;
            healthPoints -= 10;
            health.GetComponent<Text>().text = $"Health: {healthPoints}";
            Destroy(collision.gameObject);
        }
    }
}

public class BallMovement
{
    public GameObject Ball { get; set; }
    public Vector3 Target { get; set; }
    public int Owner { get; set; }
}