//The player character. Collides with certain objects and moves along them. Moves toward the cursor.
//Accelerates and decelerates, but changes direction instantly.
//Spins to attack (with acceleration and deceleration).
//Resets level on death.
//Collision sliding inspired by Gotot engine's slide function


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    private float maxSpeed = 3.5f; //the max move speed
    private float speed = 0f; //the current move speed
    private Vector2 dir = Vector2.zero; //the movement direction post slide
    private float acc = 5f; //the movement acceleration rate

    private int rotSpeed = -500; //the max rotation speed
    private int rotAcc = -1500; //the rotation acceleration rate

    private Rigidbody2D body; //the rigidbody

    private float delta;

    private Camera mainCamera; //the game camera (used for getting cursor location

    //stamina used for spinning (in seconds)
    private float stamina = 2.00f;
    //Maximum stamina (in seconds)
    private float maxStamina = 2.00f;
    //Speed of stamina regen in stamina/second
    private float staminaRegen = 2.00f;
    //The minimum stam required to be regened before spinning can begin again
    private float spinThreshold = 2.00f;
    //True when out of stamina and regening, prevents spin
    private bool coolingDown = false;
    public bool spinning { get; private set; } = false; //true if spinning

    private Vector3 movementVector = Vector3.zero;//The current movement vector (used for collisions)
    private bool colliding = false; //true if colliding
    bool sliding = false; //true if sliding along an edge

    private RectMask2D stamBarMask;//The stamina bar's mask
    private Image stamBar;//The stamina bar

    private int numEnemies = 0; //The number of enemies on the map, used to trigger the end of the game


    void Start()
    {
        body = gameObject.GetComponent<Rigidbody2D>();
        delta = Time.fixedDeltaTime;
        mainCamera = FindObjectOfType<Camera>();
        stamBarMask = GameObject.FindObjectOfType<RectMask2D>();
        stamBar = GameObject.Find("StaminaBar").GetComponent<Image>();

        numEnemies = GameObject.FindObjectsOfType<LinearEnemy>().Length; //gets the number of enemies
    }

    //every fixed duration tick
    private void FixedUpdate()
    {
        //gets the mouse position
        Vector2 mousePos = new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y);
        
        //Sets the movement vector to point toward the cursor
        movementVector = (mousePos - body.position).normalized;

        //Determines the distance at which the character begins slowing so that they reach the cursor then stop
        float distToSlow = (float)-0.5 * acc * Mathf.Pow(speed, 2) / Mathf.Pow(acc, 2) + Mathf.Pow(speed, 2) / acc;

        //The distance from the player to the mouse
        float distance = (mousePos - body.position).magnitude;

        //if the player was sliding last tick, sets the distance to match the distance to the edge (so it decelerates along the edge)
        if(sliding)
        {
            float angleToPerp = Vector2.Angle(dir, movementVector);
            distance = Mathf.Cos(Mathf.Deg2Rad * angleToPerp) * distance;
        }

        dir = movementVector; //initializes dir as the movement vector

        //if not at deceleration distance accelerate or move at max speed
        if (distance > distToSlow && distance > 0.05)
        {
            //if not at max speed, accelertate
            if (speed + acc * delta < maxSpeed)
            {
                speed = (speed + acc * delta);
            }
            else //continue at max speed
            {
                speed = maxSpeed;
            }
        }
        else //else decelerate or stop
        {
            if (acc * delta < speed && distance > 0.05)
            {
                speed = speed - acc * delta;
            }
            else
            {
                speed = 0;
                movementVector = Vector3.zero;
            }
        }

        float remainingDist = speed * delta; //used for collisions, initializes at current speed, the remaining distance to travel

        int slideCount = 0; //used to prevent infinite loop

        //The collision handler loop
        while (remainingDist > 0 && slideCount < 10)
        {
            slideCount++;

            List<RaycastHit2D> results = new List<RaycastHit2D>(); //holds the results of a shapecast that predicts collisions

            //Sets the cast to only hit obstacles
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Obstacle");
            filter.useLayerMask = true;

            //performs the shapecast
            body.Cast(dir, filter, results, remainingDist);
            
            //If there would be a collision
            if (results.Count > 0)
            {
                RaycastHit2D firstHit = new RaycastHit2D();

                Vector2 collisionNormalSum = Vector2.zero; //holds the sum of collision normals

                sliding = true;

                //for each collision, adds the collision normal to the sum
                foreach (RaycastHit2D hit in results)
                {
                    //if the hit is not parallel and there is not already a first hit, sets this collision as the first hit
                    if (Vector2.Angle(dir, hit.normal) != 90)
                    {
                        if (firstHit == new RaycastHit2D()) firstHit = hit;
                    }

                    collisionNormalSum += hit.normal;

                }

                //if there was no actual hit (only perpendicular ones), break
                if (firstHit == new RaycastHit2D())
                {
                    break;
                }

                //subtract the distance from the player to the obstacle from the remaining distance
                remainingDist -= (body.position - firstHit.centroid).magnitude;

                //move the player to the obstacle
                body.position = (firstHit.centroid);


                //if collision is not straight on and not away from the obstacle
                if (Vector2.Dot(-dir, collisionNormalSum.normalized) < 0.98 && Vector2.Dot(-dir, collisionNormalSum.normalized) > 0)
                {
                    //gets the tangent of the hit in the direction of the movement, carries into next loop iteration
                    dir = Mathf.Sign(Vector2.Dot(dir, Vector2.Perpendicular(firstHit.normal))) * Vector2.Perpendicular(firstHit.normal);

                    //if the player would slide backward, stop
                    if (Vector2.Dot(dir, movementVector) <= 0)
                    {
                        speed = 0;
                        remainingDist = 0;
                        sliding = false;
                    }
                }
                else if (Vector2.Dot(-dir, collisionNormalSum.normalized) <= 0) //if the player is moving away from the obstacle, ignore the collision
                {
                    sliding = false;
                    break;
                }
                else //if the player is colliding perpendicularly, stop
                {
                    sliding = false;
                    speed = 0;
                    remainingDist = 0;
                }
            }
            else break; //if there was no collision, continue on a normal path
        }

        if (slideCount == 10) remainingDist = 0; //if caught in an infinite slide loop, stop

        body.position = (body.position + dir * remainingDist); //moves the player toward in new direction at the remaining distance in length

        //if the player is providing left clicking and spin is not cooling down
        if (Input.GetAxis("Spin") > 0 && !coolingDown)
        {
            //if not at max spin velocity, accelerate spin
            if (Mathf.Abs(body.angularVelocity) < Mathf.Abs(rotSpeed)) body.angularVelocity += rotAcc * delta;
            else body.angularVelocity = rotSpeed; //else continue at max spin speed

            //drain stamina, if stamina reaches 0, activate cooldown to prevent spin
            if (stamina - delta > 0)
            {
                stamina -= delta;
                
            }
            else
            {
                stamina = 0;
                coolingDown = true;
            }

        }
        else //if no spin input is provided, decelerate spin and regen stamina
        {
            if (body.angularVelocity < 0) body.angularVelocity -= rotAcc * delta; //if would not change spin direction, decelerate
            else //stop
            {
                body.angularVelocity = 0;
            }

            //if not at max stamina, add stamina
            if (stamina + staminaRegen * delta < maxStamina) stamina += staminaRegen * delta;
            else stamina = maxStamina; //set stamina to max

            if (stamina >= spinThreshold) coolingDown = false; //if stamina threshold reached, deactivate cooldown
        }

        //used by enemy for collisions
        if (body.angularVelocity != 0) spinning = true;
        else spinning = false;

        //Sets the mask transform for the stamina bar
        stamBarMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, stamina / maxStamina * 100);
        stamBarMask.rectTransform.ForceUpdateRectTransforms();

        //Sets the stamina bar's color (green when the player can spin, red while on cooldown)
        if (coolingDown)
        {
            stamBar.color = new Color(152f/255f, 6f/255f, 0);
        }
        else
        {
            stamBar.color = new Color(0, 1, 0);
        }

        //gets the number of enemies, and if the last one has been defeated, swaps to the win screen
        numEnemies = FindObjectsOfType<LinearEnemy>().Length;

        if (numEnemies == 0)
        {
            SceneManager.LoadScene("Win");
        }

    }

    //resets the scene on death
    public void die()
    {
        SceneManager.LoadScene(gameObject.scene.name);
    }

}
