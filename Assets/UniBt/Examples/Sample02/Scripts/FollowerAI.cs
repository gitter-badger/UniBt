using UnityEngine;
using System.Collections;
using UniRx;
using UniRx.Triggers;

namespace UniBt.Example
{
    public class FollowerAI : MonoBehaviour, IInitializable
    {
        private NavMeshAgent _agent;
        private Transform _transform;
        private Rigidbody _rigidbody;
        private Vector3? _targetLocation;
        private Vector3 _homeLocation;
        private Transform _target;
        private float _agroRadius = 5f;

        public void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
            _homeLocation = _transform.position;
            _agent = GetComponent<NavMeshAgent>();
        }

        #region Decorators
        public bool IsThereTarget()
        {
            return _target != null ? true : false;
        }

        public bool IsThereTargetLocation()
        {
            return _targetLocation.HasValue;
        }

        public bool CloseEnough()
        {
            float distance = Vector3.Distance(_transform.position, _target.position);
            bool value = distance >= 2f ? true : false;
            if (!value)
            {
                _agent.Stop();
                _rigidbody.velocity = Vector3.zero;
            }
            return value;
        }
        #endregion Decorators

        #region Services
        public void TargetCheck()
        {
            Collider[] colliders = Physics.OverlapSphere(_transform.position, _agroRadius);
            Transform target = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider.tag == "Player")
                {
                    Vector3 v = Vector3.Normalize(collider.transform.position - _transform.position);
                    RaycastHit hitInfo = new RaycastHit();
                    if (Physics.Raycast(_transform.position, v, out hitInfo) && hitInfo.transform.tag == "Player")
                    {
                        target = collider.transform;
                        _targetLocation = target.position;
                        break;
                    }
                }
            }
            _target = target;
        }
        #endregion Services

        #region Tasks
        public System.IDisposable RapidMoveToTarget()
        {
            _agent.Resume();
            var subscription = this.UpdateAsObservable()
                .Subscribe(_ => _agent.SetDestination(_target.position));
            return subscription;
        }

        public System.IDisposable MoveToTargetLocation()
        {
            _agent.SetDestination(_targetLocation.Value);

            var subscription = this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float dist = _agent.remainingDistance;
                    if (dist != Mathf.Infinity && dist == 0 && _agent.pathStatus == NavMeshPathStatus.PathComplete)
                        this.FinishExecute(true);
                });
            return subscription;
        }

        public System.IDisposable MoveToHome()
        {
            _agent.SetDestination(_homeLocation);

            var subscription = this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float dist = _agent.remainingDistance;
                    if (dist != Mathf.Infinity && dist == 0 && _agent.pathStatus == NavMeshPathStatus.PathComplete)
                    {
                        _agent.Stop();
                        _targetLocation = null;
                        this.FinishExecute(true);
                    }
                });
            return subscription;
        }

        public System.IDisposable Wait()
        {
            var subscription = Observable.Timer(System.TimeSpan.FromSeconds(2.5f))
                .Subscribe(_ => this.FinishExecute(true));
            return subscription;
        }
        #endregion
    }
}
