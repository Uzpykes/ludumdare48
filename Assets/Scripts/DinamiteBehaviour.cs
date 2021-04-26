using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DinamiteBehaviour : MonoBehaviour
{
    public Animator anim;
    public ParticleSystem explosionEffect;
    public Vector3Int explosionPosition;
    public static UnityEvent<Vector3Int> onExplosion = new UnityEvent<Vector3Int>();

    private void OnEnable()
    {
    }

    public void Start()
    {
        DinamitePreExplosionDone.onPreExplodeDone.AddListener(OnDoExplosion);
        LevelManager.onMapMoved.AddListener(OnMapMove);
        anim.SetTrigger("Explode");
    }

    private void OnDoExplosion(DinamiteBehaviour behaviour)
    {
        if (behaviour != this)
            return;
        DinamitePreExplosionDone.onPreExplodeDone.RemoveListener(OnDoExplosion);
        var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        explosion.Play();
        AudioManager.Instance?.PlayRandomExplosion();
        onExplosion?.Invoke(explosionPosition);
        Destroy(this.gameObject);
    }

    private void OnMapMove(int diff)
    {
        explosionPosition += Vector3Int.up * diff;
        transform.position = explosionPosition;
    }

    private void OnDestroy()
    {
        LevelManager.onMapMoved.RemoveListener(OnMapMove);
    }
}
