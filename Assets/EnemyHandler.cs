using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    private Camera cam;
    public Vector3 camTopCorner { get; private set; }
    public Vector3 camBottomCorner { get; private set; }
    private GameObject pointObj;
    public LayerMask layerID { get; private set; }

    void Start()
    {
        pointObj = GameObject.Find("point");
        cam = FindObjectOfType<Camera>();
        camTopCorner = cam.ViewportToWorldPoint(cam.rect.min, Camera.MonoOrStereoscopicEye.Mono);
        camBottomCorner = cam.ViewportToWorldPoint(cam.rect.max, Camera.MonoOrStereoscopicEye.Mono);
        layerID = LayerMask.GetMask("NavColliders");
    }
    private void Update()
    {
        camTopCorner = cam.ViewportToWorldPoint(cam.rect.min, Camera.MonoOrStereoscopicEye.Mono);
        camBottomCorner = cam.ViewportToWorldPoint(cam.rect.max, Camera.MonoOrStereoscopicEye.Mono);


        List<Enemy> enemies =  new List<Enemy>(FindObjectsOfType<Enemy>());
        List<GameObject> obstacles = new List<GameObject>(GameObject.FindGameObjectsWithTag("Obstacle"));
        Player player = FindObjectOfType<Player>();

        foreach(Enemy enemy in enemies)
        {
            enemy.resetMoveVector();

            Vector3 attractionDir = (player.transform.position - enemy.transform.position).normalized;
            Vector3 perp = new Vector3(attractionDir.y, -attractionDir.x, 0);
            Vector3 attraction = attractionDir * 1f;

            foreach (GameObject obstacle in obstacles)
            {
                float obstRad = obstacle.GetComponent<Collider2D>().bounds.extents.magnitude;
                Vector2 dir = (enemy.transform.position - obstacle.transform.position).normalized;
                if (Vector2.Dot(attractionDir, dir) <= 0)
                {
                    dir = Vector2.Perpendicular(dir) * Mathf.Sign(Vector3.Dot(attractionDir, Vector2.Perpendicular(dir)));
                    float dist = (enemy.transform.position - obstacle.transform.position).magnitude - obstRad - enemy.getCollRadius();

                    Vector3 appliedVect;

                    

                    if (dist > 0) appliedVect = dir * (0.1f / dist);
                    else appliedVect = dir * 1000;
                    enemy.addToMoveVector(appliedVect);
                }
               


            }

       


            Vector3 vect = enemy.getMoveVector() + attraction;

            if (vect.magnitude < 0.1)
            {
                //enemy.resetMoveVector();
                //enemy.addToMoveVector(attractionDir);

                enemy.setIsInMinimum(true);
            }
            else enemy.setIsInMinimum(false);


            enemy.addToMoveVector(attraction);

            enemy.move();

        }

    }



}
