using AutoMapper;

namespace ERP_Government.Application.Accounting.Reports.Common;

public class ReportMappingProfile : Profile
{
    public ReportMappingProfile()
    {
        CreateMap<ReportSection, ReportSection>();
        CreateMap<ReportLine, ReportLine>();
    }
}
