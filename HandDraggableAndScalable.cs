// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Component that allows dragging an object with your hand on HoloLens.
    /// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class HandDraggableAndScalable : MonoBehaviour,
                                 IFocusable,
                                 IInputHandler,
                                 ISourceStateHandler
    {
        /// <summary>
        /// Event triggered when dragging starts.
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// Event triggered when dragging stops.
        /// </summary>
        public event Action StoppedDragging;


        public event Action StartedScaling; //TODO

        public event Action StoppedScaling; //TODO



        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        [Tooltip("Scale by which hand movement in z is multipled to move the dragged object.")]
        public float DistanceScale = 2f;

        public enum RotationModeEnum
        {
            Default,
            LockObjectRotation,
            OrientTowardUser,
            OrientTowardUserAndKeepUpright
        }

        public RotationModeEnum RotationMode = RotationModeEnum.Default;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired position")]
        [Range(0.01f, 1.0f)]
        public float PositionLerpSpeed = 0.2f;

        [Tooltip("Controls the speed at which the object will interpolate toward the desired rotation")]
        [Range(0.01f, 1.0f)]
        public float RotationLerpSpeed = 0.2f;

        public bool IsDraggingAndScalingEnabled = true;

        private Camera mainCamera;
        private bool isDragging;
        private bool isScaling;
        private bool isGazed;
        private Vector3 objRefForward;
        private Vector3 objRefUp;
        private float objRefDistance;
        private Quaternion gazeAngularOffset;
        private float handRefDistance;
        private Vector3 objRefGrabPoint;

        double refDistance;



        private Dictionary<uint, Vector3> handsPositions;
        private Dictionary<uint, Quaternion> handsRotations;
        private Dictionary<uint, IInputSource> currentInputSources;




        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
            handsPositions = new Dictionary<uint, Vector3>();
            currentInputSources = new Dictionary<uint, IInputSource>();


        }

        private void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }

            if (isGazed)
            {
                OnFocusExit();
            }
        }

        private void Update()
        {
            if (IsDraggingAndScalingEnabled && isDragging)
            {
                UpdateDragging();
            }else if (IsDraggingAndScalingEnabled && isScaling)
            {
                UpdateScaling();
            }
        }

        private void UpdateScaling()
        {

            //TODO: really should hash in but oh well doesn't matter for this sillyapp
            IInputSource inputSource0 = currentInputSources.Values.ToArray()[0];
            uint inputSourceId0 = handsPositions.Keys.ToArray()[0];

            IInputSource inputSource1 = currentInputSources.Values.ToArray()[1];
            uint inputSourceId1 = handsPositions.Keys.ToArray()[1];


            Vector3 handPosition0;
            Vector3 handPosition1;

            {
                Vector3 newHandPosition;
                inputSource0.TryGetPosition(inputSourceId0, out newHandPosition);

                handPosition1 = newHandPosition;


                /*
                Vector3 pivotPosition = GetHandPivotPosition();

                Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

                newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
                Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
                targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space

                float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

                float distanceRatio = currenthandDistance / handRefDistance;
                float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
                float targetDistance = objRefDistance + distanceOffset;

                handsPositions[inputSourceId0] = pivotPosition + (targetDirection * targetDistance);
                handPosition0 = handsPositions[inputSourceId0];
                */
            }

            {
                Vector3 newHandPosition;
                inputSource1.TryGetPosition(inputSourceId1, out newHandPosition);

                handPosition0 = newHandPosition;
                /*
                Vector3 pivotPosition = GetHandPivotPosition();

                Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

                newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
                Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
                targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space

                float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

                float distanceRatio = currenthandDistance / handRefDistance;
                float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
                float targetDistance = objRefDistance + distanceOffset;

                handsPositions[inputSourceId1] = pivotPosition + (targetDirection * targetDistance);

                handPosition1 = handsPositions[inputSourceId1];
                */
            }

            float currentDist = Vector3.Distance(handPosition0, handPosition1);

            Debug.Log("Current dist is " + currentDist);
            Debug.Log("Referenec dist is" + handRefDistance);
            float scale = currentDist / handRefDistance;

            HostTransform.localScale = new Vector3(HostTransform.localScale.x * scale, HostTransform.localScale.y, HostTransform.localScale.z * scale);


            handsPositions[inputSourceId0] = handPosition0;
            handsPositions[inputSourceId1] = handPosition1;
        }

        public void StartScaling()
        {

            if (!IsDraggingAndScalingEnabled)
            {
                return;
            }

            if (isScaling)
            {
                return;
            }

            //extract the only input source



            isScaling = true;

            IInputSource inputSource0 = currentInputSources.Values.ToArray()[0];
            uint inputSourceId0 = handsPositions.Keys.ToArray()[0];

            IInputSource inputSource1 = currentInputSources.Values.ToArray()[1];
            uint inputSourceId1 = handsPositions.Keys.ToArray()[1];

            Vector3 handPosition0;
            Vector3 handPosition1;

            {
                Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
                Vector3 handPosition;
                inputSource0.TryGetPosition(inputSourceId0, out handPosition);
                handPosition0 = handPosition;
                /*

                Vector3 pivotPosition = GetHandPivotPosition();
                handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
                objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

                Vector3 objForward = HostTransform.forward;
                Vector3 objUp = HostTransform.up;

                // Store where the object was grabbed from
                handPosition0 = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);
                */
            }

            {
                Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
                Vector3 handPosition;
                inputSource1.TryGetPosition(inputSourceId1, out handPosition);
                handPosition1 = handPosition;

                /*
                Vector3 pivotPosition = GetHandPivotPosition();
                handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
                objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

                Vector3 objForward = HostTransform.forward;
                Vector3 objUp = HostTransform.up;

                // Store where the object was grabbed from
                handPosition1 = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);
                */
            }


            handRefDistance = Vector3.Distance(handPosition0, handPosition1);


            handsPositions[inputSourceId0] = handPosition0;
            handsPositions[inputSourceId1] = handPosition1;

            StartedScaling.RaiseEvent();

        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        public void StartDragging()
        {
            if (!IsDraggingAndScalingEnabled)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            //extract the only input source
            IInputSource currentInputSource = currentInputSources.Values.ToArray()[0];
            uint currentInputSourceId = handsPositions.Keys.ToArray()[0];

            // Add self as a modal input handler, to get all inputs during the manipulation
            //InputManager.Instance.PushModalInputHandler(gameObject);

            isDragging = true;
            //GazeCursor.Instance.SetState(GazeCursor.State.Move);
            //GazeCursor.Instance.SetTargetObject(HostTransform);

            Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            Vector3 pivotPosition = GetHandPivotPosition();
            handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
            objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

            Vector3 objForward = HostTransform.forward;
            Vector3 objUp = HostTransform.up;

            // Store where the object was grabbed from
            objRefGrabPoint = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);

            Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);

            objForward = mainCamera.transform.InverseTransformDirection(objForward);       // in camera space
            objUp = mainCamera.transform.InverseTransformDirection(objUp);       		   // in camera space
            objDirection = mainCamera.transform.InverseTransformDirection(objDirection);   // in camera space
            handDirection = mainCamera.transform.InverseTransformDirection(handDirection); // in camera space

            objRefForward = objForward;
            objRefUp = objUp;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            //draggingPosition = gazeHitPosition;
            handsPositions[currentInputSourceId] = gazeHitPosition;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private Vector3 GetHandPivotPosition()
        {
            Vector3 pivot = Camera.main.transform.position + new Vector3(0, -0.2f, 0) - Camera.main.transform.forward * 0.2f; // a bit lower and behind
            return pivot;
        }

        /// <summary>
        /// Enables or disables dragging.
        /// </summary>
        /// <param name="isEnabled">Indicates whether dragging shoudl be enabled or disabled.</param>
        public void SetDragging(bool isEnabled)
        {
            if (IsDraggingAndScalingEnabled == isEnabled)
            {
                return;
            }

            IsDraggingAndScalingEnabled = isEnabled;

            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {

            //extract the only input source
            IInputSource currentInputSource = currentInputSources.Values.ToArray()[0];
            uint currentInputSourceId = handsPositions.Keys.ToArray()[0];

            Vector3 newHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

            Vector3 pivotPosition = GetHandPivotPosition();

            Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

            newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space

            float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

            float distanceRatio = currenthandDistance / handRefDistance;
            float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            float targetDistance = objRefDistance + distanceOffset;

            handsPositions[currentInputSourceId] = pivotPosition + (targetDirection * targetDistance);



            // Apply Final Position
            HostTransform.position = Vector3.Lerp(HostTransform.position, handsPositions[currentInputSourceId] + mainCamera.transform.TransformDirection(objRefGrabPoint), PositionLerpSpeed);

        }

        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // Remove self as a modal input handler
            //TODO: is this double stacking of input handlers bad?
            //InputManager.Instance.PopModalInputHandler();

            isDragging = false;
            StoppedDragging.RaiseEvent();
        }

        public void StopScaling()
        {
            if (!isScaling)
            {
                return;
            }

            // Remove self as a modal input handler
            //InputManager.Instance.PopModalInputHandler();

            isScaling = false;
            StoppedScaling.RaiseEvent();
        }

        public void OnFocusEnter()
        {
            if (!IsDraggingAndScalingEnabled)
            {
                return;
            }

            if (isGazed)
            {
                return;
            }

            isGazed = true;
        }

        public void OnFocusExit()
        {
            if (!IsDraggingAndScalingEnabled)
            {
                return;
            }

            if (!isGazed)
            {
                return;
            }

            isGazed = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
            //update the input sources
            this.currentInputSources.Remove(eventData.SourceId);
            this.handsPositions.Remove(eventData.SourceId);


            if (this.currentInputSources.Count == 0)
            {
                StopDragging();
                //InputManager.Instance.PopModalInputHandler(); //only pop the last one
            }
            else if (this.currentInputSources.Count == 1)
            {
                StopScaling();
            }
            else
            {
                throw new Exception("OH MY GOD YOU HAVE THREE HANDS AGAIN WHY!!!");
            }


        }

        public void OnInputDown(InputEventData eventData)
        {



            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }

            IInputSource currentInputSource = eventData.InputSource;
            uint currentInputSourceId = eventData.SourceId;
            this.handsPositions[currentInputSourceId] = Vector3.zero;
            this.currentInputSources[eventData.SourceId] = currentInputSource;



            if (handsPositions.Count == 1)
            {
                Debug.Log("START DRAGGING");
                //InputManager.Instance.PushModalInputHandler(gameObject); //only add one for the first one
                StartDragging();
            }
            else if (handsPositions.Count == 2)
            {
                StopDragging();
                Debug.Log("START SCALING"); //TODO
                StartScaling();
            }
            else
            {
                throw new Exception("OH MY GOD WHY DO YOU HAVE THREE HANDS????");
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            //should be functionally the same as input up for our cases, modulo the datatype
            //update the input sources
            this.currentInputSources.Remove(eventData.SourceId);
            this.handsPositions.Remove(eventData.SourceId);


            if (this.currentInputSources.Count == 0)
            {
                StopDragging();
            }
            else if (this.currentInputSources.Count == 1)
            {
                StopScaling();
                StartDragging(); //TODO: this isn't restarting dragging for some reason
            }
            else
            {
                throw new Exception("OH MY GOD YOU HAVE THREE HANDS AGAIN WHY!!!");
            }
        }
    }
}
