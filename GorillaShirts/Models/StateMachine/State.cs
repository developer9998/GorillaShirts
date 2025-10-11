namespace GorillaShirts.Models.StateMachine
{
    internal class State
    {
        protected bool initialized;

        public virtual void Enter()
        {
            if (!initialized)
            {
                Initialize();
                return;
            }

            Resume();
        }

        public virtual void Initialize()
        {
            initialized = true;
        }

        public virtual void Resume()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual void Update()
        {

        }
    }
}
