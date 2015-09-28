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
        private static string _LastUser = Environment.UserName;

        public string UserEmail;
        public DOCQRclient DClient { get; set; }
        private bool _isDummy = false;

        public LogInFrm(string defaultUrl, bool isDummy)
        {
            InitializeComponent();
            //DClient = client;
            this.textBox1.Text = _LastUser;
            this.textBox3.Text = defaultUrl;
            _isDummy = isDummy;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //// TODO: Remove
            //this.Close(); this.DialogResult = System.Windows.Forms.DialogResult.OK;  return;

            Uri server = null;
            if (Uri.TryCreate(textBox3.Text, UriKind.Absolute, out server) == false)
            {
                MessageBox.Show("Please enter a valid server URL!");
                return;
            }

            if (_isDummy)
            {
                DClient = new DOCQRclient(textBox3.Text);
                DClient.IsDummy = true;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
                return;
            }


           string username = textBox1.Text;                // get the user name and password from the user form
            string password = textBox2.Text;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                try
                {
                    DClient = new DOCQRclient(textBox3.Text);
                    DClient.SignIn(username, password);
                    _LastUser = username; // store it.
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid username/password combination!");
            }
            
        }
    }       // close class
    
}           // close namespace