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

            // Build configuration from appsettings.json next to the EXE
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read the DefaultConnection string
            var connString = config.GetConnectionString("DefaultConnection");

            // Configure EF Core with MySQL
            var options = new DbContextOptionsBuilder<DepositContext>()
                .UseMySql(connString, ServerVersion.AutoDetect(connString))
                .Options;

            _context = new DepositContext(options);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = new DepositCatalog
                {
                    BankName = txtBankName.Text,
                    DepositName = txtDepositName.Text,
                    DepositType = txtDepositType.Text,
                    ContractInterestDescription = txtContractDesc.Text,
                    InterestType = txtInterestType.Text,
                    IsInterestCapitalized = chkCapitalized.IsChecked == true,
                    InterestPayout = txtInterestPayout.Text,
                    IsMonthlyCompounding = chkMonthlyCompounding.IsChecked == true,
                    Currency = txtCurrency.Text,
                    ForWho = txtForWho.Text,
                    MinAmount = decimal.Parse(txtMinAmount.Text),
                    MaxAmount = decimal.Parse(txtMaxAmount.Text),
                    MinAmountDescription = txtMinAmountDesc.Text,
                    MinTermMonths = int.Parse(txtMinTerm.Text),
                    MaxTermMonths = int.Parse(txtMaxTerm.Text),
                    ValidTermsDescription = txtValidTerms.Text,
                    OverdraftAllowed = chkOverdraft.IsChecked == true,
                    AllowsTopUp = chkTopUp.IsChecked == true,
                    AdditionalConditions = txtAdditionalConditions.Text,
                    TaxRate = decimal.Parse(txtTaxRate.Text)
                };

                _context.DepositCatalogs.Add(item);
                _context.SaveChanges();

                MessageBox.Show("Депозитът е записан успешно!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при запис: {ex.Message}", "Грешка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
