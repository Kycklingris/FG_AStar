using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
	[SerializeField] private int height = 20;
	[SerializeField] private int width = 20;
	[SerializeField] private int segmentSize = 2;
	[SerializeField] private int obstacleChance = 25;
	[SerializeField] private SegmentFlip segmentGameObject;
	[SerializeField] private GameObject obstaclePrefab;

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
				if (Random.Range(0, 101) <= this.obstacleChance && playerPos != new Position(x, z))
				{
					GameObject obstacleInstance = Instantiate(this.obstaclePrefab, Vector3.zero, Quaternion.identity, instance.gameObject.transform);
					obstacleInstance.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
					hasObstacle = true;
				}

				this.segments.Add(new Segment() { flip = instance, hasObstacle = hasObstacle });
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

	private void PathFind(Position segmentPosition)
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
		this.target = new AlgoSegment() { segment = newTarget, f = 0, g = 0, pos = segmentPosition };


		this.player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
		var playerPos = this.ObjectGridToPosition(this.player);
		var playerSegment = this.GridPositionToSegment(playerPos);

		Queue<AlgoSegment> frontier = new Queue<AlgoSegment>();
		List<AlgoSegment> reached = new List<AlgoSegment>();

		frontier.Enqueue(new AlgoSegment() { segment = playerSegment, f = 0, g = 0, pos = playerPos });
		reached.Add(new AlgoSegment() { segment = playerSegment, f = 0, g = 0, pos = playerPos });

		while (frontier.Count > 0)
		{
			// if (reached.Count > 500 || frontier.Count > 500)
			// {
			// 	break;
			// }

			var current = frontier.Dequeue();
			var neighbors = GetSegmentNeighbors(current);

			while (neighbors.Count > 0)
			{
				var next = neighbors.Dequeue();

				var contained = false;

				foreach (var item in reached)
				{
					if (item.pos.x == next.pos.x && item.pos.y == next.pos.y)
					{
						contained = true;
						break;
					}
				}

				if (!contained)
				{
					frontier.Enqueue(next);
					reached.Add(next);
				}
			}

		}

		foreach (var item in reached)
		{
			item.segment.flip.FlipToMaterial(this.pathMaterial);
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

	Queue<AlgoSegment> GetSegmentNeighbors(AlgoSegment segment)
	{
		var ret = new Queue<AlgoSegment>();
		var pos = segment.pos;

		// Debug.Log("x: " + pos.x + ", y: " + pos.y);

		if (pos.x - 1 >= -this.width / 2)
		{
			var newPos1 = new Position(pos.x - 1, pos.y);
			ret.Enqueue(new AlgoSegment()
			{
				segment = GridPositionToSegment(newPos1),
				f = 0,
				g = 0,
				pos = newPos1,
			});

		}

		if (pos.x + 1 <= this.width / 2)
		{
			var newPos2 = new Position(pos.x + 1, pos.y);
			ret.Enqueue(new AlgoSegment()
			{
				segment = GridPositionToSegment(newPos2),
				f = 0,
				g = 0,
				pos = newPos2,
			});
		}

		if (pos.y - 1 >= -this.height / 2)
		{
			var newPos3 = new Position(pos.x, pos.y - 1);
			ret.Enqueue(new AlgoSegment()
			{
				segment = GridPositionToSegment(newPos3),
				f = 0,
				g = 0,
				pos = newPos3,
			});
		}

		if (pos.y + 1 <= this.height / 2)
		{
			var newPos4 = new Position(pos.x, pos.y + 1);
			ret.Enqueue(new AlgoSegment()
			{
				segment = GridPositionToSegment(newPos4),
				f = 0,
				g = 0,
				pos = newPos4,
			});
		}

		return ret;
	}

	Position ObjectGridToPosition(Transform obj)
	{
		return new Position(
			(int)Mathf.Floor(obj.position.x / this.segmentSize),
			(int)Mathf.Floor(obj.position.z / this.segmentSize)
		);
	}

	Segment GridPositionToSegment(Position segmentPosition)
	{
		var x = (int)Mathf.Abs(segmentPosition.x - this.width / 2);
		var z = (int)Mathf.Abs(segmentPosition.y - this.height / 2);

		return this.segments[(x * (this.height + 1)) + z];
	}
}

public class Position
{
	public int x;
	public int y;

	public Position(int x, int y)
	{
		this.x = x;
		this.y = y;
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
	public Position pos;
}