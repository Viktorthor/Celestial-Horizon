using UnityEngine;
using BulletPro;
public class Enemy : MonoBehaviour
{
    public int health;

    public void getHit()
    {
        health--;
        if (health < 1) killEnemy();
    }

    public void killEnemy()
    {
        Destroy(gameObject);
    }
}
