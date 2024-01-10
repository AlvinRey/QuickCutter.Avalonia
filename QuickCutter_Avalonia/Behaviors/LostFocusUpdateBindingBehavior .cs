using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;

namespace QuickCutter_Avalonia.Behaviors
{
    public class LostFocusUpdateBindingBehavior : Behavior<TextBox>
    {
        static LostFocusUpdateBindingBehavior()
        {
            TextProperty.Changed.Subscribe(e =>
            {
                ((LostFocusUpdateBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<LostFocusUpdateBindingBehavior, string>(
            "Text", defaultValue: String.Empty, defaultBindingMode: BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
                Text = AssociatedObject.Text;
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
                AssociatedObject.Text = Text;
        }
    }

    public class LostFocusUpdateBindingBehavior_AutoCompleteBox : Behavior<AutoCompleteBox>
    {
        static LostFocusUpdateBindingBehavior_AutoCompleteBox()
        {
            TextProperty.Changed.Subscribe(e =>
            {
                ((LostFocusUpdateBindingBehavior_AutoCompleteBox)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<LostFocusUpdateBindingBehavior_AutoCompleteBox, string>(
            "Text",  defaultValue: String.Empty, defaultBindingMode: BindingMode.TwoWay);

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (AssociatedObject != null)
                Text = AssociatedObject.Text;
        }

        private void OnBindingValueChanged()
        {
            if (AssociatedObject != null)
                AssociatedObject.Text = Text;
        }
    }
}
