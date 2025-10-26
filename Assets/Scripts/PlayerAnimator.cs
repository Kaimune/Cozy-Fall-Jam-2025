using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAnimator : MonoBehaviour
{
    public Transform Hand, Body;
    // Update is called once per frame
    [Header("Body Wiggle Settings")]
    [SerializeField] private float bodyWiggleAmount = 10f;  // degrees on X
    [SerializeField] private float bodyWiggleSpeed = 10f;    // speed of wiggle
    [SerializeField] private float bodyReturnSpeed = 10f;    // return speed

    [Header("Hand Wiggle Settings")]
    public float handWiggleAmount = 15f;
    public float handWiggleDuration = 0.3f; // time for half swing
    private Tween handWiggleTween;

    private bool isPunching;
    [SerializeField] private float punchAngle = 90f;
    [SerializeField] private float punchSpeed = 20f;

    [SerializeField] PlayerController myController;

    private void Update()
    {

        if (myController == null)
            return;

        AnimateBody();
        if (Hand == null || myController == null) return;

        if (myController.isMoving && handWiggleTween == null)
        {
            StartHandWiggle();
        }
        else if (!myController.isMoving && handWiggleTween != null)
        {
            StopHandWiggle();
        }
        if(!myController.isMoving)
        {
            Hand.localEulerAngles = new Vector3(-90, 0, 0f);
        }

    }

    private void AnimateBody()
    {
        if (Body == null) return;

        float targetX = 0f;
        if (myController.isMoving)
            targetX = Mathf.Sin(Time.time * bodyWiggleSpeed) * bodyWiggleAmount;

        Vector3 currentEuler = Body.localEulerAngles;
        float newX = Mathf.LerpAngle(currentEuler.x, targetX, Time.deltaTime * bodyReturnSpeed);
        Body.localEulerAngles = new Vector3(newX, currentEuler.y, currentEuler.z);
    }

    private void StartHandWiggle()
    {
        // Create a smooth looping wiggle on Z axis
        handWiggleTween = Hand.DOLocalRotate(new Vector3(Hand.localEulerAngles.x, Hand.localEulerAngles.y, handWiggleAmount), handWiggleDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopHandWiggle()
    {
        if (handWiggleTween != null)
        {
            handWiggleTween.Kill();
            handWiggleTween = null;
        }

        // Get current Z as signed angle (-180 to 180)
        float currentZ = Hand.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        // Smoothly return to Z = 0
        Hand.DOLocalRotate(
            new Vector3(Hand.localEulerAngles.x, Hand.localEulerAngles.y, 0f),
            handWiggleDuration
        ).SetEase(Ease.OutSine);
    }

    public void PunchAnimation()
    {
        if (!isPunching && Hand != null)
            StartCoroutine(PunchRoutine());
    }

    private System.Collections.IEnumerator PunchRoutine()
    {
        isPunching = true;

        float t = 0f;
        Quaternion currentRot = Hand.localRotation;
        Quaternion startRot = Quaternion.Euler(currentRot.eulerAngles.x, currentRot.eulerAngles.y,0);


        // Punch only rotates Z axis
        Quaternion punchRot = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y, punchAngle);

        // Rotate hand to punch angle
        while (t < 1f)
        {
            t += Time.deltaTime * punchSpeed;
            Hand.localRotation = Quaternion.Slerp(startRot, punchRot, t);
            yield return null;
        }

        // Return back to Z = 0
        t = 0f;
        Quaternion endRot = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y, 0f);
        while (t < 1f)
        {
            t += Time.deltaTime * punchSpeed;
            Hand.localRotation = Quaternion.Slerp(punchRot, endRot, t);
            yield return null;
        }

        isPunching = false;
    }

    public void StunAnimation(float duration)
    {
        if (Body == null) return;

        float fallDuration = duration * 0.25f;  // 0.2s
        float waitDuration = duration * 0.5f;   // 0.4s
        float riseDuration = duration * 0.25f;  // 0.2s

        Vector3 startEuler = Body.localEulerAngles;
        Vector3 fallEuler = new Vector3(startEuler.x, startEuler.y, startEuler.z + 90f);

        // Create sequence
        Sequence stunSeq = DOTween.Sequence();
        stunSeq.Append(Body.DOLocalRotate(fallEuler, fallDuration).SetEase(Ease.OutSine)); // fall
        stunSeq.AppendInterval(waitDuration); // wait
        stunSeq.Append(Body.DOLocalRotate(startEuler, riseDuration).SetEase(Ease.InSine)); // rise
    }
}
