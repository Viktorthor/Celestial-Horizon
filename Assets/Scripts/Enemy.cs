using UnityEngine;
using BulletPro;
public class Enemy : MonoBehaviour
{
    public int health;
    public BulletEmitter powerUp;
    public float dropRate = 0.2f;

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
