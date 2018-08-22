using System.Windows;
using AvalonDock;

namespace XCase.Gui
{
    public static class DockingManagerExt
    {
        public static MainWindow GetMainWindow(this DockingManager dockingManager)
        {
            MainWindow mainWindow = (MainWindow)Window.GetWindow(dockingManager);
            return mainWindow;
        }

        public static void BringDocumentHeaderToView(this ManagedContent document)
        {
            if (!DocumentTabPanel.GetIsHeaderVisible(document))
            {
                DocumentPane parentPane = document.ContainerPane as DocumentPane;
                if (parentPane != null)
                {
                    parentPane.Items.Remove(document);
                    parentPane.Items.Insert(0, document);
                    document.Activate();
                }
            }

            ////document.IsSelected = true;
            ////Selector.SetIsSelected(document, true);
            //if (this.GetManager() != null)
            //    this.GetManager().ActiveContent = document;
            //document.SetAsActive();
        }
    }
}