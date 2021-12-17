//The only enemy type in the game right now. Moves directly at the player.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LinearEnemy : Enemy
{


    void Start()
    {
        body = GetComponent<Rigidbody2D>(); //get the body
        speed = 3; //set the speed

        coll = GetComponent<Collider2D>(); //get the collision shape
        if (coll != null) findRad(); //if there is a collision shape, get its radius

        findPlayer(); //get the player
    }

    //Moves to the player
    public override void move()
    {
        
        Vector3 dir = movementVector.normalized; //movement vector is set by the enemy player. This gets the normalized version of it
        

        Vector3 vect = dir * speed; //Sets the magnitude of the velocity to speed (a constant)
        
        //if the enemy is not stuck in a local minimum, rotates in the direction of movement, otherwise looks at the player
        if (!isInMinimum) body.rotation = Vector2.SignedAngle(Vector2.right, dir);
        else
        {
            findPlayerPos();
            body.rotation = Vector2.SignedAngle(Vector2.right, ((Vector2)playerPos - body.position).normalized);
        }

        body.velocity = dir * speed; //sets the linear velocity of the enemy

    }

    //Returns itself as a generic enemy
    protected override Enemy getSelfAsEnemy()
    {
        return gameObject.GetComponent<LinearEnemy>();
    }

    //Returns the radius of the box collider
    protected override void findRad()
    {
        collRadius = GetComponent<BoxCollider2D>().bounds.extents.magnitude;
    }
}
