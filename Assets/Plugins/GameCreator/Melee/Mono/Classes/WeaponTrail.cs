using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCreator.Melee
{
    public class WeaponTrail
    {
        private class Data
        {
            public Vector3 pointA;
            public Vector3 pointB;
            public bool visible;
            public float time;

            public Data()
            {
                this.pointA = Vector3.zero;
                this.pointB = Vector3.zero;
                this.time = Time.time;
                this.visible = false;
            }

            public Data(Vector3 pointA, Vector3 pointB, bool invisible)
            {
                this.pointA = pointA;
                this.pointB = pointB;
                this.time = Time.time;
                this.visible = invisible;
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        private List<Data> points;
        private Mesh mesh;
        private Renderer render;
        private GameObject trail;

        private bool isActive = false;
        private float deactiveTime;
        private float fadeDuration;

        public Vector3 pointA;
        public Vector3 pointB;

        private Material material;

        private int granularity;
        private float duration;

        // INITIALIZE METHODS: --------------------------------------------------------------------

        public void Initialize(Material material, int granularity = 60, float duration = 0.5f)
        {
            this.material = material;
            this.granularity = granularity;
            this.duration = duration;

            this.points = new List<Data>();

            this.mesh = new Mesh();
            this.mesh.MarkDynamic();

            this.trail = new GameObject("Trail");
            this.trail.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            MeshFilter meshFilter = this.trail.AddComponent<MeshFilter>();
            this.render = this.trail.AddComponent<MeshRenderer>();

            meshFilter.mesh = this.mesh;
            this.render.material = this.material;
            this.render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            this.render.receiveShadows = false;

            this.trail.hideFlags = HideFlags.HideInHierarchy;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Activate()
        {
            if (this.isActive) return;

            this.isActive = true;
            foreach (Data point in this.points)
            {
                point.visible = false;
            }
        }

        public void Deactivate()
        {
            this.Deactivate(0f);
        }

        public void Deactivate(float fade)
        {
            if (!this.isActive) return;

            this.isActive = false;
            this.fadeDuration = fade;
            this.deactiveTime = Time.time;
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        public void Tick(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;

            this.GatherData();
            this.TrimData();

            this.UpdatePoints();
            this.UpdateMesh();
        }

        // GATHER METHODS: ------------------------------------------------------------------------

        private void GatherData()
        {
            bool visible = this.isActive || this.deactiveTime + this.fadeDuration >= Time.time;

            Data data = new Data(
                this.pointA,
                this.pointB,
                visible
            );

            if (this.points.Count < 2) this.points.Insert(0, data);
            else
            {
                this.points[0] = data;
                this.points.Insert(0, new Data(
                    data.pointA + (data.pointA - this.points[1].pointA),
                    data.pointB + (data.pointB - this.points[1].pointB),
                    visible
                ));
            }
        }

        // TRIM DATA: -----------------------------------------------------------------------------

        private void TrimData()
        {
            while (this.CheckLifetime() && this.CheckFading())
            {
                this.points.RemoveAt(this.points.Count - 1);
            }
        }

        private bool CheckLifetime()
        {
            if (this.points.Count == 0) return false;
            return this.points[this.points.Count - 1].time + this.duration < Time.time;
        }

        private bool CheckFading()
        {
            if (this.isActive) return true;
            if (this.points.Count == 0) return false;

            return this.deactiveTime + this.fadeDuration <= Time.time;
        }

        // UPDATE POINTS: -------------------------------------------------------------------------

        private void UpdatePoints()
        {
            float t = 0f;
            if (!this.isActive)
            {
                t = this.fadeDuration > float.Epsilon
                    ? (Time.time - this.deactiveTime) / this.fadeDuration
                    : 1f;
            }

            this.render.material.color = new Color(
                this.material.color.r,
                this.material.color.g,
                this.material.color.b,
                1f - Mathf.Clamp01(t)
            );
        }

        // CATMULL-ROM METHODS: -------------------------------------------------------------------

        private void UpdateMesh()
        {
            this.mesh.Clear();
            if (this.points.Count == 0) return;

            Vector3 previous = this.points[0].pointA;
            float magnitude = 0f;

            foreach (Data point in this.points)
            {
                Vector3 position = (point.pointA + point.pointB) * 0.5f;
                magnitude += Vector3.Distance(previous, position);
                previous = position;
            }

            List<Vector3> vertices = new List<Vector3>();

            if (magnitude <= float.Epsilon)
            {
                this.mesh.vertices = vertices.ToArray();
                this.RegenerateMesh();
                return;
            }

            int index = 0;
            int count = this.points.Count;

            foreach (Data point in this.points)
            {
                if (!point.visible)
                {
                    index += 1;
                    continue;
                }

                if (index == 0)
                {
                    index += 1;
                    continue;
                }

                if (index == count - 1)
                {
                    index += 1;
                    continue;
                }

                if (index == count - 2)
                {
                    index += 1;
                    continue;
                }

                this.GenerateSpline(index, magnitude, ref vertices);
                index += 1;
            }

            this.mesh.vertices = vertices.ToArray();
            this.RegenerateMesh();
        }

        private void GenerateSpline(int position, float magnitude, ref List<Vector3> vertices)
        {
            Vector3 pA0 = this.points[position - 1].pointA;
            Vector3 pA1 = this.points[position + 0].pointA;
            Vector3 pA2 = this.points[position + 1].pointA;
            Vector3 pA3 = this.points[position + 2].pointA;

            Vector3 pB0 = this.points[position - 1].pointB;
            Vector3 pB1 = this.points[position + 0].pointB;
            Vector3 pB2 = this.points[position + 1].pointB;
            Vector3 pB3 = this.points[position + 2].pointB;

            Vector3 positionA = pA1;
            Vector3 positionB = pB1;

            vertices.Add(positionA);
            vertices.Add(positionB);

            float distance = (
                Vector3.Distance(pA0, pA1) +
                Vector3.Distance(pA1, pA2) +
                Vector3.Distance(pA2, pA3)
            );

            if (distance <= float.Epsilon) return;

            float resolution = (distance / magnitude) / (1f / this.granularity);
            int repetitions = Mathf.FloorToInt(resolution);

            for (int i = 1; i <= repetitions; ++i)
            {
                float t = i / resolution;

                positionA = CatmullRomPosition(t, pA0, pA1, pA2, pA3);
                positionB = CatmullRomPosition(t, pB0, pB1, pB2, pB3);

                vertices.Add(positionA);
                vertices.Add(positionB);
            }
        }

        private Vector3 CatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 a = 2f * p1;
            Vector3 b = p2 - p0;
            Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
            Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

            return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        }

        private void RegenerateMesh()
        {
            Vector2[] uv = new Vector2[this.mesh.vertices.Length];
            for (int i = 0; i < uv.Length; i += 2)
            {
                float offset = (float)i / (float)uv.Length;

                uv[i + 0] = new Vector2(offset, 1f);
                uv[i + 1] = new Vector2(offset, 0f);
            }

            int[] triangles = new int[this.mesh.vertices.Length * 3];
            for (int i = 0; i < this.mesh.vertices.Length - 2; i += 2)
            {
                triangles[i * 3 + 0] = i + 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 3;

                triangles[i * 3 + 3] = i + 0;
                triangles[i * 3 + 4] = i + 3;
                triangles[i * 3 + 5] = i + 2;
            }

            this.mesh.uv = uv;
            this.mesh.triangles = triangles;
            this.mesh.RecalculateNormals();
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public void DrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (this.mesh.vertexCount == 0) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireMesh(this.mesh);

            Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.1f);
            Gizmos.DrawMesh(this.mesh);
        }
    }
}