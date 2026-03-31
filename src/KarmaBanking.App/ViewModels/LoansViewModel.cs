using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

public class LoansViewModel
{
    private readonly ILoanService _loanService;
    private readonly ILoanRepository _loanRepository;
    private readonly AmortizationCalculator _amortizationCalculator;

    public IEnumerable<Loan> loans { get; set; }

    public void loadLoans()
    {
        isLoading = true;
        loans = _loanService.GetAllLoans();
        isLoading = false;
    }
    public LoanEstimate currentEstimate { get; set; }
    public bool isLoading { get; set; }

    public ObservableCollection<AmortizationRow> AmortizationRows { get; set; }

    public LoansViewModel(ILoanService loanService, ILoanRepository loanRepository)
    {
        _loanService = loanService;
        _loanRepository = loanRepository;
        _amortizationCalculator = new AmortizationCalculator();
        AmortizationRows = new ObservableCollection<AmortizationRow>();
    }

    public void LoadAmortization(int loanId)
    {

        var rows = _loanRepository.GetAmortization(loanId);


        if (rows == null || rows.Count == 0)
        {

            var loan = _loanService.GetLoanById(loanId);

            if (loan != null)
            {
                var generatedRows = _amortizationCalculator.generate(loan);
                _loanRepository.SaveAmortization(generatedRows);


                rows = _loanRepository.GetAmortization(loanId);
            }
        }


        AmortizationRows.Clear();

        if (rows != null)
        {
            foreach (var row in rows)
            {
                AmortizationRows.Add(row);
            }
        }
    }
}