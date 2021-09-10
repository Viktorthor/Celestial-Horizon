using UnityEngine;

public class TouchPlayerController : MonoBehaviour
{

    private Vector3 touchPos;
    private Rigidbody2D rb;
    private Vector3 direction;
    private float moveSpeed = 10f;

    public float HorizontalBorder = 2.33f;
    public float VerticalBorder = 4.78f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPos = Camera.main.ScreenToWorldPoint(touch.position);
            touchPos.z = 0;
            direction = (touchPos - transform.position);
            rb.velocity = new Vector2(direction.x, direction.y) * moveSpeed;

            if (touch.phase == TouchPhase.Ended)
                rb.velocity = Vector2.zero;
        }

        //World Boundaries
        //left
        if (transform.position.x < -HorizontalBorder) transform.position = new Vector3(-HorizontalBorder, transform.position.y, transform.position.z);
        //right
        if (transform.position.x > HorizontalBorder) transform.position = new Vector3(HorizontalBorder, transform.position.y, transform.position.z);
        //bottom
        if (transform.position.y < -VerticalBorder) transform.position = new Vector3(transform.position.x, -VerticalBorder, transform.position.z);
        //top
        if (transform.position.y > VerticalBorder) transform.position = new Vector3(transform.position.x, VerticalBorder, transform.position.z);
    }
}
