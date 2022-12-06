using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils;

public class GridController : MonoBehaviour
{
	[SerializeField] private int height = 20;
	[SerializeField] private int width = 20;
	[SerializeField] private int segmentSize = 2;
	[SerializeField] private int obstacleChance = 25;
	[SerializeField] private float moveTimer = 2.5f;
	[SerializeField] private SegmentFlip segmentGameObject;
	[SerializeField] private GameObject obstaclePrefab;

	[SerializeField] private Material regularMaterial;
	[SerializeField] private Material targetMaterial;
	[SerializeField] private Material pathMaterial;


	private List<Segment> segments = new List<Segment>();

	private Transform player;

	private Position target;

	private Queue<Position> path = new Queue<Position>();

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
				if (Random.Range(0, 101) <= this.obstacleChance && playerPos.x != x && playerPos.y != z)
				{
					GameObject obstacleInstance = Instantiate(this.obstaclePrefab, Vector3.zero, Quaternion.identity, instance.gameObject.transform);
					obstacleInstance.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
					hasObstacle = true;
				}

				this.segments.Add(new Segment() { flip = instance, hasObstacle = hasObstacle });
			}
		}


		StartCoroutine(Move());
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

	private IEnumerator Move()
	{
		while (true)
		{
			yield return new WaitForSeconds(this.moveTimer);

			if (this.path.Count > 0)
			{
				var next = this.path.Dequeue();

				GridPositionToSegment(next).flip.FlipToMaterial(this.regularMaterial);

				this.player.transform.position = new Vector3(next.x * this.segmentSize, this.player.transform.position.y, next.y * this.segmentSize);
			}
		}
	}

	private void PathFind(Position segmentPosition)
	{
		if (this.GridPositionToSegment(segmentPosition).hasObstacle)
		{
			return;
		}

		this.target = segmentPosition;

		var playerPos = this.ObjectGridToPosition(this.player);

		var frontier = new PriorityQueue<Position, int>();
		var cameFrom = new Dictionary<Position, Position>();
		var costSoFar = new Dictionary<Position, int>();


		frontier.Enqueue(playerPos, 0);
		cameFrom.Add(playerPos, playerPos);
		costSoFar.Add(playerPos, 0);

		while (frontier.Count > 0)
		{
			var current = frontier.Dequeue();
			var neighbors = GetSegmentNeighbors(current);

			if (current == this.target)
			{
				break;
			}

			while (neighbors.Count > 0)
			{
				var next = neighbors.Dequeue();

				if (GridPositionToSegment(next).hasObstacle)
				{
					continue;
				}

				var newCost = costSoFar[current] + 1;

				if (!costSoFar.ContainsKey(next) ||
				(costSoFar.ContainsKey(next) && costSoFar[next] > newCost))
				{
					costSoFar.Remove(next);
					costSoFar.Add(next, newCost);

					var priority = newCost + ManhattanDistance(next.x, next.y, this.target.x, this.target.y);

					frontier.Enqueue(next, priority);

					cameFrom.Remove(next);
					cameFrom.Add(next, current);
				}
			}
		}

		if (!cameFrom.ContainsKey(this.target)) // If no path has been found, skip
		{
			return;
		}

		GridPositionToSegment(this.target).flip.FlipToMaterial(this.targetMaterial);

		var currentSegment = cameFrom[this.target];
		var revPath = new List<Position>();

		while (currentSegment != playerPos)
		{
			revPath.Add(currentSegment);
			currentSegment = cameFrom[currentSegment];
		}



		while (this.path.Count > 0)
		{
			var next = this.path.Dequeue();

			GridPositionToSegment(next).flip.FlipToMaterial(this.regularMaterial);
		}

		for (int i = revPath.Count - 1; i >= 0; i--)
		{
			GridPositionToSegment(revPath[i]).flip.FlipToMaterial(this.pathMaterial);
			this.path.Enqueue(revPath[i]);
		}

		this.path.Enqueue(this.target);
	}

	int ManhattanDistance(int x1, int z1, int x2, int z2)
	{
		return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
	}

	float DiagonalDistance(float x1, float z1, float x2, float z2)
	{
		return Mathf.Floor(Mathf.Sqrt((x1 - x2) * 2 +
		(z1 - z2) * 2));
	}

	Queue<Position> GetSegmentNeighbors(Position segment)
	{
		var ret = new Queue<Position>();
		var pos = segment;

		if (pos.x - 1 >= -this.width / 2)
		{
			var newPos = new Position(pos.x - 1, pos.y);
			ret.Enqueue(newPos);
		}

		if (pos.x + 1 <= this.width / 2)
		{
			var newPos = new Position(pos.x + 1, pos.y);
			ret.Enqueue(newPos);
		}

		if (pos.y - 1 >= -this.height / 2)
		{
			var newPos = new Position(pos.x, pos.y - 1);
			ret.Enqueue(newPos);
		}

		if (pos.y + 1 <= this.height / 2)
		{
			var newPos = new Position(pos.x, pos.y + 1);
			ret.Enqueue(newPos);
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

public struct Position
{
	public int x;
	public int y;

	public Position(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static bool operator ==(Position a, Position b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool operator !=(Position a, Position b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return false;
		}
		else
		{
			return true;
		}
	}

	public override int GetHashCode()
	{ // https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
		unchecked // Overflow is fine, just wrap
		{
			int hash = 23;
			// Suitable nullity checks etc, of course :)
			hash = hash * 31 + this.x.GetHashCode();
			hash = hash * 31 + this.y.GetHashCode();
			return hash;
		}
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}
}

public class Segment
{
	public SegmentFlip flip;
	public bool hasObstacle;
}