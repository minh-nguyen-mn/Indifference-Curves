using UnityEngine;

/*
Here is the actual definition of each chunk. Each one has the required information to dispatch to the compute shader
which then calculates the triangles needed to make up the mesh of each chunk.
*/

public class Chunk : MonoBehaviour
{
    public ComputeShader marchingShader;

    public MeshFilter meshFilter;

    ComputeBuffer _trianglesBuffer;
    ComputeBuffer _trianglesCountBuffer;
    ComputeBuffer _weightsBuffer;

    public NoiseGenerator noiseGenerator;

    public float otherSize=1.0f;

    public float size;

    private void Awake() {
        CreateBuffers();
    }

    private void OnDestroy() {
        ReleaseBuffers();
    }

    struct Triangle {
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }

    float[] _weights;

    public void Render(){
        _weights = noiseGenerator.GetNoise(); // The "noise" is the function that defines what is inside and outside the object. The mesh is made around this.
        meshFilter.sharedMesh = ConstructMesh();
    }

    Mesh ConstructMesh() {
        // Here we pass the required information to the GPU to calculate the triangles of the mesh
        marchingShader.SetBuffer(0, "_Triangles", _trianglesBuffer);
        marchingShader.SetBuffer(0, "_Weights", _weightsBuffer);
        marchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk);
        marchingShader.SetFloat("_OtherSize", otherSize);
        marchingShader.SetFloat("_IsoLevel", .5f);
        marchingShader.SetFloat("_Size", size);
        

        _weightsBuffer.SetData(_weights);
        _trianglesBuffer.SetCounterValue(0);

        // This is the actual call to the compute shader to do its thing. 
        marchingShader.Dispatch(0, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads, GridMetrics.PointsPerChunk / GridMetrics.NumThreads);

        // The triangle information is now in the _trianglesBuffer so we access that on the CPU side
        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        _trianglesBuffer.GetData(triangles);

        return CreateMeshFromTriangles(triangles);
    }

    int ReadTriangleCount() {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(_trianglesBuffer, _trianglesCountBuffer, 0);
        _trianglesCountBuffer.GetData(triCount);
        return triCount[0];
    }

    Mesh CreateMeshFromTriangles(Triangle[] triangles) {
        // Here we make the actual mesh. We make 6 vertices instead of just 3 so that the triangle is rendered on both sides.
        // The vertices in the verts list will be visible from the side that defines them clockwise 
        Vector3[] verts = new Vector3[triangles.Length * 6];
        int[] tris = new int[triangles.Length * 6];

        for (int i = 0; i < triangles.Length; i++) {
            int startIndex = i * 6;

            verts[startIndex] = triangles[i].A;
            verts[startIndex + 1] = triangles[i].B;
            verts[startIndex + 2] = triangles[i].C;

            verts[startIndex + 3] = triangles[i].C;
            verts[startIndex + 4] = triangles[i].B;
            verts[startIndex + 5] = triangles[i].A;

            tris[startIndex] = startIndex;
            tris[startIndex + 1] = startIndex + 1;
            tris[startIndex + 2] = startIndex + 2;

            tris[startIndex + 3] = startIndex + 3;
            tris[startIndex + 4] = startIndex + 4;
            tris[startIndex + 5] = startIndex + 5;
        }

        Mesh mesh = new()
        {
            vertices = verts,
            triangles = tris
        };
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    // If you want to visualize the points of the grid that the mesh is defined on, uncomment this section. WARNING: LOW FPS

    // private void OnDrawGizmos() {
    //     if (_weights == null || _weights.Length == 0) {
    //         return;
    //     }
    //     for (int x = 0; x < GridMetrics.PointsPerChunk; x++) {
    //         for (int y = 0; y < GridMetrics.PointsPerChunk; y++) {
    //             for (int z = 0; z < GridMetrics.PointsPerChunk; z++) {
    //                 int index = x + GridMetrics.PointsPerChunk * (y + GridMetrics.PointsPerChunk * z);
    //                 float noiseValue = _weights[index];
    //                 if(noiseValue == -1){
    //                     Gizmos.color = Color.blue;
    //                 } else{
    //                     Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
    //                 }
    //                 Gizmos.DrawCube(transform.position + (new Vector3(x, y, z) - Vector3.one * GridMetrics.PointsPerChunk / 2.0f) * (size / 100f), Vector3.one * .04f);
    //             }
    //         }
    //     }
    // }

    // Define the buffers that will be dispatched to the computer shader
    void CreateBuffers() {
        _trianglesBuffer = new ComputeBuffer(5 * (GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk), Triangle.SizeOf, ComputeBufferType.Append);
        _trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        _weightsBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk * GridMetrics.PointsPerChunk, sizeof(float));
    }

    // Indicate to the garbage collector that this memory is now free
    void ReleaseBuffers() {
        _trianglesBuffer.Release();
        _trianglesCountBuffer.Release();
        _weightsBuffer.Release();
    }
}