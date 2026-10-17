namespace Shop.Identity.Entities;

public enum AccountType { Customer, Merchant, PlatformStaff }
public enum AccountStatus { Pending, Active, Suspended }
public enum OrgStatus { Pending, Active, Suspended }
public enum OrgRole { Owner, Admin, CatalogManager, OrderFulfiller, Finance }

