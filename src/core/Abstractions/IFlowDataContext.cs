namespace LowlandTech.TinyTools.Core.Abstractions;

public interface IFlowDataContext
{
    static abstract Guid Id { get; }
    IQueryable<object> Query(string entityType, string criteria);
}
