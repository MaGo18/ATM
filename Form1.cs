using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;

namespace ATM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string fileRead = string.Empty;
        private SQLiteConnection QLiteConnection = new SQLiteConnection("Data Source = ATMCARDS.db");
        private SQLiteConnection LiteConnection = new SQLiteConnection("Data Source = ATMMONEY.db");

        private Hashtable GlobalCheck(int req)
        {
            int sum = 0;
            Hashtable money = new Hashtable();
            try
            {
                SQLiteDataReader QLiteData = null;
                LiteConnection.Open();
                SQLiteCommand liteCommand = new SQLiteCommand("SELECT value, number FROM[Money]", LiteConnection);
                QLiteData = liteCommand.ExecuteReader();

                while (QLiteData.Read())
                {
                    money.Add(Convert.ToInt32(QLiteData["value"]), Convert.ToInt32(QLiteData["number"]));
                    sum += Convert.ToInt32(QLiteData["value"]) * Convert.ToInt32(QLiteData["number"]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {

                if (LiteConnection != null)
                {
                    LiteConnection.Close();
                }


            }

            if (req <= sum)
            {
                return Check(req, money);
            }
            else return null;

        }
        private Hashtable Check(int req, Hashtable money)
        {
            Hashtable ret_money = new Hashtable();
            ICollection keyColl = money.Keys;
            List<int> s = new List<int>();


            foreach (int c in keyColl)
            {
                s.Add(c);
            }

            var sortList = s.OrderByDescending(i => i);
            foreach (int c in sortList)
            {
                System.Console.WriteLine(">>" + c);
                int k = 0;
                for (; ; )
                {

                    if (k >= Convert.ToInt32(money[c]))
                    {
                        break;
                    }
                    int diff = req - c * (k + 1);
                    if (diff >= 0)

                    {
                        k++;
                    }
                    else break;
                }
                req -= k * c;
                ret_money.Add(c, k);

            }
            System.Console.WriteLine("\n req:" + req);
            foreach (DictionaryEntry de in ret_money)
            {
                System.Console.WriteLine(de.Key + "  " + de.Value);
            }
            return ret_money;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.TextLength < 4)
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
                string buttonname = button.Text;
                textBox1.Text += buttonname;
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            string s = textBox1.Text;
            if (textBox1.TextLength != 0)
            {
                textBox1.Text = s.Substring(0, s.Length - 1);
            }
            else MessageBox.Show("String is empty");

        }

        private void btnok_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                string parol = textBox1.Text;
                tabControl1.SelectTab(1);
            }
            else MessageBox.Show("Enter, pin code", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


        }

        private void button11_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(0);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            QLiteConnection.Open();
            SQLiteDataReader sQLiteData = null;
            SQLiteCommand liteCommand = new SQLiteCommand("SELECT account_balance FROM[Card] WHERE card_password = @card_password and card_number = @card_number", QLiteConnection);
            liteCommand.Parameters.AddWithValue("card_password", textBox1.Text);
            liteCommand.Parameters.AddWithValue("card_number", fileRead);
            try
            {
                sQLiteData = liteCommand.ExecuteReader();
                listBox1.Items.Clear();
                while (sQLiteData.Read())
                {
                    listBox1.Items.Add("Balance in card: " + sQLiteData["account_balance"]);
                    listBox1.Items.Add("Date:" + DateTime.Now);
                    listBox1.Items.Add("ATM number #00001;");
                    listBox1.Items.Add("Card_Number:" + fileRead);
                    listBox1.Items.Add("*******************");


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), ex.Source.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (QLiteConnection != null)
                {
                    QLiteConnection.Close();
                }
            }

            tabControl1.SelectTab(2);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(3);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            button14.Image = Image.FromFile(@"D:\Education\ATM_C#\button_off.png");
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Text file |*.txt";
            dialog.Title = "Select a text file ";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fileStream = dialog.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileRead = reader.ReadToEnd();
                }
            }
            button14.Image = Image.FromFile(@"D:\Education\ATM_C#\button_on.png");
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            textBox1.PasswordChar = '*';
            button14.Image = Image.FromFile(@"D:\Education\ATM_C#\button_off.png");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (QLiteConnection != null && QLiteConnection.State != ConnectionState.Closed)
                QLiteConnection.Close();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            QLiteConnection.Open();
            LiteConnection.Open();
            SQLiteCommand command_c = new SQLiteCommand("UPDATE[Card] SET [account_balance]=@account_balance WHERE [card_number]=@card_number", QLiteConnection);
            SQLiteCommand command_m = new SQLiteCommand();

            command_c.Parameters.AddWithValue("card_number", fileRead);
            //command_c.Parameters.AddWithValue("account_balance",);



            System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
            string buttonvalue = button.Text;
            int request=Convert.ToInt32(button.Text);
            Hashtable ret = GlobalCheck(request);
           
            /* if (ret!=null)
            {
                foreach (DictionaryEntry de in ret)
                {
                    System.Console.WriteLine(de.Key + "\t" + de.Value);
                }
            }
            */
        }

        private void button23_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(4);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(3);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
            string buttonname = button.Text;
            textBox2.Text += buttonname;
        }

        private void button34_Click(object sender, EventArgs e)
        {
            string s = textBox2.Text;
            if (textBox2.TextLength != 0)
            {
                textBox2.Text = s.Substring(0, s.Length - 1);
            }
            else MessageBox.Show("String is empty");
        }

        private void button35_Click(object sender, EventArgs e)
        {
            string ww = textBox2.Text;
            GlobalCheck(Convert.ToInt32(ww));
        }
    }
}
