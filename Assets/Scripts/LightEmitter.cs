using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LighthouseEscape
{
    // Emisor del puzzle de haz: rebota en los espejos; paredes, el jugador u objetos lo cortan.
    // El haz se recalcula cada frame: si algo se aparta, vuelve a conectar.
    public class LightEmitter : MonoBehaviour
    {
        [SerializeField] private Material beamMaterial;
        [SerializeField] private LightReceiver receiver;
        [SerializeField] private float maxSegmentDistance = 20f;
        [SerializeField] private int maxBounces = 8;

        private LineRenderer beam;
        private readonly List<Vector3> points = new List<Vector3>();

        private void Awake()
        {
            beam = gameObject.AddComponent<LineRenderer>();
            beam.useWorldSpace = true;
            beam.widthMultiplier = 0.05f;
            beam.positionCount = 0;
            beam.shadowCastingMode = ShadowCastingMode.Off;
            beam.receiveShadows = false;
            beam.numCapVertices = 4;

            if (beamMaterial != null)
                beam.material = beamMaterial;
        }

        private void Update()
        {
            Trace();
        }

        private void Trace()
        {
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

            beam.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                beam.SetPosition(i, points[i]);
        }
    }
}
