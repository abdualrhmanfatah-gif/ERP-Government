namespace ERP_Government.Application.Common.Models;

public enum ErrorCategory
{
    Validation = 0,
    Authorization = 1,
    NotFound = 2,
    Conflict = 3,
    BusinessRule = 4,
    Internal = 5
}
