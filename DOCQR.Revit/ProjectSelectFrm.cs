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
    public partial class ProjectSelectFrm : Form
    {

        public string ProjectName;
        private DOCQRclient client;

        public ProjectSelectFrm(DOCQRclient Client)
        {
            InitializeComponent();
            client = Client;
        }

        private void ProjectSelectFrm_Load(object sender, EventArgs e)
        {
            this.comboBox1.DataSource = client.GetProjects();               // get info for drop down menu
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProjectName = this.comboBox1.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
