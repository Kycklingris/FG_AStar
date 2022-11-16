using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private int height = 20;
    [SerializeField] private int width = 20;
    [SerializeField] private SegmentFlip segmentGameObject; 
    [SerializeField] private int segmentSize = 2;

    private List<Segment> segments;   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public class Segment {
    public SegmentFlip flip; 
}