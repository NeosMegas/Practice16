﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        private void PrepareDataSources()
        {
            QueryMSSQL();
            QueryMSAccess();
        }

        /// <summary>
        /// Получение данных из MS SQL
        /// </summary>
        private void QueryMSSQL()
        {
            SqlConnectionStringBuilder strCon = new SqlConnectionStringBuilder
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
                daSQL.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id").SourceVersion = DataRowVersion.Original;
                daSQL.UpdateCommand.Parameters.Add("@lastName", SqlDbType.NVarChar, 50, "lastName");
                daSQL.UpdateCommand.Parameters.Add("@firstName", SqlDbType.NVarChar, 50, "firstName");
                daSQL.UpdateCommand.Parameters.Add("@middleName", SqlDbType.NVarChar, 50, "middleName");
                daSQL.UpdateCommand.Parameters.Add("@phoneNumber", SqlDbType.NVarChar, 50, "phoneNumber");
                daSQL.UpdateCommand.Parameters.Add("@email", SqlDbType.NVarChar, 50, "email");

                sql = "DELETE FROM Users WHERE Id = @Id";

                daSQL.DeleteCommand = new SqlCommand(sql, connectionSQL);
                daSQL.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int, 4, "Id");

                daSQL.Fill(dtSQL);
                dg1.DataContext = dtSQL.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "PrepareMSSQL");
            }
        }

        /// <summary>
        /// Получение данных из БД Access
        /// </summary>
        private void QueryMSAccess()
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
                var sql = @"SELECT * FROM Goods WHERE email = @email Order By Goods.ID";
                daAccess.SelectCommand = new OleDbCommand(sql, connectionAccess);
                daAccess.SelectCommand.Parameters.Add("@email", OleDbType.Char, 50, "email");
                if ((dg1.SelectedItem as DataRowView) != null)
                    daAccess.SelectCommand.Parameters["@email"].Value = (dg1.SelectedItem as DataRowView).Row[5];
                else
                    daAccess.SelectCommand.Parameters["@email"].Value = "mail@mail";

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
                daAccess.UpdateCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID").SourceVersion = DataRowVersion.Original;
                daAccess.UpdateCommand.Parameters.Add("@email", OleDbType.Char, 50, "email");
                daAccess.UpdateCommand.Parameters.Add("@code", OleDbType.Char, 50, "code");
                daAccess.UpdateCommand.Parameters.Add("@goodName", OleDbType.Char, 50, "goodName");

                sql = "DELETE FROM Goods WHERE ID = @ID";

                daAccess.DeleteCommand = new OleDbCommand(sql, connectionAccess);
                daAccess.DeleteCommand.Parameters.Add("@ID", OleDbType.Integer, 4, "ID");

                daAccess.Fill(dtAccess);
                dg2.DataContext = dtAccess.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "PrepareMSAccess");
            }
        }

        /// <summary>
        /// При изменении состояния подключения вывод информации в соответствующую область окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// При закрытии окна закрыть подключения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            connectionSQL.Close();
            connectionAccess.Close();
        }

        /// <summary>
        /// Подготовка источников данных после загрузки окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareDataSources();
        }

        /// <summary>
        /// Завершение редактирования существующего или создания нового пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (row != (DataRowView)e.Row.Item)
            {
                row = (DataRowView)e.Row.Item;
            }
            bool filled = true;
            int colCount = row.Row.Table.Columns.Count;
            for (int i = 1; i < colCount; i++)
                filled &= !string.IsNullOrEmpty(row[i].ToString());
            if (filled)
            {
                row.BeginEdit();
            }
        }

        /// <summary>
        /// Редактирование существующего или создание нового пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg1_CurrentCellChanged(object sender, EventArgs e)
        {
            if (row != null)
            {
                bool filled = true;
                int colCount = row.Row.Table.Columns.Count;
                for (int i = 1; i < colCount; i++)
                    filled &= !string.IsNullOrEmpty(row[i].ToString());
                if (filled)
                {
                    row.EndEdit();
                    daSQL.Update(dtSQL);
                }
            }
        }


        /// <summary>
        /// При нажатии Delete возможно удалить клиента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is DataGridCell)
            switch (e.Key)
            {
                case Key.Delete:
                    if (MessageBox.Show($"Вы действительно хотите удалить клиента {(dg1.SelectedItem as DataRowView)[0]}?", "Удаление клиента", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        row = (DataRowView)dg1.SelectedItem;
                        row.Row.Delete();
                        daSQL.Update(dtSQL);
                        row = null;
                    }
                    break;
            }
        }

        /// <summary>
        /// При изменении выбранной строки в первой таблице, во второй таблице отображаются покупки выбранного пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dg1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (daAccess != null && dg1.SelectedItem != null)
                QueryMSAccess();
        }

        /// <summary>
        /// Очистка БД пользователей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ClearDB(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите полностью очистить базу данных пользователей?", "Очистка базы данных", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                dg1.SelectedItem = null;
                foreach (DataRowView drv in dg1.ItemsSource)
                {
                    drv.Delete();
                    daSQL.Update(dtSQL);
                }
                dg1.SelectedIndex = 0;
            }
        }
    }
}
