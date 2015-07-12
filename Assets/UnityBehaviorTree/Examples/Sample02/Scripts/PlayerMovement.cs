using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

namespace UBT.Example
{
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 5f;

        private Transform _transform;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();

            this.UpdateAsObservable()
                .Select(_ => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")))
                .Subscribe(axis =>
                {
                    Vector3 currentPos = _transform.position;
                    currentPos += axis * Time.smoothDeltaTime * speed;
                    _rigidbody.MovePosition(currentPos);
                });
        }
    }
}
