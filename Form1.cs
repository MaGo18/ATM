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

        private void Withdraw(int req)
        {
            int request = req;
            Hashtable ret = GlobalCheck(request);
            LiteConnection.Open();
            if (ret != null)
            {
                foreach (DictionaryEntry de in ret)
                {
                    int value = Convert.ToInt32(de.Key);
                    int number = Convert.ToInt32(de.Value);
                    MessageBox.Show("Bill:> " + value.ToString() + "\t" + "Quantity:> " + number.ToString(), "OPERATION SUCCESFUL", MessageBoxButtons.OK);
                    SQLiteCommand command_m = new SQLiteCommand("UPDATE[Money] SET [number]=(SELECT number FROM Money WHERE [value]=@value)-@number WHERE [value]=@value", LiteConnection);
                    command_m.Parameters.AddWithValue("value", value);
                    command_m.Parameters.AddWithValue("number", number);
                    command_m.ExecuteNonQuery();
                }

                if (LiteConnection != null)
                {
                    LiteConnection.Close();
                }
            }
        }
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
        private void btnPin_Click(object sender, EventArgs e)
        {
            if (textPin.TextLength < 4)
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
                string buttonname = button.Text;
                textPin.Text += buttonname;
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            string s = textPin.Text;
            if (textPin.TextLength != 0)
            {
                textPin.Text = s.Substring(0, s.Length - 1);
            }
            else MessageBox.Show("String is empty");

        }

        private void btnok_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            if (!string.IsNullOrEmpty(textPin.Text) && !string.IsNullOrWhiteSpace(textPin.Text))
            {
                string parol = textPin.Text;
                tabControl1.SelectTab(1);
            }
            else MessageBox.Show("Enter, pin code", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(0);
        }

        private void btnBalanceCheck_Click(object sender, EventArgs e)
        {
            QLiteConnection.Open();
            SQLiteDataReader sQLiteData = null;
            SQLiteCommand liteCommand = new SQLiteCommand("SELECT account_balance FROM[Card] WHERE card_password = @card_password and card_number = @card_number", QLiteConnection);
            liteCommand.Parameters.AddWithValue("card_password", textPin.Text);
            liteCommand.Parameters.AddWithValue("card_number", fileRead);
            try
            {
                sQLiteData = liteCommand.ExecuteReader();
                listBalance.Items.Clear();
                while (sQLiteData.Read())
                {
                    listBalance.Items.Add("Balance in card: " + sQLiteData["account_balance"]);
                    listBalance.Items.Add("Date:" + DateTime.Now);
                    listBalance.Items.Add("ATM number #00001;");
                    listBalance.Items.Add("Card_Number:" + fileRead);
                    listBalance.Items.Add("*******************");


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

        private void btnCash_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(3);
        }

        private void btnDownloadCard_Click(object sender, EventArgs e)
        {
            btnDownloadCard.Image = Image.FromFile(@"D:\Education\ATM_C#\button_off.png");
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
            btnDownloadCard.Image = Image.FromFile(@"D:\Education\ATM_C#\button_on.png");
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            textPin.PasswordChar = '*';
            btnDownloadCard.Image = Image.FromFile(@"D:\Education\ATM_C#\button_off.png");
        }

        private void btnBack2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (QLiteConnection != null && QLiteConnection.State != ConnectionState.Closed)
                QLiteConnection.Close();
        }

        private void btnBack3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
            string buttonvalue = button.Text;
            Withdraw(Convert.ToInt32(buttonvalue));
        }

        private void btnOther_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(4);
        }

        private void btnBack4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(3);
        }

        private void btnOtherMoney_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Button button = (System.Windows.Forms.Button)(sender);
            string buttonname = button.Text;
            textMoney.Text += buttonname;
        }

        private void btnMoneyDel_Click(object sender, EventArgs e)
        {
            string s = textMoney.Text;
            if (textMoney.TextLength != 0)
            {
                textMoney.Text = s.Substring(0, s.Length - 1);
            }
            else MessageBox.Show("String is empty");
        }

        private void btnMoneyOk_Click(object sender, EventArgs e)
        {
            string MoneyValue = textMoney.Text;
            Withdraw(Convert.ToInt32(MoneyValue));
        }
    }
}
