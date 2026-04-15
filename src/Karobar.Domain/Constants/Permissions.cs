namespace Karobar.Domain.Constants;

public static class Permissions
{
    public static class Ledgers
    {
        public const string Create = "ledger.create";
        public const string View = "ledger.view";
        public const string Update = "ledger.update";
        public const string Delete = "ledger.delete";
    }

    public static class Transactions
    {
        public const string Create = "transaction.create";
        public const string View = "transaction.view";
        public const string Update = "transaction.update";
        public const string Delete = "transaction.delete";
    }

    public static class Users
    {
        public const string Manage = "users.manage";
    }

    public static class Inventory
    {
        public const string Manage = "inventory.manage";
    }
}
