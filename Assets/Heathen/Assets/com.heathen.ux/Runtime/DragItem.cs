#if HE_SYSCORE

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace HeathenEngineering.UX
{

    [RequireComponent(typeof(CanvasGroup))]
    public class DragItem : HeathenBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static DragItem dragObject;
        
        [FormerlySerializedAs("HomeContainer")]
        [Header("Settings")]
        public Transform homeContainer;
        [Tooltip("If Drag Effect type is Move Proxy then this prefab will be instantiated as a new game object and moved as the drag occurs. This new proxy object will be destroyed when the drag event ends.")]
        [FormerlySerializedAs("ProxyPrefab")]
        public GameObject proxyPrefab;
        [FormerlySerializedAs("DragEffect")]
        public DragEffect dragEffect = DragEffect.MoveSelf;
        [FormerlySerializedAs("ClearDropEffect")]
        public ClearDropBehaviour clearDropEffect = ClearDropBehaviour.ReturnHome;
        [HideInInspector]
        public Transform initalParent;
        [HideInInspector]
        public int homeIndex = -1;
        [HideInInspector]
        public int initalIndex;
        [HideInInspector]
        public CanvasGroup canvasGroup;
        [HideInInspector]
        public Canvas parentCanvas;
        [HideInInspector]
        public float localZ = 0;
        [HideInInspector]
        public bool recieved = false;
        [HideInInspector]
        public bool isClone = false;
        [HideInInspector]
        public bool cloned;

        public List<ScriptableObject> types;

        [Header("Events")]
        public UnityEvent evtDragStarted;
        public UnityEvent evtDropAccepted;
        public UnityEvent evtDropCancled;

        private Transform dragProxy;
        private GameObject ghost;

        // Use this for initialization
        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            parentCanvas = SelfTransform.parent.GetComponentInParent<Canvas>();
            localZ = SelfTransform.localPosition.z;

            if (homeContainer != null && SelfTransform.parent == homeContainer)
            {
                homeIndex = SelfTransform.GetSiblingIndex();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            initalParent = SelfTransform.parent;
            initalIndex = SelfTransform.GetSiblingIndex();
            cloned = false;
            canvasGroup.blocksRaycasts = false;
            recieved = false;

            if (dragEffect == DragEffect.MoveSelf || isClone)
            {
                dragProxy = SelfTransform;
                SelfTransform.SetParent(parentCanvas.transform);
            }
            else if (dragEffect == DragEffect.MoveClone)
            {
                GameObject newClone = Instantiate(gameObject, parentCanvas.transform);

                var clone = newClone.GetComponent<DragItem>();
                if (clone != null)
                {
                    clone.isClone = true;
                    clone.localZ = localZ;
                    clone.canvasGroup.blocksRaycasts = false;
                    clone.SelfTransform.position = SelfTransform.position;
                    clone.SelfTransform.localEulerAngles = SelfTransform.localEulerAngles;
                    clone.SelfTransform.localScale = SelfTransform.localScale;
                    clone.recieved = false;
                    dragProxy = clone.SelfTransform;
                }
            }
            else if (dragEffect == DragEffect.MoveProxy)
            {
                var nClone = Instantiate(proxyPrefab, parentCanvas.transform);
                var sTran = nClone.GetComponent<Transform>();
                sTran.position = SelfTransform.position;
                sTran.localEulerAngles = SelfTransform.localEulerAngles;
                dragProxy = sTran;
            }
            else if (dragEffect == DragEffect.LeaveProxy)
            {
                var nClone = Instantiate(proxyPrefab, SelfTransform.parent);
                var sTran = nClone.GetComponent<Transform>();
                sTran.position = SelfTransform.position;
                sTran.localEulerAngles = SelfTransform.localEulerAngles;
                ghost = nClone;
                dragProxy = SelfTransform;
                SelfTransform.SetParent(parentCanvas.transform);
                sTran.SetSiblingIndex(initalIndex);
            }

            dragObject = this;
            evtDragStarted.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                dragProxy.position = eventData.position;
            else
            {
                if (dragProxy.parent != null)
                {
                    if (parentCanvas != null)
                        //Find the current pointer position relative to the parent and set the local position accordingly
                        dragProxy.position = parentCanvas.worldCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, parentCanvas.worldCamera.WorldToScreenPoint(dragProxy.position).z));
                }
                else
                {
                    dragProxy.position = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, Camera.main.WorldToScreenPoint(dragProxy.position).z));
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragProxy != null && dragProxy != SelfTransform)
            {
                Destroy(dragProxy.gameObject);
                dragProxy = null;
            }

            if(ghost != null)
            {
                Destroy(ghost);
                ghost = null;
            }

            canvasGroup.blocksRaycasts = true;

            if (isClone)
            {
                if (!recieved)
                {
                    evtDropCancled.Invoke();

                    Destroy(gameObject);
                }
                else
                {
                    evtDropAccepted.Invoke();
                }
            }
            else
            {
                if (!recieved)
                {
                    evtDropCancled.Invoke();

                    if (clearDropEffect == ClearDropBehaviour.ReturnHome)
                    {
                        if (homeContainer != null)
                        {
                            SelfTransform.SetParent(homeContainer);
                            if (homeIndex > -1)
                                SelfTransform.SetSiblingIndex(homeIndex);

                        }
                        else
                        {
                            Destroy(gameObject);
                        }
                    }
                    else
                    {
                        if (homeContainer != null)
                        {
                            SelfTransform.SetParent(initalParent);
                            SelfTransform.SetSiblingIndex(initalIndex);

                        }
                        else
                        {
                            Destroy(gameObject);
                        }
                    }
                }
                else
                {
                    if (cloned) // Recieved and cloned, return to previous slot
                        SelfTransform.SetParent(initalParent);

                    evtDropAccepted.Invoke();
                }
                
            }

            dragObject = null;
        }
    }
}

#endif