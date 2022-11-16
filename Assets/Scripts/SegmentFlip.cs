using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentFlip : MonoBehaviour
{
    [SerializeField] private float flipTime = 0.5f;
    [SerializeField] private float intervals = 0.001f;

    private bool topUp = true;
    private MeshRenderer meshRenderer;

    void Start()
    {
        this.meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
    }

    public void SwitchMaterial(Material material)
    {
        var mats = this.meshRenderer.materials;

        if (this.topUp)
        {
            mats[0] = material;
        } else
        {
            mats[1] = material;
        }

        this.meshRenderer.materials = mats;
    }

    public void FlipToMaterial(Material material)
    {
        var mats = this.meshRenderer.materials;

        if (this.topUp)
        {
            mats[1] = material;
        } else
        {
            mats[0] = material;
        }

        this.meshRenderer.materials = mats;

        StartCoroutine(this.Flip());
    }

    IEnumerator Flip() 
    {
        var timeLeft = this.flipTime;

        var degreeSteps = 180.0f / (this.flipTime / this.intervals);

        var currentDegrees = this.gameObject.transform.localEulerAngles.x;

        while (true)
        {
            timeLeft -= this.intervals;

            currentDegrees += degreeSteps;

            this.gameObject.transform.localRotation = Quaternion.Euler(currentDegrees, 0, 0);

            if (timeLeft <= 0)
            {
                this.topUp = !this.topUp;

                if (this.topUp)
                {
                    this.gameObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                } else
                {
                    this.gameObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
                }
                
                yield break;
            }

            yield return new WaitForSeconds(this.intervals);
        }
    }
}
