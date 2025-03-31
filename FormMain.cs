using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

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
            };

            listMonitoredFolders.SelectedIndexChanged += (_, _) =>
            {
                listMonitoredFolders.Invalidate(); // trigger visual update
            };
            listMonitoredFolders.DrawMode = DrawMode.OwnerDrawFixed;
            listMonitoredFolders.SelectionMode = SelectionMode.MultiSimple;

            listMonitoredFolders.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                string path = listMonitoredFolders.Items[e.Index].ToString() ?? "";
                string name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));

                bool isSelected = listMonitoredFolders.SelectedIndices.Contains(e.Index);

                Color backColor = isSelected ? Color.LightGray : Color.White;

                using var bg = new SolidBrush(backColor);
                e.Graphics.FillRectangle(bg, e.Bounds);

                TextRenderer.DrawText(e.Graphics, name, e.Font, e.Bounds, Color.Black, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.DrawFocusRectangle();
            };

            var selectAllToggle = new Label
            {
                Text = "[ Select All ]",
                AutoSize = true,
                ForeColor = Color.LightGray,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9F, FontStyle.Underline),
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                TextAlign = ContentAlignment.MiddleLeft
            };

            bool allSelected = false;

            selectAllToggle.Click += (_, _) =>
            {
                allSelected = !allSelected;
                listMonitoredFolders.ClearSelected();

                if (allSelected)
                {
                    for (int i = 0; i < listMonitoredFolders.Items.Count; i++)
                        listMonitoredFolders.SetSelected(i, true);

                    selectAllToggle.Text = "[ Deselect All ]";
                }
                else
                {
                    listMonitoredFolders.ClearSelected(); // ✅ Visually clears
                    selectAllToggle.Text = "[ Select All ]";
                }

                listMonitoredFolders.Invalidate(); // 🔁 Force visual update
            };


            panelMonitoredContent.Controls.Clear();
            panelMonitoredContent.Controls.Add(listMonitoredFolders);
            panelMonitoredContent.Controls.Add(selectAllToggle); // add on top of list


            //loads system drives
            LoadSystemDrives();
            LoadWorkspaces();
            LoadLastSession();
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

        //select folder in treeview
        private void SelectFolderInTree(string path)
        {
            foreach (TreeNode driveNode in treeSystemFolders.Nodes)
            {
                if (driveNode.Tag?.ToString() == path)
                {
                    treeSystemFolders.SelectedNode = driveNode;
                    driveNode.Expand();
                    return;
                }

                foreach (TreeNode child in driveNode.Nodes)
                {
                    if (child.Tag?.ToString() == path)
                    {
                        treeSystemFolders.SelectedNode = child;
                        driveNode.Expand();
                        child.Expand();
                        return;
                    }
                }
            }
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
                if (monitoredPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) return;

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
            string name = PromptForWorkspaceName();
            if (string.IsNullOrWhiteSpace(name)) return;

            var workspace = new Workspace
            {
                Name = name,
                MonitoredFolders = new(monitoredPaths),
                SelectedSystemFolder = treeSystemFolders.SelectedNode?.Tag?.ToString()
            };

            savedWorkspaces.Add(workspace);
            workspaceDropDown.Items.Add(name);
            workspaceDropDown.SelectedItem = name;

            try
            {
                var json = JsonSerializer.Serialize(savedWorkspaces, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(WORKSPACES_FILE, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to save workspace: {ex.Message}");
            }

            Debug.WriteLine($"✅ Workspace '{name}' saved.");
        }

        private string PromptForWorkspaceName()
        {
            using (var prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 150;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.StartPosition = FormStartPosition.CenterParent; // ✅ Center over parent
                prompt.Text = "Save Workspace";
                prompt.TopMost = true; // ✅ Always on top

                var textLabel = new Label() { Left = 20, Top = 15, Text = "Workspace name:" };
                var textBox = new TextBox { Left = 20, Top = 20, Width = 240 };
                var buttonSave = new Button { Text = "Save", Left = 170, Width = 90, Top = 50 };

                string result = "";
                buttonSave.Click += (_, _) => { result = textBox.Text; prompt.Close(); };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(buttonSave);
                prompt.AcceptButton = buttonSave;

                if (prompt.ShowDialog(this) == DialogResult.OK)
                {
                    return textBox.Text.Trim();
                }

                return string.Empty;
            }
        }


        private void RemoveCurrentWorkspace()
        {
            var selectedName = workspaceDropDown.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selectedName))
            {
                MessageBox.Show("No workspace selected.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Remove workspace '{selectedName}'?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            savedWorkspaces.RemoveAll(ws => ws.Name == selectedName);

            try
            {
                File.WriteAllText(WORKSPACES_FILE, JsonSerializer.Serialize(savedWorkspaces, new JsonSerializerOptions { WriteIndented = true }));
                Debug.WriteLine($"✅ Workspace '{selectedName}' removed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to save workspace removal: {ex.Message}");
            }

            workspaceDropDown.Items.Remove(selectedName);
            workspaceDropDown.SelectedItem = null;
            currentWorkspace = new Workspace();

            monitoredPaths.Clear();
            listMonitoredFolders.Items.Clear();
        }


        //**********************************************************************//
        //REAL WORKSPACES - add manager helpers
        //**********************************************************************//
        private const string WORKSPACES_FILE = "workspaces.json";

        private List<Workspace> savedWorkspaces = new();
        private Workspace currentWorkspace = new();

        private void LoadWorkspaces()
        {
            try
            {
                if (!File.Exists(WORKSPACES_FILE))
                {
                    savedWorkspaces = new();
                    File.WriteAllText(WORKSPACES_FILE, "[]");
                    return;
                }
                workspaceDropDown.Items.Clear();

                var json = File.ReadAllText(WORKSPACES_FILE);
                savedWorkspaces = JsonSerializer.Deserialize<List<Workspace>>(json) ?? new();
                foreach (var ws in savedWorkspaces)
                {
                    workspaceDropDown.Items.Add(ws.Name);
                }

                // Optionally select the last one
                if (savedWorkspaces.Count > 0)
                    workspaceDropDown.SelectedItem = savedWorkspaces.Last().Name;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠ Failed to load workspaces: {ex.Message}");
                savedWorkspaces = new(); // fallback
            }
        }

        private void LoadWorkspaceByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            var match = savedWorkspaces.FirstOrDefault(w => w.Name == name);
            if (match == null) return;

            currentWorkspace = match;

            // Clear current monitored state
            monitoredPaths.Clear();
            listMonitoredFolders.Items.Clear();

            foreach (var path in match.MonitoredFolders.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(path)) continue;
                    if (monitoredPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) continue;

                    monitoredPaths.Add(path);
                    listMonitoredFolders.Items.Add(path);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"🔒 Skipped {path} (access denied): {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Failed to load monitored folder: {path} - {ex.Message}");
                }
            }


            // Select system folder if one was saved
            if (!string.IsNullOrWhiteSpace(match.SelectedSystemFolder))
            {
                SelectFolderInTree(match.SelectedSystemFolder);
            }

            Debug.WriteLine($"✅ Workspace '{name}' loaded.");
        }

        //**********************************************************************//
        //JSON basic screen
        //**********************************************************************//
        //JSON workspaces
        private void LoadLastSession()
        {
            var last = savedWorkspaces.LastOrDefault();
            if (last == null) return;

            currentWorkspace = last;

            foreach (var path in currentWorkspace.MonitoredFolders.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(path)) continue;
                    if (monitoredPaths.Contains(path, StringComparer.OrdinalIgnoreCase)) continue;

                    monitoredPaths.Add(path);
                    listMonitoredFolders.Items.Add(path);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠ Skipped monitored folder '{path}': {ex.Message}");
                }
            }
        }

    }
}
