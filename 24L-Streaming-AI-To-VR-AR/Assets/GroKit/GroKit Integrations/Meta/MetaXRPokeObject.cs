using Oculus.Interaction;

namespace Core3lb
{
    public class MetaXRPokeObject : BaseXRPokeButton
    {

        protected PokeInteractable _interactableView;
        protected IInteractableView InteractableView;

        protected bool isInit = false;

        public void Awake()
        {
            _interactableView = GetComponent<PokeInteractable>();
            InteractableView = _interactableView;
        }

        void OnEnable()
        {
            if (!isInit)
            {
                InteractableView.WhenStateChanged += UpdateVisualState;
                isInit = true;
            }
        }

        void OnDisable()
        {
            if (isInit)
            {
                InteractableView.WhenStateChanged -= UpdateVisualState;
                isInit = false;
            }
        }

        private void UpdateVisualState(InteractableStateChangeArgs args)
        {
            switch (args.NewState)
            {
                case InteractableState.Normal:
                    ExitEvent();
                    break;
                case InteractableState.Hover:
                    EnterEvent();
                    break;
                case InteractableState.Select:
                    _Poke();
                    break;
                case InteractableState.Disabled:
                    break;
                default:
                    break;
            }
        }
    }
}
