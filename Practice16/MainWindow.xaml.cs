using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Practice16
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection connectionSQL;
        OleDbConnection connectionAccess;
        SqlDataAdapter daSQL;
        DataTable dtSQL;
        OleDbDataAdapter daAccess;
        DataTable dtAccess;
        DataRowView row;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void PrepareDataSources()
        {
            Task t1 = Task.Factory.StartNew(PrepareMSSQL);
            Task t2 = Task.Factory.StartNew(PrepareMSAccess);
            await Task.WhenAll(t1, t2);
        }

        private async Task PrepareMSSQL()
        {
            SqlConnectionStringBuilder strCon = new SqlConnectionStringBuilder()
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = "Practice16",
                IntegratedSecurity = true,
            };
            
            connectionSQL = new SqlConnection(strCon.ConnectionString);
            connectionSQL.StateChange += SqlConnection_StateChange;
            try
            {
                connectionSQL.Open();
                dtSQL = new DataTable();
                daSQL = new SqlDataAdapter();
                var sql = @"SELECT * FROM Users Order By Users.Id";
                daSQL.SelectCommand = new SqlCommand(sql, connectionSQL);

                sql = @"INSERT INTO Users (lastName, firstName, middleName, phoneNumber, email) 
                                 VALUES (@lastName, @firstName, @middleName, @phoneNumber, @email); 
                     SET @Id = @@IDENTITY;";

                daSQL.InsertCommand = new SqlCommand(sql, connectionSQL);
                daSQL.InsertCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id").Direction = ParameterDirection.Output;
                daSQL.InsertCommand.Parameters.Add("@lastName", SqlDbType.NVarChar, 50, "lastName");
                daSQL.InsertCommand.Parameters.Add("@firstName", SqlDbType.NVarChar, 50, "firstName");
                daSQL.InsertCommand.Parameters.Add("@middleName", SqlDbType.NVarChar, 50, "middleName");
                daSQL.InsertCommand.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50, "phoneNumber");
                daSQL.InsertCommand.Parameters.Add("@email", SqlDbType.NVarChar, 50, "email");

                sql = @"UPDATE Users SET 
                            lastName = @lastName,
                            firstName = @firstName, 
                            middleName = @middleName,
                            phoneNumber = @phoneNumber,
                            email = @email
                            WHERE Id = @Id";

                daSQL.UpdateCommand = new SqlCommand(sql, connectionSQL);
                daSQL.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id").Direction = ParameterDirection.Output;
                daSQL.UpdateCommand.Parameters.Add("@lastName", SqlDbType.NVarChar, 50, "lastName");
                daSQL.UpdateCommand.Parameters.Add("@firstName", SqlDbType.NVarChar, 50, "firstName");
                daSQL.UpdateCommand.Parameters.Add("@middleName", SqlDbType.NVarChar, 50, "middleName");
                daSQL.UpdateCommand.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50, "phoneNumber");
                daSQL.UpdateCommand.Parameters.Add("@email", SqlDbType.NVarChar, 50, "email");

                sql = "DELETE FROM Users WHERE Id = @Id";

                daSQL.DeleteCommand = new SqlCommand(sql, connectionSQL);
                daSQL.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id");

                daSQL.Fill(dtSQL);
                await Dispatcher.InvokeAsync(() =>
                {
                    dg1.DataContext = dtSQL.DefaultView;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async Task PrepareMSAccess()
        {
            OleDbConnectionStringBuilder strCon = new OleDbConnectionStringBuilder()
            {
                Provider = @"Microsoft.ACE.OLEDB.12.0",
                DataSource = @"Practice16.accdb"
            };
            connectionAccess = new OleDbConnection(strCon.ConnectionString);
            connectionAccess.StateChange += SqlConnection_StateChange;
            try
            {
                connectionAccess.Open();
                dtAccess = new DataTable();
                daAccess = new OleDbDataAdapter();
                var sql = @"SELECT * FROM Goods Order By Goods.ID";
                daAccess.SelectCommand = new OleDbCommand(sql, connectionAccess);

                sql = @"INSERT INTO Goods (email, code, goodName) 
                                    VALUES (@email, @code, @goodName);
                                    SET @ID = @@IDENTITY;";

                daAccess.InsertCommand = new OleDbCommand(sql, connectionAccess);

                daAccess.InsertCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID").Direction = ParameterDirection.Output;
                daAccess.InsertCommand.Parameters.Add("@email", OleDbType.Char, 50, "email");
                daAccess.InsertCommand.Parameters.Add("@code", OleDbType.Char, 50, "code");
                daAccess.InsertCommand.Parameters.Add("@goodName", OleDbType.Char, 50, "goodName");

                sql = @"UPDATE Goods SET 
                            email = @email,
                            code = @code, 
                            goodName = @goodName 
                            WHERE ID = @ID";

                daAccess.UpdateCommand = new OleDbCommand(sql, connectionAccess);
                daAccess.UpdateCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID").Direction = ParameterDirection.Output;
                daAccess.UpdateCommand.Parameters.Add("@email", OleDbType.Char, 50, "email");
                daAccess.UpdateCommand.Parameters.Add("@code", OleDbType.Char, 50, "code");
                daAccess.UpdateCommand.Parameters.Add("@goodName", OleDbType.Char, 50, "goodName");

                sql = "DELETE FROM Goods WHERE ID = @ID";

                daAccess.DeleteCommand = new OleDbCommand(sql, connectionAccess);
                daAccess.DeleteCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID");

                daAccess.Fill(dtAccess);
                await Dispatcher.InvokeAsync(() =>
                {
                    dg2.DataContext = dtAccess.DefaultView;
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async void SqlConnection_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (sender as SqlConnection != null)
                {
                    tb1.Text += $@"{DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss")} {(sender as SqlConnection).Database} в состоянии:" +
                        $" {(sender as SqlConnection).State}\n"+
                        $"Строка подключения: {(sender as SqlConnection).ConnectionString}\n";
                }
                else if (sender as OleDbConnection != null)
                {
                    tb1.Text += $@"{DateTime.Now.ToString("yyyy.MM.dd hh:mm:ss")} {(sender as OleDbConnection).DataSource} в состоянии:" +
                        $" {(sender as OleDbConnection).State}\n" +
                        $"Строка подключения: {(sender as OleDbConnection).ConnectionString}\n";
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            connectionSQL.Close();
            connectionAccess.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareDataSources();
        }

        private void dg1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //row = (DataRowView)dg1.SelectedItem;
            row = (DataRowView)e.Row.Item;
            row.BeginEdit();
        }

        private void dg1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (row != null)
            {
                row.EndEdit();
                daSQL.Update(dtSQL);
            }
        }
    }
}
