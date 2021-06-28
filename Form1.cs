using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AoETreeEditor
{
    public partial class Form1 : Form
    {
        static List<string> STATS = new List<string>(); //Small passives
        static List<string> SPECIAL = new List<string>(); //Notable passives
        static List<string> MAJOR = new List<string>(); //Keystone passives
        static List<string> START = new List<string>(); //START passives
        static readonly List<string> TYPES = new List<string>() { "STAT", "MAJOR", "SPECIAL", "START" };
        static readonly LoadingBar loadingBar = new LoadingBar();
        static SkillTree tree = new SkillTree();
        static Form1 thisForm;
        static ContextMenu contextMenu = new ContextMenu();
        static bool IsOpenedFile = false;
        static string OpenedFilePath = "";
        static string OpenedFileName = "";
        static DataGridView temp;

        public Form1()
        {
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            thisForm = this;
            SetDoubleBuffer(viewport, true);
            viewport.Width = Width - 40;
            viewport.Height = Height - 90;
            temp = viewport;
            temp.Hide();
            Enabled = false;
            UseWaitCursor = true;
            loadingBar.Show();
            loadingBar.Location = new Point((Location.X + Width / 2) - (loadingBar.Width / 2),
                                (Location.Y + Height / 2) - (loadingBar.Height / 2));
            selectedValue.AutoSize = false;
            selectedValue.Text = "Select a cell";
            selectedValue.Location = new Point((thisForm.Width / 2) - (selectedValue.Width / 2), selectedValue.Location.Y);
            selectedValue.Show();
            Console.WriteLine(loadingBar.Location);
            loadingBar.SetCurrentValue(0);
            loadingBar.SetMaxValue(100000000);
            loadingBar.Focus();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerAsync();
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saved Trees");
        }

        private void ReloadPerks(object sender, DoWorkEventArgs e)
        {
            var backgroundWorker = sender as BackgroundWorker;
            STATS = new List<string>(); //Small passives
            SPECIAL = new List<string>(); //Notable passives
            MAJOR = new List<string>(); //Keystone passives
            START = new List<string>(); //START passives
            Console.WriteLine($"Loading perks...");
            int loaded = 0;
            string[] files = Directory.GetFiles("Perks/");
            Console.WriteLine($"Found {files.Length} perk files.");

            foreach (string path in files)
            {
                if (loaded % 13 == 0) Thread.Sleep(1);
                JObject j = JObject.Parse(File.ReadAllText(path));

                if (TYPES.Contains((string)j.GetValue("type")) && 
                    (((string)j.GetValue("spell")) == null || ((string)j.GetValue("spell")) == ""))
                {
                    switch((string)j.GetValue("type"))
                    {
                        case "STAT":
                            STATS.Add((string)j.GetValue("identifier"));
                            loaded++;
                            break;
                        case "MAJOR":
                            MAJOR.Add((string)j.GetValue("identifier"));
                            loaded++;
                            break;
                        case "SPECIAL":
                            SPECIAL.Add((string)j.GetValue("identifier"));
                            loaded++;
                            break;
                        case "START":
                            START.Add((string)j.GetValue("identifier"));
                            loaded++;
                            break;
                    }
                } else
                {
                    File.Delete(path);
                    Console.WriteLine($"Deleted invalid file: {path}");
                }
                Console.WriteLine($"progress...{loaded}/{files.Length}");
                backgroundWorker.ReportProgress((int)(loaded / (double)files.Length * 100));
            }
            Console.WriteLine($"progress...{loaded}/{files.Length}");
            Console.WriteLine($"Loaded {loaded} perks into database");
        }

        void SetDoubleBuffer(Control ctl, bool DoubleBuffered)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, ctl, new object[] { DoubleBuffered });
        }

        private void InitSheet(bool isBlank)
        {
            Console.WriteLine($"Attempting to initialize tree...");
            viewport = temp;
            viewport.Show();
            if (!isBlank)
            {
                tree = SkillTree.Parse(File.ReadAllText(OpenedFilePath));
            } else
            {
                IsOpenedFile = false;
                tree = new SkillTree();
            }
            viewport.RowCount = tree.Rows;
            viewport.ColumnCount = tree.Columns;

            int size = 60;

            foreach (DataGridViewRow row in viewport.Rows)
            {
                row.Height = size;
                foreach (DataGridViewColumn column in viewport.Columns)
                {
                    column.Width = size;
                }
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";
                    if (tree.Get(cell.ColumnIndex, cell.RowIndex) != null) 
                        cell.Value = Regex.Replace(tree.Get(cell.ColumnIndex, cell.RowIndex), "(.{9})", "$1" + Environment.NewLine);
                    if (cell.ColumnIndex % 2 == 0 || cell.RowIndex % 2 == 0)
                    {
                        cell.Style.BackColor = Color.FromArgb(25, 25, 25);
                    } else cell.Style.BackColor = Color.FromArgb(5, 5, 5);
                    if (cell.ColumnIndex == tree.GetCenter().X || cell.RowIndex == tree.GetCenter().Y) cell.Style.BackColor = Color.FromArgb(90, 90, 90);
                    if (cell.ColumnIndex == tree.GetCenter().X && cell.RowIndex == tree.GetCenter().Y)
                    {
                        cell.Style.BackColor = Color.FromArgb(190, 190, 0); ;
                        cell.Style.ForeColor = Color.FromArgb(0, 0, 0); ;
                    }
                }
            }

            viewport.ColumnHeadersVisible = false;
            viewport.RowHeadersVisible = false;
            //viewport.MultiSelect = false;

            BuildContextMenu();
        }

        private void InitSheet(SkillTree inputTree)
        {
            Console.WriteLine($"Attempting to initialize tree...");
            viewport = temp;
            viewport.Show();

            tree = inputTree;

            viewport.RowCount = tree.Rows;
            viewport.ColumnCount = tree.Columns;

            int size = 60;

            foreach (DataGridViewRow row in viewport.Rows)
            {
                row.Height = size;
                foreach (DataGridViewColumn column in viewport.Columns)
                {
                    column.Width = size;
                }
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (tree.Get(cell.ColumnIndex, cell.RowIndex) != null)
                        cell.Value = Regex.Replace(tree.Get(cell.ColumnIndex, cell.RowIndex), "(.{9})", "$1" + Environment.NewLine);
                    if (cell.ColumnIndex % 2 == 0 || cell.RowIndex % 2 == 0)
                    {
                        cell.Style.BackColor = Color.FromArgb(25, 25, 25);
                    }
                    else cell.Style.BackColor = Color.FromArgb(5, 5, 5);
                    if (cell.ColumnIndex == tree.GetCenter().X || cell.RowIndex == tree.GetCenter().Y) cell.Style.BackColor = Color.FromArgb(90, 90, 90);
                    if (cell.ColumnIndex == tree.GetCenter().X && cell.RowIndex == tree.GetCenter().Y)
                    {
                        cell.Style.BackColor = Color.FromArgb(190, 190, 0); ;
                        cell.Style.ForeColor = Color.FromArgb(0, 0, 0); ;
                    }
                }
            }

            viewport.ColumnHeadersVisible = false;
            viewport.RowHeadersVisible = false;
            //viewport.MultiSelect = false;

            BuildContextMenu();

            BuildContextMenu();
        }

        private void reloadSkillsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Enabled = false;
            UseWaitCursor = true;
            loadingBar.Show();
            loadingBar.Location = new Point((Location.X + Width / 2) - (loadingBar.Width / 2),
                                (Location.Y + Height / 2) - (loadingBar.Height / 2));
            Console.WriteLine(loadingBar.Location);
            loadingBar.Focus();
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loadingBar.SetCurrentValue((int)(loadingBar.GetCurrentMaxValue() * ((double)e.ProgressPercentage / 100)));
            loadingBar.Focus();
            Console.WriteLine($"Received update: {e.ProgressPercentage}");
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("COMPLETED WORK");
            InitSheet(tree);
            loadingBar.SetCurrentValue(0);
            loadingBar.Hide();
            thisForm.Activate();
            thisForm.Enabled = true;
            thisForm.UseWaitCursor = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            viewport.Width = Width - 40;
            viewport.Height = Height - 90;
            selectedValue.Location = new Point((thisForm.Width / 2) - (selectedValue.Width / 2), selectedValue.Location.Y);
        }

        private void viewport_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                viewport.CurrentCell = viewport[e.ColumnIndex, e.RowIndex];
                contextMenu.Show(thisForm, PointToClient(Cursor.Position));
            }
        }

        private string NextValue(string input)
        {
            switch(input)
            {
                case "":
                    selectedValue.Text = "O";
                    return "O";
                case "x":
                    selectedValue.Text = "Select a cell.";
                    return "";
                case "O":
                    selectedValue.Text = "T";
                    return "T";
                case "T":
                    selectedValue.Text = "K";
                    return "K";
                case "K":
                    selectedValue.Text = "x";
                    return "x";
                default:
                    selectedValue.Text = "Select a cell.";
                    return "";
            }
        }

        private void viewport_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                foreach (DataGridViewCell cell in viewport.SelectedCells)
                {
                    if (cell != null && cell.Value != null)
                    {
                        cell.Value = NextValue(cell.Value.ToString());
                        tree.Set(cell.ColumnIndex, cell.RowIndex, cell.Value.ToString());
                        if (cell.Value.ToString() == "")
                        {
                            selectedValue.Text = "Select a cell.";
                        }
                    } else
                    {
                        cell.Value = NextValue("");
                        tree.Set(cell.ColumnIndex, cell.RowIndex, cell.Value.ToString());
                        selectedValue.Text = cell.Value.ToString();
                    }
                }
            }
        }

        private void BuildContextMenu()
        {
            ContextMenu m = new ContextMenu();
            List<MenuItem> stats = new List<MenuItem>();
            foreach (string value in STATS)
            {
                MenuItem item = new MenuItem(value, new System.EventHandler(OnMenuItemClick));
                stats.Add(item);
            }
            m.MenuItems.Add("Stats", stats.ToArray());
            //
            List<MenuItem> special = new List<MenuItem>();
            foreach (string value in SPECIAL)
            {
                MenuItem item = new MenuItem(value, new System.EventHandler(OnMenuItemClick));
                special.Add(item);
            }
            m.MenuItems.Add("Special", special.ToArray());
            //
            List<MenuItem> major = new List<MenuItem>();
            foreach (string value in MAJOR)
            {
                MenuItem item = new MenuItem(value, new System.EventHandler(OnMenuItemClick));
                major.Add(item);
            }
            m.MenuItems.Add("Major", major.ToArray());
            //
            List<MenuItem> start = new List<MenuItem>();
            foreach (string value in START)
            {
                MenuItem item = new MenuItem(value, new System.EventHandler(OnMenuItemClick));
                start.Add(item);
            }
            MenuItem center = new MenuItem("[CENTER]", new System.EventHandler(OnMenuItemClick));
            start.Add(center);
            m.MenuItems.Add("Start", start.ToArray());
            contextMenu = m;
        }

        private void OnMenuItemClick(object sender, EventArgs e)
        {
            MenuItem s = sender as MenuItem;
            foreach (DataGridViewCell cell in viewport.SelectedCells)
            {
                cell.Value = Regex.Replace(s.Text, "(.{9})", "$1" + Environment.NewLine);
                tree.Set(cell.RowIndex, cell.ColumnIndex, s.Text);
                selectedValue.Text = s.Text;
                if (s.Text == "[CENTER]")
                {
                    tree.ReplaceCenter(new Point(cell.RowIndex, cell.ColumnIndex));
                    InitSheet(tree);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsOpenedFile)
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saved Trees");
                saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saved Trees";
                Console.WriteLine(saveFileDialog1.InitialDirectory);
                saveFileDialog1.FileName = "talents.csv";
                saveFileDialog1.ShowDialog();
            } else
            {
                File.WriteAllText(OpenedFilePath, tree.ToString());
                Console.WriteLine($"Wrote tree file to: { OpenedFilePath }");
                OpenedFileName = OpenedFilePath.Split('\\')[OpenedFilePath.Split('\\').Length - 1];
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            File.WriteAllText(saveFileDialog1.FileName, tree.ToString());
            Console.WriteLine($"Wrote tree file to: { saveFileDialog1.FileName }");
            OpenedFilePath = saveFileDialog1.FileName;
            OpenedFileName = OpenedFilePath.Split('\\')[OpenedFilePath.Split('\\').Length - 1];
            Console.WriteLine(OpenedFileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsOpenedFile) saveFileDialog1.FileName = OpenedFileName;
            saveFileDialog1.ShowDialog();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\Saved Trees");
            openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory() + "\\Saved Trees";
            openFileDialog1.FileName = OpenedFileName;
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            OpenedFilePath = openFileDialog1.FileName;
            OpenedFileName = OpenedFilePath.Split('\\')[OpenedFilePath.Split('\\').Length-1];
            IsOpenedFile = true;
            InitSheet(false);
        }

        private void viewport_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in viewport.SelectedCells)
            {
                if (cell != null && cell.Value != null) 
                { 
                    selectedValue.Text = tree.Get(cell.ColumnIndex, cell.RowIndex);
                    if (tree.Get(cell.ColumnIndex, cell.RowIndex) == "" || tree.Get(cell.ColumnIndex, cell.RowIndex) == null)
                    {
                        selectedValue.Text = cell.Value.ToString();
                    }
                } else
                {
                    selectedValue.Text = "Select a cell.";
                }
            }
        }

        private void selectedValue_TextChanged(object sender, EventArgs e)
        {
            if (selectedValue.Text == null || selectedValue.Text == "") selectedValue.Text = "Select a cell.";
        }

        private void newTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InitSheet(true);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            {
                // Delete Key - Delete Selected Row!
                if (keyData == Keys.Delete)
                {
                    DeleteSelectedCells();
                    return true;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void DeleteSelectedCells()
        {
            foreach(DataGridViewCell cell in viewport.SelectedCells)
            {
                cell.Value = "";
                tree.Set(cell.RowIndex, cell.ColumnIndex, "");
            }
        }
    }
}
