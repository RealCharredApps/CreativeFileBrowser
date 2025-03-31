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
        private ListBox listMonitoredFolders;
        private List<string> monitoredPaths = new();


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

            //connect the add monitored folder button to the treeview
            btnAddFolder.Click += (_, _) => AddMonitoredFolderFromTree();
            btnRemoveFolder.Click += (_, _) => RemoveSelectedMonitoredFolder();

            //listbox for monitored folders - format
            listMonitoredFolders = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                SelectionMode = SelectionMode.One,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
            };

            listMonitoredFolders.SelectedIndexChanged += (_, _) =>
            {
                listMonitoredFolders.Invalidate(); // trigger visual update
            };

            listMonitoredFolders.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                string text = listMonitoredFolders.Items[e.Index].ToString() ?? "";
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

                e.Graphics.FillRectangle(
                    new SolidBrush(selected ? Color.LightGray : Color.White),
                    e.Bounds
                );

                TextRenderer.DrawText(
                    e.Graphics, text, e.Font, e.Bounds, Color.Black, TextFormatFlags.Left
                );

                e.DrawFocusRectangle();
            };
            panelMonitoredContent.Controls.Clear();
            panelMonitoredContent.Controls.Add(listMonitoredFolders);

            //loads system drives
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
                string displayName = string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    ? drive.Name
                    : $"{drive.VolumeLabel} ({drive.Name.TrimEnd('\\')})";

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

                string path = node.Tag as string ?? "";

                try
                {
                    foreach (var dir in Directory.EnumerateDirectories(path))
                    {
                        try
                        {
                            var name = Path.GetFileName(dir);
                            if (string.IsNullOrWhiteSpace(name)) continue;

                            var subNode = new TreeNode(name)
                            {
                                Tag = dir
                            };

                            // Only add child node if there are subdirs
                            if (Directory.EnumerateDirectories(dir).Any())
                                subNode.Nodes.Add("...");

                            node.Nodes.Add(subNode);
                        }
                        catch (UnauthorizedAccessException uaEx)
                        {
                            Debug.WriteLine($"🔒 Permission denied: {dir} - {uaEx.Message}");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Error expanding {dir}: {ex.Message}");
                            continue;
                        }
                    }
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Debug.WriteLine($"🔒 Root permission denied: {path} - {uaEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Failed to expand {path}: {ex.Message}");
                }
            }
        }

        //preview load
        private void TreeSystemFolders_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeSystemFolders.SelectedNode = e.Node;
            string path = e.Node.Tag as string ?? "";

            Debug.WriteLine("📁 Folder selected: " + path);
            // 🔁 TODO: Load folder content into Preview Quadrant
        }

        //**********************************************************************//
        //ADD MONITORED FOLDERS
        //**********************************************************************//
        private void AddMonitoredFolderFromTree()
        {
            if (treeSystemFolders.SelectedNode == null) return;

            string path = treeSystemFolders.SelectedNode.Tag as string ?? "";
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                if (!Directory.Exists(path)) return;
                if (monitoredPaths.Contains(path)) return;

                monitoredPaths.Add(path);
                listMonitoredFolders.Items.Add(path);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Debug.WriteLine($"🔒 Add error: {path} - {uaEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Add error: {path} - {ex.Message}");
            }
        }
        //REMOVE MONITORED FOLDERS
        private void RemoveSelectedMonitoredFolder()
        {
            if (listMonitoredFolders.SelectedIndex == -1) return;

            try
            {
                int index = listMonitoredFolders.SelectedIndex;
                string path = listMonitoredFolders.Items[index].ToString() ?? "";

                monitoredPaths.Remove(path);
                listMonitoredFolders.Items.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Remove error: {ex.Message}");
            }
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
