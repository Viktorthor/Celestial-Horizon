using UnityEngine;
using UnityEngine.SceneManagement;

public class gameManager : MonoBehaviour
{
    public GameObject player;
    public Vector2 startPos;
    void Start()
    {
        if(GameObject.Find("Player") == null)
        {
            Instantiate(player, startPos, Quaternion.identity);
        }
    }

    public void NewGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void gameOver()
    {
        //Set gameover canvas overlay to true
        //time.deltatime = 0f etc.
    }

}
