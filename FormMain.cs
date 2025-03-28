using System;
using System.Windows.Forms;

namespace CreativeFileBrowser
{
    public partial class FormMain : Form
    {
        //declare vars/instantiate quadrant nested layout -- NESTED PANELS
        private Label labelSystemTitle, labelPreviewTitle, labelMonitoredTitle, labelGalleryTitle;
        private Panel panelSystemContent, panelPreviewContent, panelMonitoredContent, panelGalleryContent;


        public FormMain()
        {
            InitializeComponent();

            //**********************************************************************//
            //LOAD THE UI STUFF BEFORE THE LOGIC
            //**********************************************************************//            
            this.Load += FormMain_Load;

            // Lock both inner horizontal splits
            //horizontalTop.SplitterDistance = halfWidth;
            //horizontalBottom.SplitterDistance = halfWidth;

            // make the splits move together
            horizontalTop.SplitterMoved += (_, _) =>
            {
                horizontalBottom.SplitterDistance = horizontalTop.SplitterDistance;
            };

            horizontalBottom.SplitterMoved += (_, _) =>
            {
                horizontalTop.SplitterDistance = horizontalBottom.SplitterDistance;
            };

        }

        //**********************************************************************//
        //MAIN LOAD EVENT HANDLER FIRST
        //**********************************************************************//
        private void FormMain_Load(object sender, EventArgs e)
        {
            AdjustVerticalSplitPosition(); // ✅ single clean call
            ResetQuadrantsToMiddle();
            AdjustLayout();

            // Optional test
            panelSystemContent.Controls.Add(new Label
            {
                Text = "✅ Panel visible",
                Dock = DockStyle.Top,
                ForeColor = Color.Green
            });
        }
        //**********************************************************************//
        //ADJUST LAYOUT HELPER METHOD
        //**********************************************************************//
        private void AdjustLayout()
        {
            int toolHeight = toolStripMain.Height;

            int availableHeight = this.ClientSize.Height - toolStripMain.Height;
            int half = availableHeight / 2;

            availableHeight = this.ClientSize.Height - toolHeight;
            int halfHeight = availableHeight / 2;
            int halfWidth = this.ClientSize.Width / 2;

            //quadrant sizing stuff
            //create quadrant panels for NESTED content
            horizontalTop.Panel1.Controls.Clear();
            horizontalTop.Panel2.Controls.Clear();
            horizontalBottom.Panel1.Controls.Clear();
            horizontalBottom.Panel2.Controls.Clear();

            horizontalTop.Panel1.Controls.Add(CreateQuadrant("System Folders", out labelSystemTitle, out panelSystemContent));
            horizontalTop.Panel2.Controls.Add(CreateQuadrant("Folder Preview", out labelPreviewTitle, out panelPreviewContent));
            horizontalBottom.Panel1.Controls.Add(CreateQuadrant("Monitored Folders", out labelMonitoredTitle, out panelMonitoredContent));
            horizontalBottom.Panel2.Controls.Add(CreateQuadrant("Monitored Gallery", out labelGalleryTitle, out panelGalleryContent));
        }
        //size of quadrants dedicated method
        private void AdjustVerticalSplitPosition()
        {
            if (toolStripMain == null || verticalSplit == null)
                return;

            verticalSplit.Location = new Point(0, toolStripMain.Height);
            verticalSplit.Size = new Size(ClientSize.Width, ClientSize.Height - toolStripMain.Height * 2);
        }

        //**********************************************************************//
        //QUADRANT MOVER HELPER METHOD
        //**********************************************************************//
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustVerticalSplitPosition();

            //if (horizontalTop != null && horizontalBottom != null)
            //{
            //    int halfWidth = this.ClientSize.Width / 2;
            //    horizontalTop.SplitterDistance = halfWidth;
            //    horizontalBottom.SplitterDistance = halfWidth;
            //}
            if (this.IsHandleCreated && horizontalTop != null && horizontalBottom != null)
            {
                // Optionally: only recenter on first load or if flag is set
                // Otherwise: let user-dragged splits persist
            }

            if (verticalSplit == null || toolStripMain == null)
            {
                Console.WriteLine("Skipping resize: controls not ready.");
                return;
            }


        }
        private void ResetQuadrantsToMiddle()
        {
            if (verticalSplit != null)
                verticalSplit.SplitterDistance = this.ClientSize.Height / 2;

            if (horizontalTop != null && horizontalBottom != null)
            {
                int halfWidth = this.ClientSize.Width / 2;
                horizontalTop.SplitterDistance = halfWidth;
                horizontalBottom.SplitterDistance = halfWidth;
            }
        }
        //QUADRANT NEST HELPER METHOD
        private Panel CreateQuadrant(string title, out Label titleLabel, out Panel contentPanel)
        {
            var outerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 10, 10, 15), // ⬅️ spacing inside each quadrant
                BorderStyle = BorderStyle.FixedSingle,
            };

            titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 8, 0, 0),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 6) // ⬅️ space below label before content
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(4)
            };

            outerPanel.Controls.Add(contentPanel);
            outerPanel.Controls.Add(titleLabel);

            return outerPanel;
        }


        //**********************************************************************//
        //DUMMY METHOD - LOADIMAGESFOLDER
        //**********************************************************************//
        private void DebugSelectedFileTypes()
        {
            var selectedTypes = fileTypeChecklist.CheckedItems.Cast<string>().ToList();
            Console.WriteLine("Selected extensions: " + string.Join(", ", selectedTypes));
        }

        //**********************************************************************//
        //DUMMY METHOD - SAVEWORKSPACES
        //**********************************************************************//

        private void SaveCurrentWorkspace()
        {
            MessageBox.Show("Workspace saved!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RemoveCurrentWorkspace()
        {
            MessageBox.Show("Workspace removed!", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




    }
}
