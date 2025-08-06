#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    public class PlayOnEnter : MonoBehaviour, IPointerEnterHandler
    {
        public AudioSource output;
        public AudioClip sound;
        public FloatReference volumeScale = new FloatReference(1f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            output.PlayOneShot(sound, volumeScale.Value);
        }
    }


}

#endif