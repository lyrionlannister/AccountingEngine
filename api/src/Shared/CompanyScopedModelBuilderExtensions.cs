using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Shared;

public static class CompanyScopedModelBuilderExtensions
{
    private const string CompanyIdPropertyName = "CompanyId";

    public static ModelBuilder ApplyCompanyScopeFilters<TContext>(
        this ModelBuilder modelBuilder,
        TContext dbContext,
        Expression<Func<TContext, long>> currentCompanyId,
        params Type[] excluded)
        where TContext : DbContext
    {
        var companyIdExpression = new ReplaceParameterVisitor(currentCompanyId.Parameters[0], Expression.Constant(dbContext, typeof(TContext)))
            .Visit(currentCompanyId.Body);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (excluded.Contains(clrType) || !HasCompanyIdProperty(clrType))
                continue;

            modelBuilder.Entity(clrType).HasQueryFilter(BuildCompanyFilter(clrType, companyIdExpression));
        }

        return modelBuilder;
    }

    public static void ApplyCompanyIdOnAdd(this ChangeTracker changeTracker, ICurrentCompanyContext currentCompany)
    {
        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            var companyIdProperty = entry.Entity.GetType().GetProperty(CompanyIdPropertyName);
            if (companyIdProperty is null || companyIdProperty.PropertyType != typeof(long))
                continue;

            var currentValue = (long)(companyIdProperty.GetValue(entry.Entity) ?? default(long));
            if (currentValue == default)
                companyIdProperty.SetValue(entry.Entity, currentCompany.CompanyId);
        }
    }

    private static bool HasCompanyIdProperty(Type clrType)
    {
        var property = clrType.GetProperty(CompanyIdPropertyName);
        return property is not null && property.PropertyType == typeof(long);
    }

    private static LambdaExpression BuildCompanyFilter(Type clrType, Expression companyIdExpression)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var efPropertyCall = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(long)],
            parameter,
            Expression.Constant(CompanyIdPropertyName));

        return Expression.Lambda(Expression.Equal(efPropertyCall, companyIdExpression), parameter);
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}
