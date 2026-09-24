using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public GameObject prefab;
    }

    [SerializeField] List<Entry> entries = new();      

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 5));
            GameObject prefab = entries[Random.Range(0,2)].prefab;
            //if (prefab == null) continue;
            if (prefab == entries[0].prefab)
            {
                Instantiate(prefab, new Vector3(15, -1.5f), Quaternion.identity);
            }
            if (prefab == entries[1].prefab)
            {
                Instantiate(prefab, new Vector3(15, -2.25f), Quaternion.identity);
            }
        }
    }
}