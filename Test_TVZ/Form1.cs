using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;
using TextBox = System.Windows.Forms.TextBox;

namespace Test_TVZ
{
    public partial class Form1 : Form
    {
        public int countStation = 0;
        SqlConnection sqlConnection = null;
        DataTable table;
        string path = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Database.mdf;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(path);
            sqlConnection.Open();

            comboBoxCity.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxTrain.DropDownStyle = ComboBoxStyle.DropDownList;

            UpdateComboBoxRout();
        }

        private void buttonAddRout_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty)
            {
                bool check = false;
                foreach(var c in comboBoxTrain.Items)
                {
                    if(c.ToString() == textBox1.Text)
                        check = true;
                }

                if (check == false)
                {
                    SqlCommand cmd = new SqlCommand($"INSERT INTO [Trails] VALUES (N'{textBox1.Text}');", sqlConnection);
                    cmd.ExecuteNonQuery();
                    textBox1.Clear();
                    UpdateComboBoxRout();
                }
                else
                {
                    MessageBox.Show("Такое название уже используется");
                    textBox1.Clear();
                }                    
            }
            else
                MessageBox.Show("Заполните поле с названием");
        }

        private void buttonAddStation_Click(object sender, EventArgs e)
        {
            if(comboBoxTrain.SelectedIndex != -1)
            {
                if (comboBoxCity.SelectedIndex != -1)
                {
                    table.Rows.Add(comboBoxTrain.SelectedItem, comboBoxCity.SelectedItem,
                        TimeSpan.Parse(dateTimePicker2.Value.ToShortTimeString()), TimeSpan.Parse(dateTimePicker4.Value.ToShortTimeString()), TimeSpan.Zero);
                    DrowTable(table);
                    UpdateComboBoxCity();
                    dateTimePicker2.Value = DateTime.Parse(TimeSpan.Zero.ToString());
                    dateTimePicker4.Value = DateTime.Parse(TimeSpan.Zero.ToString());
                    comboBoxCity.SelectedIndex = -1;
                }
                else
                    MessageBox.Show("Выберите город");
            }
            else
                MessageBox.Show("Выберите маршрут");
        }

        private void comboBoxTrain_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePanel();
            UpdateComboBoxCity();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if(comboBoxTrain.SelectedIndex != -1)
            {
                DialogResult dialogResult = MessageBox.Show("Удалить маршрут?", "Предупреждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    SqlCommand cmd = new SqlCommand($"DELETE FROM Trails WHERE [Name] = '{comboBoxTrain.SelectedItem}'", sqlConnection);
                    cmd.ExecuteNonQuery();
                    UpdateComboBoxRout();
                }
            }
            else
            {
                MessageBox.Show("Выберите маршрут");
            }
        }

        void UpdateComboBoxRout()
        {
            comboBoxTrain.Text = string.Empty;
            comboBoxTrain.Items.Clear();
            SqlCommand cmd = new SqlCommand("SELECT Name FROM Trails", sqlConnection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comboBoxTrain.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        void UpdateComboBoxCity()
        {

            List<string> list = new List<string>();
            foreach(DataRow c in table.Rows)
            {
                list.Add(c[1].ToString());
            }

            SqlCommand cmd = new SqlCommand("SELECT Name FROM City", sqlConnection);
            SqlDataReader reader = cmd.ExecuteReader();
            comboBoxCity.Items.Clear();
            comboBoxCity.Text = string.Empty;
            while (reader.Read())
            {
                if (!list.Contains(reader.GetString(0)))
                {
                    comboBoxCity.Items.Add(reader.GetString(0));
                }

            }
            reader.Close();
        }

        void DrowTable(DataTable table)
        {
            panel1.Controls.Clear();

            for (int i = 0; i < table.Rows.Count; i++)
            {                
                TextBox t = new TextBox
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(20, 20),
                    Location = new Point(10, i * 30 - 2),
                    TextAlign = HorizontalAlignment.Center,
                    Text = (i + 1).ToString()
                };

                t.TextChanged += (s, ea) => Swap(t.Text);

                Label label = new Label
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(120, 22),
                    Location = new Point(60, i * 30),
                    Text = table.Rows[i][1].ToString()
                };

                DateTimePicker dpEnd = new DateTimePicker
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(220, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Enabled = false,
                };

                if (i != 0 && i != table.Rows.Count)
                {
                    TimeSpan summEnd = (TimeSpan)table.Rows[i - 1][3] + (TimeSpan)table.Rows[i - 1][4];
                    dpEnd.Value = DateTime.Parse(summEnd.ToString("hh\\:mm"));
                }               

                DateTimePicker dpStay = new DateTimePicker
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(320, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][2].ToString())
                };

                dpStay.ValueChanged += (s, ea) => ValueDateTimePicker(dpStay);

                DateTimePicker dpRout = new DateTimePicker
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(420, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][3].ToString())
                };

                dpRout.ValueChanged += (s, ea) => ValueDateTimePicker(dpRout);

                DateTimePicker dpStart = new DateTimePicker
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(520, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][4].ToString())
                };

                if (i != 0)
                {
                    TimeSpan summStart = TimeSpan.Parse(dpStay.Value.ToShortTimeString()) + TimeSpan.Parse(dpEnd.Value.ToShortTimeString());
                    dpStart.Value = DateTime.Parse(summStart.ToString("hh\\:mm"));
                    ValueDateTimePicker(dpStart);
                }

                dpStart.ValueChanged += (s, ea) => ValueDateTimePicker(dpStart);

                Button buttonDeleteRow = new Button
                {
                    Size = new Size(50, 25),
                    Location = new Point(605, i * 30 - 2),
                    Text = "Del"
                };

                buttonDeleteRow.Click += (s, ea) => ButtonDelRow(buttonDeleteRow);

                Control[] row = new Control[] { t, label, dpEnd, dpStay, dpStart, dpRout, buttonDeleteRow };

                foreach (var c in row)
                {
                    if (i == 0)
                    {
                        dpStart.Enabled = true;
                        dpRout.Enabled = true;
                        dpStay.Enabled = false;
                        dpEnd.Text = TimeSpan.Zero.ToString();
                        dpStay.Text = TimeSpan.Zero.ToString();
                    }
                    else if (i < table.Rows.Count - 1)
                    {
                        dpStart.Enabled = false;
                        dpRout.Enabled = true;
                        dpStay.Enabled = true;
                    }
                    else
                    {
                        dpStart.Enabled = false;
                        dpRout.Enabled = false;
                        dpStay.Enabled = false;
                    }
                    panel1.Controls.Add(c);
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Записать новые данные?", "Предупреждение", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                
                SqlCommand cmd = new SqlCommand($"DELETE FROM Rout WHERE [Trail] = N'{comboBoxTrain.SelectedItem}'", sqlConnection);
                cmd.ExecuteNonQuery();
                int i = 1;
                foreach (DataRow c in table.Rows)
                {
                    cmd = new SqlCommand($"INSERT INTO Rout (Trail, City, TimeStay, TimeRout, TimeStart) VALUES (N'{comboBoxTrain.SelectedItem}', N'{c[1]}', '{c[2]}', '{c[3]}', '{c[4]}')", sqlConnection);
                    cmd.ExecuteNonQuery();
                    i++;
                }
                UpdatePanel();
                UpdateComboBoxCity();
            }
        }

        void ButtonDelRow(Button b)
        {
            DialogResult dialogResult = MessageBox.Show("Удалить строку?", "Предупреждение", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                table.Rows.Remove(table.Rows[(b.Location.Y + 2) / 30]);
                DrowTable(table);
                UpdateComboBoxCity();
            }
        }

        void ValueDateTimePicker(DateTimePicker d)
        {
            table.Rows[d.Location.Y / 30].SetField((d.Location.X - 20) / 100 - 1, d.Value.ToShortTimeString());
        }

        void UpdatePanel()
        {
            table = new DataTable();

            SqlCommand cmd = new SqlCommand($"SELECT * FROM [Rout] WHERE Trail = N'{comboBoxTrain.SelectedItem}'", sqlConnection);
            SqlDataReader reader = cmd.ExecuteReader();

            table.Load(reader);
            countStation = table.Rows.Count;
            reader.Close();

            panel1.Controls.Clear();
            DrowTable(table);
            UpdateComboBoxCity();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            DrowTable(table);
        }

        void Swap(string number)
        {
            foreach (DataRow row in table.Rows)
            {
                row[1] = number;

            }
        }
    }
}
