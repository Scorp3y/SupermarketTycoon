using UnityEngine;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    public sealed class ShelfHighlight : MonoBehaviour
    {
        [Header("Bounds Source")]
        [SerializeField] private Collider boundsCollider;

        [Header("Visual")]
        [SerializeField] private Color color = new Color(0f, 1f, 0f, 0.28f);
        [SerializeField, Min(0.001f)] private float lineWidth = 0.025f;
        [SerializeField, Min(0f)] private float yOffset = 0.04f;
        [SerializeField, Min(0f)] private float padding = 0.08f;

        [Header("Material")]
        [SerializeField] private Material lineMaterial;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();

            if (boundsCollider == null)
                boundsCollider = GetComponent<Collider>();

            ConfigureLine();
            SetVisible(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_line == null)
                _line = GetComponent<LineRenderer>();

            if (boundsCollider == null)
                boundsCollider = GetComponent<Collider>();

            if (_line != null)
                ConfigureLine();
        }
#endif

        public void SetVisible(bool visible)
        {
            EnsureLine();

            if (visible)
                Rebuild();

            _line.enabled = visible;
        }

        private void ConfigureLine()
        {
            EnsureLine();

            _line.useWorldSpace = true;
            _line.loop = true;
            _line.positionCount = 4;
            _line.startWidth = lineWidth;
            _line.endWidth = lineWidth;
            _line.startColor = color;
            _line.endColor = color;
            _line.material = lineMaterial != null ? lineMaterial : CreateDefaultMaterial();
        }

        private void Rebuild()
        {
            Bounds bounds = GetBounds();

            float minX = bounds.min.x - padding;
            float maxX = bounds.max.x + padding;
            float minZ = bounds.min.z - padding;
            float maxZ = bounds.max.z + padding;
            float y = bounds.min.y + yOffset;

            _line.SetPosition(0, new Vector3(minX, y, minZ));
            _line.SetPosition(1, new Vector3(minX, y, maxZ));
            _line.SetPosition(2, new Vector3(maxX, y, maxZ));
            _line.SetPosition(3, new Vector3(maxX, y, minZ));
        }

        private Bounds GetBounds()
        {
            if (boundsCollider != null)
                return boundsCollider.bounds;

            return new Bounds(transform.position, Vector3.one);
        }

        private void EnsureLine()
        {
            if (_line == null)
                _line = GetComponent<LineRenderer>();
        }

        private static Material CreateDefaultMaterial()
        {
            Shader shader = Shader.Find("Sprites/Default");
            return shader != null ? new Material(shader) : null;
        }
    }
}