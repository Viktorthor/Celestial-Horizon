using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject enemy;
    public float startTimeBtwSpawns;
    public int enemyCount;
    float timeBtwSpawns;

    private void Start()
    {
        timeBtwSpawns = startTimeBtwSpawns;
    }

    private void Update()
    {
        if(enemyCount > 0)
        {
            if (timeBtwSpawns <= 0)
            {
                Vector3 spawnPosition = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
                Instantiate(enemy, spawnPosition, Quaternion.identity);
                timeBtwSpawns = (startTimeBtwSpawns - Random.value);
                enemyCount--;
            }
            else
            {
                timeBtwSpawns -= Time.deltaTime;
            }
        }
        
    }

}
