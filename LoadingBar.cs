using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AoETreeEditor
{
    public partial class LoadingBar : Form
    {
        public LoadingBar()
        {
            InitializeComponent();
        }

        private void LoadingBar_Load(object sender, EventArgs e)
        {

        }

        public void SetMaxValue(int v = 100)
        {
            this.LoadingBar1.Maximum = v;
            this.Refresh();
        }

        public void SetCurrentValue(int v = 0)
        {
            this.LoadingBar1.Value = v;
            this.LoadingBar1.Value = Math.Max(v - 1, 0);
            this.Refresh();
        }

        public int GetCurrentMaxValue()
        {
            return this.LoadingBar1.Maximum;
        }

        public int GetCurrentValue()
        {
            return this.LoadingBar1.Value;
        }

        public void _Step()
        {
            this.LoadingBar1.PerformStep();
            this.Refresh();
        }
    }
}
