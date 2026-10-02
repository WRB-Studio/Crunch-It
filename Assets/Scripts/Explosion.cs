using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float fadeOutTime = 10;


    void Start()
    {
        StartCoroutine(fadeOut());
    }

    IEnumerator fadeOut()
    {
        Color tmpColor = GetComponent<SpriteRenderer>().color;
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while(tmpColor.a > 0)
        {
            yield return wait;
            // Preserve the original fade speed at Android's default 30 FPS.
            tmpColor.a = Mathf.Max(0, tmpColor.a - fadeOutTime / 30f);
            GetComponent<SpriteRenderer>().color = tmpColor;
        }
        Destroy(gameObject);
    }
}
