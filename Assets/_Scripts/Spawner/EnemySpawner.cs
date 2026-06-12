using UnityEngine;
using System.Collections;
using System.Collections.Generic; // NOU: Folosim liste pentru a alege random

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Configuration")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public int maxEnemies = 40;          // NOU: Acum limita e 40 de inamici 
    public float spawnInterval = 5f;
    public float spawnRadius = 3.5f;     // Am mărit puțin raza de împrăștiere

    [Header("Intelligence Settings")]
    public float densityCheckRadius = 10f; // Am mărit raza de verificare a aglomerației
    public LayerMask enemyLayer;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

            if (currentEnemies < maxEnemies)
            {
                int missingAmount = maxEnemies - currentEnemies;

                for (int i = 0; i < missingAmount; i++)
                {
                    Transform bestPoint = GetLeastCrowdedSpawnPoint();
                    if (bestPoint != null)
                    {
                        // Spawnează într-o zonă circulară random în jurul punctului
                        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                        Vector3 finalSpawnPosition = bestPoint.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                        Instantiate(enemyPrefab, finalSpawnPosition, Quaternion.identity);
                    }
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Transform GetLeastCrowdedSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;

        int lowestEnemyCount = int.MaxValue;
        List<Transform> bestPoints = new List<Transform>();

        // Trecem prin toate punctele să vedem cât de aglomerate sunt
        foreach (Transform point in spawnPoints)
        {
            Collider2D[] enemiesNearby = Physics2D.OverlapCircleAll(point.position, densityCheckRadius, enemyLayer);
            int count = enemiesNearby.Length;

            if (count < lowestEnemyCount)
            {
                // Am găsit un nou record minim! Ștergem lista și adăugăm acest punct.
                lowestEnemyCount = count;
                bestPoints.Clear();
                bestPoints.Add(point);
            }
            else if (count == lowestEnemyCount)
            {
                // Este la egalitate cu minimul, îl punem și pe el în urnă pentru tragerea la sorți
                bestPoints.Add(point);
            }
        }

        // Extragem random un punct dintre cele câștigătoare
        if (bestPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, bestPoints.Count);
            return bestPoints[randomIndex];
        }

        return spawnPoints[0];
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(point.position, densityCheckRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, spawnRadius);
        }
    }
}