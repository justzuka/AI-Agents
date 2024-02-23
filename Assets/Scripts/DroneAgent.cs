using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DroneAgent : Agent
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform[] propellers;
    [SerializeField] KeyCode[] keys;
    [SerializeField] float force;
    [SerializeField] float episodeLength = 15;
    [SerializeField] Transform targetPositionForHovering;
    ActionBuffers currentActions;

    float counter;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, 5.44f, 0);
        //transform.eulerAngles = new Vector3(Random.Range(0,20), Random.Range(0, 20), Random.Range(0, 20));
        transform.eulerAngles = new Vector3(0, 0, Random.Range(-5f, 5f));

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        InvokeRepeating("AddPeriodicReward", 0.1f, 0.1f);
        counter = 0;
    }
   
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition - targetPositionForHovering.localPosition);
        sensor.AddObservation(transform.eulerAngles);
    }
    void AddPeriodicReward()
    {
        float dist = Vector3.Distance(transform.localPosition, targetPositionForHovering.localPosition);
        if (dist < 2)
        {
            AddReward(0.2f);
        }
        else
        {
            AddReward(dist * -0.05f);
        }
    }
    void FixedUpdate()
    {
        counter++;  
        if (counter <= 2)
        {
            return;
        }
        int[] map = {0,1,1,0};

        for (int i = 0; i < 4; i++)
        {
            float mult = Mathf.Sign(currentActions.ContinuousActions[map[i]]) == 1 ? 1 : 0;
            rb.AddForceAtPosition(propellers[i].transform.up * force * mult * Time.fixedDeltaTime
                , propellers[i].transform.position);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentActions = actions;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actionSegment = actionsOut.ContinuousActions;
        for (int i = 0; i < actionsOut.ContinuousActions.Length; i++)
        {
            actionSegment[i] = Input.GetKey(keys[i]) ? 1 : -1;
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("wall"))
        {
            AddReward(-10);
            CancelInvoke("AddPeriodicReward");
            EndEpisode();
        }        
    }
}
