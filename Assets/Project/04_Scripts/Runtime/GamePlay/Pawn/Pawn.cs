using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
   public Transform TransformCache {get; private set;}

   protected virtual void Awake()
   {
      TransformCache = transform;
   }
   protected virtual void Start(){}
   protected virtual void Update(){}
   protected virtual void FixedUpdate(){}
}
