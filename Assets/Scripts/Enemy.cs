using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour {
    public GameObject player;
    public GameObject bulletPrefab;
    public GameObject bulletHitPrefab;
    public GameObject smokePrefab;
    float randomSpeed;
    bool isShooting = false;
    public static float maxSpeed = 25;

    // Use this for initialization
    void OnEnable () {
        isShooting = false;
        player = GameObject.FindGameObjectWithTag("Player");
        maxSpeed = (25 + Player.distance / 100);
        if(maxSpeed > 40)
        {
            maxSpeed = 40;
        }
        randomSpeed = Random.Range(17.5f, maxSpeed);
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(transform.position, player.transform.position) < 25)
        {
            var rot = transform.eulerAngles;
            if (transform.position.x < 5)
            {
                if(rot.z > -30)
                {
                    rot.z -= 5*Time.deltaTime;
                }
            }
            else if (transform.position.x > 5)
            {
                if (rot.z < 30)
                {
                    rot.z += 5*Time.deltaTime;
                }
            }
            transform.eulerAngles = rot;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(player.transform.position - transform.position), Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, randomSpeed * Time.deltaTime);
            if (!isShooting)
            {
                StartCoroutine(fireRate());
                isShooting = true;
            }
        }
        else
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, player.transform.position.z), randomSpeed*Time.deltaTime);
    }

    IEnumerator fireRate()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.05f);
            var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.transform.DOMove(player.transform.position, 0.05f);
            yield return new WaitForSeconds(0.05f);
            var bulletHit = Instantiate(bulletHitPrefab, player.transform.position, Quaternion.identity);
            Destroy(bullet);
        }
        Player.playerLife -= 1;
        if (Player.playerLife <= 2)
        {
            var smoke = Instantiate(smokePrefab, player.transform.position, Quaternion.identity);
            smoke.transform.parent = player.transform;
            smoke.transform.localPosition = Vector3.zero;
        }
        if (Player.playerLife <= 0)
        {
            var airCraft = player.transform.GetChild(0);
            Destroy(airCraft.gameObject);
            player.GetComponent<Player>().OnDie();
        }
        yield return new WaitForSeconds(2f);
        StartCoroutine(fireRate());
    }
}
