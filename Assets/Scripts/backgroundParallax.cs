using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundParallax : MonoBehaviour
{

    public BoxCollider2D collider;

    public Rigidbody2D rb;

    private float height;

    public float scrollSpeed = -10f;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        height = collider.size.y;
        collider.enabled = false;

        rb.velocity = new Vector2(scrollSpeed, 0);
        ResetObstacle();

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -height)
        {
            Vector2 resetPosition = new Vector2(0, height * 2f);
            transform.position = (Vector2)transform.position + resetPosition;
            ResetObstacle();
        }

    }

    void ResetObstacle()
    {
        transform.GetChild(0).localPosition = new Vector3(0, Random.Range(-3, 3), 0);
    }
}
