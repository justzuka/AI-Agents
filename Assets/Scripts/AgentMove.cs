using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentMove : Agent
{
    [SerializeField] Transform target;
    [SerializeField] Rigidbody rb;
    [SerializeField] float moveSpeed = 5;
    public override void OnEpisodeBegin()
    {
        float randx = Random.Range(-2.5f, 2.5f);
        randx += Mathf.Sign(randx) * 2;
        float randz = Random.Range(-2.5f, 2.5f);
        randz += Mathf.Sign(randz) * 2;

        target.transform.localPosition = new Vector3(randx, 1, randz);
        transform.localPosition = new Vector3(0, 1, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector2 move = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        Vector3 moveDir = new Vector3(move.x, 0, move.y).normalized * moveSpeed * Time.deltaTime;
        rb.velocity = moveDir;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actionSegment = actionsOut.ContinuousActions;
        actionSegment[0] = Input.GetAxisRaw("Horizontal");
        actionSegment[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("target"))
        {
            AddReward(2);
            EndEpisode();
        }
        else if (collision.transform.CompareTag("wall"))
        {
            AddReward(-1);
            EndEpisode();
        }
    }
}
