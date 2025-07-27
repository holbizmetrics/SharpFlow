using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using SharpFlow.Desktop.Models;
using SharpFlow.Desktop.ViewModels;

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
        private bool          _isDragging;
        private Point         _lastPointerPosition;
        private WorkflowNode? _draggedNode;

        // ────────────────────────────────────────────────────────────────
        //  PointerPressed – start drag
        // ────────────────────────────────────────────────────────────────
        private void Node_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is WorkflowNode node)
            {
                // Select in view-model
                ViewModel.SelectNode(node);

                // Begin drag
                _isDragging     = true;
                _draggedNode    = node;
                _lastPointerPosition = e.GetPosition((Visual)border.Parent); // canvas coords
                e.Pointer.Capture(border);                                   // ← capture
                border.Cursor = new Cursor(StandardCursorType.SizeAll);

                e.Handled = true;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  PointerMoved – update position
        // ────────────────────────────────────────────────────────────────
        private void Node_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _draggedNode is not null && sender is Border border)
            {
                var current = e.GetPosition((Visual)border.Parent);

                var dx = current.X - _lastPointerPosition.X;
                var dy = current.Y - _lastPointerPosition.Y;

                _draggedNode.X += dx;
                _draggedNode.Y += dy;

                _lastPointerPosition = current; // for next delta
                e.Handled = true;
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  PointerReleased – finish drag
        // ────────────────────────────────────────────────────────────────
        private void Node_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Border border)
            {
                _isDragging  = false;
                _draggedNode = null;

                e.Pointer.Capture(null);                         // ← release
                border.Cursor = new Cursor(StandardCursorType.Hand);

                e.Handled = true;
            }
        }
    }
}
