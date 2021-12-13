using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

abstract public class Enemy : MonoBehaviour
{
    protected static Player player;
    protected static Vector3 playerPos;
    protected static EnemyHandler enemyHandler;

    protected int speed;
    protected int hp = 1;

    protected Vector2 dir = Vector2.zero;
    protected float collRadius = 0;
    protected Collider2D coll;
    protected Rigidbody2D body;

    protected Vector3 movementVector = new Vector3(0,0,0);

    protected bool isInMinimum = false;


    abstract public void move();

    public Collider2D getColl()
    {
        return coll;
    }

    protected void getHit(Collider2D collision)
    {
        if (collision.gameObject.name == "Arm" && player)
        {
            if(player.spinning) loseHP();
        }
    }

    protected void hit(Collision2D collision)
    {
        if (collision.gameObject.name == "Player") player.die();
        else if (collision.gameObject.name == "Arm" && player.spinning) loseHP();
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        hit(collision);
    }

    protected void loseHP()
    {
        hp -= 1;
        if (hp <= 0) die();
    }
    
    protected void die()
    {
        Destroy(gameObject);
    }

    protected static void findPlayer()
    {
        player = FindObjectOfType<Player>();
    }

    //finds the player position
    protected static void findPlayerPos()
    {
        if (player) playerPos = player.transform.position;
        else playerPos = new Vector3(-1, -1, -1);
    }

    protected abstract void findRad();

    protected abstract Enemy getSelfAsEnemy();

    public float getCollRadius()
    {
        return collRadius;
    }

    public void addToMoveVector(Vector3 vect)
    {
        movementVector += vect;
    }

    public void resetMoveVector()
    {
        movementVector = Vector3.zero;
    }


    public Vector3 getMoveVector()
    {
        return movementVector;
    }

    public Rigidbody2D getBody()
    {
        return body;
    }

    public Vector2 getDir()
    {
        return dir;
    }

    public void setIsInMinimum(bool min)
    {
        isInMinimum = min;
    }
    
}
