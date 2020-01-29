using UnityEngine;

public class BallController : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            print("Hit!");
            if (this.gameObject != null)
            {
                Destroy(this.gameObject);
                return;
            }
        }
    }
}