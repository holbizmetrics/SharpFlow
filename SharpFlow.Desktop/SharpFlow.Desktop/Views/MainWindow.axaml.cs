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

        private Point _initialClickOffset; // Add this field

        private void Node_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is WorkflowNode node)
            {
                ViewModel.SelectNode(node);
                _isDragging = true;
                _draggedNode = node;
                _draggedBorder = border;

                // Get mouse position relative to parent
                var mousePos = e.GetPosition((Visual)border.Parent);

                // Get current node position (from existing RenderTransform or 0,0)
                var currentTransform = border.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
                var nodePos = new Point(currentTransform.X, currentTransform.Y);

                // Calculate the offset between mouse and node center
                _initialClickOffset = new Point(mousePos.X - nodePos.X, mousePos.Y - nodePos.Y);

                _lastPointerPosition = mousePos;

                e.Pointer.Capture(border);
                border.Cursor = new Cursor(StandardCursorType.SizeAll);
                e.Handled = true;
            }
        }

        private void MainWindow_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedNode is not null && _draggedBorder is not null)
            {
                var mousePos = e.GetPosition(_draggedBorder.Parent as Visual);

                // Calculate new node position = mouse position - initial offset
                var newX = mousePos.X - _initialClickOffset.X;
                var newY = mousePos.Y - _initialClickOffset.Y;

                _draggedBorder.RenderTransform = new TranslateTransform(newX, newY);

                _draggedNode.X = newX;
                _draggedNode.Y = newY;

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
