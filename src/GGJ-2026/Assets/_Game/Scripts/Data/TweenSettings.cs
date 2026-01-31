using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct TweenSettings
{
    public float delay;
    public float duration;
    public Ease ease;

    [SerializeField] public bool invokeEventOnComplete;
    [SerializeField] public UnityEvent onComplete;

    [SerializeField] public int loops;
    [SerializeField] public LoopType loopType;

    public TweenSettings(float delay, float duration, Ease ease, UnityEvent onComplete = null, LoopType loopType = LoopType.Restart)
    {
        this.delay = delay;
        this.duration = duration;
        this.ease = ease;

        this.onComplete = onComplete;
        this.loopType = loopType;

        invokeEventOnComplete = true;
        loops = 1;
    }

    public TweenParams GetParams()
    {
        TweenParams tweenParams = new TweenParams();

        tweenParams.SetDelay(delay);
        tweenParams.SetEase(ease);
        tweenParams.SetLoops(loops, loopType);

        if (invokeEventOnComplete)
        {
            UnityEvent completeEvent = onComplete;
            tweenParams.OnComplete(() => completeEvent?.Invoke());
        }

        return tweenParams;
    }
}
