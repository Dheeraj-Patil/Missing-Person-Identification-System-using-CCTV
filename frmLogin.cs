using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace MultiFaceRec
{
    public partial class frmLogin : Form
    {
        MySqlConnection con;
        private string server;
        private string database;
        private string uid;
        private string password;
        public frmLogin()
        {
            InitializeComponent();

            server = "198.71.225.62";
            database = "punedb";
            uid = "punedb";
            password = "123456";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            con = new MySqlConnection(connectionString);
        }
        public void loadData()
        {
            int flg = 0;
            try
            {
                con.Open();
                String q = "select * from tblpolice";
                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(q, con);
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    String name = dt.Rows[i]["name"].ToString();
                    String pwd = dt.Rows[i]["password"].ToString();
                    if (textBox1.Text == name && textBox2.Text == pwd)
                    {
                        flg = 1;
                        this.Hide();

                        frmAddPerson f = new frmAddPerson();
                        f.Show();
                    }

                }

                con.Close();
                //   MessageBox.Show("Data save successfully");

            }
            catch (Exception ex)
            {
                MessageBox.Show("error: " + ex);
            }
            finally
            {
                con.Close();
            }
            if (flg == 0)
            {

                MessageBox.Show("Invalid user");
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "Admin")
            {
                if (textBox1.Text == "admin" && textBox2.Text == "pass")
                {
                    this.Hide();
                    FrmAdmin f = new FrmAdmin();
                    f.Show();
                }
                else
                {

                    MessageBox.Show("Invalid user");
                }

            }
            if (comboBox1.Text == "Police")
            {
                loadData();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmRecognize f = new frmRecognize();
            f.Show();
        }
    }
}
