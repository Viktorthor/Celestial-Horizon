using UnityEngine;
using BulletPro;

public class PlayerController : MonoBehaviour
{
    //Public variables
    public float moveSpeed = 1;
    public int health = 10;
    //WIP - Set borders dynamically
    public float HorizontalBorder = 2.33f;
    public float VerticalBorder = 4.78f;
    public Vector3 moveAmount;

    BulletEmitter currentProfile;
    public EmitterProfile playerPowerUp;

    private void Start()
    {
        currentProfile = GetComponent<BulletEmitter>();
    }

    private void Update()
    {
        //Movement, simple yet effective
        moveAmount += new Vector3(Input.GetAxisRaw("Horizontal") * Time.deltaTime * moveSpeed, Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeed, 0);
        Vector3 moveDiff = moveAmount * Time.deltaTime * 30;
        transform.position += moveDiff;
        moveAmount -= moveDiff;

        //Swap out for power up
       /* if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentProfile.SwitchProfile(playerPowerUp, false);
        }
       */

        //World Boundaries
        //left
        if (transform.position.x < -HorizontalBorder) transform.position = new Vector3(-HorizontalBorder, transform.position.y, transform.position.z);
        //right
        if (transform.position.x > HorizontalBorder) transform.position = new Vector3(HorizontalBorder, transform.position.y, transform.position.z);
        //bottom
        if (transform.position.y < -VerticalBorder) transform.position = new Vector3(transform.position.x, -VerticalBorder,  transform.position.z);
        //top
        if (transform.position.y > VerticalBorder) transform.position = new Vector3(transform.position.x, VerticalBorder, transform.position.z);
    }

    public void getHit()
    {
        health--;
        if (health < 1) gameOver();
    }

    public void gameOver()
    {
        Destroy(gameObject);
    }

}
