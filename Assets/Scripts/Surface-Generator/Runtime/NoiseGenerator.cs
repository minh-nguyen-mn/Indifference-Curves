using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    ComputeBuffer _weightsBuffer;
    public ComputeShader noiseShader;

    public float size;
    [SerializeField, Range(0f, 20f)] public float scale = 10;
    [SerializeField] public float amplitude = 5;
    [SerializeField] public float frequency = 0.005f;

    public Vector3 offset;

    [SerializeField] public Vector3 planeForward;
    [SerializeField] public Vector3 planeRight;
    [SerializeField] public Vector3 planePosition;

    public bool isPlane;

    public int function;

    public float otherSize = 1.0f;

    public int orientation;

    private void Awake() {
        CreateBuffers();
    }

    private void OnDestroy() {
        ReleaseBuffers();
    }

    public float[] GetNoise() {
        float[] noiseValues =
            new float[GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk];

        noiseShader.SetBuffer(0, "_Weights", _weightsBuffer);

        noiseShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        noiseShader.SetInt("_IsPlane", 0);
        noiseShader.SetInt("_Function", function);
        noiseShader.SetInt("_Orientation", orientation);
        noiseShader.SetFloat("_Scale", scale);
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetVector("_Offset", offset / (size / 100));
        noiseShader.SetFloat("_OtherSize", otherSize);

        noiseShader.Dispatch(
            0, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads
        );

        _weightsBuffer.GetData(noiseValues);

        return noiseValues;
    }

    public float[] GetNoisePlane(int planeGridPoints) {
        float[] noiseValues =
            new float[planeGridPoints * planeGridPoints];

        noiseShader.SetBuffer(1, "_Weights", _weightsBuffer);

        noiseShader.SetInt("_ChunkSize", planeGridPoints);
        noiseShader.SetInt("_IsPlane", 1);
        noiseShader.SetInt("_Function", function);
        noiseShader.SetInt("_Orientation", orientation);
        noiseShader.SetFloat("_Scale", scale);
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetVector("_Offset", offset / (size / 100));
        noiseShader.SetVector("_PlaneForward", planeForward);
        noiseShader.SetVector("_PlaneRight", planeRight);


        noiseShader.Dispatch(
            1, planeGridPoints / GridMetrics.NumThreads, planeGridPoints / GridMetrics.NumThreads, 1);

        _weightsBuffer.GetData(noiseValues);

        return noiseValues;
    }

    void CreateBuffers() {
        if(isPlane){
            int planeGridPoints = GridMetrics.PointsPerChunk * 2 + 16;
            _weightsBuffer = new ComputeBuffer(
                planeGridPoints * planeGridPoints, sizeof(float)
            );
        } else{
            _weightsBuffer = new ComputeBuffer(
                GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float)
            );
        }
        
    }

    void ReleaseBuffers() {
        _weightsBuffer.Release();
    }
}
