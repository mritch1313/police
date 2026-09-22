using UnityEngine;
using UnityEngine.UI;
using PoliceChase.Vehicle;

namespace PoliceChase.Input
{
    public class MobileInput : MonoBehaviour
    {
        [Header("References")]
        public PlayerCar PlayerCar;
        public Camera.CameraController CameraController;

        [Header("UI")]
        public Joystick VirtualJoystick; // custom
        public Button GasButton;
        public Button BrakeButton;
        public Button HandbrakeButton;
        public Button NitroButton;
        public Button StartChaseButton;

        [Header("Input State")]
        public float Motor = 0f;
        public float Steer = 0f;
        public float Brake = 0f;
        public bool Handbrake = false;

        private bool gasPressed = false;
        private bool brakePressed = false;
        private bool leftPressed = false;
        private bool rightPressed = false;

        private Vector2 cameraSwipeStart;
        private bool isSwiping = false;

        private void Start()
        {
            if (PlayerCar == null)
                PlayerCar = FindObjectOfType<PlayerCar>();
            if (CameraController == null)
                CameraController = FindObjectOfType<Camera.CameraController>();

            SetupButtons();
        }

        void SetupButtons()
        {
            if (GasButton != null)
            {
                var trigger = GasButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                AddEventTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, () => gasPressed = true);
                AddEventTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, () => gasPressed = false);
            }
            if (BrakeButton != null)
            {
                var trigger = BrakeButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                AddEventTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerDown, () => brakePressed = true);
                AddEventTrigger(trigger, UnityEngine.EventSystems.EventTriggerType.PointerUp, () => brakePressed = false);
            }
            if (HandbrakeButton != null)
            {
                HandbrakeButton.onClick.AddListener(() => Handbrake = !Handbrake);
            }
            if (NitroButton != null)
            {
                NitroButton.onClick.AddListener(() => PlayerCar?.TriggerNitro());
            }
            if (StartChaseButton != null)
            {
                StartChaseButton.onClick.AddListener(() => AI.ChaseManager.Instance?.StartChase());
            }
        }

        void AddEventTrigger(UnityEngine.EventSystems.EventTrigger trigger, UnityEngine.EventSystems.EventTriggerType type, System.Action action)
        {
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((data) => action());
            trigger.triggers.Add(entry);
        }

        private void Update()
        {
            HandleKeyboardInput(); // for editor testing
            HandleTouchInput();
            CalculateInputs();
            ApplyToCar();
        }

        void HandleKeyboardInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
                gasPressed = true;
            if (UnityEngine.Input.GetKeyUp(KeyCode.W) || UnityEngine.Input.GetKeyUp(KeyCode.UpArrow))
                gasPressed = false;

            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
                brakePressed = true;
            if (UnityEngine.Input.GetKeyUp(KeyCode.S) || UnityEngine.Input.GetKeyUp(KeyCode.DownArrow))
                brakePressed = false;

            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                leftPressed = true;
            if (UnityEngine.Input.GetKeyUp(KeyCode.A) || UnityEngine.Input.GetKeyUp(KeyCode.LeftArrow))
                leftPressed = false;

            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
                rightPressed = true;
            if (UnityEngine.Input.GetKeyUp(KeyCode.D) || UnityEngine.Input.GetKeyUp(KeyCode.RightArrow))
                rightPressed = false;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                Handbrake = !Handbrake;

            if (UnityEngine.Input.GetKeyDown(KeyCode.N))
                PlayerCar?.TriggerNitro();

            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                AI.ChaseManager.Instance?.StartChase();
#endif
        }

        void HandleTouchInput()
        {
            // Camera swipe - right side of screen
            if (UnityEngine.Input.touchCount == 1)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                // If touch on right half, treat as camera control
                if (touch.position.x > Screen.width * 0.5f && touch.position.y > Screen.height * 0.3f)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        cameraSwipeStart = touch.position;
                        isSwiping = true;
                    }
                    else if (touch.phase == TouchPhase.Moved && isSwiping)
                    {
                        Vector2 delta = touch.deltaPosition;
                        CameraController?.AddRotationInput(delta);
                    }
                    else if (touch.phase == TouchPhase.Ended)
                    {
                        isSwiping = false;
                    }
                }
            }

            // Joystick
            if (VirtualJoystick != null)
            {
                Steer = VirtualJoystick.Horizontal;
                // Motor from vertical
                float vert = VirtualJoystick.Vertical;
                if (vert > 0.1f)
                {
                    gasPressed = true;
                    brakePressed = false;
                }
                else if (vert < -0.1f)
                {
                    brakePressed = true;
                    gasPressed = false;
                }
                else
                {
                    // keep button states
                }
            }
        }

        void CalculateInputs()
        {
            // Motor
            if (gasPressed && !brakePressed)
                Motor = 1f;
            else if (brakePressed && !gasPressed)
                Motor = -1f;
            else if (gasPressed && brakePressed)
                Motor = 0f; // both pressed = brake?
            else
                Motor = 0f;

            // Steer
            if (leftPressed && !rightPressed)
                Steer = -1f;
            else if (rightPressed && !leftPressed)
                Steer = 1f;
            else if (VirtualJoystick == null)
                Steer = 0f; // if no joystick, use buttons only, otherwise joystick already set

            // Brake
            Brake = brakePressed ? 1f : 0f;
            if (Motor < 0) Brake = 0f; // reverse not brake
        }

        void ApplyToCar()
        {
            if (PlayerCar != null)
            {
                PlayerCar.SetInput(Motor, Steer, Brake, Handbrake);
            }
        }

        // Public methods for UI buttons
        public void OnGasDown() => gasPressed = true;
        public void OnGasUp() => gasPressed = false;
        public void OnBrakeDown() => brakePressed = true;
        public void OnBrakeUp() => brakePressed = false;
        public void OnLeftDown() => leftPressed = true;
        public void OnLeftUp() => leftPressed = false;
        public void OnRightDown() => rightPressed = true;
        public void OnRightUp() => rightPressed = false;
        public void OnHandbrakeToggle() => Handbrake = !Handbrake;
        public void OnNitro() => PlayerCar?.TriggerNitro();
        public void OnStartChase() => AI.ChaseManager.Instance?.StartChase();
    }

    // Simple joystick implementation
    public class Joystick : MonoBehaviour, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public RectTransform Background;
        public RectTransform Handle;
        public float Range = 100f;

        public float Horizontal { get; private set; }
        public float Vertical { get; private set; }

        private Vector2 startPos;

        private void Start()
        {
            if (Background == null) Background = GetComponent<RectTransform>();
            if (Handle == null && transform.childCount > 0) Handle = transform.GetChild(0) as RectTransform;
            startPos = Background.anchoredPosition;
        }

        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Background, eventData.position, eventData.pressEventCamera, out pos);
            pos = pos - startPos;
            float dist = pos.magnitude;
            if (dist > Range) pos = pos.normalized * Range;

            if (Handle != null) Handle.anchoredPosition = pos + startPos;

            Horizontal = pos.x / Range;
            Vertical = pos.y / Range;
        }

        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (Handle != null) Handle.anchoredPosition = startPos;
            Horizontal = 0;
            Vertical = 0;
        }
    }
}
