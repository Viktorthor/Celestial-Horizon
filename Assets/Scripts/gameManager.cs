using UnityEngine;

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

}
