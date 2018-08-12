using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public AICharacter[] aiCharacters;
    public bool enemySpawnerEnabled;
    public float enemySpawnerRate;

    private bool enemySpawnerRunning;
    private SpawnManager spawnManager;

    void Start ()
    {
        spawnManager = SpawnManager.instance;
    }

    void Update ()
    {
        if (!enemySpawnerRunning && enemySpawnerEnabled)
        {
            StartCoroutine("EnemySpawner");
        }
        else if (enemySpawnerRunning && enemySpawnerEnabled == false)
        {
            enemySpawnerRunning = false;
            StopCoroutine("EnemySpawner");
        }
    }

    [System.Serializable]
    public class AICharacter
    {
        public Character character;
        [Range(0f, 100f)] public float spawnChance;
    }

    IEnumerator EnemySpawner ()
    {
        enemySpawnerRunning = true;

        while (enemySpawnerEnabled)
        {
            yield return new WaitForSeconds(enemySpawnerRate);
            Character enemy = Instantiate(GetRandomCharacter(), spawnManager.base2.transform);
            enemy.transform.position = new Vector3(spawnManager.base2.transform.position.x - spawnManager.spawnOffsetFromBase.x,
                                                   spawnManager.base2.transform.position.y + spawnManager.spawnOffsetFromBase.y,
                                                   spawnManager.base2.transform.position.z + spawnManager.spawnOffsetFromBase.z);
            enemy.tag = "Player 2";
        }

        enemySpawnerRunning = false;
    }

    Character GetRandomCharacter ()
    {
        float spawnChanceTotal = 0;

        for (int i = 0; i < aiCharacters.Length; i++)
            spawnChanceTotal += aiCharacters[i].spawnChance;

        if (spawnChanceTotal <= 0)
        {
            Debug.LogWarning("There is no AI character with a chance to spawn!");
            return null;
        }

        float n = Random.Range(0f, spawnChanceTotal);

        for (int i = 0; i < aiCharacters.Length; i++)
        {
            if (i == 0)
            {
                if (n > 0f && n <= aiCharacters[i].spawnChance)
                    return aiCharacters[i].character;
            }
            else
            {
                float total = 0f;

                for (int j = 0; j < i; j++)
                    total += aiCharacters[j].spawnChance;

                if (n > total && n <= aiCharacters[i].spawnChance + total)
                    return aiCharacters[i].character;
            }
        }

        Debug.LogError("GetRandomCharacter failed!");
        return null;
    }
}