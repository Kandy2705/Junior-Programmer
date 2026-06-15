using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerX : MonoBehaviour
{
    public GameObject[] ballPrefabs;

    private float spawnLimitXLeft = -22;
    private float spawnLimitXRight = 7;
    private float spawnPosY = 30;

    private float startDelay = 1.0f;
    private float minSpawnInterval = 3.0f;
    private float maxSpawnInterval = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("SpawnRandomBall", startDelay);
    }

    // Spawn random ball at random x position at top of play area
    void SpawnRandomBall()
    {
        int ballIndex = Random.Range(0, ballPrefabs.Length);

        Vector3 spawnPos = new Vector3(
            Random.Range(spawnLimitXLeft, spawnLimitXRight),
            spawnPosY,
            0
        );

        Instantiate(
            ballPrefabs[ballIndex],
            spawnPos,
            ballPrefabs[ballIndex].transform.rotation
        );

        float randomDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
        Invoke("SpawnRandomBall", randomDelay);
    }

}
