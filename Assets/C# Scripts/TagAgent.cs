using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TagAgent : Agent {
    public bool isSeeker;
    public Transform opponent;
    private float previousDistance;
    private Rigidbody rb;
    private Rigidbody opponentRb;
    private Vector3 previousMoveDirection;

    [Header("Physics Settings")]
    public LayerMask groundLayers;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float turnSpeed;
    public float jumpForce;
    [Range(0f, 1f)] public float airControl = 0.1f;

    [Header("Spawn Settings")]
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [Header("Game Manager")]
    public GameManager gameManager;

    public override void Initialize() {
        rb = GetComponent<Rigidbody>();
        opponentRb = opponent.GetComponent<Rigidbody>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public override void OnEpisodeBegin() {
        // Reset physics so they don't "slide" into the next round
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Move them back to the exact spot you set in the Unity Editor
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;

        // Calculate the starting distance between the two agents
        previousDistance = Vector3.Distance(transform.position, opponent.position);

        // Add some randomization to the starting positions to encourage generalization
        Vector3 pos = transform.localPosition;
        pos.z = Random.Range(-0f, 0f);
        transform.localPosition = pos;

        // Reset move direction tracker
        previousMoveDirection = transform.forward; // Reset move direction tracker
    }

    private bool CheckGrounded()
    {
        // Radius: 2.4f (Slightly less than the 2.5f half-width of your 5-unit cube)
        // Distance: 0.2f (How far below the bottom face to look)
        // We shoot the ray starting from the center (2.5f down to reach the bottom)
        return Physics.SphereCast(transform.position, 2.4f, Vector3.down, out _, 0.2f, groundLayers);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = actions.ContinuousActions[0];
        float strafeAmount = actions.ContinuousActions[1];
        float turnAmount = actions.ContinuousActions[2];
        float jumpAction = actions.ContinuousActions[3];

        // 1. Check if we are on the ground ONCE per frame to save performance
        bool isCurrentlyGrounded = CheckGrounded();

        // 2. Calculate direction
        Vector3 moveVector = (transform.forward * forwardAmount) + (transform.right * strafeAmount);
        
        // 3. Apply movement force (Full speed on ground, reduced speed in air)
        if (isCurrentlyGrounded)
        {
            rb.AddForce(moveVector * moveSpeed);
        }
        else
        {
            rb.AddForce(moveVector * (moveSpeed * airControl));
        }

        // 4. Apply rotation (turning the body)
        transform.Rotate(transform.up, turnAmount * turnSpeed * Time.deltaTime);

        // 5. Apply jump force if grounded
        if (jumpAction > 0.5f && isCurrentlyGrounded) 
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            // if (isSeeker) AddReward(-0.05f);
        }

        // --- REWARD SHAPING ---
        if (isSeeker)
        {
            float currentDistance = Vector3.Distance(transform.position, opponent.position);
            float distanceDelta = previousDistance - currentDistance;

            // 1. Reward closing the distance (Progress)
            if (distanceDelta > 0f)
            {
                AddReward(distanceDelta * 0.01f); 
            }

            // 2. Constant Time Penalty (Urgency)
            AddReward(-0.002f); 

            previousDistance = currentDistance;
        }
        else // Runner
        {
            float currentDistance = Vector3.Distance(transform.position, opponent.position);
            float distanceDelta = currentDistance - previousDistance;

            // 1. Reward increasing the distance (Progress)
            if (distanceDelta > 0f)
            {
                AddReward(distanceDelta * 0.01f);
            }

            // 2. Constant Survival Bonus (Staying alive)
            AddReward(0.002f); 

            previousDistance = currentDistance;
        }

        // --- REWARD SHAPING ---
        // if (isSeeker)
        // {
        //     float currentDistance = Vector3.Distance(transform.position, opponent.position);

        //     // // Scaled delta — proportional to actual distance closed
        //     // AddReward((previousDistance - currentDistance) * 0.02f);
        //     float distanceDelta = previousDistance - currentDistance;
        //     if (distanceDelta > 0f)
        //     {
        //         AddReward(distanceDelta * 0.02f);
        //     }

        //     // Bonus for being close and at similar height (encourages actually catching, not just getting close on the ground)
        //     float heightDifference = Mathf.Abs(opponent.position.y - transform.position.y);
        //     if (currentDistance < 6f && heightDifference < 1.5f)
        //     {
        //         AddReward(0.01f);
        //     }

        //     // Proximity bonus tiers
        //     if (currentDistance < 10f) AddReward(0.003f);
        //     if (currentDistance < 5f)  AddReward(0.005f);
        //     if (currentDistance < 2f)  AddReward(0.010f);

        //     // Facing penalty with real teeth
        //     Vector3 relativeDir = (opponent.position - transform.position).normalized;
        //     float dot = Vector3.Dot(transform.forward, relativeDir);
        //     if (dot < -0.5f) AddReward(-0.01f);

        //     // Heavy time penalty
        //     AddReward(-0.005f);

        //     previousDistance = currentDistance;
        // }
        // else // runner
        // {
        //     float currentDistance = Vector3.Distance(transform.position, opponent.position);

        //     // Reward actively increasing distance
        //     if (currentDistance > previousDistance)
        //     {
        //         AddReward(0.003f); // running away is actively good
        //     }

        //     // Bonus for being far away (encourages staying away, not just briefly escaping)
        //     AddReward(currentDistance * 0.0001f);

        //     previousDistance = currentDistance;
        //     AddReward(0.001f); // survival reward stays
        // }
    }

    private void OnCollisionEnter(Collision collision) 
    {
        // Only the seeker handles the "Catch" logic to prevent double-counting
        if (collision.gameObject.CompareTag("Player") && isSeeker) 
        {
            AddReward(10.0f); // Reward seeker for the catch
            opponent.GetComponent<TagAgent>().AddReward(-10.0f); // Penalize runner for being caught
            
            // Tell the GameManager a catch happened!
            gameManager.SeekerCaughtRunner(); 
        }

        if (collision.gameObject.CompareTag("Wall")) 
        {
            AddReward(-0.01f); // Don't bump into walls
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        
        // W/S = Forward/Back
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        
        // A/D = Strafe
        continuousActions[1] = Input.GetAxisRaw("Horizontal");

        // Q/E or Mouse = Rotate (Let's use J and L keys for easy testing)
        float rotation = 0f;
        if (Input.GetKey(KeyCode.Q)) rotation = -1f;
        if (Input.GetKey(KeyCode.E)) rotation = 1f;
        if (Input.GetKey(KeyCode.J)) rotation = -1f;
        if (Input.GetKey(KeyCode.L)) rotation = 1f;
        continuousActions[2] = rotation;

        // Space = Jump
        continuousActions[3] = Input.GetKey(KeyCode.Space) ? 1f : 0f; // Jump
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Own velocity (3 values)
        sensor.AddObservation(rb.linearVelocity);

        // Is grounded? (1 value)
        sensor.AddObservation(CheckGrounded() ? 1f : 0f);

        // Opponent's relative position (3 values — direction + distance built in)
        Vector3 relativeOpponentPos = transform.InverseTransformPoint(opponent.position);
        sensor.AddObservation(relativeOpponentPos);

        // Opponent's velocity (3 values)
        sensor.AddObservation(opponentRb.linearVelocity);

        // Distance to opponent (1 value — explicit scalar helps)
        sensor.AddObservation(Vector3.Distance(transform.position, opponent.position));

        // Our own facing direction (3 values — helps agent know which way it's turned)
        sensor.AddObservation(transform.forward);

        // Opponent's facing direction (3 values) — enables faking/dodging
        sensor.AddObservation(opponent.forward);

        // Total: 17 observations
    }
}