#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.UX
{
    public class PlayOnDemand : MonoBehaviour
    {
        public AudioSource output;
        public AudioClip sound;
        public FloatReference volumeScale = new FloatReference(1f);

        public void PlayOneShot()
        {
            output.PlayOneShot(sound, volumeScale.Value);
        }
    }


}

#endif