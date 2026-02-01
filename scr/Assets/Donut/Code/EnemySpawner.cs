using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    public int maxEnemies = 5;
    public float spawnInterval = 3f;

    private int currentEnemyCount = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy = Instantiate(
            enemyPrefab,
            point.position,
            point.rotation
        );

        currentEnemyCount++;

        // ⭐ เชื่อมกับ Health
        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            hp.onDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        currentEnemyCount--;
    }
}

