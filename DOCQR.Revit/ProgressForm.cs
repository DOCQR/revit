using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOCQR.Revit
{
    public partial class ProgressForm : Form
    {
        public Boolean IsCancelled { get; set; }
        public ProgressForm(int numSteps)
        {
            InitializeComponent();
            IsCancelled = false;
            progressBar1.Maximum = numSteps;
        }

        public void Step()
        {
            progressBar1.Value++;
            Application.DoEvents(); // bad!
        }
        public void SetStatus(string msg)
        {
            lblStatus.Text = msg;
            Application.DoEvents(); // bad!
        }
        private void btmOK_Click(object sender, EventArgs e)
        {
            IsCancelled = true;
            btmOK.Text = "Wait...";
            btmOK.Enabled = false;

        }
    }
}
