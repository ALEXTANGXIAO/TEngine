using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameLogic
{
    public class UIButtonSound : MonoBehaviour, IPointerClickHandler
    {
        private static Action<int> _playSoundAction = null;

        public int clickSound = 2;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_playSoundAction != null)
            {
                _playSoundAction(clickSound);
            }
        }

        public static void AddPlaySoundAction(Action<int> onClick)
        {
            _playSoundAction += onClick;
        }
    }
}