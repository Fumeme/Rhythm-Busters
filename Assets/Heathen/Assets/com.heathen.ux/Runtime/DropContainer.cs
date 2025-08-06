#if HE_SYSCORE

using HeathenEngineering.UX;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HeathenEngineering.UX
{
    [DisallowMultipleComponent]
    public class DropContainer : HeathenBehaviour, IDropHandler
    {
        public enum RecieveMode
        {
            Take,
            Clone
        }

        /// <summary>
        /// Is there an item in this container
        /// </summary>
        public bool HasItem
        {
            get { return getFirstChild() != null; }
        }

        /// <summary>
        /// The item registered to this container
        /// </summary>
        public DragItem Item
        {
            get
            {
                return getFirstChild();
            }
        }

        [Header("Settings")]
        public RecieveMode recieveMode = RecieveMode.Take;
        public BoolReference mustBeEmpty;
        public MaskMode maskMode = MaskMode.NoConditions;
        public List<ScriptableObject> filterTypes;

        /// <summary>
        /// Occurs when an item is droped on this container
        /// </summary>
        [Header("Events")]
        public UnityDropHandledEventData evtItemDropped;

        private DragItem getFirstChild()
        {
            return SelfTransform.GetComponentInChildren<DragItem>();
        }

        public void OnDrop(PointerEventData eventData)
        {
            DragItem dropedItem = DragItem.dragObject;

            if ((mustBeEmpty.Value && HasItem) || dropedItem == null)
            {
                return;
            }

            var permited = maskMode == MaskMode.NoConditions;

            if (maskMode == MaskMode.AllowAnyOf)
            {
                if (filterTypes.Count == 0)
                    permited = true;
                else if (dropedItem.types == null || dropedItem.types.Count == 0)
                    permited = false;
                else
                {
                    //This container mask is inclusive only permit types in the mask
                    foreach (var type in filterTypes)
                    {
                        if (dropedItem.types.Contains(type))
                        {
                            permited = true;
                            break;
                        }
                    }
                }
            }
            else if (maskMode == MaskMode.DisallowAnyOf)
            {
                //This container mask is exclusive disallow types in the mask
                permited = true;

                foreach (var type in filterTypes)
                {
                    if (dropedItem.types.Contains(type))
                    {
                        permited = false;
                        break;
                    }
                }
            }
            else if (maskMode == MaskMode.AllowIfAllOf)
            {
                permited = true;

                foreach (var type in filterTypes)
                {
                    if (!dropedItem.types.Contains(type))
                    {
                        permited = false;
                        break;
                    }
                }
            }
            else if (maskMode == MaskMode.DisallowIfAllOf)
            {
                permited = false;

                foreach (var type in filterTypes)
                {
                    if (!dropedItem.types.Contains(type))
                    {
                        permited = true;
                        break;
                    }
                }
            }

            dropedItem.recieved = true;
            if (!permited)
            {
                //Item was rejected return it to its inital transform
                dropedItem.SelfTransform.SetParent(dropedItem.initalParent);
                return;
            }

            if (recieveMode == RecieveMode.Take)
            {
                var replacedItem = Item;
                if (replacedItem != null)
                {
                    //If the replaced item is a clone or has no home, destroy it
                    if (replacedItem.isClone || replacedItem.homeContainer == null)
                    {
                        Destroy(replacedItem.gameObject);
                    }
                    else //If the replaced item is not a clone and has a home return it home
                    {
                        replacedItem.SelfTransform.SetParent(replacedItem.homeContainer);
                    }
                }

                if (DragItem.dragObject != null)
                {
                    dropedItem.SelfTransform.SetParent(SelfTransform);
                }

                evtItemDropped.Invoke(
                    new Events.EventData<DragAndDropItemChangeData>()
                    {
                        sender = this,
                        value = new DragAndDropItemChangeData()
                        {
                            previousValue = replacedItem,
                            newValue = dropedItem
                        }
                    });
            }
            else
            {
                DragItem oldItem = Item;
                DragItem newItem;

                //If we have a replaced item
                var replacedItem = Item;
                if (replacedItem != null)
                {
                    //If the replaced item is a clone or has no home, destroy it
                    if (replacedItem.isClone || replacedItem.homeContainer == null)
                    {
                        Destroy(replacedItem.gameObject);
                    }
                    else //If the replaced item is not a clone and has a home return it home
                    {
                        replacedItem.SelfTransform.SetParent(replacedItem.homeContainer);
                    }
                }

                if (dropedItem.isClone)
                {
                    newItem = dropedItem;
                    //This is already a clone ... dont clone it again just take it
                    dropedItem.SelfTransform.SetParent(SelfTransform);

                    evtItemDropped.Invoke(
                        new Events.EventData<DragAndDropItemChangeData>() 
                        { 
                            sender = this, 
                            value = new DragAndDropItemChangeData() 
                            { 
                                previousValue = oldItem, 
                                newValue = newItem 
                            } 
                        });

                    return;
                }
                else
                {
                    //Is an original ... we can clone it
                    dropedItem.cloned = true;
                }

                GameObject newClone = Instantiate(dropedItem.gameObject, SelfTransform);

                DragItem newDropItem = newClone.GetComponent<DragItem>();
                if (newDropItem != null)
                {
                    newDropItem.isClone = true;
                    newDropItem.localZ = dropedItem.localZ;
                    newDropItem.canvasGroup.blocksRaycasts = true;
                    newClone.transform.localPosition = new Vector3(newClone.transform.localPosition.x, newClone.transform.localPosition.y, newDropItem.localZ);
                    newClone.transform.localEulerAngles = DragItem.dragObject.transform.localEulerAngles;
                    newClone.transform.localScale = DragItem.dragObject.transform.localScale;

                    evtItemDropped.Invoke(
                        new Events.EventData<DragAndDropItemChangeData>() 
                        { 
                            sender = this, 
                            value = new DragAndDropItemChangeData() 
                            { 
                                previousValue = oldItem, 
                                newValue = newDropItem 
                            } 
                        });
                }
            }
        }
    }
}

#endif
