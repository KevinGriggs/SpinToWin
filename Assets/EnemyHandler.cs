//The pathfinding handler. Finds the appropriate direction to travel for each enemy according to the obstacles that surround it and the player.
//Based off of potential fields and vector fields.
//Could be improved by using sampling for obstacle locations rather than center application in order to allow for irregularly shaped obstacles.
//Potential Fields: https://hal.inria.fr/hal-01405349/document
//Vector Fields: http://buildnewgames.com/vector-field-collision-avoidance/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    //Every Tick
    private void Update()
    {
        List<Enemy> enemies =  new List<Enemy>(FindObjectsOfType<Enemy>()); //Gets a list of all enemies
        List<GameObject> obstacles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Obstacle")); //Gets a list of all obstacles
        Player player = FindObjectOfType<Player>(); //Gets the player

        //for each enemy
        foreach(Enemy enemy in enemies) 
        {
            enemy.resetMoveVector(); //reset the movement vector

            Vector3 attractionDir = (player.transform.position - enemy.transform.position).normalized; //Get the direction to the player
            Vector3 perp = new Vector3(attractionDir.y, -attractionDir.x, 0); //Gets the vector perpendicular to the direction to the player
            Vector3 attraction = attractionDir * 1f; //Used for scaling attraction, no scaling applied here

            //for each obstacle
            foreach (GameObject obstacle in obstacles)
            {
                float obstRad = obstacle.GetComponent<Collider2D>().bounds.extents.magnitude; //get the radius of the obstacle
                Vector2 dir = (enemy.transform.position - obstacle.transform.position).normalized; //get the direction from the enemy to the obstacle

                //if the attraction vector is opposide the vector from the enemy to the obstacle, then add a vector
                //perpendicular to the vector from the enemy to the obstacle in the direction of the player 
                //(if the player is on the left, go left along the perpendicular vector)
                if (Vector2.Dot(attractionDir, dir) <= 0)
                {
                    //a vector perpendicular to the vector from the enemy to the obstacle in the direction of the player
                    dir = Vector2.Perpendicular(dir) * Mathf.Sign(Vector3.Dot(attractionDir, Vector2.Perpendicular(dir)));

                    //the distance from the edge of the obstacle to the enemy
                    float dist = (enemy.transform.position - obstacle.transform.position).magnitude - obstRad - enemy.getCollRadius();

                    Vector3 appliedVect;//The scaled vector to be added

                    //if the enemy is not on the obstacle, sets the scaled vector's magnitude to to be inversely proportional to distance
                    if (dist > 0) appliedVect = dir * (0.1f / dist);
                    else appliedVect = dir * 1000; //Else sets the magnitude to be 1000

                    enemy.addToMoveVector(appliedVect); //Adds the scaled to the enemy
                }
            }

            Vector3 vect = enemy.getMoveVector() + attraction; //Adds the attraction vector to a predicted vector

            //if the predicted magnitude is small, it is in a local minimum, otherwise it is not
            if (vect.magnitude < 0.1)
            {
                enemy.setIsInMinimum(true);
            }
            else enemy.setIsInMinimum(false);


            enemy.addToMoveVector(attraction); //adds the attraction vector to the enemy

            enemy.move(); //moves the enemy

        }

    }

}
