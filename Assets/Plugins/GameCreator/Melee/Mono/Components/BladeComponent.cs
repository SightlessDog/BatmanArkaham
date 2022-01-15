namespace GameCreator.Melee
{
    using System;
    using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class BladeComponent : MonoBehaviour
	{
        public enum CaptureHitModes
        {
            Segment,
            Sphere,
            Box
        }

        private struct BoxData
        {
            public Vector3 center;
            public Quaternion rotation;

            public BoxData(Vector3 center, Quaternion rotation)
            {
                this.center = center;
                this.rotation = rotation;
            }
        }

        [Serializable]
        public class BladeEvent : UnityEvent
        { }

        private static readonly Color GIZMOS_DEFAULT_COLOR = Color.yellow;
        private static readonly Color GIZMOS_ACTIVE_COLOR = Color.red;

        private static readonly GameObject[] EMPTY_GO_LIST = new GameObject[0];

        // PROPERTIES: ----------------------------------------------------------------------------

        public CharacterMelee Melee { get; private set; }

        public CaptureHitModes captureHits = CaptureHitModes.Box;
        public LayerMask layerMask = -1;

        public float segmentResolution = 0.05f;
        public Vector3 pointA = new Vector3(0, 0, 0);
        public Vector3 pointB = new Vector3(0, 0, 1);

        public float radius = 0.5f;
        public Vector3 offset = Vector3.zero;

        [Range(0, 19)]
        public int boxInterframePredictions = 5;
        public Vector3 boxCenter = new Vector3(0, 0, 0.75f);
        public Vector3 boxSize = new Vector3(0.3f, 0.3f, 1.5f);

        [Space]
        public bool debugMode = false;

        [Space]
        public BladeEvent EventAttackStart;
        public BladeEvent EventAttackEnd;
        public BladeEvent EventAttackActivation;
        public BladeEvent EventAttackRecovery;

        private int prevPhase = -1;

        private int prevCaptureFrame = -100;
        private Vector3 prevPositionA = Vector3.zero;
        private Vector3 prevPositionB = Vector3.zero;
        private BoxData prevBoxBounds = default;

        private readonly Collider[] bufferColliders = new Collider[20];
        private readonly RaycastHit[] bufferRaycastHits = new RaycastHit[20];
        
        private BoxData[] boxInterframeCaptures = new BoxData[20];

        // trail
        private WeaponTrail weaponTrail;
        public bool enableWeaponTrail = true;
        public Material trailMaterial;
        public int trailGranularity = 60;
        public float trailDuration = 0.5f;

        #if UNITY_EDITOR
        private float capturingHitsTime;
        #endif

        // INITIALIZERS: --------------------------------------------------------------------------

        public void Setup(CharacterMelee melee)
        {
            this.Melee = melee;
        }

        private void Awake()
        {
            if (this.enableWeaponTrail)
            {
                this.weaponTrail = new WeaponTrail()
                {
                    pointA = this.pointA,
                    pointB = this.pointB
                };

                this.weaponTrail.Initialize(
                    this.trailMaterial,
                    this.trailGranularity,
                    this.trailDuration
                );
            }
        }

        // UPDATE METHOD: -------------------------------------------------------------------------

        private void Update()
        {
            if (!this.Melee) return;

            int currPhase = this.Melee.GetCurrentPhase();
            if (currPhase == this.prevPhase) return;

            switch (currPhase)
            {
                case -1:
                    if (this.weaponTrail != null) this.weaponTrail.Deactivate(0.2f);
                    this.EventAttackEnd.Invoke();
                    break;

                case  0:
                    this.EventAttackStart.Invoke();
                    if (this.weaponTrail != null) this.weaponTrail.Deactivate(0.2f);
                    break;

                case  1:
                    if (this.weaponTrail != null) this.weaponTrail.Activate();
                    this.EventAttackActivation.Invoke();
                    break;

                case  2:
                    if (this.weaponTrail != null) this.weaponTrail.Deactivate(0.2f);
                    this.EventAttackRecovery.Invoke();
                    break;
            }

            this.prevPhase = currPhase;
        }

        private void LateUpdate()
        {
            if (this.weaponTrail != null)
            {
                this.weaponTrail.Tick(
                    this.transform.TransformPoint(this.pointA),
                    this.transform.TransformPoint(this.pointB)
                );
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public GameObject[] CaptureHits()
        {
            #if UNITY_EDITOR
            this.capturingHitsTime = Time.time;
            #endif

            GameObject[] candidates = EMPTY_GO_LIST;

            switch (this.captureHits)
            {
                case CaptureHitModes.Segment: candidates = CaptureHitsSegment(); break;
                case CaptureHitModes.Sphere: candidates = CaptureHitsSphere(); break;
                case CaptureHitModes.Box: candidates = CaptureHitsBox(); break;
            }

            this.prevCaptureFrame = Time.frameCount;
            return candidates;
        }

        private GameObject[] CaptureHitsSegment()
        {
            Vector3 curPositionA = this.transform.TransformPoint(this.pointA);
            Vector3 curPositionB = this.transform.TransformPoint(this.pointB);

            bool hasPreviousCapture = Time.frameCount <= this.prevCaptureFrame + 1;
            Vector3 prevPositionA = hasPreviousCapture ? this.prevPositionA : curPositionA;
            Vector3 prevPositionB = hasPreviousCapture ? this.prevPositionB : curPositionB;

            float maxDistance = Mathf.Max(
                Vector3.Distance(curPositionA, prevPositionA),
                Vector3.Distance(curPositionB, prevPositionB)
            );

            int partitions = Mathf.FloorToInt(maxDistance / this.segmentResolution);
            List<GameObject> candidates = new List<GameObject>();

            for (int i = 0; i <= partitions; ++i)
            {
                float t = ((float)i) / ((float)partitions);
                Vector3 pointA = Vector3.Lerp(prevPositionA, curPositionA, t);
                Vector3 pointB = Vector3.Lerp(prevPositionB, curPositionB, t);
                Vector3 direction = pointB - pointA;

                Debug.DrawLine(pointA, pointB, Color.red);

                int numCollisions = Physics.RaycastNonAlloc(
                    pointA, direction.normalized,
                    this.bufferRaycastHits,
                    direction.magnitude,
                    this.layerMask,
                    QueryTriggerInteraction.Ignore
                );

                for (int j = 0; j < numCollisions; ++j)
                {
                    GameObject target = this.bufferRaycastHits[j].collider.gameObject;
                    if (!candidates.Contains(target)) candidates.Add(target);
                }
            }

            this.prevPositionA = curPositionA;
            this.prevPositionB = curPositionB;

            return candidates.ToArray();
        }

        private GameObject[] CaptureHitsSphere()
        {
            int numCollisions = Physics.OverlapSphereNonAlloc(
                transform.TransformPoint(this.offset),
                this.radius,
                this.bufferColliders,
                this.layerMask,
                QueryTriggerInteraction.Ignore
            );

            GameObject[] collisions = new GameObject[numCollisions];
            for (int i = 0; i < numCollisions; ++i)
            {
                collisions[i] = this.bufferColliders[i].gameObject;
            }

            return collisions;
        }

        private GameObject[] CaptureHitsBox()
        {
            int predictions = 1;
            BoxData currentBoxData = new BoxData(
                this.transform.TransformPoint(this.boxCenter), 
                this.transform.rotation
            );

            this.boxInterframeCaptures[0] = currentBoxData; 
            bool hasPreviousCapture = Time.frameCount <= this.prevCaptureFrame + 1;
            
            if (hasPreviousCapture)
            {
                predictions = this.boxInterframePredictions;
                for (int i = 0; i < predictions; ++i)
                {
                    float t = ((float) (i + 1f)) / ((float) predictions);
                    this.boxInterframeCaptures[i] = new BoxData(
                        Vector3.Lerp(currentBoxData.center, this.prevBoxBounds.center, t),
                        Quaternion.Lerp(currentBoxData.rotation, this.prevBoxBounds.rotation, t)
                    );
                }
            }

            this.prevBoxBounds = currentBoxData;
            List<GameObject> candidates = new List<GameObject>();
            
            for (int i = 0; i < boxInterframeCaptures.Length; ++i)
            {
                int numCollisions = Physics.OverlapBoxNonAlloc(
                    this.boxInterframeCaptures[i].center,
                    this.boxSize / 2f,
                    this.bufferColliders,
                    this.boxInterframeCaptures[i].rotation,
                    this.layerMask,
                    QueryTriggerInteraction.Ignore
                );

                for (int j = 0; j < numCollisions; ++j)
                {
                    GameObject target = this.bufferColliders[j].gameObject;
                    if (!candidates.Contains(target)) candidates.Add(target);
                }
            }

            return candidates.ToArray();
        }

        public Vector3 GetImpactPosition()
        {
            Vector3 posA = transform.TransformPoint(this.pointA);
            Vector3 posB = transform.TransformPoint(this.pointB);
            return Vector3.Lerp(posA, posB, 0.5f);
        }

        // GIZMOZ: --------------------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            Gizmos.color = GIZMOS_DEFAULT_COLOR;
            bool isHitActive = false;

            #if UNITY_EDITOR
            if (Application.isPlaying && Time.time - this.capturingHitsTime < 0.1f)
            {
                Gizmos.color = GIZMOS_ACTIVE_COLOR;
                isHitActive = true;
            }
            #endif

            switch (this.captureHits)
            {
                case CaptureHitModes.Segment:
                    Vector3 segmentA = transform.TransformPoint(this.pointA);
                    Vector3 segmentB = transform.TransformPoint(this.pointB);
                    if (isHitActive)
                    {
                        Gizmos.DrawSphere(segmentA, 0.01f);
                        Gizmos.DrawSphere(segmentB, 0.01f);
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(segmentA, 0.01f);
                        Gizmos.DrawWireSphere(segmentB, 0.01f);
                    }

                    Gizmos.DrawLine(segmentA, segmentB);
                    break;

                case CaptureHitModes.Sphere:
                    Vector3 offset = transform.TransformPoint(this.offset);
                    if (isHitActive)
                    {
                        Gizmos.DrawSphere(offset, this.radius);
                    }
                    else
                    {
                        Gizmos.DrawWireSphere(offset, this.radius);
                    }
                    break;

                case CaptureHitModes.Box:
                    Vector3 center = transform.TransformPoint(this.boxCenter);
                    Matrix4x4 gizmosMatrix = Gizmos.matrix;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    if (isHitActive)
                    {
                        Gizmos.DrawCube(this.boxCenter, this.boxSize);

                        for (int i = 0; i < this.boxInterframeCaptures.Length; ++i)
                        {
                            if (Time.frameCount > this.prevCaptureFrame + 1) continue;
                            if (this.boxInterframeCaptures[i].rotation == default) continue;
                            if (this.boxInterframeCaptures[i].center == default) continue;
                            
                            Gizmos.matrix = Matrix4x4.TRS(
                                this.boxInterframeCaptures[i].center,
                                this.boxInterframeCaptures[i].rotation,
                                Vector3.one
                            );
                            
                            Gizmos.DrawWireCube(Vector3.zero, this.boxSize);
                        }
                    }
                    else
                    {
                        
                        Gizmos.DrawWireCube(this.boxCenter, this.boxSize);
                    }

                    Gizmos.matrix = gizmosMatrix;
                    break;
            }

            if (!this.debugMode) return;
            weaponTrail?.DrawGizmos();
        }
    }
}
