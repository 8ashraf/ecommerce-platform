using Shop.Identity.Entities;

namespace Shop.Identity.Services;

public static class Permissions
{
    public const string CatalogRead = "catalog.read";
    public const string CatalogWrite = "catalog.write";
    public const string OrdersRead = "orders.read";
    public const string OrdersFulfill = "orders.fulfill";
    public const string FinanceRead = "finance.read";
    public const string StaffManage = "staff.manage";

    public static readonly string[] All =
    [
        CatalogRead, CatalogWrite, OrdersRead, OrdersFulfill, FinanceRead, StaffManage
    ];

    public static IReadOnlyList<string> ForRole(OrgRole role)
    {
        if (role == OrgRole.Owner)
        {
            return All;
        }
        else if (role == OrgRole.Admin)
        {
            return new List<string>
            {
                CatalogRead,
                CatalogWrite,
                OrdersRead,
                OrdersFulfill,
                StaffManage
            };
        }
        else if (role == OrgRole.CatalogManager)
        {
            return new List<string>
            {
                CatalogRead,
                CatalogWrite
            };
        }
        else if (role == OrgRole.OrderFulfiller)
        {
            return new List<string>
            {
                OrdersRead,
                OrdersFulfill
            };
        }
        else if (role == OrgRole.Finance)
        {
            return new List<string>
            {
                FinanceRead,
                OrdersRead
            };
        }
        else
        {
            return new List<string>();
        }
    }
}    