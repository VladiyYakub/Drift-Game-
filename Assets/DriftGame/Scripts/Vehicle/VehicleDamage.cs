using UnityEngine;

public class VehicleDamage : MonoBehaviour
{
    public float MaxMoveDelta = 1.0f;
    public float MaxCollisionStrength = 50.0f;
    public float YforceDamp = 0.1f;
    public float DemolutionRange = 0.5f;
    public float ImpactDirManipulator = 0.0f;

    public AudioSource CrashSound;
    public MeshFilter[] OptionalMeshList;

    private MeshFilter[] Meshfilters;
    private float SqrDemRange;
    private Vector3 ColPointToMe;
    private float ColStrength;

    public void Start()
    {
        Meshfilters = OptionalMeshList.Length > 0 ? OptionalMeshList : GetComponentsInChildren<MeshFilter>();
        SqrDemRange = DemolutionRange * DemolutionRange;
    }

    public void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        Vector3 colRelVel = collision.relativeVelocity;
        colRelVel.y *= YforceDamp;

        if (collision.contacts.Length <= 0) return;

        HandleCollisionEffects(collision, colRelVel);
    }

    private void HandleCollisionEffects(Collision collision, Vector3 colRelVel)
    {
        ColPointToMe = transform.position - collision.contacts[0].point;
        ColStrength = colRelVel.magnitude * Vector3.Dot(collision.contacts[0].normal, ColPointToMe.normalized);

        if (ColPointToMe.magnitude > 1.0f && !CrashSound.isPlaying)
        {
            PlayCrashSound();
            ApplyMeshForce(collision.contacts[0].point, Mathf.Clamp01(ColStrength / MaxCollisionStrength));
        }
    }

    private void PlayCrashSound()
    {
        CrashSound.Play();
        CrashSound.volume = ColStrength / 25;
    }

    public void OnMeshForce(Vector4 originPosAndForce)
    {
        OnMeshForce((Vector3)originPosAndForce, originPosAndForce.w);
    }

    public void OnMeshForce(Vector3 originPos, float force)
    {
        ApplyMeshForce(originPos, force);
    }

    private void ApplyMeshForce(Vector3 originPos, float force)
    {
        force = Mathf.Clamp01(force);

        for (int j = 0; j < Meshfilters.Length; ++j)
        {
            Vector3[] verts = Meshfilters[j].mesh.vertices;
            ManipulateVertices(originPos, force, verts, Meshfilters[j].transform);
            Meshfilters[j].mesh.vertices = verts;
            Meshfilters[j].mesh.RecalculateBounds();
        }
    }

    private void ManipulateVertices(Vector3 originPos, float force, Vector3[] verts, Transform meshTransform)
    {
        for (int i = 0; i < verts.Length; ++i)
        {
            Vector3 scaledVert = Vector3.Scale(verts[i], transform.localScale);
            Vector3 vertWorldPos = meshTransform.position + (meshTransform.rotation * scaledVert);
            Vector3 originToMeDir = vertWorldPos - originPos;
            Vector3 flatVertToCenterDir = transform.position - vertWorldPos;
            flatVertToCenterDir.y = 0.0f;

            if (originToMeDir.sqrMagnitude < SqrDemRange)
            {
                float dist = Mathf.Clamp01(originToMeDir.sqrMagnitude / SqrDemRange);
                float moveDelta = force * (1.0f - dist) * MaxMoveDelta;
                Vector3 moveDir = Vector3.Slerp(originToMeDir, flatVertToCenterDir, ImpactDirManipulator).normalized * moveDelta;
                verts[i] += Quaternion.Inverse(transform.rotation) * moveDir;
            }
        }
    }
}


