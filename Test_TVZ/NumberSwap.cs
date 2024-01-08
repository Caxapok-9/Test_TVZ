using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_TVZ
{
    public partial class NumberSwap : Form
    {
        int count, actual;
        public NumberSwap(int x, int y)
        {
            InitializeComponent();
            count = x;
            actual = y;
        }

        private void NumberSwap_Load(object sender, EventArgs e)
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            buttonCont.BackColor = Color.LightGreen;
            buttonCancel.BackColor = Color.Pink;
            for (int i = 1; i <= count; i++)
            {
                if(i != actual)
                    comboBox1.Items.Add(i);
            }
                
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonCont_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text != string.Empty)
            {
                FormStart.swapNumber = (int)comboBox1.SelectedItem;
                this.Close();
            }
        }
    }
}
