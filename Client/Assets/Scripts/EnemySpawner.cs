using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject parent;
    public GameObject enemy;
    public GameObject player;
    public float spawnDelay;

    private bool isSpawnReady = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Spawn();
    }

    // parent의 자식 오브젝트로 추가, 스폰시 따라갈 player 지정(추가 예정)
    void Spawn()
    {
        if (isSpawnReady)
        {
            float range_X = Random.Range(-10.0f, 10.0f);
            float range_Z = Random.Range(-10.0f, 10.0f);
            
            GameObject instance = Instantiate(enemy, parent.transform);
            instance.transform.position = new Vector3(range_X, 0.5f, range_Z);
            instance.GetComponent<Enemy>().player = player.GetComponent<Player>();
            
            StartCoroutine(SpawnDelay());
        }
    }

    // 스폰 딜레이
    IEnumerator SpawnDelay()
    {
        isSpawnReady = false;
        yield return new WaitForSeconds(spawnDelay);

        isSpawnReady = true;
    }
}
