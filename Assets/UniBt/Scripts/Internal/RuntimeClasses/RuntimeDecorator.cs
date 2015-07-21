namespace UniBt
{
    public sealed class RuntimeDecorator
    {
        public Node parent;
        public Decorator decorator;
        public System.Func<bool> decoratorFunc;
        public float tick;
        public bool inversed;
        public bool activeSelf;
#if UNITY_EDITOR
        public bool closed;
#endif
        public System.IDisposable subscription;

        public RuntimeDecorator(Node parent, Decorator decorator, float tick, bool inversed)
        {
            this.parent = parent;
            this.decorator = decorator;
            this.tick = tick;
            this.inversed = inversed;
            this.activeSelf = false;
        }

        public RuntimeDecorator(Node parent, Decorator decorator, float tick, bool inversed, bool activeSelf)
        {
            this.parent = parent;
            this.decorator = decorator;
            this.tick = tick;
            this.inversed = inversed;
            this.activeSelf = activeSelf;
        }
    }
}
