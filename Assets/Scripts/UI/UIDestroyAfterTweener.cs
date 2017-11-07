using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDestroyAfterTweener : MonoBehaviour
{
	void Start ()
    {
        UITweener[] tweeners = gameObject.GetComponents<UITweener>();

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

	void DestroyAfterTweener()
    {
        Destroy(gameObject);
	}
}