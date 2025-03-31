using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace CreativeFileBrowser
{
    public partial class FormMain : Form
    {
        //declare vars/instantiate quadrant nested layout -- NESTED PANELS       
        private Label labelSystemTitle, labelPreviewTitle, labelMonitoredTitle, labelGalleryTitle;
        private Panel panelSystemContent, panelPreviewContent, panelMonitoredContent, panelGalleryContent;
        private Panel quadrantHostPanel;
        private float horizontalRatioTop = 0.5f;
        private float horizontalRatioBottom = 0.5f;
        private float verticalRatio = 0.5f; 



        public FormMain()
        {
            InitializeComponent();

            horizontalTop.SplitterMoved += (_, _) =>
            {
                if (horizontalBottom.SplitterDistance != horizontalTop.SplitterDistance)
                    horizontalBottom.SplitterDistance = horizontalTop.SplitterDistance;
            };

            horizontalBottom.SplitterMoved += (_, _) =>
            {
                if (horizontalTop.SplitterDistance != horizontalBottom.SplitterDistance)
                    horizontalTop.SplitterDistance = horizontalBottom.SplitterDistance;
            };


        }

        //**********************************************************************//
        //MAIN LOAD EVENT HANDLER FIRST
        //**********************************************************************//
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Apply actual top padding now that ToolStrip has rendered
            quadrantHostPanel.Padding = new Padding(0, 0, 0, 0);
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
            horizontalTop.Panel1.Controls.Clear();
            horizontalTop.Panel2.Controls.Clear();
            horizontalBottom.Panel1.Controls.Clear();
            horizontalBottom.Panel2.Controls.Clear();

            horizontalTop.Panel1.Controls.Add(CreateQuadrant("System Folders", out labelSystemTitle, out panelSystemContent));
            horizontalTop.Panel2.Controls.Add(CreateQuadrant("Folder Preview", out labelPreviewTitle, out panelPreviewContent));
            horizontalBottom.Panel1.Controls.Add(CreateQuadrant("Monitored Folders", out labelMonitoredTitle, out panelMonitoredContent));
            horizontalBottom.Panel2.Controls.Add(CreateQuadrant("Monitored Gallery", out labelGalleryTitle, out panelGalleryContent));
        }

        //**********************************************************************//
        //QUADRANT MOVER HELPER METHOD
        //**********************************************************************//
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!this.IsHandleCreated || verticalSplit == null || horizontalTop == null || horizontalBottom == null)
                return;

            // Maintain vertical split ratio
            verticalSplit.SplitterDistance = (int)(this.ClientSize.Height * verticalRatio);

            horizontalTop.SplitterDistance = (int)(ClientSize.Width * horizontalRatioTop);
            horizontalBottom.SplitterDistance = (int)(ClientSize.Width * horizontalRatioBottom);
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
                Padding = new Padding(2), // ⬅️ spacing inside each quadrant
                //BorderStyle = BorderStyle.FixedSingle,
            };

            titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.WhiteSmoke,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 0),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 0, 10, 2) // ⬅️ space below label before content
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
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
