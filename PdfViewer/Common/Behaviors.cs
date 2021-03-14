using System;
using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Docking;
using DevExpress.Xpf.Docking.Base;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;

namespace PdfViewer
{
    // TODO
    public class TextHighlightingBehavior : Behavior<TextEdit>
    {
        public string HighlightedText
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }
        public static readonly DependencyProperty HighlightedTextProperty = DependencyProperty.Register(nameof(HighlightedText), typeof(string), typeof(TextHighlightingBehavior),
            new PropertyMetadata(String.Empty, (obj, e) => { (obj as TextHighlightingBehavior).UpdateText(); }));

        protected void UpdateText()
        {
            if (AssociatedObject.EditMode != EditMode.InplaceInactive)
                return;

            BaseEditHelper.UpdateHighlightingText(AssociatedObject, new TextHighlightingProperties(HighlightedText, FilterCondition.Contains));
        }
    }

    public class UniformItemWidthBehavior : Behavior<DockLayoutManager>
    {
        public GridLength ItemWidth
        {
            get { return (GridLength)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register
            ("ItemWidth", typeof(GridLength), typeof(UniformItemWidthBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DockOperationCompleted += AssociatedObject_DockOperationCompleted;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DockOperationCompleted -= AssociatedObject_DockOperationCompleted;
            base.OnDetaching();
        }

        private void AssociatedObject_DockOperationCompleted(object sender, DockOperationCompletedEventArgs e)
        {
            if (e.Item is LayoutGroup)
                (e.Item as LayoutGroup).ItemWidth = ItemWidth;
        }
    }
}
