using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LinearEnemy : Enemy
{

    void Start()
    {
        enemyHandler = FindObjectOfType<EnemyHandler>();
        body = GetComponent<Rigidbody2D>();
        speed = 3;

        coll = GetComponent<Collider2D>();
        if (coll != null) findRad();

        findPlayer();
    }

    void Update()
    {
        //findPlayerPos();

       // move(findPathToPlayer());
    }

    public override void move()
    {
        
        Vector3 dir = movementVector.normalized;
        

        Vector3 vect = dir * speed;

        if (!isInMinimum) body.rotation = Vector2.SignedAngle(Vector2.right, dir);
        else
        {
            findPlayerPos();
            body.rotation = Vector2.SignedAngle(Vector2.right, ((Vector2)playerPos - body.position).normalized);
        }


        body.velocity = dir * speed;

    }

    protected override Enemy getSelfAsEnemy()
    {
        return gameObject.GetComponent<LinearEnemy>();
    }

    protected override void findRad()
    {
        collRadius = GetComponent<BoxCollider2D>().bounds.extents.magnitude;
    }
}
