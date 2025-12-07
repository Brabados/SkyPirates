using System.Collections.Generic;
using UnityEngine;

public class Foid : MonoBehaviour
{

    [SerializeField]
    private float FOVAngle;
    [SerializeField]
    private float smoothdamp;

    private List<Foid> AvoidanceNeighbours = new List<Foid>();
    private List<Foid> AlignmentNeighbours = new List<Foid>();
    private List<Foid> CohesionNeighbours = new List<Foid>();
    private Flock ParentFlock;
    private Vector3 currentVelocity;
    private float speed;


    private Transform MyTransform;

    public void Start()
    {
        MyTransform = this.GetComponent<Transform>();
    }

    public void SetSpeed(float Speed)
    {
        speed = Speed;
    }

    public void assignFlock(Flock Assign)
    {
        ParentFlock = Assign;
    }

    public void moveFoid()
    {
        FindNeighbours();
        Vector3 CohesionVector = CalculateCohesion() * ParentFlock.CohesionWeight;
        Vector3 AllignmentVector = CalculateAllignment() * ParentFlock.AllignmentWeight;
        Vector3 AvoidenceVector = CalculateAvoidence() * ParentFlock.AvoidenceWeight;
        Vector3 BoundsVector = CalculateBounds() * ParentFlock.BoundsWeight;

        Vector3 moveVector = CohesionVector + AllignmentVector + AvoidenceVector + BoundsVector;

        moveVector = Vector3.SmoothDamp(MyTransform.forward, moveVector, ref currentVelocity, smoothdamp);
        moveVector = moveVector.normalized * speed;
        MyTransform.forward = moveVector;
        MyTransform.position += moveVector * Time.deltaTime;
    }

    private Vector3 CalculateBounds()
    {
        Vector3 centerOffset = ParentFlock.transform.position - MyTransform.position;
        bool NearCenter = (centerOffset.magnitude / ParentFlock.BoundsDistance >= 0.9);
        return NearCenter ? centerOffset.normalized : Vector3.zero;
    }

    private Vector3 CalculateAllignment()
    {
        Vector3 AllignmentVector = MyTransform.forward;
        if (CohesionNeighbours.Count == 0)
        {
            return AllignmentVector;
        }

        foreach (Foid F in CohesionNeighbours)
        {
            AllignmentVector += F.MyTransform.forward;
        }
        AllignmentVector /= CohesionNeighbours.Count;
        AllignmentVector = Vector3.Normalize(AllignmentVector);
        return AllignmentVector;
    }

    private Vector3 CalculateAvoidence()
    {
        Vector3 AvoidenceVector = Vector3.zero;
        if (AvoidanceNeighbours.Count == 0)
        {
            return AvoidenceVector;
        }


        foreach (Foid F in AvoidanceNeighbours)
        {
            AvoidenceVector += (MyTransform.position - F.MyTransform.position);
        }


        AvoidenceVector /= AvoidanceNeighbours.Count;
        AvoidenceVector = Vector3.Normalize(AvoidenceVector);
        return AvoidenceVector;

    }

    private void FindNeighbours()
    {
        AvoidanceNeighbours.Clear();
        AlignmentNeighbours.Clear();
        CohesionNeighbours.Clear();

        float avoidDistSqr = ParentFlock.AvoidenceDistance * ParentFlock.AvoidenceDistance;
        float alignDistSqr = ParentFlock.AllignmentDistance * ParentFlock.AllignmentDistance;
        float cohesionDistSqr = ParentFlock.CohesionDistance * ParentFlock.CohesionDistance;

        foreach (Foid f in ParentFlock.allUnits)
        {
            if (f == this) continue;

            Vector3 offset = f.MyTransform.position - MyTransform.position;
            float distSqr = offset.sqrMagnitude;

            if (Vector3.Angle(MyTransform.forward, offset) > FOVAngle)
                continue;

            if (distSqr <= cohesionDistSqr)
                CohesionNeighbours.Add(f);

            if (distSqr <= alignDistSqr)
                AlignmentNeighbours.Add(f);

            if (distSqr <= avoidDistSqr)
                AvoidanceNeighbours.Add(f);
        }
    }


    private Vector3 CalculateCohesion()
    {
        Vector3 CohesionVector = Vector3.zero;
        if (CohesionNeighbours.Count == 0)
        {
            return CohesionVector;
        }


        foreach (Foid F in CohesionNeighbours)
        {
            CohesionVector += F.MyTransform.position;
        }


        CohesionVector /= CohesionNeighbours.Count;
        CohesionVector -= MyTransform.position;
        CohesionVector = Vector3.Normalize(CohesionVector);
        return CohesionVector;

    }

    private bool InFOV(Vector3 position)
    {

        return Vector3.Angle(MyTransform.forward, position - MyTransform.position) <= FOVAngle;
    }
}
