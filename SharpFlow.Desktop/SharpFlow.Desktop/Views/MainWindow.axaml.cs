using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.ViewModels;
using System.Diagnostics;

namespace SharpFlow.Desktop.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ────────────────────────────────────────────────────────────────
        //  Drag-and-Drop fields
        // ────────────────────────────────────────────────────────────────
        private bool _isDragging;
        private Point _lastPointerPosition;
        private WorkflowNode? _draggedNode;

        // ────────────────────────────────────────────────────────────────
        //  PointerPressed – start drag
        // ────────────────────────────────────────────────────────────────
        private Border? _draggedBorder; // Add this field

        private void Node_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is WorkflowNode node)
            {
                ViewModel.SelectNode(node);
                _isDragging = true;
                _draggedNode = node;
                _draggedBorder = border;

                // Start with NO transform - let the node stay at its current visual position
                _draggedBorder.RenderTransform = new TranslateTransform(0, 0);

                // Get the pointer position relative to the parent
                _lastPointerPosition = e.GetPosition((Visual)border.Parent);

                e.Pointer.Capture(border);
                border.Cursor = new Cursor(StandardCursorType.SizeAll);
                e.Handled = true;
            }
        }

        private void MainWindow_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedNode is not null && _draggedBorder is not null)
            {
                var current = e.GetPosition(_draggedBorder.Parent as Visual);
                var dx = current.X - _lastPointerPosition.X;
                var dy = current.Y - _lastPointerPosition.Y;

                // Get current transform and add the delta
                var currentTransform = _draggedBorder.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
                var newX = currentTransform.X + dx;
                var newY = currentTransform.Y + dy;

                _draggedBorder.RenderTransform = new TranslateTransform(newX, newY);

                // Update the node coordinates too
                _draggedNode.X = newX;
                _draggedNode.Y = newY;

                _lastPointerPosition = current;
                e.Handled = true;
            }
        }

        private void Node_PointerMoved(object? sender, PointerEventArgs e)
        {
            // ONLY process if this is the exact border we're dragging
            if (_isDragging && _draggedNode is not null && sender == _draggedBorder)
            {
                var border = (Border)sender;
                var current = e.GetPosition((Visual)border.Parent);
                var dx = current.X - _lastPointerPosition.X;
                var dy = current.Y - _lastPointerPosition.Y;

                _draggedNode.X += dx;
                _draggedNode.Y += dy;

                // Force UI update using RenderTransform
                _draggedBorder.RenderTransform = new TranslateTransform(_draggedNode.X, _draggedNode.Y);

                _lastPointerPosition = current;
                e.Handled = true;
            }
        }

        private void Node_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Border border)
            {
                _isDragging = false;
                _draggedNode = null;
                _draggedBorder = null; // Clear the reference
                e.Pointer.Capture(null);
                border.Cursor = new Cursor(StandardCursorType.Hand);
                e.Handled = true;
            }
        }
    }
}
