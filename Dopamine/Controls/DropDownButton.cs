using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Dopamine.Controls
{
    public class DropDownButton : ToggleButton
    {
        private bool swallowNextClick = false;

        public DropDownButton()
        {
            Binding binding = new Binding("Menu.IsOpen");
            binding.Source = this;
            this.SetBinding(DropDownButton.IsCheckedProperty, binding);

            this.DataContextChanged += (sender, args) =>
            {
                if (this.Menu != null)
                {
                    this.Menu.DataContext = this.DataContext;
                }
            };
        }

        public ContextMenu Menu
        {
            get { return (ContextMenu)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }
        
        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register(nameof(Menu),
            typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null, OnMenuChanged));

        private static void OnMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropDownButton dropDownButton = (DropDownButton)d;
            
            if (e.OldValue is ContextMenu oldMenu)
            {
                oldMenu.Closed -= dropDownButton.ContextMenu_Closed;
            }

            ContextMenu contextMenu = (ContextMenu)e.NewValue;
            if (contextMenu != null)
            {
                contextMenu.DataContext = dropDownButton.DataContext;
                contextMenu.Closed += dropDownButton.ContextMenu_Closed;
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            // If we receive a MouseDown event, it means the ContextMenu did NOT have mouse capture.
            // This is a fresh click on the button while the menu is closed.
            this.swallowNextClick = false;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            // When the ContextMenu closes, it releases its global mouse capture.
            // If the mouse is physically over the button right now, it means the user 
            // clicked the button to close the menu.
            // We set a flag to swallow the resulting trailing OnClick (MouseUp).
            Point pos = Mouse.GetPosition(this);
            if (pos.X >= 0 && pos.X <= this.ActualWidth && pos.Y >= 0 && pos.Y <= this.ActualHeight)
            {
                this.swallowNextClick = true;
            }
        }

        protected override void OnClick()
        {
            if (this.swallowNextClick)
            {
                this.swallowNextClick = false;
                
                // Keep the button un-checked since the menu just closed
                this.IsChecked = false;
                return;
            }

            if (this.Menu != null)
            {
                this.Menu.PlacementTarget = this;
                this.Menu.Placement = PlacementMode.Bottom;
                this.Menu.IsOpen = true;
            }
        }
    }
}
