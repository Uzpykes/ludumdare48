using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioClip> MiningClips = new List<AudioClip>();
    public List<AudioClip> ExplosionClips = new List<AudioClip>();

    public AudioSource EffectsSource;

    public static AudioManager Instance;
    private void OnEnable()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);

        Instance = this;
    }

    public void PlayRandomExplosion()
    {
        var clip = GetRandomClip(ExplosionClips);

        EffectsSource.PlayOneShot(clip);
    }

    public void PlayRandomMine()
    {
        var clip = GetRandomClip(MiningClips);

        EffectsSource.PlayOneShot(clip);
    }

    private AudioClip GetRandomClip(List<AudioClip> clips)
    {
        if (clips.Count == 1)
            return clips[0];

        var id = Random.Range(0, clips.Count - 1);
        var result = clips[id];
        var tmp = clips[clips.Count - 1];
        clips[clips.Count - 1] = clips[id];
        clips[id] = tmp;

        return result;
    }
    
}
