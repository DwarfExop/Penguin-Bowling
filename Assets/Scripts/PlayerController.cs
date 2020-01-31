using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Quaternion youPlayerRot;
    private Vector3 youTargetPosition;

    private Client client;
    private int healthPoints = 100;
    private PlayerMovement otherMovementData;

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
        CheckWin();
        GetKeyInputs();
        GetMessageData();
        UpdateBalls();
        MovePlayers();

    }

    private void CheckWin()
    {
        if (GameObject.FindGameObjectWithTag("Other") == null)
        {
            Debug.Log("You won!");
            Application.Quit();
        }
    }

    private void GetKeyInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetTargetPosition();
        }
        if (Input.GetKeyDown("space"))
        {
            FireBall();
        }
    }

    private void GetMessageData()
    {
        object data = client.CheckMessages();

        if (data != null)
        {
            if (data.GetType() == typeof(PlayerMovement))
            {
                otherMovementData = (PlayerMovement)data;
            }
            else if (data.GetType() == typeof(BallMovement))
            {
                CreateBall(other, (BallMovement)data);
            }
        }
    }

    private void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            youTargetPosition = hit.point;
            youPlayerRot = Quaternion.LookRotation(Vector3.forward, (youTargetPosition - transform.position) * -1);

            client.SendMovement(youTargetPosition, youPlayerRot, transform.position);
        }
    }

    private void FireBall()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            Vector3 normalized = (hit.point - transform.position).normalized;
            var target = transform.position + normalized * 30;
            CreateBall(gameObject, new BallMovement(target));
            client.SendBall(target);
        }
    }

    private void CreateBall(GameObject gameObject, BallMovement ballMovement)
    {
        var newBall = Instantiate(ball);
        newBall.SetActive(true);

        var ballComponent = newBall.GetComponent<Ball>();
        ballComponent.OwnerId = gameObject.GetInstanceID();

        newBall.transform.position = gameObject.transform.position;
        ballMovement.ball = newBall;
        balls.Add(ballMovement);
    }
    private void UpdateBalls()
    {
        var toRemoves = new List<BallMovement>();
        foreach (var movement in balls)
        {
            GameObject ball = movement.ball;
            if (ball == null)
            {
                toRemoves.Add(movement);
                continue;
            }

            ball.transform.position = Vector3.MoveTowards(ball.transform.position, movement.targetPosition, ballSpeed * Time.deltaTime);

            if (ball.transform.position == movement.targetPosition)
            {
                Destroy(ball.gameObject);
                toRemoves.Add(movement);
            }
        }
        toRemoves.ForEach(t => balls.Remove(t));
    }

    private void MovePlayer(GameObject player, Quaternion rotation, Vector3 targetPosition)
    {
        player.transform.rotation = rotation;
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void MovePlayers()
    {
        if (youTargetPosition != default && youTargetPosition != transform.position)
        {
            MovePlayer(gameObject, youPlayerRot, youTargetPosition);
        }

        if (otherMovementData != default && otherMovementData.targetPosition != other.transform.position)
        {
            MovePlayer(other, otherMovementData.playerRot, otherMovementData.targetPosition);
        }
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
            if (healthPoints == 0)
            {
                Destroy(gameObject);
                Debug.Log("You Lost!");
                Application.Quit();
            }
        }
    }
}