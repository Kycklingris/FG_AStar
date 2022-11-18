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

    private AlgoSegment target;

    private List<Segment> path = new List<Segment>();

    private void Awake()
    {
        this.player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        var playerPos = this.ObjectGridToPosition(this.player);

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
        { // https://forum.unity.com/threads/how-to-raycast-from-camera-through-mouse-position.293717/
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000))
            {
                this.PathFind(this.ObjectGridToPosition(hit.transform));
            }   
        }
    }

    private void PathFind(Vector2 segmentPosition)
    {
        var newTarget = this.GridPositionToSegment(segmentPosition);

        if (newTarget.hasObstacle)
        {
            return;
        }

        if (this.target != null)
        {
            this.target.segment.flip.FlipToMaterial(this.regularMaterial);
        }

        newTarget.flip.FlipToMaterial(this.targetMaterial);
        this.target = new AlgoSegment() {segment = newTarget, f = 0, g = 0, pos = segmentPosition};


        this.player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        var playerPos = this.ObjectGridToPosition(this.player);
        var playerSegment = this.GridPositionToSegment(playerPos);

        var open = new List<AlgoSegment>();
        var closed = new List<AlgoSegment>();

        open.Add(new AlgoSegment() {segment = playerSegment, f = 0, g = 0, pos = playerPos});

        while (open.Count > 0) 
        {
            var q = open[0];
            foreach(var segment in open)
            {
                if (segment.f < q.f)
                {
                    q = segment;
                }
            }

            open.Remove(q);

            if (open.Count > 25) 
            {
                break;
            }

            var successors = new List<Vector2>();

            successors.Add(new Vector2(q.pos.x + 1, q.pos.y));
            successors.Add(new Vector2(q.pos.x - 1, q.pos.y));
            successors.Add(new Vector2(q.pos.x, q.pos.y + 1));
            successors.Add(new Vector2(q.pos.x, q.pos.y - 1));

            foreach(var successor in successors)
            {
                if ( // Check for out of bounds
                    successor.x < 0 || successor.x > this.width ||
                    successor.y < 0 || successor.y > this.height
                )
                {

                    Debug.Log(successor);
                    continue;
                } 
                
                var successorSegment = this.GridPositionToSegment(new Vector2(successor.x, successor.y));

                if (successorSegment.hasObstacle) {
                    continue;
                }

                var g = q.g + ManhattanDistance(
                    q.pos.x, q.pos.y, 
                    successor.x, successor.y
                );

                var h = ManhattanDistance(
                    successor.x, successor.y,
                    this.target.pos.x, this.target.pos.y
                );

                var f = g + h;

                var skip = false;

                for (int i = 0; i < open.Count; i++)
                {
                    var openSegment = open[i];

                    if (openSegment.pos == successor)
                    {
                        skip = true;
                        if (openSegment.f < f) 
                        {
                            break;
                        } 
                        else 
                        {
                            open[i] = new AlgoSegment() {
                                segment = successorSegment,
                                f = f,
                                g = g,
                                pos = successor
                            };
                        }
                    }
                }

                for (int i = 0; i < closed.Count; i++)
                {
                    var closedSegment = closed[i];

                    if (closedSegment.pos == successor)
                    {
                        skip = true;
                        if (closedSegment.f < f) 
                        {
                            break;
                        } 
                        else 
                        {
                            closed[i] = new AlgoSegment() {
                                segment = successorSegment,
                                f = f,
                                g = g,
                                pos = successor
                            };
                        }
                    }
                }

                if (skip)
                {
                    // Debug.Log("skip");
                    continue;
                }
                else 
                {
                    open.Add(new AlgoSegment() {
                        segment = successorSegment,
                        f = f,
                        g = g,
                        pos = successor
                    });
                }
            }

            closed.Add(q);
        }

        foreach(var segment in closed)
        {
            Debug.Log(segment.pos);
            segment.segment.flip.FlipToMaterial(this.pathMaterial);
            
        }        
    }

    float ManhattanDistance(float x1, float z1, float x2, float z2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
    }

    float DiagonalDistance(float x1, float z1, float x2, float z2)
    {
        return Mathf.Floor(Mathf.Sqrt((x1 - x2) * 2 +
        (z1 - z2) * 2));
    }

    Vector2 ObjectGridToPosition(Transform obj) 
    {
        return new Vector2(
            Mathf.Floor(obj.position.x / this.segmentSize),
            Mathf.Floor(obj.position.z / this.segmentSize)
        );
    }

    Segment GridPositionToSegment(Vector2 segmentPosition)
    {
        var x = (int)Mathf.Abs(segmentPosition.x - this.width / 2);
        var z = (int)Mathf.Abs(segmentPosition.y - this.height / 2);

        return this.segments[(x * (this.height + 1)) + z];
    }
}


public class Segment 
{
    public SegmentFlip flip; 
    public bool hasObstacle;
}

public class AlgoSegment
{
    public Segment segment;
    public float f;
    public float g;
    public Vector2 pos;
}