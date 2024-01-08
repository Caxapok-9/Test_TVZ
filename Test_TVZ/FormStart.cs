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
    public partial class FormStart : Form
    {
        public static int swapNumber = -1;
        SqlConnection sqlConnection = null;
        DataTable table;
        string path = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|Database1.mdf;Integrated Security=True";
        
        public FormStart()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(path);
            sqlConnection.Open();

            comboBoxTrain.DropDownStyle = ComboBoxStyle.DropDownList;
            buttonUpdate.BackColor = Color.LightYellow;
            buttonSave.BackColor = Color.LightGreen;
            UpdateComboBoxRout();
        }

        private void FormStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            buttonSave_Click(sender, e);
        }


        private void comboBoxTrain_SelectedIndexChanged(object sender, EventArgs e) // Заполнение из БД комбобокса и панели в момент выбор маршрута
        {
            UpdatePanel(); // Заполнение таблицы 
        } 

        private void buttonAddRout_Click(object sender, EventArgs e) // Добавление маршрута и запись его в БД
        {
            if (textBox1.Text != string.Empty) // Проверка написано ли новое название
            {
                bool check = false;

                foreach(var c in comboBoxTrain.Items) // Проверка совпадает ли название с существующими
                {
                    if(c.ToString() == textBox1.Text)
                        check = true;
                }

                if (check == false) // Добавление нового маршрута
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

        private void buttonAddStation_Click(object sender, EventArgs e) // Добавление станции в таблицу НЕ в БД
        {
            if(comboBoxTrain.SelectedIndex != -1) // Проврека выбран ли маршрут
            {

                if (textBoxCity.Text != string.Empty) // Проверка выбран ли город
                {
                    bool check = true;

                    foreach (DataRow c in table.Rows)
                    {
                        if (textBoxCity.Text == c[1].ToString())
                            check = false;
                    }
                    if (check)
                    {
                        table.Rows.Add(comboBoxTrain.SelectedItem, textBoxCity.Text,
                            TimeSpan.Parse(dateTimePicker2.Value.ToShortTimeString()),
                            TimeSpan.Parse(dateTimePicker4.Value.ToShortTimeString()), TimeSpan.Zero); // Добавление новой строки в таблицу НЕ в БД 

                        DrowTable(table); // Перерисовка таблицы

                        dateTimePicker2.Value = DateTime.Parse(TimeSpan.Zero.ToString());
                        dateTimePicker4.Value = DateTime.Parse(TimeSpan.Zero.ToString());
                        textBoxCity.Clear();
                    }
                    else
                        MessageBox.Show("Этот город уже есть в маршруте");
                }
                else
                    MessageBox.Show("Введите город");
            }
            else
                MessageBox.Show("Выберите маршрут");
        }

        private void buttonDelete_Click(object sender, EventArgs e) // Удаление маршрута из БД
        {
            if (comboBoxTrain.SelectedIndex != -1) // Проверка Выбран ли маршрут
            {
                DialogResult dialogResult = MessageBox.Show("Удалить маршрут?", "Предупреждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) // Подтверждение от пользователя 
                {
                    SqlCommand cmd = new SqlCommand($"DELETE FROM Trails WHERE Name = N'{comboBoxTrain.SelectedItem}'", sqlConnection); // Удаление маршрута
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand($"DELETE FROM Rout WHERE Trail = N'{comboBoxTrain.SelectedItem}'", sqlConnection); // Удаление данных о маршруте
                    cmd.ExecuteNonQuery();

                    UpdateComboBoxRout(); // Обновление списка маршрутов
                    panel1.Controls.Clear();
                }
            }
            else
            {
                MessageBox.Show("Выберите маршрут");
            }
        }

        private void buttonSave_Click(object sender, EventArgs e) // Сохранение данных из таблицы в БД
        {
            if (comboBoxTrain.SelectedIndex != -1 && panel1.Controls.Count != 0) // Проверка выбран ли маршрут и есть данные в таблице
            {
                DialogResult dialogResult = MessageBox.Show("Сохранить новые данные ?", "Предупреждение", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) // Подтверждение пользователя
                {
                    SqlCommand cmd = new SqlCommand($"DELETE FROM Rout WHERE [Trail] = N'{comboBoxTrain.SelectedItem}'", sqlConnection); // Удаление данных о маршруте из БД
                    cmd.ExecuteNonQuery();

                    int i = 1;
                    foreach (DataRow c in table.Rows)
                    {
                        cmd = new SqlCommand($"INSERT INTO Rout (Trail, City, TimeStay, TimeRout, TimeStart) VALUES" +
                            $" (N'{comboBoxTrain.SelectedItem}', N'{c[1]}', '{c[2]}', '{c[3]}', '{c[4]}')", sqlConnection); // Запись новых данных в БД
                        cmd.ExecuteNonQuery();
                        i++;
                    }

                    UpdatePanel(); // Обновление таблицы и вывод на форму
                }
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e) // Обновление расписания на форме НЕ в БД
        {
            if (comboBoxTrain.SelectedIndex != -1)
            {
                DrowTable(table); // Вывод таблицы на форму
            }
        }


        void ButtonSwapRow(Button b) // Изменение места строки в таблице НЕ в БД
        {
            var last = (b.Location.Y + 2) / 30;
            NumberSwap swap = new NumberSwap(table.Rows.Count, last + 1); // Открытие диалогового окна
            swap.ShowDialog();

            if (swapNumber - 1 != last && swapNumber != -1)
            {
                DataRow x = table.NewRow();
                x.ItemArray = table.Rows[last].ItemArray; // Копирование выбранной строки
                if (swapNumber - 1 < last)
                {
                    table.Rows.InsertAt(x, swapNumber - 1); // Вставка в новое место в таблицу
                    table.Rows.Remove(table.Rows[last + 1]); // Удаление старой из таблицы
                }
                else
                {
                    table.Rows.InsertAt(x, swapNumber); // Вставка в новое место в таблицу
                    table.Rows.Remove(table.Rows[last]); // Удаление старой из таблицы
                }
                swapNumber = -1;
                DrowTable(table); // Вывод таблицы на форму
            }
        }

        void ButtonDelRow(Button b) // Удаление строки из таблицы НЕ из БД
        {
            DialogResult dialogResult = MessageBox.Show("Удалить строку?", "Предупреждение", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes) // ПОдтверждение пользователя
            {
                table.Rows.RemoveAt((b.Location.Y + 2) / 30); // Удаление строки из таблицы
                DrowTable(table); // Вывод таблицы на форму
            }
        }

        void ValueDateTimePicker(DateTimePicker d) // Обновление значения в таблице при изменении значений в TimePicker
        {
            table.Rows[d.Location.Y / 30].SetField((d.Location.X) / 100 - 1, d.Value.ToShortTimeString());
        }

        void UpdatePanel() // Чтение из БД данных о выбранном маршруте, создание таблицы и вывод её на форму
        {
            table = new DataTable();

            SqlCommand cmd = new SqlCommand($"SELECT * FROM [Rout] WHERE Trail = N'{comboBoxTrain.SelectedItem}'", sqlConnection); // Чтение данных о выбранном маршруте из БД
            SqlDataReader reader = cmd.ExecuteReader();

            table.Load(reader);
            reader.Close();

            panel1.Controls.Clear();
            DrowTable(table); // Вывод таблицы на форму
        }

        void UpdateComboBoxRout() // Обновление списка маршрутов из БД
        {
            comboBoxTrain.Text = string.Empty;
            comboBoxTrain.Items.Clear();

            SqlCommand cmd = new SqlCommand("SELECT Name FROM Trails", sqlConnection); // Чтение данных о существующих маршрутах из БД 
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                comboBoxTrain.Items.Add(reader.GetString(0));
            }
            reader.Close();
        }

        void DrowTable(DataTable table) // Вывод таблицы на фоорму, редактирование видимости TimePicker, подсчёт значений TimePicker
        {
            panel1.Controls.Clear();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Label countStation = new Label // Порядковый номер
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(30, 20),
                    Location = new Point(10, i * 30 - 2),
                    Text = (i + 1).ToString() // Изменение значения текста
                };

                Label labelNameCity = new Label // Название станции
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(110, 22),
                    Location = new Point(50, i * 30),
                    Text = table.Rows[i][1].ToString() // Изменение значения текста
                };

                DateTimePicker dpEnd = new DateTimePicker // Время Прибытия
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(200, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Enabled = false,
                };

                if (i != 0 && i != table.Rows.Count) // Подсчёт времени прибытия
                {
                    TimeSpan summEnd = (TimeSpan)table.Rows[i - 1][3] + (TimeSpan)table.Rows[i - 1][4];
                    dpEnd.Value = DateTime.Parse(summEnd.ToString("hh\\:mm")); // Изменение значения в TimePicker о времени прибытия
                }               

                DateTimePicker dpStay = new DateTimePicker // Время Стоянки
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(300, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][2].ToString()) // Изменение значения в TimePicker о времени стоянки
                };

                dpStay.ValueChanged += (s, ea) => ValueDateTimePicker(dpStay);  // Событие изменения пользователем значения в TimePicker о времени стоянки

                DateTimePicker dpRout = new DateTimePicker // Время в Пути
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(400, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][3].ToString()) // Изменение значения в TimePicker о времени в пути
                };

                dpRout.ValueChanged += (s, ea) => ValueDateTimePicker(dpRout); // Событие изменения пользователем значения в TimePicker о времени пути

                DateTimePicker dpStart = new DateTimePicker // Время Отправления
                {
                    Font = new Font(FontFamily.GenericSansSerif, 9),
                    Size = new Size(72, 22),
                    Location = new Point(500, i * 30),
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "HH:mm",
                    ShowUpDown = true,
                    Value = DateTime.Parse(table.Rows[i][4].ToString()) // Изменение значения в TimePicker о времени отправления
                };

                if (i != 0) // Подсчёт времени отправления
                {
                    TimeSpan summStart = TimeSpan.Parse(dpStay.Value.ToShortTimeString()) + TimeSpan.Parse(dpEnd.Value.ToShortTimeString());
                    dpStart.Value = DateTime.Parse(summStart.ToString("hh\\:mm")); // Изменение значения в TimePicker о времени отправления
                    ValueDateTimePicker(dpStart);
                }

                dpStart.ValueChanged += (s, ea) => ValueDateTimePicker(dpStart); // Событие изменения пользователем значения в TimePicker о времени отправления

                Button buttonDeleteRow = new Button // Кнопка удаления строки
                {
                    Size = new Size(50, 25),
                    Location = new Point(590, i * 30 - 2),
                    Text = "Del",
                    BackColor = Color.LightPink
                };

                buttonDeleteRow.Click += (s, ea) => ButtonDelRow(buttonDeleteRow); // Событие при нажатии на кнопку удаления

                Button buttonSwapRow = new Button // Кнопка изменения положения строки
                {
                    Size = new Size(25, 25),
                    Location = new Point(650, i * 30 - 2),
                    BackColor = Color.Gray
                };

                buttonSwapRow.Click += (s, ea) => ButtonSwapRow(buttonSwapRow); // Событие при нажатии на кнопку изменения положения

                Control[] row = new Control[] { countStation, labelNameCity, dpEnd, dpStay, dpStart, dpRout, buttonDeleteRow, buttonSwapRow}; // Строка в виде массива Controls

                foreach (var c in row) // Обработка Enabled элементов при выводе на форму
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

                    panel1.Controls.Add(c); // Создание Controls на форме
                }
            }
        }
    }
}
