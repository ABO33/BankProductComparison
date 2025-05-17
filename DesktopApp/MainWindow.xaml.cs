using System;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BusinessLogic.Data;
using BusinessLogic.Models;

namespace DesktopApp
{
    public partial class MainWindow : Window
    {
        private readonly DepositContext _context;

        public MainWindow()
        {
            InitializeComponent();

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<DepositContext>()
                .UseMySql(conn!, ServerVersion.AutoDetect(conn!))
                .Options;

            _context = new DepositContext(options);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var deposit = new Deposit
                {
                    Name = txtName.Text,
                    BankName = txtBank.Text,
                    Currency = txtCurrency.Text,
                    MinAmount = decimal.Parse(txtMinAmount.Text),
                    MaxAmount = decimal.Parse(txtMaxAmount.Text),
                    MinTermMonths = int.Parse(txtMinTerm.Text),
                    MaxTermMonths = int.Parse(txtMaxTerm.Text),
                    InterestRate = decimal.Parse(txtInterestRate.Text),
                    TaxRate = decimal.Parse(txtTaxRate.Text)
                };

                _context.Deposits.Add(deposit);
                _context.SaveChanges();

                MessageBox.Show("Deposit saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}