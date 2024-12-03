using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgentB : Agent
{
    public Rigidbody rBody;
    public float forceMultiplier = 10;
    public float bulletSpeed = 10;
    private float lastShotTime = 0.0f;
    private float shootCooldown = 1.0f;

    public GameObject bulletPrefab;

    public Transform Target;
    public RollerAgentA agentA;

    // Serialized field to monitor rotationAction
    [SerializeField]
    private float currentRotationAction;
    [SerializeField]
    private float[] currentOrientation;
    [SerializeField]
    private float currentDeviationAngle;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Set to 1 if space bar is pressed, otherwise 0
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(agentA.transform.localPosition);
        sensor.AddObservation(agentA.rBody.linearVelocity.x);
        sensor.AddObservation(agentA.rBody.linearVelocity.z);

        // Agent Orientation
        float angleInRadians = transform.eulerAngles.y * Mathf.Deg2Rad; // Convert degrees to radians
        float sinAngle = Mathf.Sin(angleInRadians); // Calculate sine of the angle
        float cosAngle = Mathf.Cos(angleInRadians); // Calculate cosine of the angle

        // Add sine and cosine as observations
        sensor.AddObservation(sinAngle);
        sensor.AddObservation(cosAngle);

        // Store current orientation as an array
        currentOrientation = new float[] { transform.eulerAngles.y, sinAngle, cosAngle };
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        float rotationAction = actionBuffers.ContinuousActions[0];
        currentRotationAction = rotationAction;

        // Directly increment the rotation around the y-axis
        transform.Rotate(Vector3.up, rotationAction * forceMultiplier * Time.deltaTime);

        int shootAction = actionBuffers.DiscreteActions[0]; // The first discrete action is used for shooting
        if (shootAction == 1 && Time.time >= lastShotTime + shootCooldown) // If cooldown has passed
        {
            Debug.Log("tried to shoot");
            Shot(); // Call the Shot method to instantiate and launch the bullet
            lastShotTime = Time.time; // Update the last shot time
        }

        // Gets a small Reward based on the angle between the target and the line of sight
        Vector3 direction1 = transform.forward.normalized; //Looking direction
        Vector3 direction2 = (agentA.transform.position - transform.position).normalized; // Get the vector from Shooter to Target

        float deviationAngle = AngleBetweenTwoLines(direction1, direction2);
        currentDeviationAngle = deviationAngle;

        //angle max is 180 if looking at target directly angle is 0
        // if angle is 0 reward is 0.180
        float reward = (180 - deviationAngle) * 0.001f;
        AddReward(reward);
    }

    float AngleBetweenTwoLines(Vector3 dir1, Vector3 dir2)
    {
        // Dot product of the two direction vectors
        float dotProduct = Vector3.Dot(dir1, dir2);

        // Clamp the dot product to avoid numerical errors
        dotProduct = Mathf.Clamp(dotProduct, -1.0f, 1.0f);

        // Calculate the angle in radians and convert to degrees
        float angleInRadians = Mathf.Acos(dotProduct);
        float angleInDegrees = angleInRadians * Mathf.Rad2Deg;

        return angleInDegrees;
    }

    public void GrantReward(float reward)
    {
        AddReward(reward);
    }

    public void Shot()
    {
        // Instantiate the bullet at this object's position and rotation
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

        // Get the Rigidbody component of the bullet
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        // Set the bullet's velocity in this object's forward direction
        bulletRb.linearVelocity = transform.forward * bulletSpeed;
    }
}
