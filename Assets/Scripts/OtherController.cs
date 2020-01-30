using UnityEngine;
using UnityEngine.UI;

public class OtherController : MonoBehaviour
{
    public GameObject health;
    private int healthPoints = 100;

    private void OnCollisionEnter(Collision collision)
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