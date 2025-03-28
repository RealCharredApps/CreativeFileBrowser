namespace CreativeFileBrowser
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;

        private ToolStrip toolStripMain;
        private ToolStripDropDownButton btnFileMenu;
        private ToolStripButton btnAddFolder;
        private ToolStripButton btnRemoveFolder;
        private ToolStripButton btnRefresh;
        private ToolStripButton btnSaveWorkspace;
        private ToolStripDropDownButton btnFileTypes;
        private CheckedListBox fileTypeChecklist;
        private ToolStripLabel lblSort;
        private ToolStripComboBox sortDropDown;
        private ToolStripLabel lblWorkspace;
        private ToolStripComboBox workspaceDropDown;

        private Panel quadrantTopLeft;
        private Panel quadrantTopRight;
        private Panel quadrantBottomLeft;
        private Panel quadrantBottomRight;
        private Panel pnlLayoutHost;
        private SplitContainer verticalSplit;
        private SplitContainer horizontalTop;
        private SplitContainer horizontalBottom;



        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            //**********************************************************************//
            //THE WINDOW ON DEFAULT OPEN
            //**********************************************************************//

            // Form Settings
            this.Text = "Creative File Browser";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);
            this.Size = new Size(940, 720);

            //**********************************************************************//
            //THE TOOLSTRIP AT THE TOP
            //**********************************************************************//
            // ToolStrip - the top menu stuff
            this.toolStripMain = new ToolStrip();
            this.toolStripMain.Dock = DockStyle.Top;

            this.btnFileMenu = new ToolStripDropDownButton("File");
            this.btnAddFolder = new ToolStripButton("Add");
            this.btnRemoveFolder = new ToolStripButton("Remove");
            this.btnRefresh = new ToolStripButton("Refresh");

            // Add FILE MENU items
            //file menu is justified to the left
            btnFileMenu.DropDownItems.Cast<ToolStripItem>().ToList().ForEach(item =>
            {
                if (item is ToolStripMenuItem mi)
                {
                    mi.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    mi.Image = null;
                    mi.Padding = new Padding(1);
                }
            });

            //btnFileMenu.DropDownItems.Add("Save Current Workspace", null, (_, _) => SaveCurrentWorkspace());
            //btnFileMenu.DropDownItems.Add("Remove Current Workspace", null, (_, _) => RemoveCurrentWorkspace());
            var itemSave = new ToolStripMenuItem("Save Current Workspace")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Image = null,
                Padding = new Padding(1), // optional: tighter spacing
            };
            itemSave.Click += (_, _) => SaveCurrentWorkspace();

            var itemRemove = new ToolStripMenuItem("Remove Current Workspace")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                Image = null,
                Padding = new Padding(1),
            };
            itemRemove.Click += (_, _) => RemoveCurrentWorkspace();

            btnFileMenu.DropDownItems.Add(new ToolStripSeparator());

            var resetLayoutItem = new ToolStripMenuItem("Reset Quadrant View");
            resetLayoutItem.Click += (_, _) => ResetQuadrantsToMiddle();

            btnFileMenu.DropDownItems.Add(itemSave);
            btnFileMenu.DropDownItems.Add(itemRemove);
            btnFileMenu.DropDownItems.Add(resetLayoutItem);


            // Filter items - file type and sort
            //container panel
            var fileTypePanel = new Panel
            {
                Size = new Size(150, 220),
                Padding = new Padding(0),
                BackColor = SystemColors.Window
            };
            //toggle all file types
            var toggleLabel = new Label
            {
                Text = "Select All",
                Dock = DockStyle.Top,
                Height = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(6, 2, 6, 2),
                Font = new Font("Segoe UI", 8.25F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                BackColor = SystemColors.ControlLight
            };
            //gives light hover effect
            toggleLabel.MouseEnter += (_, _) => toggleLabel.BackColor = SystemColors.ControlDark;
            toggleLabel.MouseLeave += (_, _) => toggleLabel.BackColor = SystemColors.ControlLight;


            // File Type Checklist
            this.fileTypeChecklist = new CheckedListBox
            {
                CheckOnClick = true,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };

            //file type options
            string[] fileTypes = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".mp4", ".mov", ".psd", ".tiff", ".bmp", ".raw", ".heic" };
            //temp
            fileTypeChecklist.ItemCheck += (_, __) => DebugSelectedFileTypes();

            fileTypeChecklist.Items.AddRange(fileTypes);
            for (int i = 0; i < fileTypeChecklist.Items.Count; i++)
                fileTypeChecklist.SetItemChecked(i, true); // check all by default

            fileTypePanel.Controls.Add(fileTypeChecklist);
            fileTypePanel.Controls.Add(toggleLabel);

            // Toggle salect all click event
            toggleLabel.Click += (_, _) =>
            {
                bool allSelected = fileTypeChecklist.CheckedItems.Count == fileTypeChecklist.Items.Count;
                for (int i = 0; i < fileTypeChecklist.Items.Count; i++)
                    fileTypeChecklist.SetItemChecked(i, !allSelected);

                toggleLabel.Text = allSelected ? "Select All" : "Deselect All";
            };

            var host = new ToolStripControlHost(fileTypePanel)
            {
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                Size = fileTypePanel.Size,
            };
            ToolStripDropDown checklistDropdown = new ToolStripDropDown();
            checklistDropdown.Items.Add(host);

            // Create the ToolStripDropDownButton
            btnFileTypes = new ToolStripDropDownButton("File Types");
            btnFileTypes.Click += (_, _) =>
            {
                var location = btnFileTypes.Owner.PointToScreen(new Point(btnFileTypes.Bounds.Left, btnFileTypes.Bounds.Bottom));
                checklistDropdown.Show(location);
            };

            // Filter items - file meta sort by dates x names
            this.lblSort = new ToolStripLabel("Sort by:");
            this.sortDropDown = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };
            this.sortDropDown.Items.AddRange(new[]
            {
                "Date (Newest)", "Date (Oldest)",
                "Filename (A-Z)", "Filename (Z-A)"
            });
            this.sortDropDown.SelectedIndex = 0;

            // Workspace otpions dropdown
            this.lblWorkspace = new ToolStripLabel("Workspaces:");
            this.workspaceDropDown = new ToolStripComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120
            };
            this.workspaceDropDown.Items.AddRange(new[]
            {
                "Default (Blank)", "Downloads", "Pictures"
            });
            this.workspaceDropDown.SelectedIndex = 0;

            //**********************************************************************//
            //END - TOOLSTRIP AT THE TOP
            //**********************************************************************//

            //**********************************************************************//
            //THE QUADRANTS
            //**********************************************************************//
            pnlLayoutHost = new Panel
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, toolStripMain.Height),
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            // Create vertical split (top/bottom)
            verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 3,
                BackColor = Color.LightGray
            };

            // Create horizontal splits (left/right)
            horizontalTop = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 3,
                BackColor = Color.LightGray
            };

            horizontalBottom = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 3,
                BackColor = Color.LightGray
            };

            // Add quadrant colors
            horizontalTop.Panel1.BackColor = Color.WhiteSmoke;     // Top Left
            horizontalTop.Panel2.BackColor = Color.White;          // Top Right
            horizontalBottom.Panel1.BackColor = Color.Gainsboro;   // Bottom Left
            horizontalBottom.Panel2.BackColor = Color.LightGray;   // Bottom Right

            // Add to Form
            //this.Controls.Add(verticalSplit);        // Main container
            pnlLayoutHost.Controls.Add(verticalSplit);
            verticalSplit.Dock = DockStyle.Fill; // Let the host manage its size

            // Nest layout
            verticalSplit.Panel1.Controls.Add(horizontalTop);
            verticalSplit.Panel2.Controls.Add(horizontalBottom);

            this.Controls.Add(pnlLayoutHost);
            this.Controls.Add(toolStripMain);
            this.Controls.SetChildIndex(toolStripMain, 0); // Bring toolbar to front


            //******************************************************************************//
            // UI - organize the buttons with a separator line
            //requires all buttons in order to show
            //******************************************************************************//
            toolStripMain.Renderer = new NoImageMarginRenderer();

            toolStripMain.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripSeparator(),
                new ToolStripSeparator(),
                btnFileMenu,
                new ToolStripSeparator(),
                btnAddFolder, btnRemoveFolder, btnRefresh,
                new ToolStripSeparator(),
                new ToolStripSeparator(),
                btnFileTypes,
                new ToolStripSeparator(),
                new ToolStripSeparator(),
                lblSort, sortDropDown,
                new ToolStripSeparator(),
                new ToolStripSeparator(),
                lblWorkspace, workspaceDropDown,
                new ToolStripSeparator(),
                new ToolStripSeparator(),
            });

            //**********************************************************************//
            //PREVIEW PANELS BELOW - RESP FLEX QUADRANT LAYOUT TABLE
            //**********************************************************************//
            //init part 
            quadrantTopLeft = new Panel { BackColor = Color.WhiteSmoke };
            quadrantTopRight = new Panel { BackColor = Color.White };
            quadrantBottomLeft = new Panel { BackColor = Color.Gainsboro };
            quadrantBottomRight = new Panel { BackColor = Color.LightGray };

            //the adjust layout portion will be handled in formmain - adjustlayout method
            //to keep the modular nature of the code clean
            //the build



            //******************************************************************************//
            // Add Controls to Form (after all are initialized)
            //******************************************************************************//
            this.Controls.Add(toolStripMain);
            //this.Controls.Add(quadrantTopLeft);
            //this.Controls.Add(quadrantTopRight);
            //this.Controls.Add(quadrantBottomLeft);
            //this.Controls.Add(quadrantBottomRight);

            if (!this.Controls.Contains(toolStripMain))
                this.Controls.Add(toolStripMain);

            this.Controls.SetChildIndex(toolStripMain, 0);
            // ──────────────────────────────────────────────
            // Resize logic (moved into FormMain_Load)
            // ──────────────────────────────────────────────

            // Auto-resize handler // ask to use from the main form event / helper
            //this.Resize += (_, _) => AdjustLayout();
            //this.Load += (_, _) => AdjustLayout();

        }

        //**********************************************************************//
        //TOOLSTRIP MENU IMAGE RENDERER
        //**********************************************************************//
        public class NoImageMarginRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
                // Do nothing = no margin rendered
            }
        }
    }
}
