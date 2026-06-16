using UnityEngine;

public class PlayerDogVisual : MonoBehaviour
{
    private CharacterController playerController;
    
    // Transforms for procedural animation
    private Transform body;
    private Transform headGroup;
    private Transform tail;
    private Transform frontLeftLeg;
    private Transform frontRightLeg;
    private Transform backLeftLeg;
    private Transform backRightLeg;
    private Transform leftEar;
    private Transform rightEar;

    // Animation settings
    private float tailWagSpeedIdle = 4f;
    private float tailWagSpeedRun = 18f;
    private float tailWagAngleIdle = 15f;
    private float tailWagAngleRun = 35f;

    private float legSwingSpeed = 12f;
    private float legSwingAngle = 30f;

    private float bodyBobSpeed = 12f;
    private float bodyBobAmount = 0.06f;

    private Vector3 initialBodyLocalPos;
    private Vector3 initialHeadLocalPos;
    private Quaternion initialTailLocalRot;
    private Quaternion initialLeftEarRot;
    private Quaternion initialRightEarRot;

    private void Start()
    {
        playerController = GetComponentInParent<CharacterController>();

        body = transform.Find("Body");
        headGroup = transform.Find("HeadGroup");
        
        if (body != null)
        {
            initialBodyLocalPos = body.localPosition;
            tail = body.Find("Tail");
            frontLeftLeg = transform.Find("FrontLeftLeg");
            frontRightLeg = transform.Find("FrontRightLeg");
            backLeftLeg = transform.Find("BackLeftLeg");
            backRightLeg = transform.Find("BackRightLeg");
        }

        if (headGroup != null)
        {
            initialHeadLocalPos = headGroup.localPosition;
            leftEar = headGroup.Find("LeftEar");
            rightEar = headGroup.Find("RightEar");
        }

        if (tail != null) initialTailLocalRot = tail.localRotation;
        if (leftEar != null) initialLeftEarRot = leftEar.localRotation;
        if (rightEar != null) initialRightEarRot = rightEar.localRotation;
    }

    private void Update()
    {
        float speed = 0f;
        if (playerController != null)
        {
            // Project velocity onto horizontal plane
            Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0f, playerController.velocity.z);
            speed = horizontalVelocity.magnitude;
        }

        bool isMoving = speed > 0.1f;

        // Tail Wagging (Dynamic based on state)
        if (tail != null)
        {
            float targetWagSpeed = isMoving ? tailWagSpeedRun : tailWagSpeedIdle;
            float targetWagAngle = isMoving ? tailWagAngleRun : tailWagAngleIdle;
            float wagZ = Mathf.Sin(Time.time * targetWagSpeed) * targetWagAngle;
            tail.localRotation = initialTailLocalRot * Quaternion.Euler(0f, wagZ, 0f);
        }

        // Bobbing & Leg swinging
        if (isMoving)
        {
            float t = Time.time;

            // Bob the body up and down while running
            if (body != null)
            {
                float bob = Mathf.Abs(Mathf.Sin(t * bodyBobSpeed)) * bodyBobAmount;
                body.localPosition = initialBodyLocalPos + new Vector3(0f, bob, 0f);
                body.localRotation = Quaternion.Euler(Mathf.Sin(t * bodyBobSpeed) * 3f, 0f, 0f);
            }

            // Head bobbing out of phase
            if (headGroup != null)
            {
                float headBob = Mathf.Sin(t * bodyBobSpeed - 0.5f) * 0.03f;
                headGroup.localPosition = initialHeadLocalPos + new Vector3(0f, headBob, 0.02f);
                headGroup.localRotation = Quaternion.Euler(Mathf.Sin(t * bodyBobSpeed) * 4f, 0f, 0f);
            }

            // Floppy ears bouncing slightly
            if (leftEar != null) leftEar.localRotation = initialLeftEarRot * Quaternion.Euler(Mathf.Sin(t * bodyBobSpeed * 1.5f) * 6f, 0f, 0f);
            if (rightEar != null) rightEar.localRotation = initialRightEarRot * Quaternion.Euler(Mathf.Sin(t * bodyBobSpeed * 1.5f + 0.5f) * 6f, 0f, 0f);

            // Swing legs back and forth (diagonal pairs out of phase)
            float swing = Mathf.Sin(t * legSwingSpeed) * legSwingAngle;

            if (frontLeftLeg != null) frontLeftLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
            if (backRightLeg != null) backRightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);

            if (frontRightLeg != null) frontRightLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            if (backLeftLeg != null) backLeftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
        }
        else
        {
            // Idle state: return to rest positions smoothly
            if (body != null)
            {
                body.localPosition = Vector3.MoveTowards(body.localPosition, initialBodyLocalPos, Time.deltaTime * 2f);
                body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.identity, Time.deltaTime * 5f);
            }

            if (headGroup != null)
            {
                headGroup.localPosition = Vector3.MoveTowards(headGroup.localPosition, initialHeadLocalPos, Time.deltaTime * 2f);
                
                // Cute head tilt occasionally
                float tiltAngle = Mathf.Sin(Time.time * 1.5f) * 2.5f;
                float pantBob = Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.01f;
                headGroup.localPosition = initialHeadLocalPos + new Vector3(0f, pantBob, 0f);
                headGroup.localRotation = Quaternion.Euler(pantBob * 10f, 0f, tiltAngle);
            }

            if (leftEar != null) leftEar.localRotation = Quaternion.Slerp(leftEar.localRotation, initialLeftEarRot, Time.deltaTime * 5f);
            if (rightEar != null) rightEar.localRotation = Quaternion.Slerp(rightEar.localRotation, initialRightEarRot, Time.deltaTime * 5f);

            // Reset legs to straight
            if (frontLeftLeg != null) frontLeftLeg.localRotation = Quaternion.Slerp(frontLeftLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
            if (frontRightLeg != null) frontRightLeg.localRotation = Quaternion.Slerp(frontRightLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
            if (backLeftLeg != null) backLeftLeg.localRotation = Quaternion.Slerp(backLeftLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
            if (backRightLeg != null) backRightLeg.localRotation = Quaternion.Slerp(backRightLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
        }
    }
}