using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : Singleton<BallShooter>
{
    public Rigidbody ball;
    public float fireRate = 3.0f;
    private float nextFire = 0.0f;
    public float velocity = 5.0f;

    void Start()
    {

    }

    // Update is called once per frame

    void Update()
    {
        if (Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Rigidbody clone = (Rigidbody)Instantiate(ball, transform.position, transform.rotation);
            //clone.velocity = transform.forward * 10f;
            clone.velocity = new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * velocity;
            clone.position = new Vector3(0.0f, 0.0f, 1.0f);
            Destroy(clone.gameObject, 30);
        }
    }
}