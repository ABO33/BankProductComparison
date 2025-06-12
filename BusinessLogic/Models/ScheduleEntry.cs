namespace BusinessLogic.Models
{
    /// <summary>
    /// One line of the repayment/deposit schedule.
    /// </summary>
    public class ScheduleEntry
    {
        public int Period { get; set; }   // month number
        public decimal DepositAmount { get; set; }   // principal
        public decimal InterestRate { get; set; }   // annual % rate, shown on maturity
        public decimal InterestGross { get; set; }   // installment interest (monetary)
        public decimal Tax { get; set; }   // tax on interest
        public decimal TotalPayout { get; set; }   // principal + net interest
    }
}
