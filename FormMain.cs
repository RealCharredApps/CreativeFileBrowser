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
        private TreeView treeSystemFolders;




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
            //panelSystemContent.Controls.Add(new Label
            //{
            //    Text = " Panel visible",
            //    Dock = DockStyle.Top,
            //    ForeColor = Color.Green
            //});

            //treeview for system folders - format
            treeSystemFolders = new TreeView
            {
                Dock = DockStyle.Fill,
                Scrollable = true,
                BorderStyle = BorderStyle.None,
                ShowLines = true,
                ShowRootLines = true,
                HideSelection = false,
                Font = new Font("Segoe UI", 9F),
            };

            treeSystemFolders.BeforeExpand += TreeSystemFolders_BeforeExpand;
            treeSystemFolders.NodeMouseClick += TreeSystemFolders_NodeMouseClick;

            panelSystemContent.Controls.Clear(); // Clear test label
            panelSystemContent.Controls.Add(treeSystemFolders);
            LoadSystemDrives();
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
        //LOAD SYSTEM DRIVES
        //**********************************************************************//
        private void LoadSystemDrives()
        {
            treeSystemFolders.Nodes.Clear();

            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                string displayName = $"{drive.VolumeLabel} ({drive.Name.TrimEnd('\\')})";
                var node = new TreeNode(string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : displayName)
                {
                    Tag = drive.RootDirectory.FullName
                };
                node.Nodes.Add("..."); // Placeholder to show expandable arrow
                treeSystemFolders.Nodes.Add(node);
            }
        }

        private void TreeSystemFolders_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
            {
                node.Nodes.Clear();
                try
                {
                    string path = node.Tag as string ?? "";
                    var dirs = Directory.GetDirectories(path);

                    foreach (var dir in dirs)
                    {
                        try
                        {
                            var subNode = new TreeNode(Path.GetFileName(dir))
                            {
                                Tag = dir
                            };

                            // Check if subfolder has children
                            if (Directory.GetDirectories(dir).Length > 0)
                                subNode.Nodes.Add("...");

                            node.Nodes.Add(subNode);
                        }
                        catch { continue; }
                    }
                }
                catch { }
            }
        }

        private void TreeSystemFolders_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeSystemFolders.SelectedNode = e.Node;
            string path = e.Node.Tag as string ?? "";

            Console.WriteLine("Selected: " + path);
            // 🔁 TODO: Load folder content into Preview Quadrant
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
