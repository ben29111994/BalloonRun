using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    public GameObject player;
    public GameObject explodeEffect;
    bool isAdded = false;

    void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            player.GetComponent<Player>().OnScoring(other.transform.position);
            var explode = Instantiate(explodeEffect, transform.position, Quaternion.identity);
            other.gameObject.SetActive(false);
            Destroy(gameObject);
        }
        if (other.gameObject.tag == "Obstacle")
        {
            Player.combo = 0;
            var explode = Instantiate(explodeEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            Player.combo = 0;
            var explode = Instantiate(explodeEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        if(onScreen && !isAdded)
        {
            Player.trapList.Add(this.gameObject);
            isAdded = true;
            Debug.Log(Player.trapList.Count);
        }
        else if(isAdded && !onScreen)
        {
            Player.trapList.Remove(this.gameObject);
        }
    }
}
