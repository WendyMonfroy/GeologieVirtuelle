//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object will get hover events and can be attached to the hands
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Activates an action set on attach and deactivates on detach")]
        public SteamVR_ActionSet activateActionSetOnAttach;

        [Tooltip("Hide the whole hand on attachment and show on detach")]
        public bool hideHandOnAttach = true;

        [Tooltip("Hide the skeleton part of the hand on attachment and show on detach")]
        public bool hideSkeletonOnAttach = false;

        [Tooltip("Hide the controller part of the hand on attachment and show on detach")]
        public bool hideControllerOnAttach = false;

        [Tooltip("The integer in the animator to trigger on pickup. 0 for none")]
        public int handAnimationOnPickup = 0;

        [Tooltip("The range of motion to set on the skeleton. None for no change.")]
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;

        public delegate void OnAttachedToHandDelegate(Hand hand);
        public delegate void OnDetachedFromHandDelegate(Hand hand);

        public event OnAttachedToHandDelegate onAttachedToHand;
        public event OnDetachedFromHandDelegate onDetachedFromHand;


        [Tooltip("Specify whether you want to snap to the hand's object attachment point, or just the raw hand")]
        public bool useHandObjectAttachmentPoint = true;

        public bool attachEaseIn = false;
        [HideInInspector]
        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;

        public bool snapAttachEaseInCompleted = false;


        [HideInInspector]
        public SteamVR_Skeleton_Poser skeletonPoser;

        [Tooltip("Should the rendered hand lock on to and follow the object")]
        public bool handFollowTransform = true;


        [Tooltip("An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.")]
        public GameObject[] hideHighlight;

        [Tooltip("Higher is better")]
        public int hoverPriority = 0;

        [System.NonSerialized]
        public Hand attachedToHand;

        [System.NonSerialized]
        public List<Hand> hoveringHands = new List<Hand>();
        public Hand hoveringHand
        {
            get
            {
                if (hoveringHands.Count > 0)
                    return hoveringHands[0];
                return null;
            }
        }


        public bool isDestroying { get; protected set; }
        public bool isHovering { get; protected set; }
        public bool wasHovering { get; protected set; }


        private void Awake()
        {
            skeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
        }

        protected virtual void Start()
        {
            if (skeletonPoser != null)
            {
                if (useHandObjectAttachmentPoint)
                {
                    //Debug.LogWarning("<b>[SteamVR Interaction]</b> SkeletonPose and useHandObjectAttachmentPoint both set at the same time. Ignoring useHandObjectAttachmentPoint.");
                    useHandObjectAttachmentPoint = false;
                }
            }
        }

        protected virtual bool ShouldIgnoreHighlight(Component component)
        {
            return ShouldIgnore(component.gameObject);
        }

        protected virtual bool ShouldIgnore(GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
            {
                if (check == hideHighlight[ignoreIndex])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a Hand starts hovering over this object
        /// </summary>
        protected virtual void OnHandHoverBegin(Hand hand)
        {
            wasHovering = isHovering;
            isHovering = true;

            hoveringHands.Add(hand);
        }


        /// <summary>
        /// Called when a Hand stops hovering over this object
        /// </summary>
        protected virtual void OnHandHoverEnd(Hand hand)
        {
            wasHovering = isHovering;

            hoveringHands.Remove(hand);

            if (hoveringHands.Count == 0)
            {
                isHovering = false;
            }
        }

        protected virtual void Update()
        {

        }


        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;

        protected virtual void OnAttachedToHand(Hand hand)
        {
            if (activateActionSetOnAttach != null)
                activateActionSetOnAttach.Activate(hand.handType);

            if (onAttachedToHand != null)
            {
                onAttachedToHand.Invoke(hand);
            }

            if (skeletonPoser != null && hand.skeleton != null)
            {
                hand.skeleton.BlendToPoser(skeletonPoser, blendToPoseTime);
            }

            attachedToHand = hand;
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            if (activateActionSetOnAttach != null)
            {
                if (hand.otherHand == null || hand.otherHand.currentAttachedObjectInfo.HasValue == false ||
                    (hand.otherHand.currentAttachedObjectInfo.Value.interactable != null &&
                     hand.otherHand.currentAttachedObjectInfo.Value.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                {
                    activateActionSetOnAttach.Deactivate(hand.handType);
                }
            }

            if (onDetachedFromHand != null)
            {
                onDetachedFromHand.Invoke(hand);
            }


            if (skeletonPoser != null)
            {
                if (hand.skeleton != null)
                    hand.skeleton.BlendToSkeleton(releasePoseBlendTime);
            }
            attachedToHand = null;
        }

        protected virtual void OnDestroy()
        {
            isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.DetachObject(this.gameObject, false);
                attachedToHand.skeleton.BlendToSkeleton(0.1f);
            }
        }


        protected virtual void OnDisable()
        {
            isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.ForceHoverUnlock();
            }
        }
    }
}
