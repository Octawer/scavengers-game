using System.Collections;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
    public float moveTime = .1f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidBody;
    private float inverseTime;  // just for computational performance (cheaper to multiply than to divide)


    // Start is called before the first frame update
    public virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();
        inverseTime = 1 / moveTime;
    }

    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        var end = start + new Vector2(xDir, yDir);

        boxCollider.enabled = false;    // we disable the object collider before we raycast, so as to not to hit it accidentally
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        // if we didnt hit anything means we can move, otherwise we cant
        if (hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon)
        {
            // why do we use here rigidBody.position vs transform.position in other sections ??
            var newPosition = Vector3.MoveTowards(rigidBody.position, end, inverseTime * Time.deltaTime);
            rigidBody.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);

        if (hit.transform == null)
        {
            return;
        }

        T hitComponent = hit.transform.GetComponent<T>();

        // if we cannot move and have hit anything, we take a specific action if we are a player or an enemy (or whatever)
        if (!canMove && hitComponent != null)
        {
            OnCantMove<T>(hitComponent);
        }
    }

    protected abstract void OnCantMove<T>(T component) where T : Component;
}
