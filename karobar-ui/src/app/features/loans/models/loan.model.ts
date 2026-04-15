export interface Loan {
  id: string;
  farmerId: string;
  farmerName: string;
  principalAmount: number;
  interestRate: number;
  startDate: string;
  outstandingBalance: number;
  totalPaid: number;
  status: 'Active' | 'Closed';
  createdAt: string;
}

export interface LoanEvent {
  id: string;
  date: string;
  type: 'LoanGiven' | 'Payment';
  amount: number;
  description?: string;
}

export interface InterestCalculation {
  accruedInterest: number;
  totalOwed: number;
  calculateUntil: string;
}

export interface CreateLoanDto {
  farmerId: string;
  principalAmount: number;
  interestRate: number;
  startDate: string;
}

export interface AddPaymentDto {
  loanId: string;
  amount: number;
  date: string;
}
