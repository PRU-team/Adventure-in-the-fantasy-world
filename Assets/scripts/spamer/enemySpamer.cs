using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector3[] spawnPositions;
    [SerializeField] private float activationRange = 50f;
    [SerializeField] private float despawnRange = 100f;
    [SerializeField] private float spawnCooldown = 2f;
    [SerializeField] private int maxEnemies = 5;

    [Header("References")]
    [SerializeField] private Transform player;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isSpawning = false;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (spawnPositions.Length == 0)
        {
            spawnPositions = new Vector3[]
            {
                Vector3.zero, new Vector3(2,0,0), new Vector3(-2,0,0), new Vector3(0,0,2), new Vector3(0,0,-2)
            };
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= activationRange)
        {
            if (!isSpawning)
                StartCoroutine(SpawnLoop());
        }
        else
        {
            isSpawning = false;
        }

        CleanupDeadEnemies();
    }

    private IEnumerator SpawnLoop()
    {
        isSpawning = true;
        while (isSpawning)
        {
            if (spawnedEnemies.Count < maxEnemies)
                SpawnEnemy();

            yield return new WaitForSeconds(spawnCooldown);
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPos = transform.TransformPoint(spawnPositions[Random.Range(0, spawnPositions.Length)]);
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);

        EnemyController ec = newEnemy.GetComponent<EnemyController>();
        if (ec != null && player != null)
            ec.SetTarget(player);
    }

    private void CleanupDeadEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = spawnedEnemies[i];
            if (enemy == null)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(enemy.transform.position, player.position);
            if (dist > despawnRange)
            {
                Destroy(enemy);
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, despawnRange);
    }
}
