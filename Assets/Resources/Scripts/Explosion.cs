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
        while(tmpColor.a > 0)
        {
            yield return new WaitForSeconds(0.2f);
            tmpColor.a -= Time.deltaTime * fadeOutTime;
            GetComponent<SpriteRenderer>().color = tmpColor;
        }
        Destroy(gameObject);
    }
}
