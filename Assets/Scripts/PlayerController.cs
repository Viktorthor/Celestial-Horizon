using UnityEngine;
using BulletPro;

public class PlayerController : MonoBehaviour
{
    //Public variables
    public float moveSpeed = 1;
    public int health;
    //WIP - Set borders dynamically
    public float HorizontalBorder = 2.33f;
    public float VerticalBorder = 4.78f;
    public Vector3 moveAmount;
    public EmitterProfile defaultShot;
    public EmitterProfile playerPowerUp;
    public float powerUpDuration;
    public gameManager gameMan;

    //Private or other variables
    BulletEmitter currentProfile;
    private bool hasPowerUp = false;
    float powerUpReset;

    private void Start()
    {
        currentProfile = GetComponent<BulletEmitter>();
        powerUpReset = powerUpDuration;
        gameMan = GetComponent<gameManager>();
    }

    private void Update()
    {
        //Movement, simple yet effective
        moveAmount += new Vector3(Input.GetAxisRaw("Horizontal") * Time.deltaTime * moveSpeed, Input.GetAxisRaw("Vertical") * Time.deltaTime * moveSpeed, 0);
        Vector3 moveDiff = moveAmount * Time.deltaTime * 30;
        transform.position += moveDiff;
        moveAmount -= moveDiff;

        //Swap out for power up
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            powerUp();
            hasPowerUp = true;
        }

        if(hasPowerUp == true)
        {
            if (powerUpDuration <= 0.0f)
            {
                currentProfile.Pause();
                currentProfile.SwitchProfile(defaultShot);
                hasPowerUp = false;
                powerUpDuration = powerUpReset;
            }
            else
            {
                powerUpDuration -= Time.deltaTime;
                Debug.Log(powerUpDuration);
            }
        }

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
        gameMan.gameOver();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            getHit();
        }
    }

    void powerUp()
    {
        currentProfile.SwitchProfile(playerPowerUp);
    }

}
