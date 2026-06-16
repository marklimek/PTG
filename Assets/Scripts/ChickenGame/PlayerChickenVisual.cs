using UnityEngine;

public class PlayerChickenVisual : MonoBehaviour
{
    private CharacterController playerController;

    private Transform body;
    private Transform headGroup;
    private Transform leftWing;
    private Transform rightWing;
    private Transform leftLeg;
    private Transform rightLeg;

    private Vector3 initialBodyLocalPos;
    private Vector3 initialHeadLocalPos;
    private Vector3 initialLeftWingPos;
    private Vector3 initialRightWingPos;
    private Quaternion initialLeftWingRot;
    private Quaternion initialRightWingRot;

    private float walkBobSpeed = 14f;
    private float walkBobAmount = 0.08f;
    private float wingFlapSpeed = 16f;

    private void Start()
    {
        playerController = GetComponentInParent<CharacterController>();

        body = transform.Find("Body");
        headGroup = transform.Find("HeadGroup");
        leftLeg = transform.Find("LeftLeg");
        rightLeg = transform.Find("RightLeg");

        if (body != null)
        {
            initialBodyLocalPos = body.localPosition;
            leftWing = body.Find("LeftWing");
            rightWing = body.Find("RightWing");
        }

        if (headGroup != null)
        {
            initialHeadLocalPos = headGroup.localPosition;
        }

        if (leftWing != null)
        {
            initialLeftWingPos = leftWing.localPosition;
            initialLeftWingRot = leftWing.localRotation;
        }
        if (rightWing != null)
        {
            initialRightWingPos = rightWing.localPosition;
            initialRightWingRot = rightWing.localRotation;
        }
    }

    private void Update()
    {
        float speed = 0f;
        if (playerController != null)
        {
            Vector3 horizontalVelocity = new Vector3(playerController.velocity.x, 0f, playerController.velocity.z);
            speed = horizontalVelocity.magnitude;
        }

        bool isMoving = speed > 0.1f;

        if (isMoving)
        {
            float t = Time.time;

            // Body bobbing and tilting
            if (body != null)
            {
                float bob = Mathf.Abs(Mathf.Sin(t * walkBobSpeed)) * walkBobAmount;
                body.localPosition = initialBodyLocalPos + new Vector3(0f, bob, 0f);
                body.localRotation = Quaternion.Euler(Mathf.Sin(t * walkBobSpeed) * 4f, 0f, Mathf.Sin(t * walkBobSpeed * 0.5f) * 6f);
            }

            // Head neck bobbing (classic chicken neck movements out of phase with body)
            if (headGroup != null)
            {
                float headBobY = Mathf.Sin(t * walkBobSpeed - 0.5f) * 0.04f;
                float headBobZ = Mathf.Cos(t * walkBobSpeed) * 0.05f;
                headGroup.localPosition = initialHeadLocalPos + new Vector3(0f, headBobY, headBobZ);
                headGroup.localRotation = Quaternion.Euler(Mathf.Sin(t * walkBobSpeed) * 6f, 0f, 0f);
            }

            // Wings flapping slightly
            if (leftWing != null)
            {
                float flap = Mathf.Sin(t * wingFlapSpeed) * 15f;
                leftWing.localRotation = initialLeftWingRot * Quaternion.Euler(0f, 0f, -flap);
            }
            if (rightWing != null)
            {
                float flap = Mathf.Sin(t * wingFlapSpeed) * 15f;
                rightWing.localRotation = initialRightWingRot * Quaternion.Euler(0f, 0f, flap);
            }

            // Simple leg swinging (out of phase)
            float legSwing = Mathf.Sin(t * walkBobSpeed) * 25f;
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(legSwing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-legSwing, 0f, 0f);
        }
        else
        {
            // Reset to idle/standing poses smoothly
            if (body != null)
            {
                body.localPosition = Vector3.MoveTowards(body.localPosition, initialBodyLocalPos, Time.deltaTime * 3f);
                body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.identity, Time.deltaTime * 6f);
            }

            if (headGroup != null)
            {
                headGroup.localPosition = Vector3.MoveTowards(headGroup.localPosition, initialHeadLocalPos, Time.deltaTime * 3f);
                headGroup.localRotation = Quaternion.Slerp(headGroup.localRotation, Quaternion.identity, Time.deltaTime * 6f);
            }

            if (leftWing != null)
            {
                leftWing.localPosition = Vector3.MoveTowards(leftWing.localPosition, initialLeftWingPos, Time.deltaTime * 3f);
                leftWing.localRotation = Quaternion.Slerp(leftWing.localRotation, initialLeftWingRot, Time.deltaTime * 6f);
            }
            if (rightWing != null)
            {
                rightWing.localPosition = Vector3.MoveTowards(rightWing.localPosition, initialRightWingPos, Time.deltaTime * 3f);
                rightWing.localRotation = Quaternion.Slerp(rightWing.localRotation, initialRightWingRot, Time.deltaTime * 6f);
            }

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 8f);
        }
    }
}