#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    public class PlayOnExit : MonoBehaviour, IPointerExitHandler
    {
        public AudioSource output;
        public AudioClip sound;
        public FloatReference volumeScale = new FloatReference(1f);

        public void OnPointerExit(PointerEventData eventData)
        {
            output.PlayOneShot(sound, volumeScale.Value);
        }
    }


}

#endif