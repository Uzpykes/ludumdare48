using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinamiteBehaviour : MonoBehaviour
{
    public Animator anim;

    public void Start()
    {
        DinamitePreExplosionDone.onPreExplodeDone.AddListener(OnDoExplosion);
        anim.SetTrigger("Explode");
    }

    private void OnDoExplosion()
    {
        Destroy(this.gameObject);
    }
}
