using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class ManipulatorAgent : Agent
{
        // Конец щупальца
        public Transform head;

        // Цель, которой необходимо коснуться
        public Transform target;

        // Настройки области спауна цели для обучения
        public Vector3 targetSpawnCenter = new Vector3(0, 1.7f, 0);
        public Vector3 targetSpawnScale = new Vector3(2f, 1.5f, 2f);
        public bool drawTargetGizmos = false;
        public JointController[] joints;

        private float previousDist = 0f;

        public override void Initialize()
        {
            joints = GetComponentsInChildren<JointController>();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = -Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
            continuousActionsOut[2] = -Input.GetAxis("Mouse X");
            continuousActionsOut[3] = Input.GetAxis("Mouse Y");
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            for (var i = 0; i < joints.Length; i++)
                joints[i].Rotate(actionBuffers.ContinuousActions[i] * 5f);

            var newDist = (head.transform.position - target.transform.position).magnitude;

            previousDist = newDist;

            if (newDist > 4f)
            {
                SetReward(-0.5f);
            }

            if (newDist < 0.3f)
            {
                SetReward(1f);
                EndEpisode();
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (var i = 0; i < joints.Length; i++)
            {
                sensor.AddObservation(joints[i].GetRotation());
            }

            sensor.AddObservation(head.position - transform.position);
            sensor.AddObservation(target.position - transform.position);
        }

        public override void OnEpisodeBegin()
        {
            target.transform.position = randomTargetPosititon();
            previousDist = (head.transform.position - target.transform.position).magnitude;
        }

        private Vector3 randomTargetPosititon()
        {
            var point = generateRandomPointInSphere();
            point.Scale(targetSpawnScale);
            point += targetSpawnCenter;
            return transform.position + point;
        }

        private Vector3 generateRandomPointInSphere()
        {
            var point = Random.insideUnitSphere;
            var dist = Vector2.Distance(new Vector2(point.x, point.z), Vector3.zero);
            while ((dist <= 0.6))
            {
                point = Random.insideUnitSphere;
                dist = Vector2.Distance(new Vector2(point.x, point.z), Vector2.zero);
            }

            return point;
        }

        public void OnDrawGizmosSelected()
        {
            if (!drawTargetGizmos) return;
            Gizmos.color = new Color(1, 0, 0, 0.75F);
            for (var i = 0; i < 100; i++)
            {
                Gizmos.DrawWireSphere(randomTargetPosititon(), 0.1f);
            }
        }
    }
