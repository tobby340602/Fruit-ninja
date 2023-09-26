using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowCube : MonoBehaviour
{
    // Cube to be thrown
    public GameObject cube;

    // Initial speed of the throw
    public float throwSpeed = 10.0f;

    // The height that the cube will reach at the peak of its arc
    public float peakHeight = 5.0f;

    private float gravity = -9.8f;

    private void Start()
    {
        gravity = -2f;
        IEnumerator ThrowCoroutine()
        {
            var random = new System.Random();
            while (true)
            {
                yield return new WaitForSeconds(0.5f + (float) random.NextDouble());
                ThrowObject();
            }
        }

        StartCoroutine(ThrowCoroutine());
    }

    private void ThrowObject()
    {
        // Create a copy of the cube
        GameObject thrownCube = Instantiate(cube, transform.position, Quaternion.identity);
        GameObject testCube = Instantiate(cube, transform.position, Quaternion.identity);

        // Get the Rigidbody component to apply forces
        Rigidbody rbObject = thrownCube.GetComponent<Rigidbody>();

        if (rbObject != null)
        {
            // Calculate the initial velocity required to reach the desired peak height
            float initialVelocity = Mathf.Sqrt(2f * Physics.gravity.magnitude * peakHeight);

            // Calculate the total time taken to reach the peak and fall back to the original height
            float totalTime = 2f * initialVelocity / Physics.gravity.magnitude;

            // Calculate the horizontal speed required to reach the target in the total time
            float horizontalSpeed = throwSpeed / totalTime;

            // Apply the initial velocities to the Rigidbody
            rb.velocity = -transform.right * horizontalSpeed + Vector3.up * initialVelocity;
        }
        else
        {
            Debug.LogError("The cube needs a Rigidbody component!");
        }
    }
}

