using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    public partial class InputForm : Form
    {
        public InputForm()
        {
            InitializeComponent();
        }

        public static string ShowForm(string content)
        {
            var inputForm = new InputForm();
            inputForm.textBox1.Text = content;
            return inputForm.ShowDialog() == DialogResult.OK ? inputForm.textBox1.Text : "";
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
