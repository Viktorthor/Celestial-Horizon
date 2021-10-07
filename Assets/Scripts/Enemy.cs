using UnityEngine;
using BulletPro;
public class Enemy : MonoBehaviour
{
    public int health;
    public EmitterProfile powerUp;
    BulletEmitter currentProfile;
    public float dropRate = 0.2f;

    private void Start()
    {
        currentProfile = GetComponent<BulletEmitter>();
    }

    public void getHit()
    {
        health--;
        if (health < 1) killEnemy();
    }

    public void killEnemy()
    {
        /*if(Random.value < 0.5)
        {
            
        } */
        currentProfile.SwitchProfile(powerUp);
        currentProfile.Play();
        Destroy(gameObject);
    }
}
