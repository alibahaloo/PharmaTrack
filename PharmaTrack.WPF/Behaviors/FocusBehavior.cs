using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace PharmaTrack.WPF.Behaviors
{
    public class FocusBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
                nameof(IsFocused),
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public bool IsFocused
        {
            get => (bool)GetValue(IsFocusedProperty);
            set => SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FocusBehavior behavior &&
                behavior.AssociatedObject is Control control &&
                (bool)e.NewValue)
            {
                control.Focus();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (!(AssociatedObject is Control))
            {
                throw new InvalidOperationException("FocusBehavior can only be applied to Controls.");
            }
        }
    }
}
