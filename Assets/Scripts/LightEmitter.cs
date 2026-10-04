using System.Collections.Generic;
using UnityEngine;

namespace LighthouseEscape
{
    // Emisor del puzzle de haz: rebota en los espejos; paredes, el jugador u objetos lo cortan.
    // El haz se recalcula cada frame: si algo se aparta, vuelve a conectar.
    // Arranca encendido por defecto (escenas antiguas); la escena del nivel lo deja apagado
    // y el candado lo enciende vía IUnlockable.
    public class LightEmitter : MonoBehaviour, IUnlockable
    {
        [SerializeField] private LightReceiver receiver;
        [SerializeField] private float maxSegmentDistance = 20f;
        [SerializeField] private int maxBounces = 8;
        [SerializeField] private bool startsPowered = true;
        [SerializeField] private float flowSpeed = 1.2f;
        [SerializeField] private float flowRate = 30f;

        private static readonly Color OffColor = new Color(0.25f, 0.25f, 0.28f);

        private ParticleSystem flujo;
        private ParticleSystem destello;
        private readonly List<Vector3> points = new List<Vector3>();
        private Renderer emitterRenderer;
        private Color onColor;
        private bool powered;

        public bool Powered => powered;

        private void Awake()
        {
            flujo = null;
            destello = null;

            ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                if (systems[i].name == "Flujo")
                    flujo = systems[i];
                else if (systems[i].name == "Destello")
                    destello = systems[i];
            }

            if (flujo != null)
            {
                ParticleSystem.EmissionModule emission = flujo.emission;
                emission.enabled = false;
            }

            if (destello != null)
            {
                ParticleSystem.EmissionModule emission = destello.emission;
                emission.enabled = false;
            }

            emitterRenderer = GetComponent<Renderer>();
            if (emitterRenderer != null)
                onColor = emitterRenderer.material.color;

            powered = startsPowered;
            ApplyPower();
        }

        public void Unlock()
        {
            powered = true;
            ApplyPower();
            Debug.Log("[Puzzle] Emisor encendido");
        }

        public void Lock()
        {
            powered = false;
            ApplyPower();
            Debug.Log("[Puzzle] Emisor apagado");
        }

        private void ApplyPower()
        {
            if (!powered)
            {
                if (flujo != null)
                    flujo.Clear();

                if (destello != null)
                    destello.Clear();
            }

            if (emitterRenderer != null)
                emitterRenderer.material.color = powered ? onColor : OffColor;
        }

        private void Update()
        {
            Trace();

            if (powered)
                EmitBeam();
        }

        private void EmitBeam()
        {
            if (points.Count < 2)
                return;

            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f)
                return;

            if (flujo != null)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector3 start = points[i];
                    Vector3 delta = points[i + 1] - start;
                    float length = delta.magnitude;
                    if (length < 0.001f)
                        continue;

                    Vector3 dir = delta / length;
                    int count = Mathf.Max(1, Mathf.CeilToInt(length * deltaTime * flowRate));

                    for (int n = 0; n < count; n++)
                    {
                        ParticleSystem.EmitParams emitParams = default;
                        emitParams.position = start + delta * Random.value + Random.insideUnitSphere * 0.02f;
                        emitParams.velocity = dir * flowSpeed + Random.insideUnitSphere * 0.15f;
                        flujo.Emit(emitParams, 1);
                    }
                }
            }

            EmitFlash(points[0]);
            for (int i = 1; i < points.Count; i++)
                EmitFlash(points[i]);
        }

        private void EmitFlash(Vector3 position)
        {
            if (destello == null)
                return;

            ParticleSystem.EmitParams emitParams = default;
            emitParams.position = position;
            destello.Emit(emitParams, 1);
        }

        private void Trace()
        {
            if (!powered)
            {
                if (receiver != null)
                    receiver.SetPowered(false);

                return;
            }

            points.Clear();
            points.Add(transform.position);

            Vector3 origin = transform.position;
            Vector3 direction = transform.forward;
            bool connected = false;

            for (int bounce = 0; bounce < maxBounces; bounce++)
            {
                if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxSegmentDistance, ~0, QueryTriggerInteraction.Ignore))
                {
                    points.Add(origin + direction * maxSegmentDistance);
                    break;
                }

                points.Add(hit.point);

                if (hit.collider.GetComponentInParent<Mirror>() != null)
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    origin = hit.point + direction * 0.05f;
                    continue;
                }

                if (hit.collider.GetComponentInParent<LightReceiver>() != null)
                    connected = true;

                break;
            }

            if (receiver != null)
                receiver.SetPowered(connected);
        }
    }
}
