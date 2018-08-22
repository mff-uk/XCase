using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigWizardUI
{
    [Flags]
    public enum WizardButtons
    {
        None = 0x0000,
        Back = 0x0001,
        Next = 0x0002,
        Finish = 0x0004,
    }

    public partial class WizardSheet : Form
    {
        public WizardSheet()
        {
            InitializeComponent();
        }

        private IList _pages = new ArrayList();

        public IList Pages
        {
            get { return _pages; }
        }

        private WizardPage _activePage;

        private void ResizeToFit()
        {
            Size maxPageSize = new Size(buttonPanel.Width, 0);

            foreach (WizardPage page in _pages)
            {
                if (page.Width > maxPageSize.Width)
                    maxPageSize.Width = page.Width;
                if (page.Height > maxPageSize.Height)
                    maxPageSize.Height = page.Height;
            }

            foreach (WizardPage page in _pages)
            {
                page.Size = maxPageSize;
            }

            Size extraSize = this.Size;
            extraSize -= pagePanel.Size;

            Size newSize = maxPageSize + extraSize;
            this.Size = newSize;
        }
        
        public void SetActivePage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _pages.Count)
                throw new ArgumentOutOfRangeException("pageIndex");

            WizardPage page = (WizardPage)_pages[pageIndex];
            SetActivePage(page);
        }

        private void SetActivePage(WizardPage newPage)
        {
            WizardPage oldActivePage = _activePage;

            // If this page isn't in the Controls collection, add it.
            // This is what causes the Load event, so we defer
            // it as late as possible.
            if (!pagePanel.Controls.Contains(newPage))
                pagePanel.Controls.Add(newPage);

            // Show this page.
            newPage.Visible = true;

            _activePage = newPage;

            // Allow the page to cancel this.
            CancelEventArgs e = new CancelEventArgs();
            newPage.OnSetActive(e);

            if (e.Cancel)
            {
                newPage.Visible = false;
                _activePage = oldActivePage;
            }


            // Hide all of the other pages.
            foreach (WizardPage page in _pages)
            {
                if (page != _activePage)
                    page.Visible = false;
            }
        }

        private void WizardSheet_Load(object sender, EventArgs e)
        {
            if (_pages.Count != 0)
            {
                ResizeToFit();
                SetActivePage(0);
            }
            else
                SetWizardButtons(WizardButtons.None);

        }

        internal void SetWizardButtons(WizardButtons buttons)
        {
            // The Back button is simple.
            backButton.Enabled = ((buttons & WizardButtons.Back) != 0);

            // The Next button is a bit more complicated.
            // If we've got a Finish button, then it's disabled and hidden.
            if ((buttons & WizardButtons.Finish) != 0)
            {
                finishButton.Visible = true;
                finishButton.Enabled = true;

                nextButton.Visible = false;
                nextButton.Enabled = false;

                this.AcceptButton = finishButton;
            }
            else
            {
                finishButton.Visible = false;
                finishButton.Enabled = false;

                nextButton.Visible = true;
                nextButton.Enabled = ((buttons & WizardButtons.Next) != 0);

                this.AcceptButton = nextButton;
            }
        }

        private WizardPageEventArgs PreChangePage(int delta)
        {
            // Figure out which page is next.
            int activeIndex = GetActiveIndex();
            int nextIndex = activeIndex + delta;

            if (nextIndex < 0 || nextIndex >= _pages.Count)
                nextIndex = activeIndex;

            // Fill in the event args.
            WizardPage newPage = (WizardPage)_pages[nextIndex];

            WizardPageEventArgs e = new WizardPageEventArgs();
            e.NewPage = newPage.Name;
            e.Cancel = false;

            return e;
        }

        private void PostChangePage(WizardPageEventArgs e)
        {
            if (!e.Cancel)
                SetActivePage(e.NewPage);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            WizardPageEventArgs wpea = PreChangePage(+1);
            _activePage.OnWizardNext(wpea);
            PostChangePage(wpea);
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            WizardPageEventArgs wpea = PreChangePage(-1);
            _activePage.OnWizardBack(wpea);
            PostChangePage(wpea);
        }

        private void finishButton_Click(object sender, EventArgs e)
        {
            CancelEventArgs cea = new CancelEventArgs();
            _activePage.OnWizardFinish(cea);
            if (cea.Cancel)
                return;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int GetActiveIndex()
        {
            WizardPage activePage = GetActivePage();

            for (int i = 0; i < _pages.Count; ++i)
            {
                if (activePage == _pages[i])
                    return i;
            }

            return -1;
        }

        private WizardPage GetActivePage()
        {
            return _activePage;
        }

        private WizardPage FindPage(string pageName)
        {
            foreach (WizardPage page in _pages)
            {
                if (page.Name == pageName)
                    return page;
            }

            return null;
        }

        private void SetActivePage(string newPageName)
        {
            WizardPage newPage = FindPage(newPageName);

            if (newPage == null)
                throw new Exception(string.Format("Can't find page named {0}", newPageName));

            SetActivePage(newPage);
        }

        private void WizardSheet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!cancelButton.Enabled)
                e.Cancel = true;
            else if (!finishButton.Enabled)
                OnQueryCancel(e);
        }

        protected virtual void OnQueryCancel(CancelEventArgs e)
        {
            _activePage.OnQueryCancel(e);
        }
    }
}
