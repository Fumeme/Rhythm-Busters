#if HE_SYSCORE

using UnityEngine;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    public class PlayOnClick : MonoBehaviour, IPointerClickHandler
    {
        public AudioSource output;
        public AudioClip sound;
        public FloatReference volumeScale = new FloatReference(1f);

        [Header("Options")]
        public BoolReference alwaysPlay = new BoolReference(false);
        /// <summary>
        /// Should the system test for left clicks
        /// </summary>
        public BoolReference handleLeftClick = new BoolReference(true);
        /// <summary>
        /// Should the system test for right clicks
        /// </summary>
        public BoolReference handleRightClick = new BoolReference(true);
        /// <summary>
        /// Should the system test for middle clicks
        /// </summary>
        public BoolReference handleMiddleClick = new BoolReference(true);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (alwaysPlay.Value 
                    || (eventData.button == PointerEventData.InputButton.Left && handleLeftClick.Value)
                    || (eventData.button == PointerEventData.InputButton.Middle && handleMiddleClick.Value)
                    || (eventData.button == PointerEventData.InputButton.Right && handleRightClick.Value))
                output.PlayOneShot(sound, volumeScale.Value);
        }
    }


}

#endif