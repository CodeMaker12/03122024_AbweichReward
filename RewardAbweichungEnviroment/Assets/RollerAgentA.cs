using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class RollerAgentA : Agent
{
    public Transform Target;
    public RollerAgentB enemyAgent;
    private Coroutine episodeTimerCoroutine;

    public Rigidbody rBody;
    public float forceMultiplier = 10;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    public override void OnEpisodeBegin()
    {
        // Destroy all bullets in the scene
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }

        if (episodeTimerCoroutine != null)
        {
            StopCoroutine(episodeTimerCoroutine);
        }
        episodeTimerCoroutine = StartCoroutine(EpisodeTimer());

        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.linearVelocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.transform.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.linearVelocity.x);
        sensor.AddObservation(rBody.linearVelocity.z);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            AddReward(1.0f);
            // Move the target to a new spot
            Target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            AddReward(-20.0f);
            enemyAgent.EndEpisode();
            EndEpisode();
        }

        // Slow the Game down if wanted to observe the agents
        if (Input.GetKey(KeyCode.N))
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 20;
        }
    }
    private IEnumerator EpisodeTimer()
    {
        yield return new WaitForSeconds(10); // Wait for 10 seconds
        enemyAgent.EndEpisode();
        EndEpisode(); // End the episode after 10 seconds
    }
}
