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
    public partial class FrmAdmin : Form
    {
        MySqlConnection con;
        private string server;
        private string database;
        private string uid;
        private string password;

        public FrmAdmin()
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
            try
            {
                con.Open();
                String q = "select * from tblpolice";
                DataTable dt = new DataTable();
                MySqlDataAdapter da = new MySqlDataAdapter(q, con);
                da.Fill(dt);
                dataGridView1.DataSource = dt;
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
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                String q = "insert into tblpolice values('" + txtname.Text + "','" + txtemail.Text + "','" + txtcontact.Text + "','" + txtpwd.Text + "')";
                MySqlCommand cmd = new MySqlCommand(q, con);
                cmd.ExecuteNonQuery();
                con.Close();
                MessageBox.Show("Data save successfully");
                loadData();

            }
            catch (Exception ex)
            {
                MessageBox.Show("error: " + ex);
            }
            finally
            {
                con.Close();
            }
        }

        private void FrmAdmin_Load(object sender, EventArgs e)
        {
            loadData();
        }
    }
}
