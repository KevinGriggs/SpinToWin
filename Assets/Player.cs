using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    private float maxSpeed = 3.5f;
    private float speed = 0f;
    private Vector2 dir = Vector2.zero;
    private float acc = 5f;
    private float aps;
    private int rotSpeed = -500;
    private int rotAcc = -1500;
    private Rigidbody2D body;
    private float delta;
    private Camera mainCamera;
    private Vector3 nextPosition;

    private int spinDir = 1;

    private Vector2 nextPos;
    //stamina used for spinning (in seconds)
    private float stamina = 2.00f;
    //Maximum stamina (in seconds)
    private float maxStamina = 2.00f;
    //Speed of stamina regen in stamina/second
    private float staminaRegen = 2.00f;

    //The minimum stam required to be regened before spinning can begin again
    private float spinThreshold = 2.00f;

    //True when out of stamina and regening
    private bool coolingDown = false;

    private Vector3 collNorm = Vector3.zero;
    private Vector3 movementVector = Vector3.zero;
    private bool colliding = false;
    bool sliding = false;
    public bool spinning { get; private set; } = false;
    private RectMask2D stamBarMask;
    private Image stamBar;

    private int numEnemies = 0;

    void Start()
    {
        nextPosition = transform.position;
        body = gameObject.GetComponent<Rigidbody2D>();
        delta = Time.fixedDeltaTime;
        aps = acc * delta;
        mainCamera = FindObjectOfType<Camera>();
        nextPos = transform.position;
        stamBarMask = GameObject.FindObjectOfType<RectMask2D>();
        stamBar = GameObject.Find("StaminaBar").GetComponent<Image>();

        numEnemies = GameObject.FindObjectsOfType<LinearEnemy>().Length;
    }

    private void FixedUpdate()
    {
        Vector2 mousePos = new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y);


        movementVector = (mousePos - body.position).normalized;

        Camera cam = FindObjectOfType<Camera>();
        Vector3 camTopCorner = cam.ViewportToWorldPoint(cam.rect.max, Camera.MonoOrStereoscopicEye.Mono);
        Vector3 camBottomCorner = cam.ViewportToWorldPoint(cam.rect.min, Camera.MonoOrStereoscopicEye.Mono);

        float distToSlow = (float)-0.5 * acc * Mathf.Pow(speed, 2) / Mathf.Pow(acc, 2) + Mathf.Pow(speed, 2) / acc;

        float distance = (mousePos - body.position).magnitude ;
        if(sliding)
        {
            float angleToPerp = Vector2.Angle(dir, movementVector);
            distance = Mathf.Cos(Mathf.Deg2Rad * angleToPerp) * distance;
        }

        dir = movementVector;


        if (distance > distToSlow && distance > 0.05)
            {
                if (speed + acc * delta < maxSpeed)
                {
                    speed = (speed + acc * delta);
                }
                else
                {
                    speed = maxSpeed;
                }
            }
            else
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

        float remainingDist = speed * delta;

        int slideCount = 0;

        while (remainingDist > 0 && slideCount < 10)
        {
            slideCount++;

            List<RaycastHit2D> results = new List<RaycastHit2D>();

            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Obstacle");
            filter.useLayerMask = true;

            body.Cast(dir, filter, results, remainingDist);
            

            if (results.Count > 0)
            {
                RaycastHit2D firstHit = new RaycastHit2D();

                Vector2 collisionNormalSum = Vector2.zero;
                sliding = true;
                foreach (RaycastHit2D hit in results)
                {
                    if (Vector2.Angle(dir, hit.normal) != 90)
                    {
                        if (firstHit == new RaycastHit2D()) firstHit = hit;
                    }
                    collisionNormalSum += hit.normal;

                }

                if (firstHit == new RaycastHit2D())
                {
                    break;
                }

                remainingDist -= (body.position - firstHit.centroid).magnitude;

                body.position = (firstHit.centroid);


                //if not to the straight on and not away from
                if (Vector2.Dot(-dir, collisionNormalSum.normalized) < 0.98 && Vector2.Dot(-dir, collisionNormalSum.normalized) > 0)
                {

                    dir = Mathf.Sign(Vector2.Dot(dir, Vector2.Perpendicular(firstHit.normal))) * Vector2.Perpendicular(firstHit.normal);
                    if (Vector2.Dot(dir, movementVector) <= 0)//if would slide backward
                    {
                        speed = 0;
                        remainingDist = 0;
                        sliding = false;
                    }
                }
                else if (Vector2.Dot(-dir, collisionNormalSum.normalized) <= 0) //if away from
                {
                    sliding = false;
                    break;
                }
                else //if straight on
                {
                    sliding = false;
                    speed = 0;
                    remainingDist = 0;
                }
            }
            else break;
        }
        if (slideCount == 10) remainingDist = 0;
        body.position = (body.position + dir * remainingDist);





        if (Input.GetAxis("Spin") > 0 && !coolingDown)
        {
            if (Mathf.Abs(body.angularVelocity) < Mathf.Abs(rotSpeed)) body.angularVelocity += rotAcc * delta;
            else body.angularVelocity = rotSpeed;

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
        else
        {
            if (body.angularVelocity < 0) body.angularVelocity -= rotAcc * delta;
            else
            {
                body.angularVelocity = 0;
            }

            if (stamina + staminaRegen * delta < maxStamina) stamina += staminaRegen * delta;
            else stamina = maxStamina;

            if (stamina >= spinThreshold) coolingDown = false;
        }
        if (body.angularVelocity != 0) spinning = true;
        else spinning = false;

        stamBarMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, stamina / maxStamina * 100);
        stamBarMask.rectTransform.ForceUpdateRectTransforms();

        if (coolingDown)
        {
            stamBar.color = new Color(152f/255f, 6f/255f, 0);
        }
        else
        {
            stamBar.color = new Color(0, 1, 0);
        }


        numEnemies = FindObjectsOfType<LinearEnemy>().Length;

        if (numEnemies == 0)
        {
            SceneManager.LoadScene("Win");
        }

    }

    public void die()
    {
        SceneManager.LoadScene(gameObject.scene.name);
    }

}
