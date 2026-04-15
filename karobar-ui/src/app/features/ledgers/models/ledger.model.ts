export type LedgerType = 'Farmer' | 'Bank' | 'Party' | 'Product' | 'Expense' | 'Interest' | 'Equity';

export const LEDGER_TYPES: LedgerType[] = ['Farmer', 'Bank', 'Party', 'Product', 'Expense', 'Interest', 'Equity'];

export interface Ledger {
  id: string;
  name: string;
  type: LedgerType;
  isActive: boolean;
}

// Matches LedgerBalanceDto from backend
export interface LedgerBalanceDto {
  ledgerId: string;
  ledgerName: string;
  totalDebit: number;
  totalCredit: number;
  balance: number;
}

// Matches StatementLineDto from backend
export interface StatementLineDto {
  date: string;
  referenceNo: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

// Matches LedgerStatementDto from backend
export interface LedgerStatementDto {
  ledgerId: string;
  ledgerName: string;
  openingBalance: number;
  closingBalance: number;
  lines: StatementLineDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Matches CreateLedgerCommand — type as numeric enum index
export interface CreateLedgerRequest {
  name: string;
  type: number; // 0=Farmer, 1=Bank, 2=Party, 3=Product, 4=Expense, 5=Interest
  openingBalance: number;
  balanceType: 'Receivable' | 'Payable';
  openingBalanceDate: string;
}

export const LEDGER_TYPE_MAP: Record<LedgerType, number> = {
  Farmer: 0,
  Bank: 1,
  Party: 2,
  Product: 3,
  Expense: 4,
  Interest: 5,
  Equity: 6
};

export const LEDGER_TYPE_REVERSE_MAP: Record<number, LedgerType> = {
  0: 'Farmer',
  1: 'Bank',
  2: 'Party',
  3: 'Product',
  4: 'Expense',
  5: 'Interest',
  6: 'Equity'
};

export const LEDGER_TYPE_LABELS: Record<LedgerType, string> = {
  Farmer: 'Farmer',
  Bank: 'Bank',
  Party: 'Party',
  Product: 'Product',
  Expense: 'Expense',
  Interest: 'Interest',
  Equity: 'Equity/System'
};

export interface LedgerStatementParams {
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}
