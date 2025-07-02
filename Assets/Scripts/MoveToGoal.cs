using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToGoal : Agent
{

    [SerializeField] Transform target;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [Header("random")]   
    [SerializeField] private float minZ = -5;
    [SerializeField] private float maxZ = 3;
    [SerializeField] private float minX = -5;
    [SerializeField] private float maxX = 4;

    private float previousDistance;
    public override void OnEpisodeBegin()
    {
        Vector3 randomAgentPosition = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));
        transform.localPosition = randomAgentPosition;

        target.localPosition = new Vector3(Random.Range(minX, maxX), 0, Random.Range(minZ, maxZ));

        previousDistance = Vector3.Distance(transform.localPosition, target.localPosition);
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        float currentDistance = Vector3.Distance(transform.localPosition, target.localPosition);

        // Nếu gần target rất sát thì cộng thêm thưởng nhẹ
        if (currentDistance < 0.5f)
        {
            AddReward(0.3f);
        }

        // Phạt thời gian
        AddReward(-0.001f);

        // Cập nhật khoảng cách cũ
        previousDistance = currentDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal")*3f;
        continuousActions[1] = Input.GetAxisRaw("Vertical")*3f;

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;

            EndEpisode();
        }

    }
}
