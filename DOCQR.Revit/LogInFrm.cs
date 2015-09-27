using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RestSharp;

namespace DOCQR.Revit
{
    public partial class LogInFrm : Form
    {

        public string UserEmail;
        private DOCQRclient DClient;

        public LogInFrm(DOCQRclient client)
        {
            InitializeComponent();
            DClient = client;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           string username = textBox1.Text;                // get the user name and password from the user form
            string password = textBox2.Text;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    DClient.SignIn(username, password);
                    this.DialogResult = DialogResult.OK;
                }
                catch( Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            this.Close();
        }
    }       // close class
    
}           // close namespace