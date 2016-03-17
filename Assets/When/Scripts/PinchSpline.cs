using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using When.Interfaces;

namespace When {
    public class PinchSpline : MonoBehaviour {

        [SerializeField] Material _material = null;
        [SerializeField] Color _drawColor = Color.white;
        [SerializeField] [Range(0, 1)] float _smoothingDelay = 0.01f;
        [SerializeField] [Range(0, 1)] float _drawRadius = 0.002f;
        [SerializeField] [Range(3, 24)] int _drawResolution = 8;
        [SerializeField] [Range(0, 1)] float _minSegmentLength = 0.005f;

        readonly Dictionary<IPosition, Entry> _entries = new Dictionary<IPosition, Entry>();

        void Start() {}

        public void OnBegin(IPosition iPosition) {
            if (!_entries.ContainsKey(iPosition)) {
                _entries.Add(iPosition, new Entry(this, iPosition));
            }
            _entries[iPosition].Start();
        }

        public void OnFinish(IPosition iPosition) {
            _entries[iPosition].Stop();
        }

        class Entry {
            readonly DrawState _drawState;

            readonly PinchSpline _parent;
            readonly IPosition _position;
            Coroutine _coroutine;

            public Entry(PinchSpline parent, IPosition iPosition) {
                _parent = parent;
                _position = iPosition;
                _drawState = new DrawState(parent);
            }

            public void Start() {
                _coroutine = _parent.StartCoroutine(Draw());
            }

            IEnumerator Draw() {
                _drawState.BeginNewLine();
                
                yield return null;

                while (true) {
                    _drawState.UpdateLine(_position.Position);
                    yield return null;
                }
            }

            public void Stop() {
                _drawState.FinishLine();
                _parent.StopCoroutine(_coroutine);
            }
        }

        class DrawState {
            static readonly Transform LinesGameObject = new GameObject("[Lines]").transform;

            List<Vector3> _vertices = new List<Vector3>();
            List<int> _tris = new List<int>();
            List<Vector2> _uvs = new List<Vector2>();
            List<Color> _colors = new List<Color>();

            PinchSpline _parent;

            int _rings = 0;

            Vector3 _prevRing0 = Vector3.zero;
            Vector3 _prevRing1 = Vector3.zero;

            Vector3 _prevNormal0 = Vector3.zero;

            Mesh _mesh;
            SmoothedVector3 _smoothedPosition;

            public DrawState(PinchSpline parent) {
                _parent = parent;

                _smoothedPosition = new SmoothedVector3();
                _smoothedPosition.delay = _parent._smoothingDelay;
                _smoothedPosition.reset = true;
            }

            public GameObject BeginNewLine() {
                _rings = 0;
                _vertices.Clear();
                _tris.Clear();
                _uvs.Clear();
                _colors.Clear();

                _smoothedPosition.reset = true;

                _mesh = new Mesh();
                _mesh.name = "Line Mesh";
                _mesh.MarkDynamic();

                GameObject lineObj = new GameObject("Line Object");
                lineObj.transform.position = Vector3.zero;
                lineObj.transform.rotation = Quaternion.identity;
                lineObj.transform.localScale = Vector3.one;
                lineObj.AddComponent<MeshFilter>().mesh = _mesh;
                lineObj.AddComponent<MeshRenderer>().sharedMaterial = _parent._material;
                lineObj.transform.SetParent(LinesGameObject.transform);

                return lineObj;
            }

            public void UpdateLine(Vector3 position) {
                _smoothedPosition.Update(position, Time.deltaTime);

                bool shouldAdd = false;

                shouldAdd |= _vertices.Count == 0;
                shouldAdd |= Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._minSegmentLength;

                if (shouldAdd) {
                    addRing(_smoothedPosition.value);
                    updateMesh();
                }
            }

            public void FinishLine() {
                _mesh.Optimize();
                _mesh.UploadMeshData(true);
            }

            void updateMesh() {
                _mesh.SetVertices(_vertices);
                _mesh.SetColors(_colors);
                _mesh.SetUVs(0, _uvs);
                _mesh.SetIndices(_tris.ToArray(), MeshTopology.Triangles, 0);
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
            }

            void addRing(Vector3 ringPosition) {
                _rings++;

                if (_rings == 1) {
                    addVertexRing();
                    addVertexRing();
                    addTriSegment();
                }

                addVertexRing();
                addTriSegment();

                Vector3 ringNormal = Vector3.zero;
                if (_rings == 2) {
                    Vector3 direction = ringPosition - _prevRing0;
                    float angleToUp = Vector3.Angle(direction, Vector3.up);

                    if (angleToUp < 10 || angleToUp > 170) {
                        ringNormal = Vector3.Cross(direction, Vector3.right);
                    } else {
                        ringNormal = Vector3.Cross(direction, Vector3.up);
                    }

                    ringNormal = ringNormal.normalized;

                    _prevNormal0 = ringNormal;
                } else if (_rings > 2) {
                    Vector3 prevPerp = Vector3.Cross(_prevRing0 - _prevRing1, _prevNormal0);
                    ringNormal = Vector3.Cross(prevPerp, ringPosition - _prevRing0).normalized;
                }

                if (_rings == 2) {
                    updateRingVerts(0,
                                    _prevRing0,
                                    ringPosition - _prevRing1,
                                    _prevNormal0,
                                    0);
                }

                if (_rings >= 2) {
                    updateRingVerts(_vertices.Count - _parent._drawResolution,
                                    ringPosition,
                                    ringPosition - _prevRing0,
                                    ringNormal,
                                    0);
                    updateRingVerts(_vertices.Count - _parent._drawResolution * 2,
                                    ringPosition,
                                    ringPosition - _prevRing0,
                                    ringNormal,
                                    1);
                    updateRingVerts(_vertices.Count - _parent._drawResolution * 3,
                                    _prevRing0,
                                    ringPosition - _prevRing1,
                                    _prevNormal0,
                                    1);
                }

                _prevRing1 = _prevRing0;
                _prevRing0 = ringPosition;

                _prevNormal0 = ringNormal;
            }

            void addVertexRing() {
                for (int i = 0; i < _parent._drawResolution; i++) {
                    _vertices.Add(Vector3.zero);  //Dummy vertex, is updated later
                    _uvs.Add(new Vector2(i / (_parent._drawResolution - 1.0f), 0));
                    _colors.Add(_parent._drawColor);
                }
            }

            // Connects the most recently added vertex ring to the one before it
            void addTriSegment() {
                for (int i = 0; i < _parent._drawResolution; i++) {
                    int i0 = _vertices.Count - 1 - i;
                    int i1 = _vertices.Count - 1 - ((i + 1) % _parent._drawResolution);

                    _tris.Add(i0);
                    _tris.Add(i1 - _parent._drawResolution);
                    _tris.Add(i0 - _parent._drawResolution);

                    _tris.Add(i0);
                    _tris.Add(i1);
                    _tris.Add(i1 - _parent._drawResolution);
                }
            }

            void updateRingVerts(int offset, Vector3 ringPosition, Vector3 direction, Vector3 normal, float radiusScale) {
                direction = direction.normalized;
                normal = normal.normalized;

                for (int i = 0; i < _parent._drawResolution; i++) {
                    float angle = 360.0f * (i / (float)(_parent._drawResolution));
                    Quaternion rotator = Quaternion.AngleAxis(angle, direction);
                    Vector3 ringSpoke = rotator * normal * _parent._drawRadius * radiusScale;
                    _vertices[offset + i] = ringPosition + ringSpoke;
                }
            }
        }
    }
}
