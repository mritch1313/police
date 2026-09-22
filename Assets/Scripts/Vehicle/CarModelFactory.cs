using UnityEngine;

namespace PoliceChase.Vehicle
{
    /// <summary>
    /// Procedural car model factory. Creates a visually acceptable car without external assets.
    /// Allows replacement of model separately from logic.
    /// </summary>
    public class CarModelFactory : MonoBehaviour, ICarModel
    {
        private Transform bodyTransform;
        private Transform[] wheelVisuals = new Transform[4];
        private Renderer bodyRenderer;
        private VehicleConfig config;

        // Car dimensions
        private const float bodyLength = 4.2f;
        private const float bodyWidth = 1.8f;
        private const float bodyHeight = 0.8f;
        private const float cabinLength = 2.2f;
        private const float cabinWidth = 1.6f;
        private const float cabinHeight = 0.7f;

        public void Initialize(VehicleConfig cfg, Transform parent)
        {
            config = cfg;
            CreateCar(parent);
        }

        void CreateCar(Transform parent)
        {
            GameObject root = new GameObject("CarModel");
            root.transform.SetParent(parent);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            bodyTransform = root.transform;

            // Body - main chassis
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform);
            body.transform.localPosition = new Vector3(0, 0.6f, 0);
            body.transform.localScale = new Vector3(bodyWidth, bodyHeight, bodyLength);
            bodyRenderer = body.GetComponent<Renderer>();
            Destroy(body.GetComponent<BoxCollider>());

            // Cabin / roof
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform);
            cabin.transform.localPosition = new Vector3(0, 0.6f + bodyHeight/2 + cabinHeight/2 -0.1f, -0.2f);
            cabin.transform.localScale = new Vector3(cabinWidth, cabinHeight, cabinLength);
            var cabinRend = cabin.GetComponent<Renderer>();
            Destroy(cabin.GetComponent<BoxCollider>());

            // Windshield - slanted
            GameObject windshield = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windshield.name = "Windshield";
            windshield.transform.SetParent(root.transform);
            windshield.transform.localPosition = new Vector3(0, 1.1f, 0.6f);
            windshield.transform.localRotation = Quaternion.Euler(30f, 0, 0);
            windshield.transform.localScale = new Vector3(1.5f, 0.05f, 0.8f);
            Destroy(windshield.GetComponent<BoxCollider>());
            var wsRend = windshield.GetComponent<Renderer>();
            SetGlassMaterial(wsRend);

            // Rear window
            GameObject rearWindow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rearWindow.name = "RearWindow";
            rearWindow.transform.SetParent(root.transform);
            rearWindow.transform.localPosition = new Vector3(0, 1.1f, -0.9f);
            rearWindow.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
            rearWindow.transform.localScale = new Vector3(1.5f, 0.05f, 0.6f);
            Destroy(rearWindow.GetComponent<BoxCollider>());
            var rwRend = rearWindow.GetComponent<Renderer>();
            SetGlassMaterial(rwRend);

            // Headlights
            CreateHeadlight(root.transform, new Vector3(-0.6f, 0.5f, bodyLength/2 -0.05f));
            CreateHeadlight(root.transform, new Vector3(0.6f, 0.5f, bodyLength/2 -0.05f));

            // Taillights
            CreateTaillight(root.transform, new Vector3(-0.6f, 0.6f, -bodyLength/2 +0.05f));
            CreateTaillight(root.transform, new Vector3(0.6f, 0.6f, -bodyLength/2 +0.05f));

            // Wheels visual placeholders
            for (int i = 0; i < 4; i++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Wheel_{i}";
                wheel.transform.SetParent(root.transform);
                // Positions: 0 FL, 1 FR, 2 RL, 3 RR
                float x = (i % 2 == 0) ? -bodyWidth/2 -0.2f : bodyWidth/2 +0.2f;
                float z = (i < 2) ? bodyLength/2 -0.6f : -bodyLength/2 +0.6f;
                wheel.transform.localPosition = new Vector3(x, 0.33f, z);
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                wheel.transform.localScale = new Vector3(0.66f, 0.2f, 0.66f);
                Destroy(wheel.GetComponent<CapsuleCollider>());
                var wr = wheel.GetComponent<Renderer>();
                if (wr != null)
                {
                    wr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    wr.material.color = Color.black;
                }
                wheelVisuals[i] = wheel.transform;
            }

            ApplyMaterials();
        }

        void CreateHeadlight(Transform parent, Vector3 pos)
        {
            GameObject hl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hl.name = "Headlight";
            hl.transform.SetParent(parent);
            hl.transform.localPosition = pos;
            hl.transform.localScale = new Vector3(0.3f, 0.2f, 0.1f);
            Destroy(hl.GetComponent<BoxCollider>());
            var r = hl.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.white;
            mat.SetColor("_EmissionColor", Color.white * 2f);
            mat.EnableKeyword("_EMISSION");
            r.material = mat;
        }

        void CreateTaillight(Transform parent, Vector3 pos)
        {
            GameObject tl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tl.name = "Taillight";
            tl.transform.SetParent(parent);
            tl.transform.localPosition = pos;
            tl.transform.localScale = new Vector3(0.25f, 0.15f, 0.1f);
            Destroy(tl.GetComponent<BoxCollider>());
            var r = tl.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.red;
            mat.SetColor("_EmissionColor", Color.red * 1.5f);
            mat.EnableKeyword("_EMISSION");
            r.material = mat;
        }

        void SetGlassMaterial(Renderer r)
        {
            if (r == null) return;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.4f, 0.6f, 0.6f);
            mat.SetFloat("_Surface", 1); // transparent
            mat.SetFloat("_Blend", 0);
            r.material = mat;
        }

        void ApplyMaterials()
        {
            if (config == null) return;
            Color col = config.BodyColor;
            if (bodyRenderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = col;
                bodyRenderer.material = mat;
            }
        }

        public void UpdateWheels(float steerAngle, float[] wheelRPM, bool[] grounded)
        {
            if (wheelVisuals == null) return;
            // Steer front wheels
            for (int i = 0; i < 2 && i < wheelVisuals.Length; i++)
            {
                if (wheelVisuals[i] == null) continue;
                wheelVisuals[i].localRotation = Quaternion.Euler(0, steerAngle, 90f);
                // Rotate based on RPM
                if (wheelRPM != null && i < wheelRPM.Length)
                {
                    float rot = wheelRPM[i] * 6f * Time.deltaTime;
                    wheelVisuals[i].Rotate(rot, 0, 0, Space.Self);
                }
            }
            for (int i = 2; i < wheelVisuals.Length; i++)
            {
                if (wheelVisuals[i] == null) continue;
                if (wheelRPM != null && i < wheelRPM.Length)
                {
                    float rot = wheelRPM[i] * 6f * Time.deltaTime;
                    wheelVisuals[i].Rotate(rot, 0, 0, Space.Self);
                }
            }
        }

        public Transform GetBodyTransform() => bodyTransform;

        public void SetColor(Color color)
        {
            if (bodyRenderer != null)
                bodyRenderer.material.color = color;
        }
    }
}
