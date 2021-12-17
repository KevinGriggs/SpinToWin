//This class defines a generic enemy with collisions, damage, death, and movement vector, but not its actual pathfinding or movement.

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

abstract public class Enemy : MonoBehaviour
{
    protected static Player player; //Holds a reference to the player object
    protected static Vector3 playerPos; //The player's position

    protected int speed; //The speed of the enemy
    protected int hp = 1; //The health of the enemy

    protected Vector2 dir = Vector2.zero; //The movement direction
    protected float collRadius = 0; //The radius of the collision shape of the enemy
    protected Collider2D coll; //The collision shape of the enmy
    protected Rigidbody2D body; // The enemies rigidbody

    protected Vector3 movementVector = new Vector3(0,0,0); //The movement vector of the enemy determined by pathfinding and used to generate dir

    protected bool isInMinimum = false; //Whether or not the enemy is in a local minimum in pathfinding, used for preventing akward rotation

    //An abstract function for the move function of the enemy types
    abstract public void move();

    /* Begin Accessors */
    public Collider2D getColl()
    {
        return coll;
    }

    public float getCollRadius()
    {
        return collRadius;
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

    /* End Accessors */


    //Damages the enemy when hit by the player weapon if the player is spinning 
    protected void getHit(Collider2D collision)
    {
        if (collision.gameObject.name == "Arm" && player)
        {
            if(player.spinning) loseHP();
        }
    }

    //Kills the player on collision with the player or damages self on collision with the player weapon
    protected void hit(Collision2D collision)
    {
        if (collision.gameObject.name == "Player") player.die();
        else if (collision.gameObject.name == "Arm" && player.spinning) loseHP();
    }

    //Kills the player on collision with the player or damages self on collision with the player weapon
    private void OnCollisionStay2D(Collision2D collision)
    {
        hit(collision);
    }

    //Subtracts 1 hp
    protected void loseHP()
    {
        hp -= 1;
        if (hp <= 0) die();
    }
    
    //Destroys the enemy object on death
    protected void die()
    {
        Destroy(gameObject);
    }

    //Gets the player object
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

    //finds the radius of the collision shape
    protected abstract void findRad();

    //Used to get other enemy types as an Enemy. Used by the pathfinding handler for gathering all enemies in a single list.
    protected abstract Enemy getSelfAsEnemy();

    //Adds an input vector to the movement vector
    public void addToMoveVector(Vector3 vect)
    {
        movementVector += vect;
    }
   
    //Sets the movement vector to 0
    public void resetMoveVector()
    {
        movementVector = Vector3.zero;
    }
    
}
