using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private int height = 20;
    [SerializeField] private int width = 20;
    [SerializeField] private SegmentFlip segmentGameObject; 
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int segmentSize = 2;
    [SerializeField] private int obstacleChance = 25;

    [SerializeField] private Material regularMaterial;
    [SerializeField] private Material targetMaterial;
    [SerializeField] private Material pathMaterial;


    private List<Segment> segments = new List<Segment>();

    private Transform player;

    private void Awake()
    {
        this.player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        var playerPos = this.ObjectGridPosition(this.player);

        var startX = (this.width / 2) * this.segmentSize;
        var startZ = (this.height / 2) * this.segmentSize;

        var endX = -((this.width * this.segmentSize) / 2);
        var endZ = -((this.height * this.segmentSize) / 2);

        for (int x = startX; x >= endX; x = x - this.segmentSize)
        {
            for (int z = startZ; z >= endZ; z = z - this.segmentSize)
            {
                SegmentFlip instance = Instantiate(this.segmentGameObject, new Vector3(x, 0.0f, z), Quaternion.Euler(-90, 0, 0), this.gameObject.transform);

                instance.SwitchMaterial(this.regularMaterial);

                var hasObstacle = false;
                if (Random.Range(0, 101) <= this.obstacleChance && playerPos != new Vector2(x, z)) {
                    GameObject obstacleInstance = Instantiate(this.obstaclePrefab, Vector3.zero, Quaternion.identity, instance.gameObject.transform);
                    obstacleInstance.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
                    hasObstacle = true;
                }

                this.segments.Add(new Segment() {flip = instance, hasObstacle = hasObstacle});
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                this.PathFind(this.ObjectGridPosition(hit.transform));
            }   
        }
    }

    private void PathFind(Vector2 segmentPosition)
    {
        var x = (int)Mathf.Abs(segmentPosition.x - this.width / 2);
        var z = (int)Mathf.Abs(segmentPosition.y - this.height / 2);

        var target = this.segments[(x * (this.height + 1)) + z];

        if (target.hasObstacle)
        {
            return;
        }

        target.flip.FlipToMaterial(this.targetMaterial);
    }

    Vector2 ObjectGridPosition(Transform obj) 
    {
        return new Vector2(
            Mathf.Floor(obj.position.x / this.segmentSize),
            Mathf.Floor(obj.position.z / this.segmentSize)
        );
    }
}


public class Segment 
{
    public SegmentFlip flip; 
    public bool hasObstacle;
}