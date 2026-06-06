using UnityEngine;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class ShelfHighlight : MonoBehaviour
    {
        [Header("Line")]
        [SerializeField] private float yOffset = 0.05f;
        [SerializeField] private float lineWidth = 0.04f;
        [SerializeField] private Color color = new Color(0f, 1f, 0f, 0.65f);

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            ConfigureLine();
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (_line == null)
                _line = GetComponent<LineRenderer>();

            if (visible)
                Rebuild();

            _line.enabled = visible;
        }

        private void ConfigureLine()
        {
            _line.useWorldSpace = true;
            _line.loop = true;
            _line.positionCount = 4;
            _line.startWidth = lineWidth;
            _line.endWidth = lineWidth;
            _line.startColor = color;
            _line.endColor = color;
        }

        private void Rebuild()
        {
            Bounds bounds = GetObjectBounds();
            float y = bounds.min.y + yOffset;

            _line.SetPosition(0, new Vector3(bounds.min.x, y, bounds.min.z));
            _line.SetPosition(1, new Vector3(bounds.min.x, y, bounds.max.z));
            _line.SetPosition(2, new Vector3(bounds.max.x, y, bounds.max.z));
            _line.SetPosition(3, new Vector3(bounds.max.x, y, bounds.min.z));
        }

        private Bounds GetObjectBounds()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);

            if (renderers.Length > 0)
                return BuildRendererBounds(renderers);

            var colliders = GetComponentsInChildren<Collider>(true);

            if (colliders.Length > 0)
                return BuildColliderBounds(colliders);

            return new Bounds(transform.position, Vector3.one);
        }

        private static Bounds BuildRendererBounds(Renderer[] renderers)
        {
            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        private static Bounds BuildColliderBounds(Collider[] colliders)
        {
            Bounds bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return bounds;
        }
    }
}