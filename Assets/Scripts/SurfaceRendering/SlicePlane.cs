using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SlicePlane : MonoBehaviour
{
    public ComputeShader MarchingShader;

    ComputeBuffer _segmentsBuffer;
    ComputeBuffer _segmentsCountBuffer;
    ComputeBuffer _weightsBuffer;
    // ComputeBuffer _vectorsBuffer;

    public NoiseGenerator NoiseGenerator;
    public SurfaceControl surfaceControl;

    public Material segmentMat;
    public Mesh segmentMesh;
    
    public GameObject segmentPrefab;
    public Transform planeIntersectParent;
    public GameObject grabPlane;
    public Surface closestSurface;
    private int orientation;

    int planeGridPoints;

    private void Awake()
    {
        planeGridPoints = GridMetrics.PointsPerChunk * 2 + 16;
        CreateBuffers();

        closestSurface = surfaceControl.surfaces[0];

    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    struct Segment
    {
        public Vector3 a;
        public Vector3 b;

        public static int SizeOf => sizeof(float) * 3 * 2;
        public new string ToString => string.Format("{0}, {1}", a, b);
    }

    float[] _weights;

    public void Render()
    {
        NoiseGenerator.isPlane = true;
        NoiseGenerator.planeForward = transform.forward ;
        NoiseGenerator.planeRight = transform.right ;
        NoiseGenerator.Offset = transform.position - closestSurface.transform.position;
        NoiseGenerator.size = closestSurface.size;
        NoiseGenerator.function = closestSurface.function;
        NoiseGenerator.orientation = closestSurface.orientation;
        _weights = NoiseGenerator.GetNoisePlane(planeGridPoints);
        ConstructLine();
    }

    Vector3 oldPosition = Vector3.zero;
    Quaternion oldRotation = Quaternion.identity;

    void Update()
    {

        
        for (int i = 0; i < surfaceControl.numberOfChildren; i++)
        {
            if ((surfaceControl.surfaces[i].transform.position - transform.position).magnitude < 1) //should this be 1?
            {
                closestSurface = surfaceControl.surfaces[i];
                // Debug.Log(i);
            }
        }
        

        if ((oldPosition - transform.position).magnitude > 0 || (oldRotation.eulerAngles - transform.rotation.eulerAngles).magnitude > 0)
        {
            Render();
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }

        switch (orientation)
        {
            case 0:
                transform.SetPositionAndRotation(grabPlane.transform.position, grabPlane.transform.rotation);
                break;

            case 1:
                transform.position = new Vector3(0.0f, grabPlane.transform.position.y, 0.0f);
                break;

            case 2:
                transform.position = new Vector3(grabPlane.transform.position.x, 1.5f, 0.0f);
                break;

            case 3:
                transform.position = new Vector3(0.0f, 1.5f, grabPlane.transform.position.z);
                break;
        }

                grabPlane.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
    }

    void ConstructLine()
    {
        MarchingShader.SetBuffer(0, "_Segments", _segmentsBuffer);
        MarchingShader.SetBuffer(0, "_Weights", _weightsBuffer);
        MarchingShader.SetFloat("_OtherSize", 1.0f);
        MarchingShader.SetInt("_ChunkSize", planeGridPoints);
        MarchingShader.SetFloat("_IsoLevel", .5f);
        MarchingShader.SetVector("_PlaneForward", transform.forward);
        MarchingShader.SetVector("_PlaneRight", transform.right);
        // MarchingShader.SetBuffer(0, "_Vectors", _vectorsBuffer);

        _weightsBuffer.SetData(_weights);
        _segmentsBuffer.SetCounterValue(0);

        MarchingShader.Dispatch(0, planeGridPoints / GridMetrics.NumThreads, planeGridPoints / GridMetrics.NumThreads, 1);

        // segmentMat.SetBuffer("_Vectors", _vectorsBuffer);
        // var bounds = new Bounds(transform.position, new Vector3(GridMetrics.PointsPerChunk, GridMetrics.PointsPerChunk, GridMetrics.PointsPerChunk));
        // Graphics.DrawMeshInstancedProcedural(segmentMesh, 0, segmentMat, bounds, GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk);

        Segment[] segments = new Segment[ReadSegmentCount()];
        _segmentsBuffer.GetData(segments);

        
        foreach (Transform child in planeIntersectParent)
        {
            GameObject.Destroy(child.gameObject);
        }
        

        foreach (Segment seg in segments)
        {

            Vector3 pt1 = transform.position + (transform.right * seg.a.x + transform.forward * seg.a.z) * (1.0f / 100);
            Vector3 pt2 = transform.position + (transform.right * seg.b.x + transform.forward * seg.b.z) * (1.0f / 100);
            Vector3 pos = (pt1 + pt2) * 0.5f;
            Vector3 fromVolume = (pos - closestSurface.transform.position) * 100.0f;


            if (Mathf.Abs(fromVolume.x) < GridMetrics.PointsPerChunk &&
                Mathf.Abs(fromVolume.y) < GridMetrics.PointsPerChunk &&
                Mathf.Abs(fromVolume.z) < GridMetrics.PointsPerChunk)
            {
                Vector3 dir = (pt2 - pt1);
                GameObject newSeg = Instantiate(segmentPrefab, pos, Quaternion.FromToRotation(Vector3.up, dir), planeIntersectParent);
                newSeg.transform.localScale = new Vector3(0.005f, dir.magnitude * 0.6f, 0.005f);
            }
        }

    }

    private void OnDrawGizmos()
    {
        if (_weights == null || _weights.Length == 0)
        {
            return;
        }
        for (int x = 0; x < planeGridPoints; x++)
        {
            for (int y = 0; y < planeGridPoints; y++)
            {
                int index = x + planeGridPoints * y;
                float noiseValue = _weights[index];
                if (noiseValue == -1)
                {
                    Gizmos.color = Color.blue;
                }
                else
                {
                    if (noiseValue > 0.5f)
                    {
                        Gizmos.color = Color.black;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }
                }
                Gizmos.DrawCube(transform.position + (transform.right * (x - planeGridPoints / 2) + transform.forward * (y - planeGridPoints / 2)) * (1.0f / 100), Vector3.one * .004f);
            }
        }
    }

    int ReadSegmentCount()
    {
        int[] segCount = { 0 };
        ComputeBuffer.CopyCount(_segmentsBuffer, _segmentsCountBuffer, 0);
        _segmentsCountBuffer.GetData(segCount);
        return segCount[0];
    }

    void CreateBuffers()
    {
        _segmentsBuffer = new ComputeBuffer(5 * (planeGridPoints * planeGridPoints), Segment.SizeOf, ComputeBufferType.Append);
        _segmentsCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(planeGridPoints * planeGridPoints * planeGridPoints, sizeof(float));
        // _vectorsBuffer = new ComputeBuffer(planeGridPoints * planeGridPoints, 3 * 4 * sizeof(float));
    }

    void ReleaseBuffers()
    {
        _segmentsBuffer.Release();
        _segmentsCountBuffer.Release();
        _weightsBuffer.Release();
        // _vectorsBuffer.Release();
    }

    public void ChangeVisibility()
    {
        Renderer ren = GetComponent<Renderer>();
        bool visibility = ren.enabled;
        if (visibility)
        {
            ren.enabled = false;
        }
        else
        {
            ren.enabled = true;
        }
        int numberOfChildren = transform.childCount;
        for (int i=0; i<numberOfChildren; i++)
        {
            if (transform.GetChild(i).gameObject.CompareTag("grabPlane"))
            {
                continue;
            }
            ren = transform.GetChild(i).gameObject.GetComponent<Renderer>();
            if (visibility)
            {
                ren.enabled = false;
            }
        else
            {
                ren.enabled = true;
            }
        }

    }

    public void SetOrientation(int or)
    {
        orientation = or;
    }

    public void Release()
    {
        grabPlane.GetComponent<XRGrabInteractable>().trackRotation = true;
    }

}