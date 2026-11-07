namespace ImprovedFires.Utilities
{
    /// <summary>
    /// Wraps a cloned options-menu slider (Panel_OptionsMenu.m_FieldOfViewSlider).
    /// Maps the UISlider's 0..1 value onto [from, to], shows it in the slider's value label using `format`
    /// (e.g. "{0:0.00}"), and calls `write` whenever the player moves it.
    /// Call Refresh() to pull the current value from `read` into the slider.
    /// Adapted from the TLD-UI-Guide FireMenu example (Rows.cs → SliderRow).
    /// </summary>
    public sealed class SliderRow
    {
        public GameObject Root { get; }
        public UILabel? Name { get; }

        private readonly ConsoleSlider mConsole;
        private readonly UISlider mSlider;
        private readonly UILabel mValue;
        private readonly float mFrom;
        private readonly float mTo;
        private readonly Func<float, string> mDisplay;
        private readonly Func<float> mRead;
        private readonly Action<float> mWrite;
        private bool mUpdating;

        public SliderRow(GameObject root, string name, float from, float to, int steps, Func<float, string> display, Func<float> read, Action<float> write)
        {
            Root = root;
            Name = NameLabel(root);
            if (Name != null)
            {
                Name.text = name;
            }

            mConsole = root.GetComponent<ConsoleSlider>();
            mSlider = mConsole.m_SliderObject.GetComponentInChildren<UISlider>(true);
            mValue = mConsole.m_SliderObject.GetComponentInChildren<UILabel>(true);
            mFrom = from;
            mTo = to;
            mDisplay = display;
            mRead = read;
            mWrite = write;
            mSlider.numberOfSteps = steps;
            EventDelegate.Set(mConsole.onChange, new Action(OnChange));
            EventDelegate.Set(mSlider.onChange, new Action(OnChange));
        }

        public float Value => mFrom + mSlider.value * (mTo - mFrom);

        public void Step(int direction)
        {
            if (direction < 0) mConsole.OnDecrease(); else mConsole.OnIncrease();
        }

        public void Refresh()
        {
            if (UICamera.hoveredObject != null && UICamera.hoveredObject.transform.IsChildOf(Root.transform) && Input.GetMouseButton(0))
            {
                return;
            }
            float value = Mathf.Clamp(mRead(), mFrom, mTo);
            mUpdating = true;
            mSlider.value = (value - mFrom) / (mTo - mFrom);
            mUpdating = false;
            SetText(mValue, mDisplay(value));
        }

        public static UILabel? NameLabel(GameObject root)
        {
            for (int i = 0; i < root.transform.childCount; i++)
            {
                Transform child = root.transform.GetChild(i);
                if (child.name.StartsWith("Label") && child.name != "Label_Value" && child.GetComponent<UILabel>() != null)
                {
                    return child.GetComponent<UILabel>();
                }
            }
            return null;
        }

        private void OnChange()
        {
            float value = Value;
            SetText(mValue, mDisplay(value));
            if (!mUpdating)
            {
                mWrite(value);
            }
        }

        private static void SetText(UILabel label, string text)
        {
            if (label.text != text)
            {
                label.text = text;
            }
        }
    }
}
