namespace GorillaShirts.Models.StateMachine
{
    public class State
    {
        public bool Active;

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
