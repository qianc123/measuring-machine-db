﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace MeasuringMachineApp.PagesM2
{
    /// <summary>
    /// Interaction logic for Data2.xaml
    /// </summary>
    public partial class Data2 : Page
    {
        public Data2()
        {
            InitializeComponent();
            Loaded += Data2PageLoaded;
        }

        // Database string - Change if needed
        static string MySQLconnectionString = "datasource=127.0.0.1;port=3306;username=root;password=;database=mjernastanica;SslMode=none";
        // Select all data in database
        //private string query = "SELECT * FROM `stroj2` WHERE 1 ";
        static string query = $"SELECT * FROM `stroj2` ORDER BY IDMjerenje DESC LIMIT 30";
        MySqlConnection databaseConnection = new MySqlConnection(MySQLconnectionString);
        MySqlDataAdapter adapter = new MySqlDataAdapter(query, MySQLconnectionString);

        private void Data2PageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open com with database
                databaseConnection.Open();
                // Fill database with data
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                Machine2Data.DataContext = ds.Tables[0].DefaultView;
                Console.WriteLine("Baza je učitana na stranicu!");
            }
            catch (Exception b)
            {
                Console.WriteLine("Submit error" + b.Message);
            }
            finally
            {
                if (databaseConnection != null)
                {
                    databaseConnection.Close();
                    Console.WriteLine("Veza sa bazom podataka je zatvorena!");
                }
            }
        }
    }
}
