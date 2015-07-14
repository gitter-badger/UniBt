namespace UniBt
{
    public class RuntimeTask
    {
        public Task parent;
        public string methodName;
        public System.Func<System.IDisposable> taskFunc;
        private System.IDisposable _disposable;

        public RuntimeTask(Task parent, string methodName)
        {
            this.parent = parent;
            this.methodName = methodName;
        }

        public void Start()
        {
            if (_disposable != null)
                _disposable.Dispose();
            _disposable = taskFunc();
        }

        public void Finish()
        {
            if (_disposable != null)
                _disposable.Dispose();
        }
    }
}
