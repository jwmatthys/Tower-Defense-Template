using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public float speed = 1f;
    public float damage = 1f;
    [SerializeField] private float yPos = 0.5f;

    private Transform[] _waypoints;
    private int _currentWaypointIndex;

    public float PathProgress
    {
        get
        {
            if (_waypoints == null || _waypoints.Length == 0) return 0f;

            // If the enemy has reached the end, return max progress
            if (_currentWaypointIndex >= _waypoints.Length)
                return 1f;

            // Base progress from completed waypoints
            float waypointProgress = (float)_currentWaypointIndex / _waypoints.Length;

            // Additional progress within the current segment
            Vector3 currentWaypoint = _waypoints[_currentWaypointIndex].position;
            Vector3 previousWaypoint = _currentWaypointIndex > 0
                ? _waypoints[_currentWaypointIndex - 1].position
                : currentWaypoint;

            float segmentLength = Vector3.Distance(previousWaypoint, currentWaypoint);
            float segmentTravelled = Vector3.Distance(previousWaypoint, transform.position);
            float segmentProgress = segmentLength > 0f ? segmentTravelled / segmentLength : 0f;

            return waypointProgress + (segmentProgress / _waypoints.Length);
        }
    }

    void Start()
    {
        _waypoints = GetWaypoints();
        if (_waypoints.Length > 0)
        {
            transform.position = new Vector3(
                _waypoints[0].position.x,
                yPos,
                _waypoints[0].position.z
            );
        }
    }

    void Update()
    {
        if (_currentWaypointIndex >= _waypoints.Length) return;

        MoveTowardsWaypoint();
    }

    Transform[] GetWaypoints()
    {
        GameObject pathParent = GameObject.FindWithTag("Path");
        if (pathParent == null)
        {
            Debug.LogError("No GameObject with tag 'Path' found!");
            return new Transform[0];
        }

        Transform[] children = new Transform[pathParent.transform.childCount];
        for (int i = 0; i < pathParent.transform.childCount; i++)
        {
            children[i] = pathParent.transform.GetChild(i);
        }

        return children;
    }

    void MoveTowardsWaypoint()
    {
        Transform target = _waypoints[_currentWaypointIndex];

        Vector3 targetPos = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );

        if (transform.position == targetPos)
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _waypoints.Length)
            {
                ReachEnd();
            }
        }
    }

    void ReachEnd()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager)
        {
            gameManager.DealDamage(damage);
        }
        else
        {
            Debug.LogError("No GameManager found in scene!");
        }

        Destroy(gameObject);
    }
}