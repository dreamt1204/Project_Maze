using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldHUD : MonoBehaviour
{
    [HideInInspector] public UITweener[] tweeners;

    public bool hiddenInBeginning;
    public bool destroyAfterTweener;

    void Start()
    {
        tweeners = gameObject.GetComponents<UITweener>();

        TryInitDestroyAfterTweener();
    }

    void TryInitDestroyAfterTweener()
    {
        if (destroyAfterTweener)
        {
            if (tweeners.Length > 0)
            {
                UITweener longestTween = tweeners[0];
                foreach (UITweener tween in tweeners)
                {
                    if (tween.duration > longestTween.duration)
                        longestTween = tween;
                }

                EventDelegate.Add(longestTween.onFinished, DestroyAfterTweener);
            }
        }
    }

    void DestroyAfterTweener()
    {
        Destroy(gameObject);
    }
}
