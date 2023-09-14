using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// using HS2VR.HS2Settings;
using HS2VR;
using HS2VR.Util;
using Studio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Valve.VR;
using VRGIN.Controls;
using VRGIN.Controls.Handlers;
using VRGIN.Controls.Tools;
using VRGIN.Core;
using VRGIN.Helpers;
using VRGIN.Visuals;

namespace HS2VR.StudioControl
{
    public class StudioControlTool : Tool
    // MAIN STUDIO CONTROL TOOL
    // move camera with grip, manipulate objects with trigger
    // todo: test controller joystyick, seems this is not working

    {
        private GUIQuad internalGui;

        private float pressDownTime;

        private Vector2 touchDownPosition;

        private float menuDownTime;

        private float touchpadDownTime;

        private double _DeltaX;

        private double _DeltaY;

        private EVRButtonId moveSelfButton = EVRButtonId.k_EButton_Grip;

        private EVRButtonId grabScreenButton = EVRButtonId.k_EButton_Axis1;

        private string moveSelfButtonName = "rgrip";

        // private StudioSettings _settings;

        private float triggerDownTime;

        private float gripDownTime;

        private GameObject mirror1;

        private GameObject grabHandle;

        private GameObject pointer;

        private bool screenGrabbed;

        private GameObject lastGrabbedObject;

        private GameObject grabbingObject;

        private MenuHandler menuHandlder;

        private GripMenuHandler gripMenuHandler;

        private IKTool ikTool;

        private float nearestGrabable = float.MaxValue;

        private string[] FINGER_KEYS = new string[5] { "cf_J_Hand_Thumb", "cf_J_Hand_Index", "cf_J_Hand_Middle", "cf_J_Hand_Ring", "cf_J_Hand_Little" };

        private static FieldInfo f_dicGuideObject = typeof(GuideObjectManager).GetField("dicGuideObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private GameObject marker;

        public GameObject target;

        // replaced for general setting 
        // private bool lockRotXZ = true;

        private float? _GripStartTime = null;

        public override Texture2D Image => UnityHelper.LoadImage("icon_gripmove.png");

        public GUIQuad Gui { get; private set; }

        public static int preventSync;

        // changed to Steam
        // private DeviceLegacyAdapter controller => Controller;
        private SteamVR_Controller.Device controller => Controller;

        protected override void OnAwake()
        {
            base.OnAwake();
            SceneManager.sceneLoaded += OnSceneWasLoaded;
            Setup();
        }

        private void resetGUIPosition()
        // place/reset the floating GUI
        {
            var head = VR.Camera.Head;
            internalGui.transform.parent = transform;
            internalGui.transform.localScale = Vector3.one * 0.4f;
            if (head != null)
            {
                internalGui.transform.position = head.TransformPoint(new Vector3(0f, 0f, 0.3f));
                internalGui.transform.rotation = Quaternion.LookRotation(head.TransformVector(new Vector3(0f, 0f, 1f)));
            }
            else
            {
                internalGui.transform.localPosition = new Vector3(0f, 0.05f, -0.06f);
                internalGui.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }

            internalGui.transform.parent = transform.parent;
            internalGui.UpdateAspect();
        }


        // creates a sphere to show the handling point, like a 3d cursor
        private void CreatePointer()
        {
            if (pointer == null)
            {
                pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pointer.name = "pointer";
                pointer.GetComponent<SphereCollider>();
                pointer.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f) * VR.Context.Settings.IPDScale;
                pointer.transform.parent = transform;
                pointer.transform.localPosition = new Vector3(0f, -0.03f, 0.03f);
                var component = pointer.GetComponent<Renderer>();
                component.enabled = true;
                component.shadowCastingMode = ShadowCastingMode.Off;
                component.receiveShadows = false;
                component.material = new Material(MaterialHelper.GetColorZOrderShader());
            }
        }

        protected override void OnDestroy()
        {
            if (marker != null) Destroy(marker);
            if (mirror1 != null) Destroy(mirror1);
            if (grabHandle != null) Destroy(grabHandle);
            if (internalGui != null) DestroyImmediate(internalGui.gameObject);
        }

        private void Setup()
        {
            try
            {
                VRLog.Info("Creating floating GUI windows");
                // no settings for now
                // _settings = VR.Manager.Context.Settings as StudioSettings;

                // create floating GUI windows (internalGUi)
                VRLog.Info("HS2VR.StudioControlTools: creating floating GUI");
                internalGui = GUIQuad.Create(null);
                internalGui.gameObject.AddComponent<MoveableGUIObject>();
                internalGui.gameObject.AddComponent<BoxCollider>();
                internalGui.IsOwned = true;
                DontDestroyOnLoad(internalGui.gameObject);
                
                CreatePointer();
                gripMenuHandler = gameObject.AddComponent<GripMenuHandler>();
                // switch handler off till .onEnable
                VRLog.Info("HS2VR.StudioControlTools: MenuHandler disabled");
                gripMenuHandler.enabled = false;
            }
            catch (Exception obj)
            {
                VRLog.Info(obj);
            }

            // create one marker sphere on top of each controller
            if (marker == null)
            {
                marker = new GameObject("__GripMoveMarker__");
                marker.transform.parent = transform.parent;
                marker.transform.position = transform.position;
                marker.transform.rotation = transform.rotation;
            }

            // set up the controller rgrip for moving around
            moveSelfButton = EVRButtonId.k_EButton_Grip;
            moveSelfButtonName = "rgrip";
            grabScreenButton = EVRButtonId.k_EButton_Axis1;
            menuHandlder = GetComponent<MenuHandler>();
            // start an instance of IKTool
            ikTool = IKTool.instance;
            // CheckHandlers();
        }

        protected override void OnStart()
        {
            base.OnStart();
            StartCoroutine(ResetGUIPositionCo());
            // CheckHandlers();
        }

        private IEnumerator ResetGUIPositionCo()
        {
            yield return new WaitForSeconds(0.1f);
            resetGUIPosition();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (gripMenuHandler != null) gripMenuHandler.enabled = false;
            if (menuHandlder != null) menuHandlder.enabled = true;
            if ((bool)internalGui) internalGui.gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CheckHandlers();
        }

        protected void OnSceneWasLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (sceneMode == LoadSceneMode.Single) StopAllCoroutines();
            CheckHandlers();
        }


        // reacts to controller action
        // move objects, floating gui or self (camera origin)
        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (controller == null) return;
            if (controller.GetPressDown(EVRButtonId.k_EButton_Axis1)) triggerDownTime = Time.time;
            if (controller.GetPressDown(EVRButtonId.k_EButton_Grip)) gripDownTime = Time.time;
            if (controller.GetPressDown(EVRButtonId.k_EButton_ApplicationMenu)) menuDownTime = Time.time;
            if (controller.GetPressDown(EVRButtonId.k_EButton_Axis0) || controller.GetPressDown(EVRButtonId.k_EButton_A)) touchpadDownTime = Time.time;
            if (controller.GetPress(EVRButtonId.k_EButton_Axis1) && controller.GetPress(EVRButtonId.k_EButton_Grip) && controller.GetPress(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - menuDownTime > 0.5f)
            {
                // todo: test pitch lock
                VR.Settings.PitchLock = !VR.Settings.PitchLock;
                if (VR.Settings.PitchLock == true) ResetRotation();
            }

            //reset floating GUI position if long controller button press
            if (controller.GetPress(EVRButtonId.k_EButton_ApplicationMenu) && Time.time - menuDownTime > 1.5f)
            {
                resetGUIPosition();
                menuDownTime = Time.time;
            }
            
            // move object?
            if (controller.GetPressDown(EVRButtonId.k_EButton_Axis0) || controller.GetPressDown(EVRButtonId.k_EButton_A)) controller.GetPress(EVRButtonId.k_EButton_Grip);
            var pressDown = controller.GetPressDown(grabScreenButton);
            var press = controller.GetPress(grabScreenButton);
            var pressUp = controller.GetPressUp(grabScreenButton);
            if (grabHandle == null)
            {
                grabHandle = new GameObject("__GripMoveGrabHandle__");
                grabHandle.transform.parent = transform;
                grabHandle.transform.position = transform.position;
                grabHandle.transform.rotation = transform.rotation;
            }

            // move object
            if (pressDown && screenGrabbed && lastGrabbedObject != null && grabHandle != null)
            {
                grabbingObject = lastGrabbedObject;
                grabHandle.transform.position = lastGrabbedObject.transform.position;
                grabHandle.transform.rotation = lastGrabbedObject.transform.rotation;

                if (lastGrabbedObject.GetComponent<MoveableGUIObject>() != null)
                {
                    _ = lastGrabbedObject.transform.parent;
                    var component = lastGrabbedObject.GetComponent<MoveableGUIObject>();
                    if (component.guideObject != null)
                    {
                        ApplyFingerFKIfNeeded(component.guideObject);
                        grabHandle.transform.rotation = component.guideObject.transformTarget.rotation;
                        grabbingObject.transform.rotation = component.guideObject.transformTarget.rotation;
                        component.OnMoveStart();
                    }
                }
            }

            // move object?
            var flag = false;
            if ((controller.GetPressDown(EVRButtonId.k_EButton_Axis0) || controller.GetPressDown(EVRButtonId.k_EButton_A)) && lastGrabbedObject != null)

                if ((controller.GetPressDown(EVRButtonId.k_EButton_Axis0) || controller.GetPressDown(EVRButtonId.k_EButton_A)) && lastGrabbedObject != null &&
                    lastGrabbedObject.GetComponent<MoveableGUIObject>() != null)
                {
                    var guideObject = lastGrabbedObject.GetComponent<MoveableGUIObject>().guideObject;
                    if (guideObject != null)
                    {
                        if (guideObject.guideSelect != null && guideObject.guideSelect.treeNodeObject != null)
                            guideObject.guideSelect.treeNodeObject.OnClickSelect();
                        else
                            Singleton<GuideObjectManager>.Instance.selectObject = guideObject;
                        flag = true;
                    }
                }
            if ((controller.GetPressDown(EVRButtonId.k_EButton_Axis0) || controller.GetPressDown(EVRButtonId.k_EButton_A) && !flag) && (bool)gripMenuHandler &&
               gripMenuHandler.LaserVisible) VRItemObjMoveHelper.Instance.VRToggleObjectSelectOnCursor();

            // move GUI
            if (press && grabbingObject != null)
            {
                grabbingObject.transform.position = grabHandle.transform.position;
                grabbingObject.transform.rotation = grabHandle.transform.rotation;
                if (grabbingObject.GetComponent<MoveableGUIObject>() != null) grabbingObject.GetComponent<MoveableGUIObject>().OnMoved();
            }
            if (screenGrabbed && grabbingObject != null && pressUp)
            {
                if (grabbingObject.GetComponent<MoveableGUIObject>() != null) grabbingObject.GetComponent<MoveableGUIObject>().OnReleased();
                grabbingObject = null;
            }

            // move self (camera origin) on rgrip press
            // marker movement will be applied to the camera origin
            if (controller.GetPress(moveSelfButton) && grabbingObject == null)
            {
                target = VR.Camera.SteamCam.origin.gameObject;  // works in standing, resets each frame in seated
                // will not try to move in seated mode
                if (target != null && !VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode))) 
                {

                    if (mirror1 == null)
                    {
                        mirror1 = new GameObject("__GripMoveMirror1__");
                        mirror1.transform.position = transform.position;
                        mirror1.transform.rotation = transform.rotation;
                    }
                    var vector = marker.transform.position - transform.position;
                    // todo: test PitchLock 
                    var q = marker.transform.rotation * Quaternion.Inverse(transform.rotation);
                    var quaternion = RemoveLockedAxisRot(q);

                    var parent = target.transform.parent;
                    mirror1.transform.position = transform.position;
                    mirror1.transform.rotation = transform.rotation;
                    target.transform.parent = mirror1.transform;
                    mirror1.transform.rotation = quaternion * mirror1.transform.rotation;
                    mirror1.transform.position = mirror1.transform.position + vector;
                    target.transform.parent = parent;
                }
            }

            lastGrabbedObject = null;
            nearestGrabable = float.MaxValue;
            marker.transform.position = transform.position;
            marker.transform.rotation = transform.rotation;
            CheckHandlers();

        }


        // disable floating GUI if seated, enable if standing
        // ideally a mode change should be captured, this should not be in onUpdate
        // todo: switch enable/disable GUI with controller button Menu
        // todo: setting to enable only left or right hand gui
        private void CheckHandlers()
        {
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericStandingMode)))
            {
                if (gripMenuHandler != null) gripMenuHandler.enabled = true;
                if (menuHandlder != null) menuHandlder.enabled = false;
                if ((bool)internalGui) internalGui.gameObject.SetActive(true);
            }
            if (VRManager.Instance.Mode.GetType().Equals(typeof(GenericSeatedMode)))
            {
                if (gripMenuHandler != null) gripMenuHandler.enabled = false;
                if (menuHandlder != null) menuHandlder.enabled = true;
                if ((bool)internalGui) internalGui.gameObject.SetActive(false);
            }
        }


        private void ApplyFingerFKIfNeeded(GuideObject guideObject)
        {
            new List<Transform>();
            var list = new List<GuideObject>();
            if (IsFinger(guideObject.transformTarget)) list.Add(guideObject);
            foreach (var item in list) item.transformTarget.localEulerAngles = item.changeAmount.rot;
        }

        private bool IsFinger(Transform t)
        {
            var fINGER_KEYS = FINGER_KEYS;
            foreach (var value in fINGER_KEYS)
                if (t.name.Contains(value))
                    return true;
            return false;
        }

        public override List<HelpText> GetHelpTexts()
        {
            return new List<HelpText>(new HelpText[3]
            {
                HelpText.Create("Swipe as wheel.", FindAttachPosition("touchpad"), new Vector3(0.06f, 0.04f, 0f)),
                HelpText.Create("Grip and move controller to move yourself", FindAttachPosition("rgrip"), new Vector3(0.06f, 0.04f, 0f)),
                HelpText.Create("Trigger to grab objects / IK markers and move them along with controller.", FindAttachPosition("trigger"), new Vector3(-0.06f, -0.04f, 0f))
            });
        }

        private void ResetRotation()
        {
            if (target != null)
            {
                var eulerAngles = target.transform.rotation.eulerAngles;
                eulerAngles.x = 0f;
                eulerAngles.z = 0f;
                target.transform.rotation = Quaternion.Euler(eulerAngles);
            }
        }

        private IEnumerator UpdateMarkerPos()
        {
            yield return new WaitForEndOfFrame();
            marker.transform.position = transform.position;
            marker.transform.rotation = transform.rotation;
        }

        private Quaternion RemoveLockedAxisRot(Quaternion q)
        {
            if (VR.Settings.PitchLock == true) return RemoveXZRot(q);
            return q;
        }

        public static Quaternion RemoveXZRot(Quaternion q)
        {
            var eulerAngles = q.eulerAngles;
            eulerAngles.x = 0f;
            eulerAngles.z = 0f;
            return Quaternion.Euler(eulerAngles);
        }

        private void OnTriggerStay(Collider collider)
        {
            if (collider.GetComponent<GUIQuad>() != null)
            {
                screenGrabbed = true;
                lastGrabbedObject = collider.gameObject;
            }
           
            else if (collider.GetComponent<MoveableGUIObject>() != null)
            {
                screenGrabbed = true;
                if (lastGrabbedObject != null)
                {
                    var sqrMagnitude = (collider.gameObject.transform.position - pointer.transform.position).sqrMagnitude;
                    if (sqrMagnitude < nearestGrabable)
                    {
                        lastGrabbedObject = collider.gameObject;
                        nearestGrabable = sqrMagnitude;
                    }
                }
                else
                {
                    lastGrabbedObject = collider.gameObject;
                }
            }

            if (screenGrabbed && lastGrabbedObject != null && pointer != null) pointer.GetComponent<MeshRenderer>().material.color = Color.red;
        }

        private void OnTriggerEnter(Collider collider)
        {
        }

        private void OnTriggerExit(Collider collider)
        {
            var gameObject = collider.gameObject;
            if (screenGrabbed && collider.GetComponent<MoveableGUIObject>() != null && gameObject == lastGrabbedObject)
            {
                pointer.GetComponent<MeshRenderer>().material.color = Color.white;
                screenGrabbed = false;
                lastGrabbedObject = null;
            }
            
        }
    }
}
