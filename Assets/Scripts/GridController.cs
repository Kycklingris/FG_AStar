using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private int height = 20;
    [SerializeField] private int width = 20;
    [SerializeField] private SegmentFlip segmentGameObject; 
    [SerializeField] private int segmentSize = 2;

    [SerializeField] private Material regular;


    private List<Segment> segments = new List<Segment>();   

    // Start is called before the first frame update
    void Awake()
    {
        var startX = (this.width / 2) * this.segmentSize;
        var startZ = (this.height / 2) * this.segmentSize;

        var endX = -((this.width * this.segmentSize) / 2);
        var endZ = -((this.height * this.segmentSize) / 2);

        for (int x = startX; x >= endX; x = x - this.segmentSize)
        {
            for (int z = startZ; z >= endZ; z = z - this.segmentSize)
            {
                SegmentFlip instance = Instantiate(this.segmentGameObject, new Vector3(x, 0.0f, z), Quaternion.Euler(-90, 0, 0), this.gameObject.transform);

                instance.SwitchMaterial(this.regular);

                this.segments.Add(new Segment() {flip = instance});
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public class Segment {
    public SegmentFlip flip; 
}